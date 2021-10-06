using CommandLine;
using DbUp;
using DbUp.Engine.Transactions;
using DbUp.Helpers;
using DbUp.SqlServer;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace AdsGoFastDbUp
{

    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Option('c', "connectionString", Required = true, HelpText = "Target Database Connection String.")]
        public string connectionString { get; set; }
    }

    class Program
    {
        public static string JournalTableSchema = "dbo";
        public static string JournalTableName = "dbupschemaversions";

        static int Main(string[] args)
        {
            int RetVal = -1;

#if DEBUG
            //Set args from local.settings
            using FileStream openStream = File.OpenRead("local.settings.json");
            var o = JsonSerializer.DeserializeAsync<Options>(openStream).Result;
            RetVal = MethodBody(o);
#endif
#if !(DEBUG)
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => { RetVal = MethodBody(o);  });
#endif
            return RetVal;

        }
        
        
        private  static int MethodBody(Options o)
        {
            if (o.Verbose)
            {
                Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                Console.WriteLine("Quick Start Example! App is in Verbose mode!");                
            }
            else
            {
                Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                Console.WriteLine("Quick Start Example!");
            }

            
            //GetAllScripts so that we can loop through versions
            List<DbUp.Engine.SqlScript> AllScripts = DeployChanges.To
                       .SqlDatabase(o.connectionString, "dbo", false).WithScriptsEmbeddedInAssembly(
                              Assembly.GetExecutingAssembly()).Build().GetDiscoveredScripts();

            List<string> Releases = new List<string>();
            foreach (var script in AllScripts)
            {
                string[] parts = script.Name.Split('.');
                if (!(Releases.Contains(parts[1])))
                {
                    Releases.Add(parts[1]);
                }
            }

            var connectionString = o.connectionString;
            //EnsureDatabase.For.SqlDatabase(connectionString);

            foreach (string r in Releases.OrderBy(r => r))
            {
                var A = GetEngine(o, r + "." + "A_Journaled", true);

                var result_A = A.PerformUpgrade();
                if (!result_A.Successful)
                {
                    return ReturnError(result_A.Error.ToString());
                }

            }

            ShowSuccess();
            
            return 0;
            

            
        }


        private static void ShowSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
        }

        private static int ReturnError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
            return -1;
        }
        

        private static DbUp.Engine.UpgradeEngine GetEngine(Options o, string filterstring, bool JournalYN)
        {
            DbUp.Builder.UpgradeEngineBuilder engine = DeployChanges.To
                        .SqlDatabase(o.connectionString, "dbo",false)
                        .WithVariable("TestVariable", "Value")
                        .WithTransactionPerScript()
                        .LogToConsole();
                        
            if (JournalYN)
            {
                return
                        engine                        
                        .WithScriptsEmbeddedInAssembly(
                              Assembly.GetExecutingAssembly(), s => s.Contains(filterstring)).JournalToSqlTable(JournalTableSchema, JournalTableName)                        
                        .Build();
            }
            else
            {
                return
                        engine
                        .WithScriptsEmbeddedInAssembly(
                              Assembly.GetExecutingAssembly(),
                              s => s.Contains(filterstring)).JournalTo(new NullJournal())
                        .Build();
            }            
        }   
    }
}

/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AdsGoFast.SqlServer
{
    public static class TransientDapperExtensions
    {

        public static void CheckDapperRows(dynamic DapperRow, List<string> Keys)
        {
            ICollection<string> _keys = ((IDictionary<string, object>)DapperRow).Keys;
            foreach (string k in Keys)
            {
                if (_keys.Contains(k) == false)
                {
                    //Logging.LogWarning("Dapperrow did not contain column: " + k);
                }
            }

        }
        public static void OpenWithRetry(this SqlConnection cnn)
        {
            int MaxRetry = 3;
            for (int i = 0; i < MaxRetry; i++)
            {
                try
                {
                    if (cnn.State == ConnectionState.Closed)
                    {
                        cnn.Open();
                    }
                    i = MaxRetry;
                    System.Threading.Thread.Sleep(2000);
                }
                catch (SqlException e)
                {
                    DetermineIfRetry(e, ref i, MaxRetry);

                }
            }
        }

        public static void ExecuteWithRetry(this SqlConnection cnn, string Query)
        {
            cnn.ExecuteWithRetry(Query, 30);
        }

            public static void ExecuteWithRetry(this SqlConnection cnn, string Query, int Timeout)
        {
            int MaxRetry = 3;
            for (int i = 0; i < MaxRetry; i++)
            {
                try
                {
                    cnn.Execute(Query,null,null, Timeout,null);
                    i = MaxRetry;                   
                }
                catch (SqlException e)
                {
                    if (DetermineIfRetry(e, ref i, MaxRetry) == false) { i = MaxRetry; };
                    if (i != MaxRetry)
                    { System.Threading.Thread.Sleep(2000); }
                }
            }
        }

        public static bool DetermineIfRetry(SqlException e, ref int i, int MaxRetry)
        {

            bool Ret = false;

            string LogEvent = "SQL Exception Number: " + e.Number.ToString() + ". Error was  " + e.Message.ToString();
            //Logging.LogErrors(new Exception(String.Format(LogEvent)));

            if ((e.Number == 53 || e.Number == 40) && (i < MaxRetry))
            {
                //Logging.LogErrors(new Exception(String.Format("Retrying Connection. Attempt:" + i.ToString())));
                Ret = true;
            }
            else
            {
                //Non Retry Error
                i = MaxRetry;
                //Logging.LogInformation("Non Retry Error... Exiting Retry Loop");
                throw new Exception(LogEvent);
            }

            return Ret;

        }

        public static IEnumerable<T> QueryWithRetry<T>(
            this SqlConnection cnn, string sql, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null
            )
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.OpenWithRetry();
            }

            return cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);

        }

        public static dynamic QueryWithRetry(
        this SqlConnection cnn, string sql, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null
            )
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.OpenWithRetry();
            }

            return cnn.Query(sql, param, transaction, buffered, commandTimeout, commandType);

        }
    }



    public static class SQLExtensionMethods
    {



        public static int FillWithVerboseThrow(this SqlDataAdapter da, DataSet ds)
        {
            int ret = 0;
            try
            {
                da.SelectCommand.Connection.OpenWithRetry();
                ret = da.Fill(ds);
            }
            catch (SqlException e)
            {
                if (e.Number == -2)
                {
                    throw new Exception("Timeout during execution of SQL:" + da.SelectCommand.CommandText.ToString() + ". Timeout was set to " + da.SelectCommand.CommandTimeout.ToString());
                }
                else
                {

                    throw new Exception("Error during execution of SQL:" + da.SelectCommand.CommandText.ToString() + ". Error was  " + e.Message.ToString());
                }
            }
            return ret;
        }

        public static int ExecuteNonQueryWithVerboseThrow(this SqlCommand Cmd)
        {

            int ret = 0;
            try
            {
                Cmd.Connection.OpenWithRetry();
                int MaxRetry = 3;
                for (int i = 0; i < MaxRetry; i++)
                {
                    try
                    {
                        Cmd.ExecuteNonQuery();
                        //Utilities.StaticMethods.LogProvider.GetLog().Info("SqlExecution Succeeded.. Exiting Retry Loop");
                        i = MaxRetry;
                    }
                    catch (SqlException e)
                    {
                        if (TransientDapperExtensions.DetermineIfRetry(e, ref i, MaxRetry) == false) { i = MaxRetry; };
                    }
                }

            }
            catch (SqlException e)
            {
                if (e.Number == -2)
                {
                    //Logging.LogErrors(new Exception(String.Format("Timeout during execution of SQL:{0}. Timeout was set to {1}", Cmd.CommandText.ToString(), Cmd.CommandTimeout.ToString())));
                    throw new Exception("Timeout during execution of SQL:" + Cmd.CommandText.ToString() + ". Timeout was set to " + Cmd.CommandTimeout.ToString());

                }
                else
                {
                    //Logging.LogErrors(new Exception(String.Format("Timeout during execution of SQL:{0}. Error was {1}", Cmd.CommandText.ToString(), e.Message.ToString())));
                    throw new Exception("Error during execution of SQL:" + Cmd.CommandText.ToString() + ". Error was  " + e.Message.ToString());
                }
            }
            return ret;
        }


        public static int ExecuteScalarIntWithRetry(this SqlCommand Cmd)
        {

            int ret = 0;
            try
            {
                Cmd.Connection.OpenWithRetry();
                int MaxRetry = 3;
                for (int i = 0; i < MaxRetry; i++)
                {
                    try
                    {
                        ret = System.Convert.ToInt16(Cmd.ExecuteScalar());
                        //Logging.LogInformation("SqlExecution Succeeded.. Exiting Retry Loop");
                        i = MaxRetry;
                    }
                    catch (SqlException e)
                    {
                        if (TransientDapperExtensions.DetermineIfRetry(e, ref i, MaxRetry) == false) { i = MaxRetry; };
                    }
                }

            }
            catch (SqlException e)
            {
                if (e.Number == -2)
                {
                    //Logging.LogErrors(new Exception(String.Format("Timeout during execution of SQL:{0}. Timeout was set to {1}", Cmd.CommandText.ToString(), Cmd.CommandTimeout.ToString())));
                    throw new Exception("Timeout during execution of SQL:" + Cmd.CommandText.ToString() + ". Timeout was set to " + Cmd.CommandTimeout.ToString());

                }
                else
                {
                    //Logging.LogErrors(new Exception(String.Format("Timeout during execution of SQL:{0}. Error was {1}", Cmd.CommandText.ToString(), e.Message.ToString())));
                    throw new Exception("Error during execution of SQL:" + Cmd.CommandText.ToString() + ". Error was  " + e.Message.ToString());
                }
            }
            return ret;
        }

    }


    public static class ListExtensionMethods
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> pItems)
        {
            DataTable dt = new DataTable();
            T[] data = pItems as T[] ?? pItems.ToArray();
            T fieldNameRow = data.First();

            foreach (System.Reflection.PropertyInfo pInfo in fieldNameRow.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Type type;
                //ToDo: Add error if this fails
                if (pInfo.PropertyType.FullName.Contains("Nullable"))
                {
                    type = Nullable.GetUnderlyingType(pInfo.PropertyType);

                }
                else
                {
                    type = Type.GetType(pInfo.PropertyType.FullName);
                }

                dt.Columns.Add(pInfo.Name, type);
            }

            foreach (T result in data)
            {
                DataRow nr = dt.NewRow();
                foreach (System.Reflection.PropertyInfo pi in result.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        if (pi.GetValue(result, null) == null)
                        {
                            nr[pi.Name] = DBNull.Value;
                        }
                        else
                        {
                            nr[pi.Name] = pi.GetValue(result, null);
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                dt.Rows.Add(nr);
            }

            return dt;

        }
    }

}

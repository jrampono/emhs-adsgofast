/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using FormatWith;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AdsGoFast.SqlServer
{
    public class Utilities
    {

        public void BulkLoadDataToSQL(Logging LogHelper, DataTable Data, SyncTask s, bool DropTempAfterSuccess, bool ExecuteMerge, bool StringifyTargetTable)
        {
            bool NoData = false;
            if (Data == null)
            {
                LogHelper.LogWarning("Table " + s.UniqueName + " has no Columns.");
                NoData = true;
            }
            else
            {
                if ((Data.Columns.Count == 0))
                {
                    LogHelper.LogWarning("Table " + s.UniqueName + " has no Columns.");
                    NoData = true;
                }

            }

            if (NoData == false)
            {

                AdsGoFast.SqlServer.Table tbl = new AdsGoFast.SqlServer.Table
                {
                    ConString = s.TargetConString,
                    Name = string.Format("{0}", s.TargetTableName),
                    Schema = s.TargetSchema
                };

                //SQLObjects.AzureSync.PerformSync_WriteToAzure(Data, tbl, SQLObjects.AzureSync.SyncType.Full);

                SQLTempTableHelper.TemporarySQLDestinationTable DestTable = new SQLTempTableHelper.TemporarySQLDestinationTable();
                Guid g = Guid.NewGuid();

                AdsGoFast.SqlServer.Table TempTable = new AdsGoFast.SqlServer.Table
                {
                    Schema = "",
                    Name = "#Temp_" + s.TargetTableName + "_" + g.ToString() + "",
                    ConString = tbl.ConString
                };

                //Check to see if final target exists.. if not then create temp as final target
                if (tbl.ExistsInDB() == false)
                {
                    TempTable.Schema = tbl.Schema;
                    TempTable.Name = tbl.Name;
                    DropTempAfterSuccess = false;
                    ExecuteMerge = false;
                }

                //Target
                using (SqlConnection conn = new SqlConnection(tbl.ConString))
                {
                    conn.Open();
                    DestTable.CreateTable(SQLTempTableHelper.TemporarySQLDestinationTable.DropType.DropOnFirstCreateOnly, TempTable.Schema, TempTable.Name, Data, conn, true, StringifyTargetTable);
                    TempTable.PersistedCon = conn;

                    int BatchSize = 1000;
                    int TotalRows = Data.Rows.Count;
                    int NumberOfLoops = (TotalRows / BatchSize) + 1;
                    for (int BatchCounter = 0; BatchCounter < NumberOfLoops; BatchCounter++)
                    {
                        int BatchEnd = ((BatchCounter + 1) * BatchSize);
                        if (BatchEnd >= TotalRows)
                        { BatchEnd = TotalRows; }
                        int BatchStart = BatchCounter * BatchSize;

                        LogHelper.LogDebug(string.Format("Writing {2} Records To Database for Table {1}. Total of {0} written.", BatchEnd.ToString(), tbl.QuotedSchemaAndName(), (BatchStart - BatchEnd).ToString()));
                        //using (SqlConnection conn = new SqlConnection(tbl.ConString))
                        //{
                        DataTable Data2 = Data.Clone();

                        for (int i = BatchStart; i < BatchEnd; ++i)
                        {
                            Data2.ImportRow(Data.Rows[i]);
                        }

                        //conn.Open();
                        LogHelper.LogDebug(DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + " Bulk Insert " + TempTable.QuotedSchemaAndName() + " Started..");
                        DestTable.BulkInsertTableData(conn, TempTable.QuotedSchemaAndName(), Data2);
                        LogHelper.LogDebug(DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + " Bulk Insert " + TempTable.QuotedSchemaAndName() + " Completed..");

                        Data2.Clear();
                        Data2.Dispose();
                        //}
                    }


                    if (ExecuteMerge)
                    {
                        //Generate generic merge
                        Console.WriteLine(DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + " Executing Merge..");
                        //using (var Conn = new SqlConnection(tbl.ConString))
                        {
                            SqlTransaction trans = null;
                            string SQL = "";
                            try
                            {
                                //conn.Open();
                                string PrimaryKeyJoin = AdsGoFast.SqlServer.Snippets.GenerateColumnJoinOrUpdateSnippet(TempTable, tbl, "a", "b", "=", " and ", true, true, false, false, false, null, false, false);
                                string ColList = AdsGoFast.SqlServer.Snippets.GenerateColumnJoinOrUpdateSnippet(TempTable, tbl, "", "", "=", ",", true, true, false, false, true, null, true, false);
                                string SelectListForInsert = AdsGoFast.SqlServer.Snippets.GenerateColumnJoinOrUpdateSnippet(TempTable, tbl, "b", "", "", ",", true, false, false, false, true, null, true, false);
                                string InsertList = AdsGoFast.SqlServer.Snippets.GenerateColumnJoinOrUpdateSnippet(TempTable, tbl, "", "", "", ",", true, false, false, false, true, null, true, false);
                                string UpdateClause = AdsGoFast.SqlServer.Snippets.GenerateColumnJoinOrUpdateSnippet(TempTable, tbl, "b", "", "=", ",", false, false, false, false, true, null, false, false);

                                if (s.SyncronisationType == SyncTask.SyncTypeEnum.Full)
                                {
                                    //SQL = GenerateSQLStatementTemplates.GetSQL("SqlTemplates", "GenericMerge",);                                    
                                }

                                if (s.SyncronisationType == SyncTask.SyncTypeEnum.Incremental)
                                {
                                    //SQLT4Templates.IncrementalLoad GM = new SQLT4Templates.IncrementalLoad(TempTable.QuotedSchemaAndName(), tbl.Name, tbl.Schema, ColList, PrimaryKeyJoin, ColList, ColList, UpdateClause, InsertList, SelectListForInsert, ColList);
                                    //SQL = GM.TransformText();
                                }

                                //PrprocessingSQL
                                if (s.PreProcessingQuery != null && s.PreProcessingQuery != "")
                                {
                                    using (SqlCommand Com = new SqlCommand(s.PreProcessingQuery.Replace("<@TempTable/>", TempTable.QuotedSchemaAndName()), conn, trans))
                                    {
                                        Com.CommandTimeout = 600;
                                        Com.ExecuteNonQuery();
                                    }

                                }
                                //Main Merge
                                using (SqlCommand Com = new SqlCommand(SQL, conn, trans))
                                {
                                    Com.CommandTimeout = 600;
                                    Com.ExecuteNonQuery();
                                }

                                //Post Processing
                                if (s.PostProcessingQuery != null && s.PostProcessingQuery != "")
                                {
                                    using (SqlCommand Com = new SqlCommand(s.PostProcessingQuery, conn, trans))
                                    {
                                        Com.CommandTimeout = 600;
                                        Com.ExecuteNonQuery();
                                    }
                                }

                                TempTable.DropTable();

                            }
                            catch (Exception Ex)
                            {
                                if (trans != null)
                                {
                                    trans.Rollback();
                                }

                                throw new Exception("Error Executing SQL: " + SQL + " ErrorMessage: " + Ex.Message);
                            }
                            finally
                            {
                                //TempTable.DropTable();
                            }

                            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd hh:mm:ss") + " Merge Completed..");
                        }
                    }
                }
            }//If Statement attached to Zero Col Datatable check
        }

    }

    public class Database
    {

        public SqlConnection PersistedCon { get; set; }
        public string ConString { get; set; }
        private List<Table> Tables { get; set; }

        public List<Table> GetTables(string Name, string Schema, bool PatternMatchName)
        {

            List<Table> ret = new List<Table>();
            List<string> WHERE = new List<string>();
            if (Name != null && Name != "")
            {
                WHERE.Add("[TABLE_NAME]");
                if (PatternMatchName) { WHERE[WHERE.Count - 1] += string.Format(" like '{0}%'", Name); } else { WHERE[WHERE.Count - 1] += string.Format(" = '{0}'", Name); };
            }
            if (Schema != null && Schema != "")
            {
                WHERE.Add("[SCHEMA_NAME]");
                WHERE[WHERE.Count - 1] += string.Format(" = '{0}'", Schema);
            }

            string SQL = @"
                SELECT Table_Schema [Schema], Table_Name [Name], Table_Type [Type] FROM INFORMATION_SCHEMA.TABLES t WHERE [Table_Type] = 'BASE TABLE'
                ";
            string WHEREC = "";
            foreach (string s in WHERE)
            {
                { WHEREC += " AND "; }
                WHEREC += s;
            }

            using (System.Data.SqlClient.SqlConnection Con = new System.Data.SqlClient.SqlConnection(ConString))
            {
                IEnumerable<Table> res = Con.QueryWithRetry<Table>(SQL + WHEREC);
                foreach (Table t in res)
                {
                    t.ConString = ConString;
                    ret.Add(t);
                }
            }
            return ret;

        }

        public void DeleteAllTempTables(Logging LogHelper)
        {
            foreach (Table t in GetTables("Temp_", "", true))
            {
                LogHelper.LogInformation(string.Format("Deleting Table: {0}", t.Name));
                t.DropTable();
            }

        }
    }
    public class Table
    {
        public SqlConnection PersistedCon { get; set; }

        private bool ColsPopulated = false;

        public string ConString { get; set; }
        public void DropTable()
        {
            string SQL = "";
            if (Name.Contains("#"))
            {
                SQL = @" IF object_id('tempdb.." + Name + @"') IS NOT NULL
                                    BEGIN
                                        DROP TABLE [" + Name + @"]
                                    END";
                //SQLAccess.ExecuteNonQuery(SQLAccess.FPMSqlCon, SQL, true);
            }
            else
            {
                SQL = string.Format(@" IF object_id('[{0}].[{1}]') IS NOT NULL
                                    BEGIN
                                        DROP TABLE [{0}].[{1}]
                                    END", Schema, Name);
            }

            using (System.Data.SqlClient.SqlConnection Con = new System.Data.SqlClient.SqlConnection(ConString))
            {
                Con.ExecuteWithRetry(SQL);
            }
        }

        public string Schema { get; set; }

        public string Name { get; set; }

        public string QuotedSchemaAndName()
        {
            if (Name.Contains("#"))
            {
                return @"[" + Name + "]";
            }
            else
            {
                return @"[" + Schema + "].[" + Name + "]";
            }
        }

        public string SchemaAndName()
        {
            if (Name.Contains("#"))
            {
                return Name;
            }
            else
            {
                return Schema + "." + Name;
            }
        }

        public bool ExistsInDB()
        {
            using (SqlConnection conn = new SqlConnection(ConString))
            {
                string SQL = string.Format(@"
                          SELECT
                                  COUNT(*)
                                FROM
                                  sys.tables t
                                JOIN
                                  sys.schemas s
                                    ON t.schema_id = s.schema_id
                                WHERE
                                  s.name = '{0}' AND t.name = '{1}'", Schema, Name);
                SqlCommand cmd = new SqlCommand(SQL, conn);
                int RetCount = cmd.ExecuteScalarIntWithRetry();
                if (RetCount == 1)
                { return true; }
                else
                { return false; }
            }
        }

        private List<Column> Columns = new List<Column>();

        public List<Column> GetColumnsFromExistingDB(bool ForceRefresh)
        {
            if (ColsPopulated && ForceRefresh == false)
            {
                return Columns;
            }
            else
            {
                List<Column> ret = new List<Column>();

                if (PersistedCon == null)
                {
                    using (System.Data.SqlClient.SqlConnection Con = new System.Data.SqlClient.SqlConnection(ConString))
                    {
                        ret = GetColumnsFromExistingDB(ForceRefresh, Con);
                    }
                }
                else
                {
                    ret = GetColumnsFromExistingDB(ForceRefresh, PersistedCon);
                }
                Columns = ret;
                ColsPopulated = true;
                return ret;
            }

        }

        private List<Column> GetColumnsFromExistingDB(bool ForceRefresh, System.Data.SqlClient.SqlConnection Con = null)
        {
            if (ColsPopulated && ForceRefresh == false)
            {
                return Columns;
            }
            else
            {
                List<Column> ret = new List<Column>();
                string sql = "";

                if (Name.StartsWith("#") == false)
                {

                    sql = string.Format(@"
                            SELECT
                                c.ORDINAL_POSITION,
                                c.COLUMN_NAME,
                                c.DATA_TYPE,
                                IS_NULLABLE = cast(case when c.IS_NULLABLE = 'Yes' then 1 else 0 END as bit),
                                c.NUMERIC_PRECISION,
                                c.CHARACTER_MAXIMUM_LENGTH,
                                c.NUMERIC_SCALE,
                                is_identity = case when COLUMNPROPERTY(object_id(t.TABLE_SCHEMA+'.'+t.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') = 1 THEN 1 else 0 END,
                                is_computed = case when COLUMNPROPERTY(object_id(t.TABLE_SCHEMA+'.'+t.TABLE_NAME), c.COLUMN_NAME, 'IsComputed') = 1 THEN 1 else 0 END,
                                KEY_COLUMN = cast(CASE WHEN kcu.TABLE_NAME IS NULL THEN 0 ELSE 1 END as bit)
                            FROM INFORMATION_SCHEMA.COLUMNS c with (NOLOCK)
                            INNER JOIN INFORMATION_SCHEMA.tables t with (NOLOCK) on c.TABLE_NAME = t.TABLE_NAME and c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_CATALOG = c.TABLE_CATALOG
                            LEFT OUTER join INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu with (NOLOCK) ON c.TABLE_CATALOG = kcu.TABLE_CATALOG and c.TABLE_SCHEMA = kcu.TABLE_SCHEMA AND c.TABLE_NAME = kcu.TABLE_NAME and c.COLUMN_NAME = kcu.COLUMN_NAME
                            WHERE t.TABLE_NAME = '{0}' AND t.TABLE_SCHEMA = '{1}'               
                        ", Name, Schema);
                }
                else
                {
                    sql = string.Format(@"     
                          SELECT
                             c.ORDINAL_POSITION,
                             c.COLUMN_NAME,
                             c.DATA_TYPE,
                             IS_NULLABLE = cast(case when c.IS_NULLABLE = 'Yes' then 1 else 0 END as bit),
                             c.NUMERIC_PRECISION,
                             c.CHARACTER_MAXIMUM_LENGTH,
                             c.NUMERIC_SCALE,
                             is_identity = case when COLUMNPROPERTY(object_id('tempdb..'+QUOTENAME(t.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') = 1 THEN 1 else 0 END,
                             is_computed = case when COLUMNPROPERTY(object_id('tempdb..'+QUOTENAME(t.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed') = 1 THEN 1 else 0 END,
                             KEY_COLUMN = cast(CASE WHEN kcu.TABLE_NAME IS NULL THEN 0 ELSE 1 END as bit)
                        FROM Tempdb.INFORMATION_SCHEMA.COLUMNS c with (NOLOCK)

                        INNER JOIN Tempdb.INFORMATION_SCHEMA.tables t with (NOLOCK) on c.TABLE_NAME = t.TABLE_NAME and c.TABLE_SCHEMA = t.TABLE_SCHEMA and c.TABLE_CATALOG = c.TABLE_CATALOG

                        LEFT OUTER join Tempdb.INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu with (NOLOCK) ON c.TABLE_CATALOG = kcu.TABLE_CATALOG and c.TABLE_SCHEMA = kcu.TABLE_SCHEMA AND c.TABLE_NAME = kcu.TABLE_NAME and c.COLUMN_NAME = kcu.COLUMN_NAME

                        WHERE object_id('tempdb..'+QUOTENAME(t.Table_name)) = object_id('tempdb..'+'{0}') 
                        ", Name);
                }



                IEnumerable<Column> res = Con.QueryWithRetry<Column>(sql);


                foreach (Column c in res)
                {
                    c.COLUMN_NAME = c.COLUMN_NAME.ToLower();
                    ret.Add(c);
                }

                Columns = ret;
                ColsPopulated = true;
                return ret;
            }

        }

        public class Column
        {
            public string ORDINAL_POSITION { get; set; }
            public string COLUMN_NAME { get; set; }
            public string DATA_TYPE { get; set; }
            public bool IS_NULLABLE { get; set; }
            public bool IS_COMPUTED { get; set; }
            public bool IS_IDENTITY { get; set; }
            public int NUMERIC_PRECISION { get; set; }
            public int CHARACTER_MAXIMUM_LENGTH { get; set; }
            public int NUMERIC_SCALE { get; set; }
            public bool KEY_COLUMN { get; set; }

        }

        public void GenerateMergeProcedure()
        {
            using (SqlConnection Conn = new SqlConnection(ConString))
            {
                SqlTransaction trans = null;
                string SQL = "";
                try
                {
                    Conn.Open();
                    string PrimaryKeyJoin = Snippets.GenerateColumnJoinOrUpdateSnippet(this, this, "a", "b", "=", " and ", true, true, false, false, false, null, false, false);
                    string ColList = Snippets.GenerateColumnJoinOrUpdateSnippet(this, this, "", "", "=", ",", true, true, false, false, true, null, true, false);
                    string SelectListForInsert = Snippets.GenerateColumnJoinOrUpdateSnippet(this, this, "b", "", "", ",", true, false, false, false, true, null, true, false);
                    string InsertList = Snippets.GenerateColumnJoinOrUpdateSnippet(this, this, "", "", "", ",", true, false, false, false, true, null, true, false);
                    string UpdateClause = Snippets.GenerateColumnJoinOrUpdateSnippet(this, this, "b", "", "=", ",", false, false, false, false, true, null, false, false);

                    {
                        //SQLT4Templates.IncrementalLoad GM = new SQLT4Templates.IncrementalLoad(this.QuotedSchemaAndName(), this.Name, this.Schema, ColList, PrimaryKeyJoin, ColList, ColList, UpdateClause, InsertList, SelectListForInsert, ColList);
                        //SQL = GM.TransformText();
                    }



                }
                catch (Exception Ex)
                {
                    if (trans != null)
                    {
                        trans.Rollback();
                    }

                    throw new Exception("Error Executing SQL: " + SQL + " ErrorMessage: " + Ex.Message);
                }
                finally
                {
                    //TempTable.DropTable();
                }


            }

        }
    }


    public class Snippets
    {
        public static string GenerateColumnJoinOrUpdateSnippet(Table SourceTable, Table TargetTable, string SourceAlias, string TargetAlias, string BooleanOperator, string Concatenator, bool IncludePrimaryKeys, bool IncludeIdentities, bool IncludeCalculatedColumns, bool IncludeAdhocColumnList, bool IncludeOtherColumns, List<string> AdhocColumnList, bool SourceListOnly, bool IncludeNullExclusionForWhereClauses)
        {
            string output = "";
            bool clusteredIndexColumn = false;

            foreach (Table.Column c in SourceTable.GetColumnsFromExistingDB(false))
            {

                if (c.DATA_TYPE.ToLower() != "timestamp" && c.IS_COMPUTED == false) //Exclude all timestamp and calculated columns
                {
                    if (TargetTable.GetColumnsFromExistingDB(false).Exists(x => x.COLUMN_NAME == c.COLUMN_NAME)) //Column Exists in Both Source and Target
                    {
                        Table.Column cTarget = TargetTable.GetColumnsFromExistingDB(false).Find(x => x.COLUMN_NAME == c.COLUMN_NAME);
                        clusteredIndexColumn = false;

                        if (cTarget.KEY_COLUMN)
                        {
                            clusteredIndexColumn = true;
                        }

                        if (
                              (IncludeAdhocColumnList && AdhocColumnList.Contains(cTarget.COLUMN_NAME) == false) //AdhocInclusionExclusion
                            || (IncludeIdentities && cTarget.IS_IDENTITY == true)  //IdentityInclusion
                            || (IncludePrimaryKeys && (cTarget.KEY_COLUMN || clusteredIndexColumn == true))  //IdentityInclusion
                            || (IncludeCalculatedColumns && cTarget.IS_COMPUTED == true)  //ComputedInclusion
                            || (cTarget.KEY_COLUMN == false && cTarget.IS_IDENTITY == false && cTarget.IS_COMPUTED == false && IncludeOtherColumns == true) //Include other columns
                           )
                        {
                            output = (output == "" ? "" : output + Concatenator);

                            if (!(SourceListOnly))
                            {
                                output = output
                                            + (TargetAlias == "" ? "" : TargetAlias + ".")
                                            + (string.Concat("[", c.COLUMN_NAME, "]") + " ")
                                            + (BooleanOperator + " ");

                            }

                            output = output
                                        + (SourceAlias == "" ? "" : SourceAlias + ".")
                                         + (string.Concat("[", c.COLUMN_NAME, "]"));



                        }
                    }
                }



            }
            return output;


        }


    }

    public class SyncTask
    {
        public string UniqueName { get; set; }
        public string TargetSchema { get; set; }
        public string TargetTableName { get; set; }
        public string TargetConString { get; set; }

        public List<string> ExcludedColumns { get; set; }

        public SyncTypeEnum SyncronisationType { get; set; }
        public string PreProcessingQuery { get; set; }

        public string PostProcessingQuery { get; set; }

        public bool StringifyTargetTable { get; set; }

        public bool IgnoreSchemaFile { get; set; }

        public bool PatternMatchSourceFileName { get; set; }

        public enum SyncTypeEnum { Full, Incremental, FullWithTableSwap, FullPurgeExsiting }

    }

    public static class GenerateSQLStatementTemplates
    {
        public static string GetSQL(string PathReference, string FileReference, Dictionary<string, string> Params)
        {
            string SQL = System.IO.File.ReadAllText(PathReference + FileReference + ".sql");
            SQL = SQL.FormatWith(Params, MissingKeyBehaviour.ThrowException, null, '{', '}');
            return SQL;
        }

        public static string GetCreateTable(JArray Array, string TargetTableSchema, string TargetTableName, bool DropIfExist)
        {
            string _DropIfExistStatement = null;
            string _CreateSchema = null;
            if (!TargetTableName.Contains("#"))
            {
                if (DropIfExist == true)
                {
                    _DropIfExistStatement = string.Format(@"IF EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'U'))
                    BEGIN
                        DROP TABLE [{0}].[{1}]
                    END " + System.Environment.NewLine, TargetTableSchema, TargetTableName);
                }
                else
                {
                    _DropIfExistStatement = string.Format(@"IF NOT EXISTS(SELECT* FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'U'))
                    BEGIN
                        DROP TABLE [{0}].[{1}]
                    " + System.Environment.NewLine, TargetTableSchema, TargetTableName);
                }

                if (!TargetTableSchema.Equals("dbo"))
                {
                    _CreateSchema = string.Format(@"
                    if not exists(select 1 from information_schema.schemata where schema_name='{0}')
                    BEGIN
                        EXEC ('CREATE SCHEMA {0} AUTHORIZATION dbo;')
                    END
                    " + System.Environment.NewLine, TargetTableSchema);
                }
            }

            string _CreateStatement = string.Format(@"Create Table [{0}].[{1}] (" + System.Environment.NewLine, TargetTableSchema, TargetTableName);
            string _CreateStatementPK = null;
            int LengthCounter = 0;
            foreach (JObject r in Array)
            {
                LengthCounter = LengthCounter + 1;
                string SqlType = "varchar(max)";

                SqlType = r["DATA_TYPE"].ToString();
                string np = r["NUMERIC_PRECISION"].ToString();
                string ns = r["NUMERIC_SCALE"].ToString();
                string cml = r["CHARACTER_MAXIMUM_LENGTH"].ToString();

                if (SqlType.Contains("varchar") || SqlType.Contains("varbinary"))
                {
                    cml = (cml == "-1") ? "Max" : cml;
                }

                if (SqlType.Contains("xml"))
                {
                    cml = null;
                }

                if ((np != "" || ns != "") && SqlType.Equals("decimal"))
                {
                    SqlType += " (";
                    if (np != null) { SqlType += np + ","; } else { SqlType += "0,"; };
                    if (ns != null) { SqlType += ns + ")"; } else { SqlType += "0)"; };
                }

                if (cml != "" && cml != null)
                {
                    SqlType += "(" + cml + ")";
                }

                string NullableFlag = "";
                if (System.Convert.ToBoolean(r["IS_NULLABLE"])) { NullableFlag = "null"; } else { NullableFlag = "not null"; };

                _CreateStatement += string.Format("[{0}] {1} {2}" + System.Environment.NewLine, r["COLUMN_NAME"].ToString(), SqlType, NullableFlag);
                if (LengthCounter < Array.Count) { _CreateStatement += ","; };

                string kc = r["PKEY_COLUMN"].ToString();

                if (kc.Equals("True"))
                {
                    if (_CreateStatementPK == null)
                    {
                        _CreateStatementPK = string.Format(@"CONSTRAINT [PK_{0}_{1}] PRIMARY KEY CLUSTERED (" + System.Environment.NewLine, TargetTableSchema, TargetTableName);
                    }

                    _CreateStatementPK += string.Format("[{0}],", r["COLUMN_NAME"].ToString());
                }
            }
            if (_CreateStatementPK != null)
            {
                _CreateStatementPK = _CreateStatementPK.TrimEnd(new char[] { ',' }) + ")" + Environment.NewLine;
            }

            _CreateStatement += Environment.NewLine + _CreateStatementPK + ")";

            if (DropIfExist == true || TargetTableName.Contains("#"))
            {
                _CreateStatement = _DropIfExistStatement + _CreateSchema + _CreateStatement;
            }
            else
            {
                _CreateStatement = _DropIfExistStatement + _CreateSchema + _CreateStatement + Environment.NewLine + "END";
            }

            return _CreateStatement;

        }

    }


}

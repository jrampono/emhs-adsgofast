using AdsGoFast.SqlServer;
using AdsGoFast.TaskMetaData;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace AdsGoFast
{
    //Todo Update to IDisposable
    public class TaskMetaDataDatabase
    {
        public void LogTaskInstanceCompletion(System.Int64 TaskInstanceId, System.Guid Executionid, BaseTasks.TaskStatus Status, System.Guid AdfRunUid, string Comment)
        {
            SqlConnection _con = this.GetSqlConnection();
            Dictionary<string, object> SqlParams = new Dictionary<string, object>
                    {
                        { "ExecutionStatus", Status.ToString() },
                        { "TaskInstanceId", TaskInstanceId},
                        { "ExecutionUid", Executionid},
                        { "AdfRunUid", AdfRunUid},
                        { "Comment", Comment}
                    };

            string _cmd = @"exec UpdTaskInstanceExecution @ExecutionStatus, @TaskInstanceId, @ExecutionUid, @AdfRunUid, @Comment";

            _con.Execute(_cmd, SqlParams);
            
        }

        public string GetConnectionString()
        {
            SqlConnectionStringBuilder _scsb = new SqlConnectionStringBuilder
            {
                DataSource = Shared.GlobalConfigs.GetStringConfig("AdsGoFastTaskMetaDataDatabaseServer"),
                InitialCatalog = Shared.GlobalConfigs.GetStringConfig("AdsGoFastTaskMetaDataDatabaseName")
            };
            if (Shared.GlobalConfigs.GetBoolConfig("AdsGoFastTaskMetaDataDatabaseUseTrustedConnection"))
            {
                _scsb.IntegratedSecurity = true;
            }
            else
            {
                _scsb.IntegratedSecurity = false;
            }
            return _scsb.ConnectionString;

        }

        public void ExecuteSql(string SqlCommandText)
        {
            SqlCommand _cmd = new SqlCommand
            {
                Connection = GetSqlConnection(),
                CommandText = SqlCommandText
            };
            _cmd.ExecuteNonQueryWithVerboseThrow();
        }


        public void ExecuteSql(string SqlCommandText, SqlConnection _con)
        {
            SqlCommand _cmd = new SqlCommand
            {
                Connection = _con,
                CommandText = string.Format(SqlCommandText)
            };
            _cmd.ExecuteNonQueryWithVerboseThrow();

        }

        public SqlConnection GetSqlConnection()
        {
            SqlConnection _con = new SqlConnection(GetConnectionString());
            if (!Shared.GlobalConfigs.GetBoolConfig("AdsGoFastTaskMetaDataDatabaseUseTrustedConnection"))
            {
                string _token = Shared.Azure.AzureSDK.GetAzureRestApiToken("https://database.windows.net/");
                _con.AccessToken = _token;
            }
            return _con;
        }

        public void BulkInsert(DataTable data, Table TargetTable, bool CreateTable)
        {
            SqlConnection _con = GetSqlConnection();
            _con.Open();
            BulkInsert(data, TargetTable, CreateTable, _con);
            _con.Close();
        }

        public void BulkInsert(DataTable data, Table TargetTable, bool CreateTable,
            SqlConnection _con)
        {
            SqlServer.SQLTempTableHelper.TemporarySQLDestinationTable DestTable = new SqlServer.SQLTempTableHelper.TemporarySQLDestinationTable();
            if (CreateTable == true)
            {
                DestTable.CreateTable(SqlServer.SQLTempTableHelper.TemporarySQLDestinationTable.DropType.DropOnFirstCreateOnly, TargetTable.Schema, TargetTable.Name, data, _con, true, false);
            }

            DestTable.BulkInsertTableData(_con, TargetTable.QuotedSchemaAndName(), data);
        }

        public void AutoBulkInsertAndMerge(DataTable dt, string StagingTableName, string TargetTableName)
        {
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using (SqlConnection conn = TMD.GetSqlConnection())
            {

                Table SourceTable = new Table
                {
                    Name = StagingTableName,
                    Schema = null,
                    PersistedCon = conn
                };


                Table TargetTable = new Table
                {
                    Name = TargetTableName,
                    Schema = "dbo",
                    PersistedCon = conn
                };
                TargetTable.GetColumnsFromExistingDB(true);

                TMD.BulkInsert(dt, SourceTable, true, conn);
                SourceTable.GetColumnsFromExistingDB(true);

                string PrimaryKeyJoin = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "a", "b", "=", " and ", true, true, false, false, false, null, false, false);
                string ColList = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "", "", "=", ",", true, true, false, false, true, null, true, false);
                string SelectListForInsert = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "b", "", "", ",", true, false, false, false, true, null, true, false);
                string InsertList = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "", "", "", ",", true, false, false, false, true, null, true, false);
                string UpdateClause = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "b", "", "=", ",", false, false, false, false, true, null, false, false);


                Dictionary<string, string> SqlParams = new Dictionary<string, string>
                {
                    { "TargetFullName", TargetTable.QuotedSchemaAndName() },
                    { "SourceFullName", SourceTable.QuotedSchemaAndName() },
                    { "PrimaryKeyJoin_AB", PrimaryKeyJoin },
                    { "UpdateClause", UpdateClause },
                    { "SelectListForInsert", SelectListForInsert },
                    { "InsertList", InsertList }
                };


                string MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "GenericMerge", SqlParams);

                conn.Execute(MergeSQL);
            }
        }
        public string GenerateMergeSQL(string StagingTableSchema, string StagingTableName, string TargetTableSchema, string TargetTableName, SqlConnection _con, bool CheckSchemaDrift, Logging logging)
        {
            if (CheckSchemaDrift)
            {
                SqlCommand cmd = new SqlCommand(string.Format("Select * from {0}.{1}", StagingTableSchema, StagingTableName), _con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable stagingDT = new DataTable();
                da.Fill(stagingDT);

                cmd = new SqlCommand(string.Format("Select * from {0}.{1}", TargetTableSchema, TargetTableName), _con);
                da = new SqlDataAdapter(cmd);
                DataTable targetDT = new DataTable();
                da.Fill(targetDT);

                bool schemaEqual = DataTableSchemaCompare.SchemaEquals(stagingDT, targetDT);


                if (schemaEqual == false)
                {
                    logging.LogWarning(string.Format("****Schema Drift for Table {0}.{1} to {2}.{3}", StagingTableSchema, StagingTableName.Replace("#Temp_", ""), TargetTableSchema, TargetTableName.Replace("#Temp_", "")));
                }
            }

            Table SourceTable = new Table
            {
                Name = StagingTableName,
                Schema = StagingTableSchema,
                PersistedCon = _con
            };

            Table TargetTable = new Table
            {
                Name = TargetTableName,
                Schema = TargetTableSchema,
                PersistedCon = _con
            };
            TargetTable.GetColumnsFromExistingDB(true);
            SourceTable.GetColumnsFromExistingDB(true);

            string PrimaryKeyJoin = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "a", "b", "=", " and ", true, true, false, false, false, null, false, false);
            string ColList = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "", "", "=", ",", true, true, false, false, true, null, true, false);
            string SelectListForInsert = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "b", "", "", ",", true, false, false, false, true, null, true, false);
            string InsertList = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "", "", "", ",", true, false, false, false, true, null, true, false);
            string UpdateClause = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "b", "", "=", ",", false, false, false, false, true, null, false, false);


            Dictionary<string, string> SqlParams = new Dictionary<string, string>
            {
                { "TargetFullName", TargetTable.SchemaAndName() },
                { "SourceFullName", SourceTable.SchemaAndName() },
                { "PrimaryKeyJoin_AB", PrimaryKeyJoin },
                { "UpdateClause", UpdateClause },
                { "SelectListForInsert", SelectListForInsert },
                { "InsertList", InsertList }
            };

            string MergeSQL = string.Empty;

            if (PrimaryKeyJoin.Length>=4)
            {
                MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "GenericMerge", SqlParams);
            }
            else
            {
                MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "GenericTruncateInsert", SqlParams);
            }

            return MergeSQL;
        }

        public string GenerateInsertSQL(string StagingTableSchema, string StagingTableName, string TargetTableSchema, string TargetTableName, SqlConnection _con)
        {
            Table SourceTable = new Table
            {
                Name = StagingTableName,
                Schema = StagingTableSchema,
                PersistedCon = _con
            };

            Table TargetTable = new Table
            {
                Name = TargetTableName,
                Schema = TargetTableSchema,
                PersistedCon = _con
            };
            TargetTable.GetColumnsFromExistingDB(true);
            SourceTable.GetColumnsFromExistingDB(true);


            string PrimaryKeyJoin = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "a", "b", "=", " and ", true, true, false, false, false, null, false, false);
            string ColList = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "", "", "=", ",", true, true, false, false, true, null, true, false);
            string SelectListForInsert = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "b", "", "", ",", true, false, false, false, true, null, true, false);
            string InsertList = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "", "", "", ",", true, false, false, false, true, null, true, false);
            string UpdateClause = Snippets.GenerateColumnJoinOrUpdateSnippet(SourceTable, TargetTable, "b", "", "=", ",", false, false, false, false, true, null, false, false);


            Dictionary<string, string> SqlParams = new Dictionary<string, string>
            {
                { "TargetFullName", TargetTable.SchemaAndName() },
                { "SourceFullName", SourceTable.SchemaAndName() },
                { "PrimaryKeyJoin_AB", PrimaryKeyJoin },
                { "UpdateClause", UpdateClause },
                { "SelectListForInsert", SelectListForInsert },
                { "InsertList", InsertList }
            };


            string MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "GenericInsert", SqlParams);

            return MergeSQL;
        }

    }

    public static class DataTableSchemaCompare
    {
        public static bool SchemaEquals(this DataTable dt, DataTable value)
        {
            if (dt.Columns.Count != value.Columns.Count)
            {
                return false;
            }

            IEnumerable<DataColumn> dtColumns = dt.Columns.Cast<DataColumn>();
            IEnumerable<DataColumn> valueColumns = value.Columns.Cast<DataColumn>();


            int exceptCount = dtColumns.Except(valueColumns, DataColumnEqualityComparer.Instance).Count();
            return (exceptCount == 0);


        }
    }

    internal class DataColumnEqualityComparer : IEqualityComparer<DataColumn>
    {
        #region IEqualityComparer Members

        private DataColumnEqualityComparer() { }
        public static DataColumnEqualityComparer Instance = new DataColumnEqualityComparer();


        public bool Equals(DataColumn x, DataColumn y)
        {
            if (x.ColumnName != y.ColumnName)
            {
                return false;
            }

            if (x.DataType != y.DataType)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(DataColumn obj)
        {
            int hash = 17;
            hash = 31 * hash + obj.ColumnName.GetHashCode();
            hash = 31 * hash + obj.DataType.GetHashCode();

            return hash;
        }

        #endregion
    }
}

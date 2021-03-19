/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;


namespace AdsGoFast.SqlServer.SQLTempTableHelper
{

    public class TemporarySQLDestinationTable
    {
        public bool TableCreated { get; set; }
        public bool DataInserted { get; set; }

        public TemporarySQLDestinationTable()
        {
            TableCreated = false;
            DataInserted = false;
        }

        public enum DropType { DropAlways, DropOnFirstCreateOnly, DontDrop };

        public void CreateTable(DropType DropType, string TempTableSchemaNoQuotes, string TempTableNameNoQuotes, DataTable DataTbl, SqlConnection conn, bool AddPkey, bool forceString)
        {
            string SQL = "";
            if ((DropType == TemporarySQLDestinationTable.DropType.DropAlways) || ((DropType == TemporarySQLDestinationTable.DropType.DropOnFirstCreateOnly) && (TableCreated == false)))
            {
                if (TempTableNameNoQuotes.StartsWith("#"))
                {
                    SQL = @" IF object_id('tempdb.." + TempTableNameNoQuotes + @"') IS NOT NULL
                                    BEGIN
                                        DROP TABLE [" + TempTableNameNoQuotes + @"]
                                    END";
                    //SQLAccess.ExecuteNonQuery(SQLAccess.FPMSqlCon, SQL, true);
                }
                else
                {
                    SQL = string.Format(@" IF object_id('[{0}].[{1}]') IS NOT NULL
                                    BEGIN
                                        DROP TABLE [{0}].[{1}]
                                    END", TempTableSchemaNoQuotes, TempTableNameNoQuotes);
                }
                SqlCommand sql_com = new SqlCommand(SQL, conn);
                sql_com.ExecuteNonQueryWithVerboseThrow();
                SQL = "";
                sql_com = null;
            }

            if (TableCreated == false)
            {
                //SQL = SQLDataAccess.SqlTableCreator.GetCreateFromDataTableSQL(TempTableName, DataTbl, false);
                SQL = SqlTableCreator.GetCreateFromDataTableSQL(TempTableSchemaNoQuotes, TempTableNameNoQuotes, DataTbl, false, forceString);
                //SQLAccess.ExecuteNonQuery(SQLAccess.FPMSqlCon, SQL, true);
                SqlCommand sql_com = new SqlCommand(SQL, conn);
                sql_com.ExecuteNonQueryWithVerboseThrow();
                SQL = "";
                if (AddPkey)
                {
                    if (TempTableNameNoQuotes.StartsWith("#"))
                    {
                        SQL = string.Format(@"Alter Table [{0}] add [Pkey{1}] bigint identity(1,1) PRIMARY KEY CLUSTERED", TempTableNameNoQuotes, Guid.NewGuid().ToString());
                    }
                    else
                    {
                        SQL = string.Format(@"Alter Table [{0}].[{1}] add [Pkey{2}] bigint identity(1,1) PRIMARY KEY CLUSTERED", TempTableSchemaNoQuotes, TempTableNameNoQuotes, Guid.NewGuid().ToString());
                    }
                    sql_com = new SqlCommand(SQL, conn);
                    sql_com.ExecuteNonQueryWithVerboseThrow();
                }
                TableCreated = true;
                sql_com = null;
            }
        }

        /// <summary>
        /// Bulk insert DataTable rows in to the specified SQL Table
        /// </summary>
        /// <param name="sqlConnection">SQL Connection</param>
        /// <param name="destSqlTableName">Destination SQL Table name</param>
        /// <param name="dataTable">DataTable woth rows to insert in DB</param>
        public void BulkInsertTableData(SqlConnection sqlConnection, string destSqlTableName, DataTable dataTable)
        {
            int BCTimeOut = 260;
            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.FireTriggers, null))
                {
                    bulkCopy.BulkCopyTimeout = BCTimeOut;
                    bulkCopy.DestinationTableName = destSqlTableName;
                    bulkCopy.WriteToServer(dataTable);
                    bulkCopy.Close();
                    DataInserted = true; // indicate copying completed successfully
                }
            }
            catch (SqlException e)
            {
                DataInserted = false;
                if (e.Number == -2)
                {
                    throw new Exception("Timeout during bulk copy of TempTable:" + destSqlTableName + ". Timeout was set to " + BCTimeOut.ToString() + "; RowCount of incoming Table: " + dataTable.Rows.Count.ToString());
                }
                else
                {
                    throw new Exception("Error during bulk copy of TempTable:" + destSqlTableName + ". Error was  " + e.Message.ToString() + "; RowCount of incoming Table: " + dataTable.Rows.Count.ToString());
                }
            }




        }
    }

    public class SqlTableCreator
    {
        #region Instance Variables
        private SqlConnection _connection;
        public SqlConnection Connection
        {
            get => _connection;
            set => _connection = value;
        }

        private SqlTransaction _transaction;
        public SqlTransaction Transaction
        {
            get => _transaction;
            set => _transaction = value;
        }

        private string _tableName;
        public string DestinationTableName
        {
            get => _tableName;
            set => _tableName = value;
        }
        #endregion

        #region Constructor
        public SqlTableCreator() { }
        public SqlTableCreator(SqlConnection connection) : this(connection, null) { }
        public SqlTableCreator(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }
        #endregion

        #region Instance Methods
        public object Create(DataTable schema, bool forceString)
        {
            return Create(schema, null, forceString);
        }
        public object Create(DataTable schema, int numKeys, bool forceString)
        {
            int[] primaryKeys = new int[numKeys];
            for (int i = 0; i < numKeys; i++)
            {
                primaryKeys[i] = i;
            }
            return Create(schema, primaryKeys, forceString);
        }
        public object Create(DataTable schema, int[] primaryKeys, bool forceString)
        {
            string sql = GetCreateSQL(_tableName, schema, primaryKeys, forceString);

            SqlCommand cmd;
            if (_transaction != null && _transaction.Connection != null)
            {
                cmd = new SqlCommand(sql, _connection, _transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, _connection);
            }

            return cmd.ExecuteNonQueryWithVerboseThrow();
        }

        public object CreateFromDataTable(DataTable table, bool forceString)
        {
            string sql = GetCreateFromDataTableSQL("dbo", _tableName, table, forceString);

            SqlCommand cmd;
            if (_transaction != null && _transaction.Connection != null)
            {
                cmd = new SqlCommand(sql, _connection, _transaction);
            }
            else
            {
                cmd = new SqlCommand(sql, _connection);
            }

            return cmd.ExecuteNonQueryWithVerboseThrow();
        }
        #endregion

        #region Static Methods

        public static string GetCreateSQL(string tableName, DataTable schema, int[] primaryKeys, bool forceString)
        {
            string sql = "CREATE TABLE " + tableName + " (\n";

            // columns
            foreach (DataRow column in schema.Rows)
            {
                if (!(schema.Columns.Contains("IsHidden") && (bool)column["IsHidden"]))
                {
                    sql += column["ColumnName"].ToString() + " " + SQLGetType(column, forceString) + ",\n";
                }
            }
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + "\n";

            // primary keys
            string pk = "CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED (";
            bool hasKeys = (primaryKeys != null && primaryKeys.Length > 0);
            if (hasKeys)
            {
                // user defined keys
                foreach (int key in primaryKeys)
                {
                    pk += schema.Rows[key]["ColumnName"].ToString() + ", ";
                }
            }
            else
            {
                // check schema for keys
                string keys = string.Join(", ", GetPrimaryKeys(schema));
                pk += keys;
                hasKeys = keys.Length > 0;
            }
            pk = pk.TrimEnd(new char[] { ',', ' ', '\n' }) + ")\n";
            if (hasKeys)
            {
                sql += pk;
            }

            sql += ")";

            return sql;
        }

        public static string GetCreateFromDataTableSQL(string TableSchema, string tableName, DataTable table, bool forceString)
        {
            return GetCreateFromDataTableSQL(TableSchema, tableName, table, true, forceString);
        }
        public static string GetCreateFromDataTableSQL(string TableSchema, string tableName, DataTable table, bool IncludePrimaryKey, bool forceString)
        {

            string sql = "";

            if (tableName.StartsWith("#"))
            {
                sql = "CREATE TABLE [" + tableName + @"] (" + Environment.NewLine;
            }
            else
            {
                sql = "CREATE TABLE [" + TableSchema + "].[" + tableName + @"] (" + Environment.NewLine;
            }

            // columns
            foreach (DataColumn column in table.Columns)
            {
                sql += "[" + column.ColumnName + "] " + SQLGetType(column, forceString) + "," + Environment.NewLine;
            }
            sql = sql.TrimEnd(new char[] { ',' }) + Environment.NewLine;
            // primary keys
            if ((table.PrimaryKey.Length > 0) && (IncludePrimaryKey == true))
            {
                sql += "CONSTRAINT [PK_" + TableSchema + "_" + tableName + "] PRIMARY KEY CLUSTERED (";
                foreach (DataColumn column in table.PrimaryKey)
                {
                    sql += "[" + column.ColumnName + "],";
                }
                sql = sql.TrimEnd(new char[] { ',' }) + "))" + Environment.NewLine;
            }
            else
            {
                sql += ")";
            }
            return sql;
        }

        public static string[] GetPrimaryKeys(DataTable schema)
        {
            List<string> keys = new List<string>();

            foreach (DataRow column in schema.Rows)
            {
                if (schema.Columns.Contains("IsKey") && (bool)column["IsKey"])
                {
                    keys.Add(column["ColumnName"].ToString());
                }
            }

            return keys.ToArray();
        }

        // Return T-SQL data type definition, based on schema definition for a column
        public static string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale, bool forceString)
        {
            string typestring = type.ToString();
            typestring = typestring.Replace("System.", "");
            typestring = typestring.ToLower();


            if (forceString)
            {
                return "NVARCHAR(MAX)";
            }
            else
            {
                switch (typestring)
                {
                    case "string":
                        return "VARCHAR(" + ((columnSize == -1) ? "MAX" : columnSize.ToString()) + ") COLLATE database_default ";

                    case "decimal":
                        if (numericScale > 0)
                        {
                            return "REAL";
                        }
                        else if (numericPrecision > 10)
                        {
                            return "BIGINT";
                        }
                        else
                        {
                            return "INT";
                        }

                    case "double":
                    case "single":
                        return "REAL";

                    case "int64":
                        return "BIGINT";

                    case "int16":
                    case "int32":
                    case "int":
                        return "INT";

                    case "tinyint":
                        return "INT";

                    case "byte":
                        return "INT";

                    case "datetime":
                        return "DATETIME";

                    case "datetimeoffset":
                        return "DATETIMEOFFSET";

                    case "bit":
                    case "boolean":
                        return "BIT";

                    case "guid":
                        return "UNIQUEIDENTIFIER";
                    case "byte[]":
                        return "VARBINARY";
                    case "timespan":
                        return "TIME(7)";
                    case "SByte":
                        return "TINYINT";
                    case "uint64":
                        return "BIGINT";
                    case "uint32":
                        return "INT";


                    default:
                        throw new Exception(type.ToString() + " conversion not implemented. Please add conversion logic to SQLGetType");
                }
            }
        }

        // Overload based on row from schema table
        public static string SQLGetType(DataRow schemaRow, bool forceString)
        {
            return SQLGetType(schemaRow["DataType"],
                                int.Parse(schemaRow["ColumnSize"].ToString()),
                                int.Parse(schemaRow["NumericPrecision"].ToString()),
                                int.Parse(schemaRow["NumericScale"].ToString()),
                                forceString);
        }
        // Overload based on DataColumn from DataTable type
        public static string SQLGetType(DataColumn column, bool forceString)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2, forceString);
        }
        #endregion
    }




    namespace SQLTableFromClassGenerator
    {
        public static class SQLTableFromClassGenerator
        {
            public static void Generate(string file)
            {
                List<TableClass> tables = new List<TableClass>();

                // This text is added only once to the file.
                if (!System.IO.File.Exists(file))
                {
                    //AdsGoFast.Logging.LogErrors(new Exception("File does not exist: " + file));
                }

                // Pass assembly name via argument
                System.Reflection.Assembly a = System.Reflection.Assembly.LoadFile(file);

                Type[] types = a.GetTypes();

                // Get Types in the assembly.
                foreach (Type t in types)
                {
                    TableClass tc = new TableClass(t);
                    tables.Add(tc);
                }

                // Create SQL for each table
                foreach (TableClass table in tables)
                {
                    Console.WriteLine(table.CreateTableScript());
                    Console.WriteLine();
                }

                // Total Hacked way to find FK relationships! Too lazy to fix right now
                foreach (TableClass table in tables)
                {
                    foreach (KeyValuePair<string, Type> field in table.Fields)
                    {
                        foreach (TableClass t2 in tables)
                        {
                            if (field.Value.Name == t2.ClassName)
                            {
                                // We have a FK Relationship!
                                Console.WriteLine("GO");
                                Console.WriteLine("ALTER TABLE " + table.ClassName + " WITH NOCHECK");
                                Console.WriteLine("ADD CONSTRAINT FK_" + field.Key + " FOREIGN KEY (" + field.Key + ") REFERENCES " + t2.ClassName + "(ID)");
                                Console.WriteLine("GO");

                            }
                        }
                    }
                }
            }
        }

        public class TableClass
        {
            private List<KeyValuePair<string, Type>> _fieldInfo = new List<KeyValuePair<string, Type>>();
            private string _className = string.Empty;

            private Dictionary<Type, string> dataMapper
            {
                get
                {
                    // Add the rest of your CLR Types to SQL Types mapping here
                    Dictionary<Type, string> dataMapper = new Dictionary<Type, string>
                    {
                        { typeof(int), "BIGINT" },
                        { typeof(string), "NVARCHAR(500)" },
                        { typeof(bool), "BIT" },
                        { typeof(DateTime), "DATETIME" },
                        { typeof(float), "FLOAT" },
                        { typeof(decimal), "DECIMAL(18,0)" },
                        { typeof(Guid), "UNIQUEIDENTIFIER" },
                        { typeof(DateTimeOffset), "DATETIMEOFFSET(7)" }
                    };

                    return dataMapper;
                }
            }

            public List<KeyValuePair<string, Type>> Fields
            {
                get => _fieldInfo;
                set => _fieldInfo = value;
            }

            public string ClassName
            {
                get => _className;
                set => _className = value;
            }

            public TableClass(Type t)
            {
                _className = t.Name;

                foreach (System.Reflection.PropertyInfo p in t.GetProperties())
                {
                    KeyValuePair<string, Type> field = new KeyValuePair<string, Type>(p.Name, p.PropertyType);

                    Fields.Add(field);
                }
            }

            public string CreateTableScript()
            {
                System.Text.StringBuilder script = new System.Text.StringBuilder();

                script.AppendLine("CREATE TABLE " + ClassName);
                script.AppendLine("(");
                script.AppendLine("\t ID BIGINT,");
                for (int i = 0; i < Fields.Count; i++)
                {
                    KeyValuePair<string, Type> field = Fields[i];

                    if (dataMapper.ContainsKey(field.Value))
                    {
                        script.Append("\t " + field.Key + " " + dataMapper[field.Value]);
                    }
                    else
                    {
                        // Complex Type? 
                        script.Append("\t " + field.Key + " BIGINT");
                    }

                    if (i != Fields.Count - 1)
                    {
                        script.Append(",");
                    }

                    script.Append(Environment.NewLine);
                }

                script.AppendLine(")");

                return script.ToString();
            }
        }
    }
}



//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Data;
//using System.Data.SqlClient;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace Mayb.DAL
//{
//    //public class SqlTypeAttribute : Attribute
//    //{
//    //    SqlDbType sqlDbType;

//    //    public SqlDbType SqlDbType
//    //    {
//    //        get { return sqlDbType; }
//    //        set { sqlDbType = value; }
//    //    }
//    //    public SqlTypeAttribute(SqlDbType type)
//    //    {
//    //        sqlDbType = type;
//    //    }
//    //}
//    public class BaseTable<T> where T : new()
//    {
//        #region 属性字段
//        string updateCommandText;
//        string insertCommandText;
//        string deleteCommandText;
//        string selectCommandText;
//        public string UpdateCommandText
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(updateCommandText))
//                {
//                    foreach (var item in ModelProperties)
//                    {
//                        if (string.Equals(item.Name, "id", StringComparison.OrdinalIgnoreCase)) continue;
//                        updateCommandText += item.Name + "=@" + item.Name + ",";
//                    }
//                    updateCommandText = updateCommandText.TrimEnd(',');
//                    updateCommandText = " UPDATE " + tableName + " SET " + updateCommandText + " WHERE ID=@ID ";
//                }
//                return updateCommandText;
//            }
//            set { updateCommandText = value; }
//        }
//        public string InsertCommandText
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(insertCommandText))
//                {
//                    string keys = "";
//                    foreach (var item in ModelProperties)
//                    {
//                        if (string.Equals(item.Name, "id", StringComparison.OrdinalIgnoreCase)) continue;
//                        keys += "@" + item.Name + ",";
//                    }
//                    keys = keys.TrimEnd(',');
//                    insertCommandText = string.Format(" INSERT {0} ({1}) VALUES({2}) ", tableName, keys.Replace("@", ""), keys);
//                }
//                return insertCommandText;
//            }
//            set { insertCommandText = value; }
//        }
//        public string DeleteCommandText { get { return deleteCommandText ?? " DELETE FROM " + tableName + " WHERE {0} "; } set { deleteCommandText = value; } }
//        public string SelectCommandText { get { return selectCommandText ?? " SELECT {0} FROM " + tableName + " {1} {2} "; } set { selectCommandText = value; } }
//        public T Model { get; set; }
//        public List<T> Models { get; set; }

//        System.Reflection.PropertyInfo[] modelProperties;

//        public System.Reflection.PropertyInfo[] ModelProperties
//        {
//            get { return modelProperties ?? (modelProperties = Model.GetType().GetProperties()); }
//            set { modelProperties = value; }
//        }

//        SqlService sql;
//        public SqlService Sql { get { return sql ?? (sql = new SqlService()); } set { sql = value; } }
//        protected SqlDataReader reader;
//        protected string tableName;
//        //int recordCount;
//        //public int RecordCount { get { return recordCount; } set { recordCount = value; } }

//        Dictionary<string, SqlDbType> columns;
//        public Dictionary<string, SqlDbType> Columns { get { return columns ?? (columns = new Dictionary<string, SqlDbType>()); } set { columns = value; } }

//        //string where;
//        //public string Where { get { return where ?? (where = "ID=@ID"); } set { where = value; } }
//        //string selectColumns;
//        //public string SelectColumns { get { return selectColumns ?? (selectColumns = "*"); } set { selectColumns = value; } }

//        #endregion

//        public BaseTable(string tableName) { this.tableName = tableName; }
//        public BaseTable(string tableName, bool initModel) : this(tableName) { if (initModel) Model = new T(); }

//        public BaseTable(string tableName, long id)
//            : this(tableName)
//        {
//            Sql.AddParameter("@ID", SqlDbType.Int, id);
//            reader = Sql.ExecuteSqlReader("SELECT * FROM " + tableName + " WHERE ID=@ID");
//            ReadModels();
//            Sql.Reset();
//        }

//        void ReadModels()
//        {
//            Models = new List<T>();
//            while (reader.Read())
//            {
//                Model = new T();
//                foreach (var item in ModelProperties)
//                {
//                    if (!Convert.IsDBNull(reader[item.Name]))
//                        item.SetValue(Model, reader[item.Name]);
//                }
//                Models.Add(Model);
//            }
//            if (!reader.IsClosed) reader.Close();
//        }

//        //通过反射Model参数化赋值
//        void SetParametersValue()
//        {
//            if (Model != null)
//            {
//                System.Reflection.PropertyInfo[] pis = Model.GetType().GetProperties();
//                string key;
//                foreach (var item in pis)
//                {
//                    key = "@" + item.Name;
//                    Sql.AddParameter(key, Columns[item.PropertyType.ToString()], Sql.PrepareSqlValue(item.GetValue(Model, null)));
//                }
//            }
//        }
//        public int Update()
//        {
//            SetParametersValue();
//            return Sql.ExecuteSql(UpdateCommandText);
//        }
//        public int Delete(string where)
//        {
//            DeleteCommandText = string.Format(DeleteCommandText, where);
//            return Sql.ExecuteSql(DeleteCommandText);
//        }
//        public int Insert()
//        {
//            SetParametersValue();
//            try { return Convert.ToInt32(Sql.ExecuteSqlScalar(InsertCommandText)); }
//            catch (Exception ex) { throw ex; }
//        }
//        public List<T> Select(string where = null, int? top = null)
//        {
//            SelectCommandText = string.Format(SelectCommandText, top == null ? "*" : "top " + top + " *", string.IsNullOrEmpty(where) ? "" : " where " + where, "");
//            reader = Sql.ExecuteSqlReader(SelectCommandText);
//            ReadModels();
//            return Models;
//        }
//        public DataTable Select(string where, string columns, string orderBy)
//        {
//            SelectCommandText = string.Format(SelectCommandText, string.IsNullOrEmpty(columns) ? "*" : columns, string.IsNullOrEmpty(where) ? "" : " where " + where, string.IsNullOrEmpty(orderBy) ? "" : " order by " + orderBy);
//            DataSet ds = Sql.ExecuteSqlDataSet(SelectCommandText);
//            if (null != ds && ds.Tables.Count > 0) return ds.Tables[0];
//            return null;
//        }

//        SqlDbType GetSqlDbType(string propertyType)
//        {
//            switch (propertyType)
//            {
//                case "System.Boolean":
//                    return SqlDbType.Bit;
//                case "System.Byte":
//                    return SqlDbType.TinyInt;
//                case "System.Int16":
//                    return SqlDbType.SmallInt;
//                case "System.Int32":
//                    return SqlDbType.Int;
//                case "System.Int64":
//                    return SqlDbType.BigInt;
//                case "System.Single":
//                    return SqlDbType.Real;
//                case "System.Double":
//                    return SqlDbType.Float;
//                case "System.Decimal":
//                    return SqlDbType.Decimal;
//                case "System.DateTime":
//                    return SqlDbType.DateTime;
//                case "System.Byte[]":
//                    return SqlDbType.Binary;
//                case "System.String":
//                    return SqlDbType.Text;
//                case "System.Guid":
//                    return SqlDbType.UniqueIdentifier;
//                case "System.Object":
//                    return SqlDbType.Variant;
//                default:
//                    return SqlDbType.NVarChar;
//            }
//        }
//    }

//    public class SqlService
//    {
//        protected string _connectionString = String.Empty;
//        protected List<SqlParameter> parameterList = new List<SqlParameter>();
//        SqlParameterCollection parameters;
//        protected bool _isSingleRow = false;
//        protected bool _convertEmptyValuesToDbNull = true;
//        protected bool _convertMinValuesToDbNull = true;
//        protected bool _convertMaxValuesToDbNull = false;
//        protected bool _autoCloseConnection = true;
//        protected SqlConnection _connection;
//        protected SqlTransaction _transaction;
//        private SqlCommand _cmd;
//        protected int _commandTimeout = 30;
//        public SqlService() { _connectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"]; }
//        //public SqlService() { _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
//        public SqlService(string connectionString) { _connectionString = connectionString; }
//        public SqlService(string server, string database, string user, string password) { this.ConnectionString = "Server=" + server + ";Database=" + database + ";User ID=" + user + ";Password=" + password + ";"; }
//        public SqlService(string server, string database) { this.ConnectionString = "Server=" + server + ";Database=" + database + ";Integrated Security=true;"; }
//        public SqlService(SqlConnection connection) { this.Connection = connection; this.AutoCloseConnection = false; }
//        public string ConnectionString { get { return _connectionString; } set { _connectionString = value; } }
//        public int CommandTimeout { get { return _commandTimeout; } set { _commandTimeout = value; } }
//        public bool IsSingleRow { get { return _isSingleRow; } set { _isSingleRow = value; } }
//        public bool AutoCloseConnection { get { return _autoCloseConnection; } set { _autoCloseConnection = value; } }
//        public SqlConnection Connection { get { return _connection; } set { _connection = value; this.ConnectionString = _connection.ConnectionString; } }
//        public SqlTransaction Transaction { get { return _transaction; } set { _transaction = value; } }
//        public bool ConvertEmptyValuesToDbNull { get { return _convertEmptyValuesToDbNull; } set { _convertEmptyValuesToDbNull = value; } }
//        public bool ConvertMinValuesToDbNull { get { return _convertMinValuesToDbNull; } set { _convertMinValuesToDbNull = value; } }
//        public bool ConvertMaxValuesToDbNull { get { return _convertMaxValuesToDbNull; } set { _convertMaxValuesToDbNull = value; } }
//        public int ReturnValue { get { if (Parameters.Contains("@ReturnValue")) { return (int)Parameters["@ReturnValue"].Value; } else { throw new Exception("必须设置ReturnValue值。"); } } }
//        public SqlParameterCollection Parameters
//        {
//            get
//            {
//                if (parameters == null) parameters = cmd.Parameters;
//                //parameters.Clear();
//                foreach (SqlParameter item in parameterList)
//                {
//                    if (!parameters.Contains(item))
//                        parameters.Add(item);
//                }
//                return parameters;
//            }
//            set { parameters = value; }
//        }

//        protected SqlCommand cmd { get { return _cmd ?? (_cmd = new SqlCommand()); } set { _cmd = value; } }

//        private void CopyParameters(SqlCommand command)
//        {
//            command.Parameters.Clear();
//            foreach (SqlParameter item in parameterList)
//            {
//                command.Parameters.Add(item);
//            }
//        }

//        public int ExecuteSql(string sql)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = sql;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            int result = cmd.ExecuteNonQuery();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//            return result;
//        }
//        public object ExecuteSqlScalar(string sql)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = sql;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            object obj = cmd.ExecuteScalar();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//            return obj;
//        }
//        public SqlDataReader ExecuteSqlReader(string sql)
//        {
//            SqlDataReader reader;
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = sql;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            CommandBehavior behavior = CommandBehavior.Default;
//            if (this.AutoCloseConnection) behavior = behavior | CommandBehavior.CloseConnection;
//            if (_isSingleRow) behavior = behavior | CommandBehavior.SingleRow;
//            reader = cmd.ExecuteReader(behavior);
//            cmd.Dispose();
//            return reader;
//        }
//        public XmlReader ExecuteSqlXmlReader(string sql)
//        {
//            XmlReader reader;
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = sql;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            reader = cmd.ExecuteXmlReader();
//            cmd.Dispose();
//            return reader;
//        }
//        public DataSet ExecuteSqlDataSet(string sql)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            SqlDataAdapter da = new SqlDataAdapter();
//            DataSet ds = new DataSet();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandText = sql;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            da.SelectCommand = cmd;
//            da.Fill(ds);
//            da.Dispose();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//            return ds;
//        }
//        public DataSet ExecuteSqlDataSet(string sql, string tableName)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            SqlDataAdapter da = new SqlDataAdapter();
//            DataSet ds = new DataSet();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandText = sql;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            da.SelectCommand = cmd;
//            da.Fill(ds, tableName);
//            da.Dispose();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//            return ds;
//        }
//        public void ExecuteSqlDataSet(ref DataSet dataSet, string sql, string tableName)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            SqlDataAdapter da = new SqlDataAdapter();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandText = sql;
//            cmd.CommandType = CommandType.Text;
//            this.CopyParameters(cmd);
//            da.SelectCommand = cmd;
//            da.Fill(dataSet, tableName);
//            da.Dispose();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//        }
//        public DataSet ExecuteSPDataSet(string procedureName)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            SqlDataAdapter da = new SqlDataAdapter();
//            DataSet ds = new DataSet();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = procedureName;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.StoredProcedure;
//            this.CopyParameters(cmd);
//            da.SelectCommand = cmd;
//            da.Fill(ds);
//            Parameters = cmd.Parameters;
//            da.Dispose();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//            return ds;
//        }
//        public DataSet ExecuteSPDataSet(string procedureName, string tableName)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            SqlDataAdapter da = new SqlDataAdapter();
//            DataSet ds = new DataSet();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = procedureName;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.StoredProcedure;
//            this.CopyParameters(cmd);
//            da.SelectCommand = cmd;
//            da.Fill(ds, tableName);
//            Parameters = cmd.Parameters;
//            da.Dispose();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//            return ds;
//        }
//        public void ExecuteSPDataSet(ref DataSet dataSet, string procedureName, string tableName)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            SqlDataAdapter da = new SqlDataAdapter();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = procedureName;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.StoredProcedure;
//            this.CopyParameters(cmd);
//            da.SelectCommand = cmd;
//            da.Fill(dataSet, tableName);
//            Parameters = cmd.Parameters;
//            da.Dispose();
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//        }
//        public void ExecuteSP(string procedureName)
//        {
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = procedureName;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.StoredProcedure;
//            this.CopyParameters(cmd);
//            cmd.ExecuteNonQuery();
//            Parameters = cmd.Parameters;
//            cmd.Dispose();
//            if (this.AutoCloseConnection) this.Disconnect();
//        }
//        public SqlDataReader ExecuteSPReader(string procedureName)
//        {
//            SqlDataReader reader;
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = procedureName;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.StoredProcedure;
//            this.CopyParameters(cmd);
//            CommandBehavior behavior = CommandBehavior.Default;
//            if (this.AutoCloseConnection) behavior = behavior | CommandBehavior.CloseConnection;
//            if (_isSingleRow) behavior = behavior | CommandBehavior.SingleRow;
//            reader = cmd.ExecuteReader(behavior);
//            Parameters = cmd.Parameters;
//            cmd.Dispose();
//            return reader;
//        }
//        public XmlReader ExecuteSPXmlReader(string procedureName)
//        {
//            XmlReader reader;
//            //SqlCommand cmd = new SqlCommand();
//            this.Connect();
//            cmd.CommandTimeout = this.CommandTimeout;
//            cmd.CommandText = procedureName;
//            cmd.Connection = _connection;
//            if (_transaction != null) cmd.Transaction = _transaction;
//            cmd.CommandType = CommandType.StoredProcedure;
//            this.CopyParameters(cmd);
//            reader = cmd.ExecuteXmlReader();
//            Parameters = cmd.Parameters;
//            cmd.Dispose();
//            return reader;
//        }
//        public SqlParameter AddParameter(string name, SqlDbType type, object value)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Input;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            prm.Value = this.PrepareSqlValue(value);
//            parameterList.Add(prm);
//            return prm;
//        }
//        public SqlParameter AddParameter(string name, SqlDbType type, object value, bool convertZeroToDBNull)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Input;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            prm.Value = this.PrepareSqlValue(value, convertZeroToDBNull);
//            parameterList.Add(prm);
//            return prm;
//        }
//        public SqlParameter AddParameter(string name, DbType type, object value, bool convertZeroToDBNull)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Input;
//            prm.ParameterName = name;
//            prm.DbType = type;
//            prm.Value = this.PrepareSqlValue(value, convertZeroToDBNull);
//            parameterList.Add(prm);
//            return prm;
//        }
//        public SqlParameter AddParameter(string name, SqlDbType type, object value, int size)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Input;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            prm.Size = size;
//            prm.Value = this.PrepareSqlValue(value);
//            parameterList.Add(prm);

//            return prm;
//        }
//        public SqlParameter AddParameter(string name, SqlDbType type, object value, ParameterDirection direction)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = direction;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            prm.Value = this.PrepareSqlValue(value);
//            parameterList.Add(prm);

//            return prm;
//        }
//        public SqlParameter AddParameter(string name, SqlDbType type, object value, int size, ParameterDirection direction)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = direction;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            prm.Size = size;
//            prm.Value = this.PrepareSqlValue(value);
//            parameterList.Add(prm);

//            return prm;
//        }
//        public void AddParameter(SqlParameter parameter)
//        {
//            parameterList.Add(parameter);

//        }
//        public void UpdateParameter(string[] names, params object[] values)
//        {
//            for (int i = 0; i < names.Length; i++)
//            {
//                Parameters[names[i]].Value = values[i];
//            }
//        }
//        public SqlParameter AddOutputParameter(string name, SqlDbType type)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Output;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            parameterList.Add(prm);
//            return prm;
//        }
//        public SqlParameter AddOutputParameter(string name, SqlDbType type, int size)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Output;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            prm.Size = size;
//            parameterList.Add(prm);

//            return prm;
//        }
//        public SqlParameter AddReturnValueParameter()
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.ReturnValue;
//            prm.ParameterName = "@ReturnValue";
//            prm.SqlDbType = SqlDbType.Int;
//            parameterList.Add(prm);

//            return prm;
//        }
//        public SqlParameter AddStreamParameter(string name, Stream value)
//        {
//            return this.AddStreamParameter(name, value, SqlDbType.Image);
//        }
//        public SqlParameter AddStreamParameter(string name, Stream value, SqlDbType type)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Input;
//            prm.ParameterName = name;
//            prm.SqlDbType = type;
//            value.Position = 0;
//            byte[] data = new byte[value.Length];
//            value.Read(data, 0, (int)value.Length);
//            prm.Value = data;
//            parameterList.Add(prm);

//            return prm;
//        }
//        public SqlParameter AddTextParameter(string name, string value)
//        {
//            SqlParameter prm = new SqlParameter();
//            prm.Direction = ParameterDirection.Input;
//            prm.ParameterName = name;
//            prm.SqlDbType = SqlDbType.Text;
//            prm.Value = this.PrepareSqlValue(value);
//            parameterList.Add(prm);

//            return prm;
//        }

//        public object PrepareSqlValue(object value)
//        {
//            return this.PrepareSqlValue(value, false);
//        }
//        public object PrepareSqlValue(object value, bool convertZeroToDBNull)
//        {
//            if (value is String)
//            {
//                if (this.ConvertEmptyValuesToDbNull && (string)value == String.Empty)
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Guid)
//            {
//                if (this.ConvertEmptyValuesToDbNull && (Guid)value == Guid.Empty)
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is DateTime)
//            {
//                if ((this.ConvertMinValuesToDbNull && (DateTime)value == DateTime.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (DateTime)value == DateTime.MaxValue))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Int16)
//            {
//                if ((this.ConvertMinValuesToDbNull && (Int16)value == Int16.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (Int16)value == Int16.MaxValue)
//                    || (convertZeroToDBNull && (Int16)value == 0))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Int32)
//            {
//                if ((this.ConvertMinValuesToDbNull && (Int32)value == Int32.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (Int32)value == Int32.MaxValue)
//                    || (convertZeroToDBNull && (Int32)value == 0))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Int64)
//            {
//                if ((this.ConvertMinValuesToDbNull && (Int64)value == Int64.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (Int64)value == Int64.MaxValue)
//                    || (convertZeroToDBNull && (Int64)value == 0))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Single)
//            {
//                if ((this.ConvertMinValuesToDbNull && (Single)value == Single.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (Single)value == Single.MaxValue)
//                    || (convertZeroToDBNull && (Single)value == 0))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Double)
//            {
//                if ((this.ConvertMinValuesToDbNull && (Double)value == Double.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (Double)value == Double.MaxValue)
//                    || (convertZeroToDBNull && (Double)value == 0))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value is Decimal)
//            {
//                if ((this.ConvertMinValuesToDbNull && (Decimal)value == Decimal.MinValue)
//                    || (this.ConvertMaxValuesToDbNull && (Decimal)value == Decimal.MaxValue)
//                    || (convertZeroToDBNull && (Decimal)value == 0))
//                {
//                    return DBNull.Value;
//                }
//                else
//                {
//                    return value;
//                }
//            }
//            else if (value == null)
//            {
//                return DBNull.Value;
//            }
//            else
//            {
//                return value;
//            }
//        }
//        private Hashtable ParseConfigString(string config)
//        {
//            Hashtable attributes = new Hashtable(10, new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture), new CaseInsensitiveComparer(CultureInfo.InvariantCulture));
//            string[] keyValuePairs = config.Split(';');
//            for (int i = 0; i < keyValuePairs.Length; i++)
//            {
//                string[] keyValuePair = keyValuePairs[i].Split('=');
//                if (keyValuePair.Length == 2)
//                {
//                    attributes.Add(keyValuePair[0].Trim(), keyValuePair[1].Trim());
//                }
//                else
//                {
//                    attributes.Add(keyValuePairs[i].Trim(), null);
//                }
//            }
//            return attributes;
//        }
//        public void Connect()
//        {
//            if (_connection != null)
//            {
//                if (_connection.State != ConnectionState.Open)
//                {
//                    _connection.Open();
//                }
//            }
//            else
//            {
//                if (_connectionString != String.Empty)
//                {
//                    StringCollection initKeys = new StringCollection();
//                    initKeys.AddRange(new string[] { "ARITHABORT", "ANSI_NULLS", "ANSI_WARNINGS", "ARITHIGNORE", "ANSI_DEFAULTS", "ANSI_NULL_DFLT_OFF", "ANSI_NULL_DFLT_ON", "ANSI_PADDING", "ANSI_WARNINGS" });
//                    StringBuilder initStatements = new StringBuilder();
//                    StringBuilder connectionString = new StringBuilder();
//                    Hashtable attribs = this.ParseConfigString(_connectionString);
//                    foreach (string key in attribs.Keys)
//                    {
//                        if (initKeys.Contains(key.Trim().ToUpper()))
//                        {
//                            initStatements.AppendFormat("SET {0} {1};", key, attribs[key]);
//                        }
//                        else if (key.Trim().Length > 0)
//                        {
//                            connectionString.AppendFormat("{0}={1};", key, attribs[key]);
//                        }
//                    }
//                    _connection = new SqlConnection(connectionString.ToString());
//                    _connection.Open();
//                    if (initStatements.Length > 0)
//                    {
//                        //SqlCommand cmd = new SqlCommand();
//                        cmd.CommandTimeout = this.CommandTimeout;
//                        cmd.CommandText = initStatements.ToString();
//                        cmd.Connection = _connection;
//                        cmd.CommandType = CommandType.Text;
//                        cmd.ExecuteNonQuery();
//                        cmd.Dispose();
//                    }
//                }
//                else
//                {
//                    throw new InvalidOperationException("在调用连接之前，必须设置连接对象或指定连接字符串。");
//                }
//            }
//        }
//        public void Disconnect() { if ((_connection != null) && (_connection.State != ConnectionState.Closed)) { _connection.Close(); } if (_connection != null) _connection.Dispose(); if (_transaction != null) _transaction.Dispose(); _transaction = null; _connection = null; }
//        public void BeginTransaction() { if (_connection != null) { _transaction = _connection.BeginTransaction(); } else { throw new InvalidOperationException("数据库连接对象为NULL"); } }
//        public void CommitTransaction() { if (_transaction != null) { try { _transaction.Commit(); } catch (Exception) { throw; } } else { throw new InvalidOperationException("没有可提交的事务"); } }
//        public void RollbackTransaction() { if (_transaction != null) { try { _transaction.Rollback(); } catch (Exception) { throw; } } else { throw new InvalidOperationException("没有可回滚的事务。"); } }
//        public void Reset() { if (parameterList != null) { parameterList.Clear(); } if (parameters != null) { parameters = null; } }
//    }
//}

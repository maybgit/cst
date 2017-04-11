using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mayb.DAL
{
    public class SqlService
    {
        protected string _connectionString = String.Empty;
        protected List<SqlParameter> parameterList = new List<SqlParameter>();
        SqlParameterCollection parameters;
        protected bool _isSingleRow = false;
        protected bool _convertEmptyValuesToDbNull = true;
        protected bool _convertMinValuesToDbNull = true;
        protected bool _convertMaxValuesToDbNull = false;
        protected bool _autoCloseConnection = true;
        protected SqlConnection _connection;
        protected SqlTransaction _transaction;
        private SqlCommand _cmd;
        protected int _commandTimeout = 30;
        public SqlService() { _connectionString = ConfigurationSettings.AppSettings["ConnectionString"]; }
        public SqlService(string connectionString) { _connectionString = connectionString; }
        public SqlService(string server, string database, string user, string password) { this.ConnectionString = "Server=" + server + ";Database=" + database + ";User ID=" + user + ";Password=" + password + ";"; }
        public SqlService(string server, string database) { this.ConnectionString = "Server=" + server + ";Database=" + database + ";Integrated Security=true;"; }
        public SqlService(SqlConnection connection) { this.Connection = connection; this.AutoCloseConnection = false; }
        public string ConnectionString { get { return _connectionString; } set { _connectionString = value; } }
        public int CommandTimeout { get { return _commandTimeout; } set { _commandTimeout = value; } }
        public bool IsSingleRow { get { return _isSingleRow; } set { _isSingleRow = value; } }
        public bool AutoCloseConnection { get { return _autoCloseConnection; } set { _autoCloseConnection = value; } }
        public SqlConnection Connection { get { return _connection; } set { _connection = value; this.ConnectionString = _connection.ConnectionString; } }
        public SqlTransaction Transaction { get { return _transaction; } set { _transaction = value; } }
        public bool ConvertEmptyValuesToDbNull { get { return _convertEmptyValuesToDbNull; } set { _convertEmptyValuesToDbNull = value; } }
        public bool ConvertMinValuesToDbNull { get { return _convertMinValuesToDbNull; } set { _convertMinValuesToDbNull = value; } }
        public bool ConvertMaxValuesToDbNull { get { return _convertMaxValuesToDbNull; } set { _convertMaxValuesToDbNull = value; } }
        public int ReturnValue { get { if (Parameters.Contains("@ReturnValue")) { return (int)Parameters["@ReturnValue"].Value; } else { throw new Exception("必须设置ReturnValue值。"); } } }
        public SqlParameterCollection Parameters
        {
            get
            {
                if (parameters == null) parameters = cmd.Parameters;
                //parameters.Clear();
                foreach (SqlParameter item in parameterList)
                {
                    if (!parameters.Contains(item))
                        parameters.Add(item);
                }
                return parameters;
            }
            set { parameters = value;}
        }

        protected SqlCommand cmd { get { return _cmd??(_cmd=new SqlCommand());} set { _cmd = value; } }

        private void CopyParameters(SqlCommand command)
        {
            command.Parameters.Clear();
            foreach (SqlParameter item in parameterList)
            {
                command.Parameters.Add(item);
            }
        }

        public int ExecuteSql(string sql)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = sql;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            int result = cmd.ExecuteNonQuery();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
            return result;
        }
        public object ExecuteSqlScalar(string sql)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = sql;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            object obj = cmd.ExecuteScalar();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
            return obj;
        }
        public SqlDataReader ExecuteSqlReader(string sql)
        {
            SqlDataReader reader;
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = sql;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            CommandBehavior behavior = CommandBehavior.Default;
            if (this.AutoCloseConnection) behavior = behavior | CommandBehavior.CloseConnection;
            if (_isSingleRow) behavior = behavior | CommandBehavior.SingleRow;
            reader = cmd.ExecuteReader(behavior);
            cmd.Dispose();
            return reader;
        }
        public XmlReader ExecuteSqlXmlReader(string sql)
        {
            XmlReader reader;
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = sql;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            reader = cmd.ExecuteXmlReader();
            cmd.Dispose();
            return reader;
        }
        public DataSet ExecuteSqlDataSet(string sql)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            da.SelectCommand = cmd;
            da.Fill(ds);
            da.Dispose();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
            return ds;
        }
        public DataSet ExecuteSqlDataSet(string sql, string tableName)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            da.SelectCommand = cmd;
            da.Fill(ds, tableName);
            da.Dispose();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
            return ds;
        }
        public void ExecuteSqlDataSet(ref DataSet dataSet, string sql, string tableName)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            this.CopyParameters(cmd);
            da.SelectCommand = cmd;
            da.Fill(dataSet, tableName);
            da.Dispose();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
        }
        public DataSet ExecuteSPDataSet(string procedureName)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = procedureName;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(cmd);
            da.SelectCommand = cmd;
            da.Fill(ds);
            Parameters = cmd.Parameters;
            da.Dispose();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
            return ds;
        }
        public DataSet ExecuteSPDataSet(string procedureName, string tableName)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            SqlDataAdapter da = new SqlDataAdapter();
            DataSet ds = new DataSet();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = procedureName;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(cmd);
            da.SelectCommand = cmd;
            da.Fill(ds, tableName);
            Parameters = cmd.Parameters;
            da.Dispose();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
            return ds;
        }
        public void ExecuteSPDataSet(ref DataSet dataSet, string procedureName, string tableName)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = procedureName;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(cmd);
            da.SelectCommand = cmd;
            da.Fill(dataSet, tableName);
            Parameters = cmd.Parameters;
            da.Dispose();
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
        }
        public void ExecuteSP(string procedureName)
        {
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = procedureName;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(cmd);
            cmd.ExecuteNonQuery();
            Parameters = cmd.Parameters;
            cmd.Dispose();
            if (this.AutoCloseConnection) this.Disconnect();
        }
        public SqlDataReader ExecuteSPReader(string procedureName)
        {
            SqlDataReader reader;
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = procedureName;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(cmd);
            CommandBehavior behavior = CommandBehavior.Default;
            if (this.AutoCloseConnection) behavior = behavior | CommandBehavior.CloseConnection;
            if (_isSingleRow) behavior = behavior | CommandBehavior.SingleRow;
            reader = cmd.ExecuteReader(behavior);
            Parameters = cmd.Parameters;
            cmd.Dispose();
            return reader;
        }
        public XmlReader ExecuteSPXmlReader(string procedureName)
        {
            XmlReader reader;
            //SqlCommand cmd = new SqlCommand();
            this.Connect();
            cmd.CommandTimeout = this.CommandTimeout;
            cmd.CommandText = procedureName;
            cmd.Connection = _connection;
            if (_transaction != null) cmd.Transaction = _transaction;
            cmd.CommandType = CommandType.StoredProcedure;
            this.CopyParameters(cmd);
            reader = cmd.ExecuteXmlReader();
            Parameters = cmd.Parameters;
            cmd.Dispose();
            return reader;
        }
        public SqlParameter AddParameter(string name, SqlDbType type, object value)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Value = this.PrepareSqlValue(value);
            parameterList.Add(prm);
            return prm;
        }
        public SqlParameter AddParameter(string name, SqlDbType type, object value, bool convertZeroToDBNull)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Value = this.PrepareSqlValue(value, convertZeroToDBNull);
            parameterList.Add(prm);
            return prm;
        }
        public SqlParameter AddParameter(string name, DbType type, object value, bool convertZeroToDBNull)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.DbType = type;
            prm.Value = this.PrepareSqlValue(value, convertZeroToDBNull);
            parameterList.Add(prm);
            return prm;
        }
        public SqlParameter AddParameter(string name, SqlDbType type, object value, int size)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            prm.Value = this.PrepareSqlValue(value);
            parameterList.Add(prm);

            return prm;
        }
        public SqlParameter AddParameter(string name, SqlDbType type, object value, ParameterDirection direction)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = direction;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Value = this.PrepareSqlValue(value);
            parameterList.Add(prm);

            return prm;
        }
        public SqlParameter AddParameter(string name, SqlDbType type, object value, int size, ParameterDirection direction)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = direction;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            prm.Value = this.PrepareSqlValue(value);
            parameterList.Add(prm);

            return prm;
        }
        public void AddParameter(SqlParameter parameter)
        {
            parameterList.Add(parameter);

        }
        public void UpdateParameter(string[] names, params object[] values)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Parameters[names[i]].Value = values[i];
            }
        }
        public SqlParameter AddOutputParameter(string name, SqlDbType type)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Output;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            parameterList.Add(prm);
            return prm;
        }
        public SqlParameter AddOutputParameter(string name, SqlDbType type, int size)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Output;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            parameterList.Add(prm);

            return prm;
        }
        public SqlParameter AddReturnValueParameter()
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.ReturnValue;
            prm.ParameterName = "@ReturnValue";
            prm.SqlDbType = SqlDbType.Int;
            parameterList.Add(prm);

            return prm;
        }
        public SqlParameter AddStreamParameter(string name, Stream value)
        {
            return this.AddStreamParameter(name, value, SqlDbType.Image);
        }
        public SqlParameter AddStreamParameter(string name, Stream value, SqlDbType type)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            value.Position = 0;
            byte[] data = new byte[value.Length];
            value.Read(data, 0, (int)value.Length);
            prm.Value = data;
            parameterList.Add(prm);

            return prm;
        }
        public SqlParameter AddTextParameter(string name, string value)
        {
            SqlParameter prm = new SqlParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.Text;
            prm.Value = this.PrepareSqlValue(value);
            parameterList.Add(prm);

            return prm;
        }

        public object PrepareSqlValue(object value)
        {
            return this.PrepareSqlValue(value, false);
        }
        public object PrepareSqlValue(object value, bool convertZeroToDBNull)
        {
            if (value is String)
            {
                if (this.ConvertEmptyValuesToDbNull && (string)value == String.Empty)
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Guid)
            {
                if (this.ConvertEmptyValuesToDbNull && (Guid)value == Guid.Empty)
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is DateTime)
            {
                if ((this.ConvertMinValuesToDbNull && (DateTime)value == DateTime.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (DateTime)value == DateTime.MaxValue))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Int16)
            {
                if ((this.ConvertMinValuesToDbNull && (Int16)value == Int16.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (Int16)value == Int16.MaxValue)
                    || (convertZeroToDBNull && (Int16)value == 0))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Int32)
            {
                if ((this.ConvertMinValuesToDbNull && (Int32)value == Int32.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (Int32)value == Int32.MaxValue)
                    || (convertZeroToDBNull && (Int32)value == 0))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Int64)
            {
                if ((this.ConvertMinValuesToDbNull && (Int64)value == Int64.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (Int64)value == Int64.MaxValue)
                    || (convertZeroToDBNull && (Int64)value == 0))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Single)
            {
                if ((this.ConvertMinValuesToDbNull && (Single)value == Single.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (Single)value == Single.MaxValue)
                    || (convertZeroToDBNull && (Single)value == 0))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Double)
            {
                if ((this.ConvertMinValuesToDbNull && (Double)value == Double.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (Double)value == Double.MaxValue)
                    || (convertZeroToDBNull && (Double)value == 0))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value is Decimal)
            {
                if ((this.ConvertMinValuesToDbNull && (Decimal)value == Decimal.MinValue)
                    || (this.ConvertMaxValuesToDbNull && (Decimal)value == Decimal.MaxValue)
                    || (convertZeroToDBNull && (Decimal)value == 0))
                {
                    return DBNull.Value;
                }
                else
                {
                    return value;
                }
            }
            else if (value == null)
            {
                return DBNull.Value;
            }
            else
            {
                return value;
            }
        }
        private Hashtable ParseConfigString(string config)
        {
            Hashtable attributes = new Hashtable(10, new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture), new CaseInsensitiveComparer(CultureInfo.InvariantCulture));
            string[] keyValuePairs = config.Split(';');
            for (int i = 0; i < keyValuePairs.Length; i++)
            {
                string[] keyValuePair = keyValuePairs[i].Split('=');
                if (keyValuePair.Length == 2)
                {
                    attributes.Add(keyValuePair[0].Trim(), keyValuePair[1].Trim());
                }
                else
                {
                    attributes.Add(keyValuePairs[i].Trim(), null);
                }
            }
            return attributes;
        }
        public void Connect()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
            }
            else
            {
                if (_connectionString != String.Empty)
                {
                    StringCollection initKeys = new StringCollection();
                    initKeys.AddRange(new string[] { "ARITHABORT", "ANSI_NULLS", "ANSI_WARNINGS", "ARITHIGNORE", "ANSI_DEFAULTS", "ANSI_NULL_DFLT_OFF", "ANSI_NULL_DFLT_ON", "ANSI_PADDING", "ANSI_WARNINGS" });
                    StringBuilder initStatements = new StringBuilder();
                    StringBuilder connectionString = new StringBuilder();
                    Hashtable attribs = this.ParseConfigString(_connectionString);
                    foreach (string key in attribs.Keys)
                    {
                        if (initKeys.Contains(key.Trim().ToUpper()))
                        {
                            initStatements.AppendFormat("SET {0} {1};", key, attribs[key]);
                        }
                        else if (key.Trim().Length > 0)
                        {
                            connectionString.AppendFormat("{0}={1};", key, attribs[key]);
                        }
                    }
                    _connection = new SqlConnection(connectionString.ToString());
                    _connection.Open();
                    if (initStatements.Length > 0)
                    {
                        //SqlCommand cmd = new SqlCommand();
                        cmd.CommandTimeout = this.CommandTimeout;
                        cmd.CommandText = initStatements.ToString();
                        cmd.Connection = _connection;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                }
                else
                {
                    throw new InvalidOperationException("You must set a connection object or specify a connection string before calling Connect.");
                }
            }
        }
        public void Disconnect() { if ((_connection != null) && (_connection.State != ConnectionState.Closed)) { _connection.Close(); } if (_connection != null) _connection.Dispose(); if (_transaction != null) _transaction.Dispose(); _transaction = null; _connection = null; }
        public void BeginTransaction() { if (_connection != null) { _transaction = _connection.BeginTransaction(); } else { throw new InvalidOperationException("You must have a valid connection object before calling BeginTransaction."); } }
        public void CommitTransaction() { if (_transaction != null) { try { _transaction.Commit(); } catch (Exception) { throw; } } else { throw new InvalidOperationException("You must call BeginTransaction before calling CommitTransaction."); } }
        public void RollbackTransaction() { if (_transaction != null) { try { _transaction.Rollback(); } catch (Exception) { throw; } } else { throw new InvalidOperationException("You must call BeginTransaction before calling RollbackTransaction."); } }
        public void Reset() { if (parameterList != null) { parameterList.Clear(); } if (parameters != null) { parameters = null; } }
    }
}

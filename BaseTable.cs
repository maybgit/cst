using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Mayb.DAL
{
    public class BaseTable<T> where T : new()
    {
        #region 属性字段
        string updateCommandText;
        string insertCommandText;
        string deleteCommandText;
        string selectCommandText;
        public string UpdateCommandText
        {
            get
            {
                if (string.IsNullOrEmpty(updateCommandText))
                {
                    foreach (var item in ModelProperties)
                    {
                        if (string.Equals(item.Name, "id", StringComparison.OrdinalIgnoreCase)) continue;
                        updateCommandText += item.Name + "=@" + item.Name + ",";
                    }
                    updateCommandText = updateCommandText.TrimEnd(',');
                    updateCommandText = " UPDATE " + TableName + " SET " + updateCommandText + " WHERE ID=@ID ";
                }
                return updateCommandText;
            }
            set { updateCommandText = value; }
        }
        public string InsertCommandText
        {
            get
            {
                if (string.IsNullOrEmpty(insertCommandText))
                {
                    string keys = "";
                    foreach (var item in ModelProperties)
                    {
                        if (string.Equals(item.Name, "id", StringComparison.OrdinalIgnoreCase)) continue;
                        keys += "@" + item.Name + ",";
                    }
                    keys = keys.TrimEnd(',');
                    insertCommandText = string.Format(" INSERT {0} ({1}) VALUES({2}) ", TableName, keys.Replace("@", ""), keys);
                }
                return insertCommandText;
            }
            set { insertCommandText = value; }
        }
        public string DeleteCommandText { get { return deleteCommandText ?? " DELETE FROM " + TableName + " WHERE {0} "; } set { deleteCommandText = value; } }
        public string SelectCommandText { get { return selectCommandText ?? " SELECT {0} FROM " + TableName + " {1} {2} "; } set { selectCommandText = value; } }
        public T Model { get; set; }
        public List<T> Models { get; set; }

        System.Reflection.PropertyInfo[] modelProperties;

        public System.Reflection.PropertyInfo[] ModelProperties
        {
            get { return modelProperties ?? (modelProperties = Model.GetType().GetProperties()); }
            set { modelProperties = value; }
        }

        //private Dictionary<string, SqlDbType> columns;
        //public Dictionary<string, SqlDbType> Columns { get { return columns ?? (columns = new Dictionary<string, SqlDbType>()); } set { columns = value; } }

        SqlService sql;
        public SqlService Sql
        {
            get
            {
                return sql ?? (sql = new SqlService());
            }

            set
            {
                sql = value;
            }
        }
        protected SqlDataReader reader;
        protected string TableName;
        int recordCount;
        public int RecordCount
        {
            get
            {
                return recordCount;
            }

            set
            {
                recordCount = value;
            }
        }

        #endregion
        public BaseTable() { }
        public BaseTable(string tableName) { TableName = tableName; }

        /// <summary>
        /// 当数据库表存在ID字段时可用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName">表名</param>
        public BaseTable(long id, string tableName)
        {
            TableName = tableName;
            Sql.AddParameter("@ID", SqlDbType.Int, id);
            reader = Sql.ExecuteSqlReader("SELECT * FROM " + TableName + " WHERE ID = @ID");
            ReadModels();
            Sql.Reset();
        }
        /// <summary>
        /// 从SqlDataReader读取数据为Model赋值
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fun"></param>
        protected void ReadModels(SqlDataReader reader, Func<SqlDataReader, T> fun)
        {
            Models = new List<T>();
            while (reader.Read()) Models.Add(fun(reader));
            if (!reader.IsClosed) reader.Close();
        }

        protected void ReadModels()
        {
            Models = new List<T>();
            while (reader.Read())
            {
                Model = new T();
                foreach (var item in ModelProperties)
                {
                    if (!Convert.IsDBNull(reader[item.Name]))
                        item.SetValue(Model, reader[item.Name]);
                }
                Models.Add(Model);
            }
            if (!reader.IsClosed) reader.Close();
        }

        //通过反射Model参数化赋值
        void SetParametersValue()
        {
            if (Model != null)
            {
                System.Reflection.PropertyInfo[] pis = Model.GetType().GetProperties();
                string key;
                foreach (var item in pis)
                {
                    key = "@" + item.Name;
                    Sql.AddParameter(key, GetSqlDbType(item.PropertyType.ToString()), Sql.PrepareSqlValue(item.GetValue(Model, null)));
                }
            }
        }
        public int Update()
        {
            SetParametersValue();
            return Sql.ExecuteSql(UpdateCommandText);
        }
        public int Delete(string where)
        {
            DeleteCommandText = string.Format(DeleteCommandText, where);
            return Sql.ExecuteSql(DeleteCommandText);
        }
        public int Insert()
        {
            SetParametersValue();
            try { return Convert.ToInt32(Sql.ExecuteSqlScalar(InsertCommandText)); }
            catch (Exception ex) { throw ex; }
        }
        public List<T> Select(string where = null, int? top = null)
        {
            SelectCommandText = string.Format(SelectCommandText, top == null ? "*" : "top " + top + " *", string.IsNullOrEmpty(where) ? "" : " where " + where, "");
            reader = Sql.ExecuteSqlReader(SelectCommandText);
            ReadModels();
            return Models;
        }
        public DataTable Select(string where, string columns, string orderBy)
        {
            SelectCommandText = string.Format(SelectCommandText, string.IsNullOrEmpty(columns) ? "*" : columns, string.IsNullOrEmpty(where) ? "" : " where " + where, string.IsNullOrEmpty(orderBy) ? "" : " order by " + orderBy);
            DataSet ds = Sql.ExecuteSqlDataSet(SelectCommandText);
            if (null != ds && ds.Tables.Count > 0) return ds.Tables[0];
            return null;
        }

        SqlDbType GetSqlDbType(string propertyType)
        {
            switch (propertyType)
            {
                case "System.Boolean":
                    return SqlDbType.Bit;
                case "System.Byte":
                    return SqlDbType.TinyInt;
                case "System.Int16":
                    return SqlDbType.SmallInt;
                case "System.Int32":
                    return SqlDbType.Int;
                case "System.Int64":
                    return SqlDbType.BigInt;
                case "System.Single":
                    return SqlDbType.Real;
                case "System.Double":
                    return SqlDbType.Float;
                case "System.Decimal":
                    return SqlDbType.Decimal;
                case "System.DateTime":
                    return SqlDbType.DateTime;
                case "System.Byte[]":
                    return SqlDbType.Binary;
                case "System.String":
                    return SqlDbType.NVarChar;
                case "System.Guid":
                    return SqlDbType.UniqueIdentifier;
                case "System.Object":
                    return SqlDbType.Variant;
                default:
                    return SqlDbType.NVarChar;
            }
        }
    }

}

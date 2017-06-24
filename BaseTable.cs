using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Mayb.DAL
{
    public class BaseTable<T>
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
                    foreach (var item in columns)
                    {
                        if (string.Equals(item.Key, "@id", StringComparison.OrdinalIgnoreCase)) continue;
                        updateCommandText += item.Key.TrimStart('@') + "=" + item.Key + ",";
                    }
                    updateCommandText = updateCommandText.TrimEnd(',');
                    updateCommandText = " Update " + TableName + " Set " + updateCommandText + " Where ID=@ID ";
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
                    foreach (var item in columns)
                    {
                        if (string.Equals(item.Key, "@id", StringComparison.OrdinalIgnoreCase)) continue;
                        keys += item.Key + ",";
                    }
                    keys = keys.TrimEnd(',');
                    insertCommandText = string.Format(" Insert Into {0} ({1}) Values({2}) ", TableName, keys.Replace("@", ""), keys);
                }
                return insertCommandText;
            }
            set { insertCommandText = value; }
        }
        public string DeleteCommandText { get { return deleteCommandText ?? " Delete from " + TableName + " where {0} "; } set { deleteCommandText = value; } }
        public string SelectCommandText { get { return selectCommandText ?? " SELECT {0} FROM " + TableName + " {1} {2} "; } set { selectCommandText = value; } }
        public T Model { get; set; }
        public List<T> Models { get; set; }
        private Dictionary<string, SqlDbType> columns;
        public Dictionary<string, SqlDbType> Columns { get { return columns ?? (columns = new Dictionary<string, SqlDbType>()); } set { columns = value; } }

        //public SqlService Sql = new SqlService();
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
        public BaseTable(long id, string tableName)
        {
            TableName = tableName;
            Sql.AddParameter("@ID", SqlDbType.Int, id);
            reader = Sql.ExecuteSqlReader("SELECT * FROM dbo." + TableName + " WHERE ID = @ID");
            Sql.Reset();
        }
        protected void ReadModels(SqlDataReader reader, Func<SqlDataReader, T> fun)
        {
            Models = new List<T>();
            while (reader.Read()) { Models.Add(fun(reader)); }
            if (!reader.IsClosed) reader.Close();
        }
        void SetParametersValue()
        {
            if (Model != null)
            {
                System.Reflection.PropertyInfo[] pis = Model.GetType().GetProperties();
                string key;
                foreach (var item in pis)
                {
                    key = "@" + item.Name;
                    Sql.AddParameter(key, columns[key], Sql.PrepareSqlValue(item.GetValue(Model, null)));
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
            try { return Convert.ToInt32(Sql.ExecuteSqlScalar(InsertCommandText)); } catch (Exception ex) { throw ex; }
        }
        internal void Select(string where, int? top, Func<SqlDataReader, T> fun)
        {
            SelectCommandText = string.Format(SelectCommandText, top == null ? "*" : "top " + top + " *", string.IsNullOrEmpty(where) ? "" : " where " + where, "");
            reader = Sql.ExecuteSqlReader(SelectCommandText);
            ReadModels(reader, fun);
        }
        public DataTable Select(string where, string columns, string orderBy)
        {
            SelectCommandText = string.Format(SelectCommandText, string.IsNullOrEmpty(columns) ? "*" : columns, string.IsNullOrEmpty(where) ? "" : " where " + where, string.IsNullOrEmpty(orderBy) ? "" : " order by " + orderBy);
            DataSet ds = Sql.ExecuteSqlDataSet(SelectCommandText);
            if (null != ds && ds.Tables.Count > 0) return ds.Tables[0];
            return null;
        }

        public DataTable GetPager(string fields, string where, string orderField, int pageIndex, int pageSize)
        {
            DataSet ds = new DataSet();
            recordCount = 0;
            try
            {
                int endIndex = pageIndex * pageSize;
                int startIndex = endIndex - pageSize + 1;
                DAL.Procedures.P_Pager(ref ds, TableName, fields, where, orderField, startIndex, endIndex, ref recordCount);
                if (ds != null && ds.Tables.Count > 0) return ds.Tables[0];
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return null;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mayb.DAL
{
    public class BaseTable<T>
    {
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
                }
                return " Update " + TableName + " Set " + updateCommandText + " Where ID=@ID ";
            }
            set => updateCommandText = value;
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
            set => insertCommandText = value;
        }
        public string DeleteCommandText { get => deleteCommandText ?? " Delete from " + TableName + " where {0} "; set => deleteCommandText = value; }
        public string SelectCommandText { get => selectCommandText ?? " SELECT {0} FROM " + TableName + " {1} {2} "; set => selectCommandText = value; }
        public T Model { get; set; }
        public List<T> Models { get; set; }
        private Dictionary<string, SqlDbType> columns;
        public Dictionary<string, SqlDbType> Columns { get => columns ?? (columns = new Dictionary<string, SqlDbType>()); set => columns = value; }
        public SqlService Sql = new SqlService();
        internal SqlDataReader reader;
        internal string TableName;
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
            System.Reflection.PropertyInfo[] pis = Model.GetType().GetProperties();
            string key;
            foreach (var item in pis)
            {
                key = "@" + item.Name;
                Sql.AddParameter(key, columns[key], Sql.PrepareSqlValue(item.GetValue(Model, null)));
            }
        }
        public int Update()
        {
            SetParametersValue();
            return Sql.ExecuteSql(UpdateCommandText);
            return 0;
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
            //SelectCommandText = string.Format(SelectCommandText, string.IsNullOrEmpty(columns) ? "*" : columns, string.IsNullOrEmpty(where) ? "" : " where " + where, string.IsNullOrEmpty(orderBy) ? "" : " order by " + orderBy);
            DataSet ds = Sql.ExecuteSqlDataSet(SelectCommandText);
            if (null != ds && ds.Tables.Count > 0) return ds.Tables[0];
            return null;
        }
    }

}

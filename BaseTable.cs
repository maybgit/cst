using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Mayb.DAL
{
    public abstract class BaseTable<T> where T : new()
    {
        public BaseTable(string tableName) { this.tableName = tableName; }
        Dictionary<string, SqlDbType> columnType; 
        public Dictionary<string, SqlDbType> ColumnType
        {
            get
            {
                return columnType ?? (columnType = new Dictionary<string, SqlDbType>());
            }

            set
            {
                columnType = value;
            }
        }
        int top;
        public int Top
        {
            get
            {
                return top;
            }

            set
            {
                top = value;
            }
        }

        string columns;

        public string Columns
        {
            get { return columns ?? (columns = "*"); }
            set { columns = value; }
        }
        string where;

        public string Where
        {
            get { return where; }
            set { where = " where " + value; }
        }
        string orderBy;
        public string OrderBy
        {
            get
            {
                return orderBy;
            }

            set
            {
                orderBy = " order by "+value;
            }
        }
        T model;
        public T Model
        {
            get {
                if (model==null)
                {
                    List<T> list = GetModels();
                    if (list.Count > 0) model = list[0];
                }
                return model;
            }
        }

        string FormatSelect { get { return string.Format("select {0} * from {1} {2} {3}", Top < 1 ? "" : "top " + Top, tableName, Where, OrderBy); } }

        List<T> GetModels() {
            List<T> list=new List<T>();
            //if (EnableTransaction) Sql.BeginTransaction();
            using (SqlDataReader reader = Sql.ExecuteSqlReader(FormatSelect))
            {
                while (reader.Read())
                {
                    T t = new T();
                    foreach (var item in ModelProperties)
                        if (!Convert.IsDBNull(reader[item.Name]))
                            item.SetValue(t, reader[item.Name]);
                    list.Add(t);
                }
            }
            return list;
        }
        List<T> models;
        public List<T> Models
        {
            get
            {
                return models ?? (models=GetModels());
            }
        }

        System.Reflection.PropertyInfo[] modelProperties;

        System.Reflection.PropertyInfo[] ModelProperties
        {
            get { return modelProperties ?? (modelProperties = new T().GetType().GetProperties()); }
            set { modelProperties = value; }
        }

        SqlService sql;
        public SqlService Sql { get { return sql ?? (sql = new SqlService()); } set { sql = value; } }

        public Dictionary<string, object> Parameters
        {
            get
            {
                return parameters ?? (parameters = new Dictionary<string, object>());
            }

            set
            {
                parameters = value;
            }
        }

        Dictionary<string, object> parameters;

        protected SqlDataReader reader;
        protected string tableName;

        public bool EnableTransaction { get; set; }

        public void AddParameter(string name, object value)
        {
            name = name.TrimStart('@').ToLower();
            Sql.AddParameter("@" + name, ColumnType[name], value);
        }
        public int Update()
        {
            try
            {
                if (string.IsNullOrEmpty(Columns)) throw new Exception("Columns 不能为空。");
                string[] cols = Columns.Split(',');
                for (int i = 0; i < cols.Length; i++) cols[i] = cols[i] + "=@" + cols[i];
                string sql = string.Format("update {0} set {1} {2}",tableName, string.Join(",", cols),Where);
                //throw new Exception(sql);
                if (string.IsNullOrEmpty(Where)) throw new Exception("更新语句 Where 条件不能为空 " + sql);
                return Sql.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int Delete()
        {
            try
            {
                if (string.IsNullOrEmpty(Where)) throw new Exception("delete 语句 Where 条件不能为空,如需全部删除，可设置为1=1。");
                string sql = string.Format("delete from {0} {1}", tableName, Where);
                return Sql.ExecuteSql(sql);
            }
            catch (Exception ex) { throw ex; }
        }
        public int Insert()
        {
            try
            {
                string sql = string.Format("insert {0}({1}) values(@{2})", tableName, Columns, Columns.Replace(",", ",@"));
                return Sql.ExecuteSql(sql);
            }
            catch (Exception ex) { throw ex; }
        }
       public void ClearParameter() { Sql.Reset(); }
    }

}

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

        public T Model
        {
            get
            {
                if (Models.Count > 0) return Models[0];
                return default(T);
            }
        }
        public List<T> Models
        {
            get
            {
                List<T> list = new List<T>();
                using (SqlDataReader reader = Sql.ExecuteSqlReader(string.Format("select {0} from {1} {2}", Columns, tableName, Where)))
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
        }

        System.Reflection.PropertyInfo[] modelProperties;

        public System.Reflection.PropertyInfo[] ModelProperties
        {
            get { return modelProperties ?? (modelProperties = new T().GetType().GetProperties()); }
            set { modelProperties = value; }
        }

        SqlService sql;
        public SqlService Sql { get { return sql ?? (sql = new SqlService()); } set { sql = value; } }
        protected SqlDataReader reader;
        protected string tableName;

        public BaseTable(string tableName) { this.tableName = tableName; }

        public int Update()
        {
            try
            {
                if (string.IsNullOrEmpty(Columns)) throw new Exception("Columns 不能为空。");
                string[] cols = Columns.Split(',');
                for (int i = 0; i < cols.Length; i++) cols[i] = cols[i] + "=@" + cols[i];
                string sql = "update set " + string.Join(",", cols) + Where;
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
    }

}

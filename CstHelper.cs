using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.Design;
using CodeSmith.Engine;
using SchemaExplorer;

public class CstHelper : CodeSmith.BaseTemplates.OutputFileCodeTemplate {

    /// <summary>
    /// 驼峰命名，首字母小写
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string GetCamelCaseName(string value)
    {
    	return value.Substring(0, 1).ToLower() + value.Substring(1);
    }
    /// <summary>
    /// 返回下划线+驼峰命名后的列名
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public string GetMemberVariableName(ColumnSchema column)
    {
    	string propertyName = GetPropertyName(column);
    	string memberVariableName = "_" + GetCamelCaseName(propertyName);
    	return memberVariableName;
    }

    /// <summary>
    /// 获取列名
    /// </summary>
    /// <param name="column">ColumnSchema 列对象</param>
    /// <returns></returns>
    public string GetPropertyName(ColumnSchema column)
    {
    	string propertyName = column.Name;
    	return propertyName;
    	if (propertyName == column.Table.Name + "Name") return "Name";
    	if (propertyName == column.Table.Name + "Description") return "Description";
    	
    	if (propertyName.EndsWith("TypeCode")) propertyName = propertyName.Substring(0, propertyName.Length - 4);
    	
    	return propertyName;
    }
    		
    public string GetMemberVariableDefaultValue(ColumnSchema column)
    {
    	switch (column.DataType)
    	{
    		case DbType.Guid:
    		{
    			return "Guid.Empty";
    		}
    		case DbType.AnsiString:
    		case DbType.AnsiStringFixedLength:
    		case DbType.String:
    		case DbType.StringFixedLength:
    		{
    			return "String.Empty";
    		}
    		default:
    		{
    			return "";
    		}
    	}
    }



    public string GetClassName(TableSchema table)
    {
    	if (table.Name.EndsWith("s"))
    	{
    		return table.Name.Substring(0, table.Name.Length - 1);
    	}
    	else
    	{
    		return table.Name;
    	}
    }
}
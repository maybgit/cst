 -- =============================================
 -- AUTHOR:		<MAYB>
 -- CREATE DATE: 
 -- DESCRIPTION:	<分页存储过程>
 -- =============================================
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[P_Pager]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[P_Pager]
GO
CREATE PROCEDURE [dbo].[P_Pager]
	@TABLENAME VARCHAR(100),					--表名
	@Fields varchar(max),						--字段
	@WHERE VARCHAR(MAX),						--条件
	@ORDERFIELD VARCHAR(50),					--排序字段
	@STARTINDEX INT,							--开始
	@ENDINDEX INT,								--结束
	@RECORDCOUNT INT OUTPUT						--检索的数据总数
AS
BEGIN
	SET @where = REPLACE(@where, '\', '''')
	SET @where = REPLACE(@where, 'ｗｈｅｒｅ', 'where')
	SET @TABLENAME = REPLACE(@TABLENAME, '\', '''')
	IF(@ORDERFIELD='')
	BEGIN
		SET @ORDERFIELD = 'id desc'
	END
	DECLARE @PAGINGWHERE VARCHAR(MAX) = ' WHERE ROWNUM BETWEEN ' + CAST(@STARTINDEX AS VARCHAR(10)) + ' AND ' + CAST(@ENDINDEX AS VARCHAR(10)) + ''
	DECLARE @COUNTSQL NVARCHAR(MAX) = 'SELECT @RECORDCOUNT = COUNT(ID) FROM ' + @TABLENAME + ' WHERE 1 = 1 '
	DECLARE @SQL VARCHAR(MAX) = 'SELECT TOP 100 PERCENT ROW_NUMBER() OVER(ORDER BY '+@ORDERFIELD+') AS ROWNUM, * FROM ' + @TABLENAME + ' WHERE 1 = 1 '
	IF(@Fields = '')
	BEGIN
		SET @Fields = '*'
	END
	
	SET @COUNTSQL += @WHERE
	SET @SQL += @WHERE
	SET @SQL = 'SELECT '+@Fields+' FROM (' + @SQL + ') AS T ' + @PAGINGWHERE
	PRINT @SQL
	EXEC(@SQL)
	EXEC SP_EXECUTESQL @COUNTSQL, N'@RECORDCOUNT INT OUT', @RECORDCOUNT OUT
END

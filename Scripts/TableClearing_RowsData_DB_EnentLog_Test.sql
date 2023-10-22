--�����: �������� ������
--����: 19.04.2022

--���� ������� ��������� ����� ���� ������ �������� ���� ������� ����������� 1� ���
--������ ������� ��� ������ ������ ������ 6 �������. � ����� � ���, ��� ������ �����, � ����� ����,
--�� �������� ������� �������� ���������� � ���������� @BatchSize

SET NOCOUNT ON
DECLARE
	@BatchNum INT,
	@BatchSize INT,
	@StatusMsg nvarchar(100),
	@Sql NVARCHAR(max) = ''

SET @BatchNum = 1
SET @BatchSize = 50000 --����� ����� �� 50 �����

--������� ����� ������������
BEGIN TRANSACTION

WHILE @BatchSize > 0
BEGIN
    SET @StatusMsg =
        N'Deleting rows - batch #' + CAST(@BatchNum AS nvarchar(5)) + '; batch size ' + CAST(@BatchSize AS NVARCHAR(8))
    RAISERROR(@StatusMsg, 0, 1) WITH NOWAIT

	SET @sql = 'DELETE TOP (' + CAST(@BatchSize AS NVARCHAR(8)) + ')
				FROM [dbo].[RowsData]
					WHERE CAST(Period AS date) <= EOMONTH(current_timestamp,-7)'

	BEGIN TRY	
		--������� ����� ������������
		--RAISERROR(1, 16, 1)
		EXEC sp_executesql @Sql
		SELECT @BatchSize = @@ROWCOUNT	
		SET @BatchNum = @BatchNum + 1
	END TRY

	BEGIN CATCH
		--���������� ������ � �������� ���
		RAISERROR('������ �������� ����� �� ������� [dbo].[RowsData] �� EventLog!', 17, 1) WITH LOG;
		BREAK
	END CATCH
END

--������� ����� ������������
SELECT * FROM [dbo].[RowsData]
	 WHERE CAST(Period AS date) <= EOMONTH(current_timestamp,-7)

--������� ����� ������������
ROLLBACK TRANSACTION




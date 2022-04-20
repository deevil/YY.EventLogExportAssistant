--Автор: Валентин Козлов
--Дата: 19.04.2022

--Цель запроса сократить объем базы данных хранящей логи журнала регистрации 1С БИТ
--Запрос удаляет все данные старше полных 6 месяцев. В связи с тем, что данных много, а места мало,
--то удаление сделано порциями указанными в переменной @BatchSize

SET NOCOUNT ON
DECLARE
	@BatchNum INT,
	@BatchSize INT,
	@StatusMsg nvarchar(100),
	@Sql NVARCHAR(max) = ''

SET @BatchNum = 1
SET @BatchSize = 50000 --Берем пакет по 50 тысяч

--Удалить после тестирования
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
		--Удалить после тестирования
		--RAISERROR(1, 16, 1)
		EXEC sp_executesql @Sql
		SELECT @BatchSize = @@ROWCOUNT	
		SET @BatchNum = @BatchNum + 1
	END TRY

	BEGIN CATCH
		--Записываем ошибку в виндовый лог
		RAISERROR('Ошибка удаления строк из таблицы [dbo].[RowsData] БД EventLog!', 17, 1) WITH LOG;
		BREAK
	END CATCH
END

--Удалить после тестирования
SELECT * FROM [dbo].[RowsData]
	 WHERE CAST(Period AS date) <= EOMONTH(current_timestamp,-7)

--Удалить после тестирования
ROLLBACK TRANSACTION




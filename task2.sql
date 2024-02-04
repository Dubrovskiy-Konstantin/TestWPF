USE TestWPF;
GO

CREATE TABLE [dbo].[Banks](
	Id BIGINT IDENTITY(1,1) PRIMARY KEY,
	[Name] NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE [dbo].[Periods](
	Id BIGINT IDENTITY(1,1) PRIMARY KEY,
	[From] DATE NOT NULL,
	[To] DATE NOT NULL
);
GO

CREATE TABLE [dbo].[Bills](
	Id BIGINT IDENTITY(1,1) PRIMARY KEY,
	Id_Bank BIGINT NOT NULL,
	Id_Period BIGINT NOT NULL,
	Bill_Number INT NOT NULL,
	Opening_Balance_Asset DECIMAL(19,2) DEFAULT 0.0, -- 10-19 = 9 bytes, 20-28 = 13 bytes
	Opening_Balance_Liability DECIMAL(19,2) DEFAULT 0.0,
	Turnover_Debit DECIMAL(19,2) DEFAULT 0.0,
	Turnover_Credit DECIMAL(19,2) DEFAULT 0.0,
	Closing_Balance_Asset DECIMAL(19,2) DEFAULT 0.0,
	Closing_Balance_Liability DECIMAL(19,2) DEFAULT 0.0,
);
GO

ALTER TABLE [dbo].[Bills] ADD CONSTRAINT FK_BillBank FOREIGN KEY (Id_Bank) REFERENCES [Banks](Id);
GO
ALTER TABLE [dbo].[Bills] ADD CONSTRAINT FK_BillPeriod FOREIGN KEY (Id_Period) REFERENCES [Periods](Id);
GO

CREATE TABLE [dbo].[Sheets](
	Id INT IDENTITY(1,1) PRIMARY KEY,
	Id_Bank BIGINT NOT NULL,
	Id_Period BIGINT NOT NULL,
	[Location] NVARCHAR(MAX) NULL
);

GO
ALTER TABLE [dbo].[Sheets] ADD CONSTRAINT FK_SheetBank FOREIGN KEY (Id_Bank) REFERENCES [Banks](Id);
GO
ALTER TABLE [dbo].[Sheets] ADD CONSTRAINT FK_SheetPeriod FOREIGN KEY (Id_Period) REFERENCES [Periods](Id);
GO

CREATE PROCEDURE IncertFile @FileLocation NVARCHAR(MAX), @BankName NVARCHAR(255), @DateFrom DATE, @DateTo DATE					
AS
BEGIN
	-- Проверка: не существует ли уже в базе отчет с таким путем
	IF (
			SELECT COUNT([Location]) 
			FROM [dbo].[Sheets] 
			WHERE [Location] LIKE @FileLocation) > 0
				THROW 51000, 'Such file already in database.', 1;

	-- Проверка: не существует ли уже в базе отчет по такому периоду у такого банка
	IF (
			SELECT COUNT([Id_Bank]) 
			FROM [dbo].[Sheets] 
			WHERE Id_Bank = (	
								SELECT TOP 1 [Id] FROM [dbo].[Banks] 
								WHERE [Name] LIKE @BankName)) > 0 AND 
		(
			SELECT COUNT([Id_Period]) 
			FROM [dbo].[Sheets] 
			WHERE [Id_Period] = (
									SELECT TOP 1 [Id] FROM [dbo].[Periods]
									WHERE [From] = @DateFrom AND [To] = @DateTo)) > 0
				THROW 51001, 'Bills for such Period of such Bank already in database.', 1;

	BEGIN TRY
	BEGIN TRANSACTION
		-- Получить или создать Id банка
		DECLARE @Id_Bank BIGINT;
		IF (
				SELECT COUNT([Name]) 
				FROM [dbo].[Banks] 
				WHERE [Name] LIKE @BankName) > 0
			BEGIN
				SET @Id_Bank = (
									SELECT TOP 1 [Id] 
									FROM [Banks] 
									WHERE [Name] LIKE @BankName);
			END;
		ELSE
			BEGIN
				INSERT INTO [dbo].[Banks] (
					[Name]) 
				VALUES (
					@BankName);
				SET @Id_Bank = @@IDENTITY;
			END;

		-- Получить или создать Id периода
		DECLARE @Id_Period BIGINT;
		IF (
				SELECT COUNT(*) 
				FROM [dbo].[Periods] 
				WHERE [From] = @DateFrom AND [To] = @DateTo) > 0
			BEGIN
				SET @Id_Period = (
									SELECT TOP 1 [Id] 
									FROM [dbo].[Periods] 
									WHERE [From] = @DateFrom AND [To] = @DateTo);
			END;
		ELSE
			BEGIN
				INSERT INTO [dbo].[Periods] (
					[From], 
					[To]) 
				VALUES (
					@DateFrom, 
					@DateTo);
				SET @Id_Period = @@IDENTITY;
			END;

		-- Создать новую запись
		INSERT INTO [dbo].[Sheets] (
			Id_Bank, 
			Id_Period, 
			[Location])
		VALUES (
			@Id_Bank,
			@Id_Period,
			@FileLocation);
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
	COMMIT TRANSACTION;
	RETURN;
END;

CREATE PROCEDURE GetBankAndPeriodId (@BankName NVARCHAR(255), @DateFrom DATE, @DateTo DATE, @IdBank BIGINT OUTPUT, @IdPeriod BIGINT OUTPUT)
AS
	SELECT TOP 1 @IdBank = [Id_Bank] , @IdPeriod = [Id_Period]
	FROM [dbo].[Sheets] 
	WHERE [Id_Bank] = (
						SELECT TOP 1 [Banks].[Id] 
						FROM [dbo].[Banks] 
						WHERE  [Banks].[Name] LIKE @BankName)
	AND [Id_Period] = (
						SELECT TOP 1 [Periods].[Id] 
						FROM [dbo].[Periods] 
						WHERE [Periods].[From] = @DateFrom 
						AND [Periods].[To] = @DateTo);

CREATE PROCEDURE IncertBill @IdBank BIGINT, @IdPeriod BIGINT, @BillNumber INT, 
					@OpeningBalanceAsset DECIMAL(19,2), @OpeningBalanceLiability DECIMAL(19,2),
					@TurnoverDebit DECIMAL(19,2), @TurnoverCredit DECIMAL(19,2),
					@ClosingBalanceAsset DECIMAL(19,2), @ClosingBalanceLiability DECIMAL(19,2)
AS
BEGIN 
	INSERT INTO [dbo].[Bills] (
		[Id_Bank],
		[Id_Period],
		[Bill_Number],
		[Opening_Balance_Asset],
		[Opening_Balance_Liability],
		[Turnover_Debit],
		[Turnover_Credit],
		[Closing_Balance_Asset],
		[Closing_Balance_Liability])
	VALUES (
		@IdBank,
		@IdPeriod,
		@BillNumber,
		@OpeningBalanceAsset,
		@OpeningBalanceLiability,
		@TurnoverDebit,
		@TurnoverCredit,
		@ClosingBalanceAsset,
		@ClosingBalanceLiability);
END;

EXEC IncertFile N'C:\work\file.txt', N'bank', '2020-01-01', '2020-12-31';
EXEC IncertFile N'C:\work\file2.txt', N'bank2', '2020-01-01', '2020-12-31';
DECLARE @bank BIGINT, @period BIGINT;
EXEC GetBankAndPeriodId N'bank', '2020-01-01', '2020-12-31', @bank OUTPUT, @period OUTPUT;
SELECT @bank, @period;
EXEC IncertBill '1', '1', '1000', '123123123.22', '123123132.00', '0.0', '0.0', '11111.20', '1.1';
EXEC IncertBill 1, 1, 1001, 123123123.22, 123123132.00, 0.0, 0.0, 11111.20, 1.1;
EXEC IncertBill @bank, @period, 1002, 123123123.22, 123123132.00, 0.0, 0.0, 11111.20, 1.1;
SELECT * FROM Bills;
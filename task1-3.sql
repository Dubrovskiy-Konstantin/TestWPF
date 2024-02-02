CREATE DATABASE TestWPF;
GO
USE TestWPF;
GO

CREATE TABLE [dbo].[Task1]
(
	[Id] INT IDENTITY(1,1) PRIMARY KEY,
	[RndDate] DATE NOT NULL,
	[RndLatSymbols] CHAR(10) NOT NULL,
	[RndRusSymbols] NCHAR(10) NOT NULL,
	[RndEvenNumber] INT NOT NULL,
	[RndFloatNumber] FLOAT NOT NULL,
);

GO

CREATE PROCEDURE IntNumberSum
AS
BEGIN
	SELECT  SUM(CAST(RndEvenNumber AS BIGINT)) as Total FROM [dbo].[Task1]
END;

GO

CREATE PROCEDURE FloatNumberAvg
AS
BEGIN
	SELECT AVG(RndFloatNumber) as Average FROM [dbo].[Task1]
END;

--INSERT INTO [dbo].[Task1] ([RndDate],[RndLatSymbols],[RndRusSymbols],[RndEvenNumber],[RndFloatNumber]) VALUES (
--	'2022.07.18', 'AdddaAAddD', N'ÔûûûûûûûÂÂ', '4545456', '1.29992201');

--select * from [dbo].[Task1];
--exec FloatNumberAvg
-- The tables and the second login that Spring.Data.NHibernate5.Integration.Tests need in the Spring database.
-- The Spring database and the springqa login come from build-support/create-integration-test-databases-and-users.sql.

IF EXISTS (SELECT * FROM sys.server_principals WHERE name = N'springqa2')
    DROP LOGIN [springqa2]
GO

CREATE LOGIN [springqa2] WITH PASSWORD=N'springqa2', DEFAULT_DATABASE=[Spring], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
GO

USE [Spring]
GO

CREATE USER [springqa2] FOR LOGIN [springqa2] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa2'
GO

CREATE TABLE [dbo].[Credits]
(
    [CreditID] [int] IDENTITY(1,1) NOT NULL,
    [CreditAmount] [float] NOT NULL,
    CONSTRAINT [PK_CreditID] PRIMARY KEY CLUSTERED ([CreditID] ASC)
)
GO

CREATE TABLE [dbo].[Debits]
(
    [DebitID] [int] IDENTITY(1,1) NOT NULL,
    [DebitAmount] [float] NOT NULL,
    CONSTRAINT [PK_DebitID] PRIMARY KEY CLUSTERED ([DebitID] ASC)
)
GO

CREATE TABLE [dbo].[TestObjects]
(
    [TestObjectNo] [int] IDENTITY(1,1) NOT NULL,
    [Age] [int] NOT NULL,
    [Name] [nvarchar](1024) NULL,
    CONSTRAINT [TestObjectNo] PRIMARY KEY CLUSTERED ([TestObjectNo] ASC)
)
GO

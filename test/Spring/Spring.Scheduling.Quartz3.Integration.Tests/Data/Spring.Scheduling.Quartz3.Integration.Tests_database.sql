-- The quartz database that Spring.Scheduling.Quartz3.Integration.Tests use, owned by the springqa login.
-- tables_sqlServer.sql (vendored from Quartz.NET) then creates the Quartz tables in it.
-- The springqa login comes from build-support/create-integration-test-databases-and-users.sql.

-- drop the quartz DB if it exists (the script is run against a server that may already have been used)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'quartz')
BEGIN
    ALTER DATABASE [quartz] SET SINGLE_USER WITH ROLLBACK IMMEDIATE

    DROP DATABASE [quartz]
END
GO

CREATE DATABASE [quartz]
GO

USE [quartz]
GO

CREATE USER [springqa] FOR LOGIN [springqa] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa'
GO

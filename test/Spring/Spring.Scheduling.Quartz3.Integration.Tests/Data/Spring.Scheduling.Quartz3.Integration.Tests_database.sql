-- The quartz database that Spring.Scheduling.Quartz3.Integration.Tests use, owned by the springqa login.
-- tables_sqlServer.sql (vendored from Quartz.NET) then creates the Quartz tables in it.
-- The springqa login comes from build-support/create-integration-test-databases-and-users.sql.

CREATE DATABASE [quartz]
GO

USE [quartz]
GO

CREATE USER [springqa] FOR LOGIN [springqa] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa'
GO

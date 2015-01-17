--drop Spring DB if exists
IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'Spring')
BEGIN
    ALTER DATABASE Spring 
        SET SINGLE_USER 
        WITH ROLLBACK IMMEDIATE

    DROP DATABASE Spring
END
GO

--drop Credits DB if exists
IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'Credits')
BEGIN
    ALTER DATABASE Credits 
        SET SINGLE_USER 
        WITH ROLLBACK IMMEDIATE

    DROP DATABASE Credits
END
GO

--drop Debits DB if exists
IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'Debits')
BEGIN
    ALTER DATABASE Debits 
        SET SINGLE_USER 
        WITH ROLLBACK IMMEDIATE

    DROP DATABASE Debits
END
GO

--drop CreditsAndDebits DB if exists
IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'CreditsAndDebits')
BEGIN
    ALTER DATABASE CreditsAndDebits 
        SET SINGLE_USER 
        WITH ROLLBACK IMMEDIATE

    DROP DATABASE CreditsAndDebits
END
GO

-- if SpringQA user exists, drop it
IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'springqa')
    DROP LOGIN [springqa]
GO

-- create the databases
CREATE DATABASE Spring
GO

CREATE DATABASE Credits
GO

CREATE DATABASE Debits
GO

CREATE DATABASE CreditsAndDebits
GO

-- create the SpringQA login itself
CREATE LOGIN [springqa] WITH PASSWORD=N'springqa', DEFAULT_DATABASE=[Spring], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF
GO

-- set the SpringQA user as DB_OWNER for each of the databases...
USE Spring
CREATE USER [springqa] FOR LOGIN [springqa] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa'
GO

USE Credits
CREATE USER [springqa] FOR LOGIN [springqa] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa'
GO

USE Debits
CREATE USER [springqa] FOR LOGIN [springqa] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa'
GO

USE CreditsAndDebits
CREATE USER [springqa] FOR LOGIN [springqa] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa'
GO

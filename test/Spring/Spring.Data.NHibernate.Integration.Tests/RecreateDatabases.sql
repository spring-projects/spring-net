IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'Spring')
BEGIN
    ALTER DATABASE Spring 
        SET SINGLE_USER 
        WITH ROLLBACK IMMEDIATE

    DROP DATABASE Spring
END
GO

IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'NHibernate')
BEGIN
    ALTER DATABASE NHibernate 
        SET SINGLE_USER 
        WITH ROLLBACK IMMEDIATE

    DROP DATABASE NHibernate
END
GO

IF  EXISTS (SELECT * FROM sys.server_principals WHERE name = N'springqa2')
    DROP LOGIN [springqa2]
GO

CREATE DATABASE Spring
GO
CREATE DATABASE NHibernate
GO

CREATE LOGIN [springqa2] WITH PASSWORD=N'springqa2', DEFAULT_DATABASE=[Spring], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION = OFF, CHECK_POLICY = OFF

GO

USE Spring
CREATE USER [springqa2] FOR LOGIN [springqa2] WITH DEFAULT_SCHEMA=[dbo]
EXEC sp_addrolemember 'db_owner', 'springqa2'
GO

-- ALTERNATIVELY JUST DROP ALL TABLES:
-- EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
-- EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL';
-- EXEC sp_MSForEachTable 'DROP TABLE ?';

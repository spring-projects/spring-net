/*
This script installs 2 procedures 'BackupNorthwind' and 'RestoreNorthwind'

These procedures allow for easy backup and restore the Northwind Database during testing.
The backup is done to C:\northwind.bak and also takes care to kill existing connections before doing a restore.

NOTE: You must ensure, that your MSSQL-Server process has write access to C:\ (or change directory below)
*/

USE [master]
GO

/****** Object:  StoredProcedure [dbo].[spdd_DropDatabaseConnections] *****/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DropDatabaseConnections]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_DropDatabaseConnections]
GO
CREATE proc [dbo].[sp_DropDatabaseConnections]
	@dbname sysname
as
begin
	declare @spid int
	declare @killCmd nvarchar(50)
	declare processes cursor for
		select spid from master..sysprocesses
		where dbid = DB_ID(@dbname)

	OPEN processes
	FETCH processes into @spid
	WHILE(@@fetch_status=0) BEGIN
		set @killCmd = N'KILL '+CAST(@spid as nvarchar(10))
		print N'Executing ''' + @killCmd + ''''
		exec sp_executesql @killCmd
		FETCH processes into @spid
	END

	CLOSE processes
	DEALLOCATE processes
end
GO

/****** Object:  StoredProcedure [dbo].[BackupNorthwind]    Script Date: 05/25/2007 13:34:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BackupNorthwind]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BackupNorthwind]
GO
create proc [dbo].[BackupNorthwind]
as
begin
BACKUP DATABASE [northwind]
	TO DISK = N'C:\northwind.bak'
	WITH NAME = N'Northwind Full Database Backup', FORMAT, INIT, SKIP
end
GO

/****** Object:  StoredProcedure [dbo].[RestoreNorthwind]    Script Date: 05/25/2007 13:34:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RestoreNorthwind]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RestoreNorthwind]
GO
CREATE proc [dbo].[RestoreNorthwind]
as
begin
exec dbo.sp_DropDatabaseConnections N'northwind'
RESTORE DATABASE [northwind] FROM  DISK = N'C:\northwind.bak' WITH  FILE = 1,  REPLACE
end
GO

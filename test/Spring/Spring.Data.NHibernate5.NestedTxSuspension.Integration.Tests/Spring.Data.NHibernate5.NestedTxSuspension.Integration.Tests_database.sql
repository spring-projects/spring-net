-- The table that Spring.Data.NHibernate5.NestedTxSuspension.Integration.Tests map (Mappings.hbm.xml) in the Spring database.
-- The Spring database and the springqa login come from build-support/create-integration-test-databases-and-users.sql.

USE [Spring]
GO

CREATE TABLE [dbo].[Container]
(
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](255) NULL,
    CONSTRAINT [PK_Container] PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO

create database Credits
GO
CREATE TABLE [Credits](
	[CreditID] [int] IDENTITY NOT NULL,
	[CreditAmount] [float] NOT NULL,
	CONSTRAINT [PK_CreditID] PRIMARY KEY CLUSTERED
	(
		[CreditID] ASC
	)
)
go

create database Debits
GO
CREATE TABLE [Debits](
	[DebitID] [int] IDENTITY NOT NULL,
	[DebitAmount] [float] NOT NULL,
	CONSTRAINT [PK_DebitID] PRIMARY KEY CLUSTERED
	(
	[DebitID] ASC
	) ON [PRIMARY]
)
GO

create database CreditsAndDebits
GO
CREATE TABLE [Debits](
	[DebitID] [int] IDENTITY NOT NULL,
	[DebitAmount] [float] NOT NULL,
	CONSTRAINT [PK_DebitID] PRIMARY KEY CLUSTERED
	(
	[DebitID] ASC
	) ON [PRIMARY]
)
CREATE TABLE [Credits](
	[CreditID] [int] IDENTITY NOT NULL,
	[CreditAmount] [float] NOT NULL,
	CONSTRAINT [PK_CreditID] PRIMARY KEY CLUSTERED
	(
		[CreditID] ASC
	)
)
GO

create database Spring
GO
CREATE TABLE [Debits](
	[DebitID] [int] IDENTITY NOT NULL,
	[DebitAmount] [float] NOT NULL,
	CONSTRAINT [PK_DebitID] PRIMARY KEY CLUSTERED
	(
	[DebitID] ASC
	) ON [PRIMARY]
)
CREATE TABLE [Credits](
	[CreditID] [int] IDENTITY NOT NULL,
	[CreditAmount] [float] NOT NULL,
	CONSTRAINT [PK_CreditID] PRIMARY KEY CLUSTERED
	(
		[CreditID] ASC
	)
)
CREATE TABLE [dbo].[TestObjects](
	[TestObjectNo] [int] IDENTITY(1,1) NOT NULL,
	[Age] [int] NOT NULL,
	[Name] [nvarchar](1024) NULL,
	CONSTRAINT [TestObjectNo] PRIMARY KEY CLUSTERED
	(
		[TestObjectNo] ASC
	)
)

create database Spring2
GO
CREATE TABLE [Debits](
	[DebitID] [int] IDENTITY NOT NULL,
	[DebitAmount] [float] NOT NULL,
	CONSTRAINT [PK_DebitID] PRIMARY KEY CLUSTERED
	(
	[DebitID] ASC
	) ON [PRIMARY]
)
CREATE TABLE [Credits](
	[CreditID] [int] IDENTITY NOT NULL,
	[CreditAmount] [float] NOT NULL,
	CONSTRAINT [PK_CreditID] PRIMARY KEY CLUSTERED
	(
		[CreditID] ASC
	)
)
CREATE TABLE [dbo].[TestObjects](
	[TestObjectNo] [int] IDENTITY(1,1) NOT NULL,
	[Age] [int] NOT NULL,
	[Name] [nvarchar](1024) NULL,
	CONSTRAINT [TestObjectNo] PRIMARY KEY CLUSTERED
	(
		[TestObjectNo] ASC
	)
)

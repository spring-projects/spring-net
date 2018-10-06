
CREATE TABLE [TestObjects] (
	[TestObjectNo] int IDENTITY NOT NULL,
	[Age] int NOT NULL,
    [Name] nvarchar(1024),
	CONSTRAINT [TestObjectNo] PRIMARY KEY CLUSTERED 
	(
		[TestObjectNo] ASC
	)
);
CREATE PROCEDURE CreateTestObject
{
  @Age int,
  @Name varchar(15)
} AS

INSERT INTO into TestObjects(Age, Name) VALUES (@Age, @Name)



USE Spring
CREATE TABLE TestObjects
(
    TestObjectNo    int     IDENTITY NOT NULL,
    Age             int,
    Name            varchar(50)
)


CREATE PROCEDURE SelectByName 
(
  @Name varchar(50)
  
)

as 
  select TestObjectNo, Age,Name from TestObjects where Name = @Name

return

CREATE PROCEDURE SelectByNameWithReturnValue
(
  @Name varchar(50)
  
)

as 
  select * from TestObjects where Name = @Name

return 5


CREATE PROCEDURE SelectByNameWithReturnAndOutValue
(
  @Name varchar(50),
  @Count int output
  
)

as 
  select * from TestObjects where Name = @Name
  set @Count = 10
return 5


CREATE PROCEDURE CreateTestObject
(
  @Name varchar(50),
  @Age  int
)

as
  insert into TestObjects(Name, Age) Values (@Name, @Age)





INSERT INTO TestObjects 
    (Age, Name)
VALUES
    (1, 'Gabriel')
    
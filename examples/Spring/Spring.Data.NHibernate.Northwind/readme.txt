Database configuration
======================

Default: SQLite
---------------

The *default configuration* of the application uses a SQLite database therefore no extra setup is required to run the example.



For SQLServer
-------------

A database file containing the Northwind database schema and data is located in the 
directory src\Spring.Northwind.Web\App_Data named NORTHWND.MDF.  This file can me mounted explicitly using SQL Server 2005 or
SQL Server Express.  The web project's <db:provider> configuration is
setup to point to this database file.  

The integratation tests point to a copy of that database located in the
directory \test\Spring.Northwind.IntegrationTests\bin\Debug.  You will need to copy the .mdf file to the bin\Debug
directory to run the integration test.

SQL Server Express or SQL Server 2005/2008 will need to running before the web application or integration test is launched.

If you prefer to run against a different database, the schema is located in the test_northwind.sql and you will need to
modify accordingly.
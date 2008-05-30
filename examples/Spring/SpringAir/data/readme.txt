This directory contains scripts for creating and populating the SpringAir database.

Scripts are provided for a number of popular commercial and open source databases. Hopefully you will be able to find one set of scripts to match your current environment, and if not, well... at least you have some choice in the matter of which database to download and experiment with. Remember, Spring.NET is all about choice :)

Marketing over, the scripts for each supported database are in subdirectories of the directory in which this current file resides, in subdirectories named after the particular database. (For example, Oracle scripts are in the 'oracle' subdirectory). All of the scripts follow a standard format...

The 'schema' script creates users / schemas (as appropriate) for the 'springair' database.
The 'drop' script drops all of the *relations* (note that it does not drop users / schemas).
The 'create' script creates all of the required relations.
The 'populate' script adds some reference and test data to the various relations created by the 'create' script.
The 'refresh' script just encapsulates all of the above scripts into one convenient script. You can use it to teardown, create, and populate the 'springair' database in one convenient script execution.

As with all scripts, *please* do open them up and have a quick shuftie at the contents. They are just simple 'create some relations, bung some data in' scripts, but you run them at your own risk. No really, please look inside the scripts before running them... remember, safety first :)

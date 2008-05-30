drop database springair;
create database springair;
use mysql;
grant all privileges on *.* to 'spring'@'localhost' identified by 'spring';
grant all privileges on *.* to 'spring'@'%' identified by 'spring';
commit;
use springair;
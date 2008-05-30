drop user springair cascade;
create user springair identified by spring;
grant create procedure, create session, resource, create table, create view, create synonym to springair;
commit;
CREATE DATABASE smart;
create tablespace sup owner postgres location '/sup';
create tablespace rtl owner postgres location '/rtl';
ALTER USER postgres WITH PASSWORD 'CS123456';

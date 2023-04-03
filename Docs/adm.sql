

-- chown postgres /mntsup
-- CREATE TABLESPACE sup location '/mntsup';

-- chown postgres /mntrtl
-- CREATE TABLESPACE rtl location '/mntrtl';

SELECT * FROM pg_tablespace;

-- pg_default

SELECT tablename AS relname
FROM pg_tables WHERE tablespace IS NULL AND schemaname = 'public'
UNION
SELECT indexname AS relname
FROM pg_indexes WHERE tablespace IS NULL AND schemaname = 'public';

-- sup

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='sup');

ALTER TABLE books SET TABLESPACE sup;
ALTER TABLE bookclrs SET TABLESPACE sup;
ALTER TABLE bookaggs_typ SET TABLESPACE sup;

ALTER INDEX books_pk SET TABLESPACE sup;
ALTER INDEX books_single_idx SET TABLESPACE sup;
ALTER INDEX books_ctridstatus_idx SET TABLESPACE sup;
ALTER INDEX books_mktidstatus_idx SET TABLESPACE sup;
ALTER INDEX books_shpidstatus_idx SET TABLESPACE sup;
ALTER INDEX books_srcidstatus_idx SET TABLESPACE sup;

-- rtl

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='rtl');

ALTER TABLE buys SET TABLESPACE rtl;
ALTER TABLE buyclrs SET TABLESPACE rtl;
ALTER TABLE buyaggs_typ SET TABLESPACE rtl;

ALTER INDEX buys_pk SET TABLESPACE rtl;
ALTER INDEX buys_single_idx SET TABLESPACE rtl;
ALTER INDEX buys_mktidstatus_idx SET TABLESPACE rtl;
ALTER INDEX buys_shpidstatus_idx SET TABLESPACE rtl;
ALTER INDEX buys_uidstatus_idx SET TABLESPACE rtl;





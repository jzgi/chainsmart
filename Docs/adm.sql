

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

ALTER TABLE purs SET TABLESPACE sup;
ALTER TABLE puraps SET TABLESPACE sup;
ALTER TABLE purldgs_hub_lotid SET TABLESPACE sup;

ALTER INDEX purs_pk SET TABLESPACE sup;
ALTER INDEX purs_uidx SET TABLESPACE sup;
ALTER INDEX purs_ctridstatus_idx SET TABLESPACE sup;
ALTER INDEX purs_mktidstatus_idx SET TABLESPACE sup;
ALTER INDEX purs_rtlidstatus_idx SET TABLESPACE sup;
ALTER INDEX purs_supidstatus_idx SET TABLESPACE sup;

-- rtl

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='rtl');

ALTER TABLE buys SET TABLESPACE rtl;
ALTER TABLE buyaps SET TABLESPACE rtl;
ALTER TABLE buyldgs_typ SET TABLESPACE rtl;

ALTER INDEX buys_pk SET TABLESPACE rtl;
ALTER INDEX buys_uidx SET TABLESPACE rtl;
ALTER INDEX buys_mktidstatus_idx SET TABLESPACE rtl;
ALTER INDEX buys_rtlidtypstatus_idx SET TABLESPACE rtl;
ALTER INDEX buys_uidstatus_idx SET TABLESPACE rtl;







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

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='rtl');

ALTER TABLE buys_uidstatus_idx SET TABLESPACE rtl;


ALTER INDEX purs_pk SET TABLESPACE sup;
ALTER INDEX purs_uidx SET TABLESPACE sup;
ALTER INDEX purs_hubidstatusmktid_idx SET TABLESPACE sup;
ALTER INDEX purs_mktidstatus_idx SET TABLESPACE sup;
ALTER INDEX purs_rtlidstatus_idx SET TABLESPACE sup;
ALTER INDEX purs_supidstatustyp_idx SET TABLESPACE sup;

-- rtl

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='rtl');

ALTER TABLE buys SET TABLESPACE rtl;
ALTER TABLE buyaps SET TABLESPACE rtl;
ALTER TABLE buyldgs_typ SET TABLESPACE rtl;

ALTER INDEX buys_pk SET TABLESPACE rtl;
ALTER INDEX buys_uidx SET TABLESPACE rtl;
ALTER INDEX buys_mktidstatus_idx SET TABLESPACE rtl;
ALTER INDEX buys_rtlidtyp_idx SET TABLESPACE rtl;
ALTER INDEX buys_uidstatus_idx SET TABLESPACE rtl;





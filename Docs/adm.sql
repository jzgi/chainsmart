

-- chown postgres /mntsup
-- CREATE TABLESPACE sup location '/mntsup';

-- chown postgres /mntrtl
-- CREATE TABLESPACE rtl location '/mntrtl';

SELECT oid, spcname FROM pg_tablespace;

-- pg_default

SELECT tablename AS relname
FROM pg_tables WHERE tablespace IS NULL AND schemaname = 'public'
UNION
SELECT indexname AS relname
FROM pg_indexes WHERE tablespace IS NULL AND schemaname = 'public';

-- tablespace sup

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='sup');

ALTER TABLE purs SET TABLESPACE sup;
ALTER TABLE purgens SET TABLESPACE sup;
ALTER TABLE puraps SET TABLESPACE sup;
ALTER TABLE purldgs_lotid SET TABLESPACE sup;
ALTER TABLE purldgs_typ SET TABLESPACE sup;

ALTER INDEX purs_pk SET TABLESPACE sup;
ALTER INDEX purs_hubidstatusmktid_idx SET TABLESPACE sup;
ALTER INDEX purs_mktidstatustyp_idx SET TABLESPACE sup;
ALTER INDEX purs_rtlidstatustyp_idx SET TABLESPACE sup;
ALTER INDEX purs_supidstatustyp_idx SET TABLESPACE sup;

ALTER INDEX puraps_pk SET TABLESPACE sup;
ALTER INDEX purgens_pk SET TABLESPACE sup;
ALTER INDEX purldgs_lotid_pk SET TABLESPACE sup;
ALTER INDEX purldgs_typ_pk SET TABLESPACE sup;

-- tablespace rtl

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='rtl');

ALTER TABLE buys SET TABLESPACE rtl;
ALTER TABLE buygens SET TABLESPACE rtl;
ALTER TABLE buyaps SET TABLESPACE rtl;
ALTER TABLE buyldgs_itemid SET TABLESPACE rtl;
ALTER TABLE buyldgs_typ SET TABLESPACE rtl;

ALTER INDEX buys_pk SET TABLESPACE rtl;
ALTER INDEX buys_gen_idx SET TABLESPACE rtl;
ALTER INDEX buys_mktidstatustypucomoked_idx SET TABLESPACE rtl;
ALTER INDEX buys_orgidstatustyp_idx SET TABLESPACE rtl;
ALTER INDEX buys_uidstatus_idx SET TABLESPACE rtl;

ALTER INDEX buyaps_pk SET TABLESPACE rtl;
ALTER INDEX buygens_pk SET TABLESPACE rtl;
ALTER INDEX buyldgs_itemid_pk SET TABLESPACE rtl;
ALTER INDEX buyldgs_typ_pk SET TABLESPACE rtl;





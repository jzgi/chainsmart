

-- chown postgres /mntsup
-- CREATE TABLESPACE rtl location '/mntrtl';

SELECT * FROM pg_tablespace;

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='sup');

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='rtl');

-- pg_default
SELECT tablename AS relname
FROM pg_tables WHERE tablespace IS NULL AND schemaname = 'public'
UNION
SELECT indexname AS relname
FROM pg_indexes WHERE tablespace IS NULL AND schemaname = 'public';


ALTER TABLE wares SET TABLESPACE rtl;

ALTER INDEX buyldgs_orgidacctdt_idx SET TABLESPACE rtl;






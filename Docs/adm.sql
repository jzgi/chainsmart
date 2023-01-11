

-- chown postgres /mntsup
-- CREATE TABLESPACE rtl location '/mntrtl';

SELECT spcname FROM pg_tablespace;

select relname from pg_class
where reltablespace=(select oid from pg_tablespace where spcname='sup');

ALTER TABLE buyldgs SET TABLESPACE rtl;

ALTER INDEX lots_pk SET TABLESPACE sup;


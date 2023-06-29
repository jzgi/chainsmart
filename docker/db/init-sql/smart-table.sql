CREATE TYPE public.buyitem AS (
    itemid integer,
    lotid integer,
    name character varying(12),
    unit character varying(4),
    unitx smallint,
    price money,
    off money,
    qty numeric(6,1)
    );
ALTER TYPE public.buyitem OWNER TO postgres;

--
-- Name: stockop; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.stockop AS (
    dt timestamp(0) without time zone,
    tip character varying(20),
    qty integer,
    avail integer,
    by character varying(12)
    );


ALTER TYPE public.stockop OWNER TO postgres;

--
-- Name: booksgen(date, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.booksgen(till date, opr character varying) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE

past date;
    now timestamp(0) = localtimestamp(0);
    tillstamp timestamp(0);
    paststamp timestamp(0);

    TYP_PLAT constant int = 1;
    TYP_SRC constant int = 2;
    TYP_CTR constant int = 3;

--     rates in thousandth
    
    BASE constant int = 100;
    RATE_PLAT constant int = 1;
    RATE_SRC constant int = 97;
    RATE_CTR constant int = 2;

BEGIN

    opr = coalesce(opr, 'SYS');

SELECT coalesce(booksgen, '2000-01-01'::date)FROM global WHERE pk INTO past;
tillstamp = (till + interval '1 day')::timestamp(0);

    opr = coalesce(opr, 'SYS');

    paststamp = (past + interval '1 day')::timestamp(0);

    -- books for source

INSERT INTO bookaggs (orgid, dt, typ, coid, trans, amt, created, creator)
SELECT srcid,
       oked::date,
        typ,
    first(ctrid),
    count(pay),
    sum(pay - coalesce(refund, 0::money)),
    now,
    opr
FROM books
WHERE status = 4 AND oked >= paststamp AND oked < tillstamp
GROUP BY srcid, oked::date, typ;


INSERT INTO bookclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
SELECT TYP_SRC,
    first(creator),
    now,
    opr,
    orgid,
    till,
    sum(trans),
    sum(amt),
    RATE_SRC,
    sum(amt * RATE_SRC / BASE)
FROM bookaggs
WHERE typ = 1 AND dt > past AND dt <= till GROUP BY orgid;

INSERT INTO bookclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
SELECT TYP_CTR,
    first(creator),
    now,
    opr,
    coid,
    till,
    sum(trans),
    sum(amt),
    RATE_CTR,
    sum(amt * RATE_CTR / BASE)
FROM bookaggs
WHERE typ = 1 AND dt > past AND dt <= till GROUP BY coid;

UPDATE global SET booksgen = till WHERE pk;

END
$$;


ALTER FUNCTION public.booksgen(till date, opr character varying) OWNER TO postgres;

SET default_tablespace = rtl;

--
-- Name: buyaggs_itemid; Type: TABLE; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE TABLE public.buyaggs_itemid (
                                       orgid integer NOT NULL,
                                       dt date NOT NULL,
                                       typ integer NOT NULL,
                                       name character varying(12),
                                       corgid integer,
                                       trans integer,
                                       amt money
);


ALTER TABLE public.buyaggs_itemid OWNER TO postgres;

--
-- Name: buyitem_agg_func(public.buyaggs_itemid[], public.buyitem[], integer, date, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.buyitem_agg_func(ret public.buyaggs_itemid[], items public.buyitem[], orgid integer, dt date, corgid integer) RETURNS public.buyaggs_itemid[]
    LANGUAGE plpgsql
    AS $$
DECLARE
agg buyaggs_itemid;
    itm buyitem;
    fnd bool;
BEGIN

    FOREACH itm IN ARRAY items LOOP
        
        fnd = FALSE;
        
        IF ret IS NOT NULL THEN
        FOREACH agg IN ARRAY ret LOOP
            IF agg.typ = itm.itemid THEN -- found
                agg.trans = agg.trans + itm.qty;
                agg.amt = agg.amt + (itm.price - itm.off) * itm.qty;
                fnd = TRUE;
CONTINUE;
END IF;
END LOOP;
END IF;
        
        IF ret IS NULL OR NOT fnd THEN
            agg = (orgid, dt, itm.itemid, itm.name, corgid, itm.qty, (itm.price - itm.off) * itm.qty);
            ret = ret || agg;
end if;
END LOOP;

RETURN ret;
END;
$$;


ALTER FUNCTION public.buyitem_agg_func(ret public.buyaggs_itemid[], items public.buyitem[], orgid integer, dt date, corgid integer) OWNER TO postgres;

--
-- Name: buys_trig_func(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.buys_trig_func() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
DECLARE
itm buyitem;
BEGIN
    -- update ware avail values
    IF (TG_OP = 'INSERT' AND NEW.status = 4) THEN
        
        FOREACH itm IN ARRAY NEW.items LOOP -- oked
UPDATE items SET avail = avail - itm.qty, stock = stock - itm.qty WHERE id = itm.itemid;
END LOOP;
        
    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 1 AND OLD.status < 1) THEN -- paid

        FOREACH itm IN ARRAY NEW.items LOOP
UPDATE items SET avail = avail - itm.qty WHERE id = itm.itemid;
END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 4 AND OLD.status < 4) THEN -- delivered

        FOREACH itm IN ARRAY NEW.items LOOP
UPDATE items SET stock = stock - itm.qty WHERE id = itm.itemid;
END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 0 AND OLD.status > 0) THEN -- voided

        FOREACH itm IN ARRAY NEW.items LOOP
UPDATE items SET avail = avail + itm.qty, stock = stock + itm.qty WHERE id = itm.itemid;
END LOOP;

END IF;

RETURN NEW;
END
$$;


ALTER FUNCTION public.buys_trig_func() OWNER TO postgres;

--
-- Name: buysgen(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.buysgen() RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
curr timestamp(0) = localtimestamp(0);
    till date;
    tillstamp timestamp(0);
last date;
    laststamp timestamp(0);

    TYP_PLAT constant int = 1;
    TYP_SHP constant int = 2;
    TYP_MKT constant int = 3;

    BASE constant int = 100;
    RATE_PLAT constant int = 1;
    RATE_SHP constant int = 97;
    RATE_MKT constant int = 2;

BEGIN

    -- adjust parameters

SELECT coalesce(max(gens.till), '2000-01-01'::date) FROM gens WHERE typ = 1 INTO last;
laststamp = (last + interval '1 day')::timestamp(0);

    till = curr::date - interval '1 day';
    tillstamp = (till + interval '1 day')::timestamp(0);
    
    IF (till <= last) THEN
        RETURN;
end if;

    -- aggregate buys by typ

INSERT INTO buyaggs_typ
SELECT shpid,
       created::date,
        typ,
    first(name),
    first(mktid),
    count(*),
    sum(pay - coalesce(refund, 0::money))
FROM buys
WHERE
    created >= laststamp AND created < tillstamp AND status > 0
GROUP BY
    shpid, created::date, typ;

-- aggregate buys by itemid

INSERT INTO buyaggs_itemid
SELECT
    (unnest(buys_agg(items,shpid, created::date,mktid))).*
FROM buys
WHERE
    created >= laststamp AND created < tillstamp AND status > 0
GROUP BY shpid, created::date;


INSERT INTO buyclrs
(orgid, dt, typ, name, trans, amt, rate, topay)
SELECT
    orgid, dt, TYP_SHP, first(name), sum(trans), sum(amt), RATE_SHP, sum(amt * RATE_SHP / BASE)
FROM buyaggs_typ
WHERE
    typ = 1 AND dt > last AND dt <= till GROUP BY orgid, dt;

INSERT INTO
    buyclrs (orgid, dt, typ, name, trans, amt, rate, topay)
SELECT
    corgid, dt, TYP_MKT, first(name), sum(trans), sum(amt), RATE_MKT, sum(amt * RATE_MKT / BASE)
FROM buyaggs_typ
WHERE
    typ = 1 AND dt > last AND dt <= till GROUP BY corgid, dt;


INSERT INTO gens (typ, till, started, ended)
VALUES
    (1, till, curr, localtimestamp(0));
END
$$;


ALTER FUNCTION public.buysgen() OWNER TO postgres;

--
-- Name: first_agg(anyelement, anyelement); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.first_agg(anyelement, anyelement) RETURNS anyelement
    LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
    AS $_$SELECT $1$_$;


ALTER FUNCTION public.first_agg(anyelement, anyelement) OWNER TO postgres;

--
-- Name: last_agg(anyelement, anyelement); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.last_agg(anyelement, anyelement) RETURNS anyelement
    LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
    AS $_$SELECT $2$_$;


ALTER FUNCTION public.last_agg(anyelement, anyelement) OWNER TO postgres;

--
-- Name: buys_agg(public.buyitem[], integer, date, integer); Type: AGGREGATE; Schema: public; Owner: postgres
--

CREATE AGGREGATE public.buys_agg(items public.buyitem[], orgid integer, dt date, corgid integer) (
    SFUNC = public.buyitem_agg_func,
    STYPE = public.buyaggs_itemid[]
);


ALTER AGGREGATE public.buys_agg(items public.buyitem[], orgid integer, dt date, corgid integer) OWNER TO postgres;

--
-- Name: first(anyelement); Type: AGGREGATE; Schema: public; Owner: postgres
--

CREATE AGGREGATE public.first(anyelement) (
    SFUNC = public.first_agg,
    STYPE = anyelement,
    PARALLEL = safe
);


ALTER AGGREGATE public.first(anyelement) OWNER TO postgres;

--
-- Name: last(anyelement); Type: AGGREGATE; Schema: public; Owner: postgres
--

CREATE AGGREGATE public.last(anyelement) (
    SFUNC = public.last_agg,
    STYPE = anyelement,
    PARALLEL = safe
);


ALTER AGGREGATE public.last(anyelement) OWNER TO postgres;

SET default_tablespace = '';

--
-- Name: entities; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.entities (
                                 typ smallint NOT NULL,
                                 name character varying(12) NOT NULL,
                                 tip character varying(40),
                                 created timestamp(0) without time zone,
                                 creator character varying(12),
                                 adapted timestamp(0) without time zone,
                                 adapter character varying(10),
                                 oked timestamp(0) without time zone,
                                 oker character varying(10),
                                 status smallint DEFAULT 1 NOT NULL
);


ALTER TABLE public.entities OWNER TO postgres;

--
-- Name: _accts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public._accts (
                               no character varying(18),
                               balance money
)
    INHERITS (public.entities);


ALTER TABLE public._accts OWNER TO postgres;

--
-- Name: _asks; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public._asks (
                              acct character varying(20),
                              name character varying(12),
                              amt integer,
                              bal integer,
                              cs uuid,
                              blockcs uuid,
                              stamp timestamp(0) without time zone
);


ALTER TABLE public._asks OWNER TO postgres;

--
-- Name: _ledgs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public._ledgs (
                               seq integer,
                               acct character varying(20),
                               name character varying(12),
                               amt integer,
                               bal integer,
                               cs uuid,
                               blockcs uuid,
                               stamp timestamp(0) without time zone
);


ALTER TABLE public._ledgs OWNER TO postgres;

--
-- Name: assets; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.assets (
                               id integer NOT NULL,
                               orgid integer,
                               rank smallint,
                               cap integer,
                               cern character varying(12),
                               factor double precision,
                               x double precision,
                               y double precision,
                               specs jsonb,
                               icon bytea,
                               pic bytea,
                               m1 bytea,
                               m2 bytea,
                               m3 bytea,
                               m4 bytea
)
    INHERITS (public.entities);


ALTER TABLE public.assets OWNER TO postgres;

--
-- Name: assets_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.assets_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.assets_id_seq OWNER TO postgres;

--
-- Name: assets_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.assets_id_seq OWNED BY public.assets.id;


--
-- Name: assets_vw; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.assets_vw AS
SELECT o.typ,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.oked,
       o.oker,
       o.status,
       o.id,
       o.orgid,
       o.rank,
       o.cap,
       o.cern,
       o.factor,
       o.x,
       o.y,
       o.specs,
       (o.icon IS NOT NULL) AS icon,
       (o.pic IS NOT NULL) AS pic,
       (o.m1 IS NOT NULL) AS m1,
       (o.m2 IS NOT NULL) AS m2,
       (o.m3 IS NOT NULL) AS m3,
       (o.m4 IS NOT NULL) AS m4
FROM public.assets o;


ALTER TABLE public.assets_vw OWNER TO postgres;

SET default_tablespace = sup;

--
-- Name: bookaggs_lotid; Type: TABLE; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE TABLE public.bookaggs_lotid (
                                       orgid integer NOT NULL,
                                       dt date,
                                       typ integer NOT NULL,
                                       corgid integer,
                                       trans integer,
                                       qty integer,
                                       amt money,
                                       created timestamp(0) without time zone,
                                       creator character varying(12)
);


ALTER TABLE public.bookaggs_lotid OWNER TO postgres;

--
-- Name: bookaggs_typ; Type: TABLE; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE TABLE public.bookaggs_typ (
                                     orgid integer NOT NULL,
                                     dt date,
                                     typ integer NOT NULL,
                                     corgid integer,
                                     trans integer,
                                     qty integer,
                                     amt money,
                                     created timestamp(0) without time zone,
                                     creator character varying(12)
);


ALTER TABLE public.bookaggs_typ OWNER TO postgres;

--
-- Name: bookclrs; Type: TABLE; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE TABLE public.bookclrs (
                                 orgid integer NOT NULL,
                                 dt date NOT NULL,
                                 trans integer,
                                 amt money,
                                 rate smallint,
                                 topay money,
                                 pay money
)
    INHERITS (public.entities);


ALTER TABLE public.bookclrs OWNER TO postgres;

--
-- Name: books; Type: TABLE; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE TABLE public.books (
                              id bigint NOT NULL,
                              shpid integer NOT NULL,
                              shpname character varying(12),
                              mktid integer NOT NULL,
                              ctrid integer NOT NULL,
                              srcid integer NOT NULL,
                              srcname character varying(12),
                              lotid integer,
                              unit character varying(4),
                              unitx smallint,
                              price money,
                              off money,
                              qty integer,
                              topay money,
                              pay money,
                              ret integer,
                              refund money,
                              CONSTRAINT typ_chk CHECK (((typ >= 1) AND (typ <= 2)))
)
    INHERITS (public.entities);


ALTER TABLE public.books OWNER TO postgres;

--
-- Name: books_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.books_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.books_id_seq OWNER TO postgres;

--
-- Name: books_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.books_id_seq OWNED BY public.books.id;


SET default_tablespace = rtl;

--
-- Name: buyaggs_typ; Type: TABLE; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE TABLE public.buyaggs_typ (
                                    orgid integer NOT NULL,
                                    dt date NOT NULL,
                                    typ integer NOT NULL,
                                    name character varying(12),
                                    corgid integer,
                                    trans integer,
                                    amt money
);


ALTER TABLE public.buyaggs_typ OWNER TO postgres;

--
-- Name: buyclrs; Type: TABLE; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE TABLE public.buyclrs (
                                id integer NOT NULL,
                                orgid integer NOT NULL,
                                dt date NOT NULL,
                                typ smallint,
                                name character varying(12),
                                trans integer,
                                amt money,
                                rate smallint,
                                topay money,
                                pay money,
                                acct character varying(20),
                                status smallint
);


ALTER TABLE public.buyclrs OWNER TO postgres;

--
-- Name: buyclrs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.buyclrs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.buyclrs_id_seq OWNER TO postgres;

--
-- Name: buyclrs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.buyclrs_id_seq OWNED BY public.buyclrs.id;


--
-- Name: buys; Type: TABLE; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE TABLE public.buys (
                             id bigint NOT NULL,
                             shpid integer NOT NULL,
                             mktid integer NOT NULL,
                             uid integer,
                             uname character varying(12),
                             utel character varying(11),
                             ucom character varying(12),
                             uaddr character varying(30),
                             uim character varying(28),
                             topay money,
                             pay money,
                             ret numeric(6,1),
                             refund money,
                             items public.buyitem[],
                             CONSTRAINT typ_chk CHECK (((typ >= 1) AND (typ <= 3)))
)
    INHERITS (public.entities);


ALTER TABLE public.buys OWNER TO postgres;

--
-- Name: buys_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.buys_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.buys_id_seq OWNER TO postgres;

--
-- Name: buys_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.buys_id_seq OWNED BY public.buys.id;


SET default_tablespace = '';

--
-- Name: cats; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.cats (
                             id smallint NOT NULL,
                             idx smallint,
                             size smallint
)
    INHERITS (public.entities);


ALTER TABLE public.cats OWNER TO postgres;

--
-- Name: gens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.gens (
                             id smallint NOT NULL,
                             typ smallint,
                             till date,
                             started timestamp(0) without time zone,
                             ended timestamp(0) without time zone,
                             opr character varying(12)
);


ALTER TABLE public.gens OWNER TO postgres;

--
-- Name: gens_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gens_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.gens_id_seq OWNER TO postgres;

--
-- Name: gens_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.gens_id_seq OWNED BY public.gens.id;


--
-- Name: items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.items (
                              id integer NOT NULL,
                              shpid integer NOT NULL,
                              lotid integer,
                              catid smallint,
                              unit character varying(4),
                              unitx smallint,
                              price money,
                              off money,
                              minx smallint,
                              stock smallint,
                              avail smallint NOT NULL,
                              ops public.stockop[],
                              icon bytea,
                              pic bytea,
                              CONSTRAINT items_avail_chk CHECK ((avail >= 0))
)
    INHERITS (public.entities);


ALTER TABLE public.items OWNER TO postgres;

--
-- Name: items_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.items_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.items_id_seq OWNER TO postgres;

--
-- Name: items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.items_id_seq OWNED BY public.items.id;


--
-- Name: items_vw; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.items_vw AS
SELECT o.typ,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.oked,
       o.oker,
       o.status,
       o.id,
       o.shpid,
       o.lotid,
       o.catid,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.minx,
       o.stock,
       o.avail,
       o.ops,
       (o.icon IS NOT NULL) AS icon,
       (o.pic IS NOT NULL) AS pic
FROM public.items o;


ALTER TABLE public.items_vw OWNER TO postgres;

--
-- Name: lots; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.lots (
                             id integer NOT NULL,
                             srcid integer,
                             srcname character varying(12),
                             assetid integer,
                             targs integer[],
                             catid smallint,
                             started date,
                             unit character varying(4),
                             unitx smallint,
                             price money,
                             off money,
                             minx smallint,
                             cap integer,
                             stock integer,
                             avail integer,
                             nstart integer,
                             nend integer,
                             ops public.stockop[],
                             icon bytea,
                             pic bytea,
                             m1 bytea,
                             m2 bytea,
                             m3 bytea,
                             m4 bytea,
                             CONSTRAINT lots_typ_chk CHECK (((typ >= 1) AND (typ <= 2) AND (avail >= 0)))
)
    INHERITS (public.entities);


ALTER TABLE public.lots OWNER TO postgres;

--
-- Name: lots_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.lots_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.lots_id_seq OWNER TO postgres;

--
-- Name: lots_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.lots_id_seq OWNED BY public.lots.id;


--
-- Name: lots_vw; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.lots_vw AS
SELECT o.typ,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.oked,
       o.oker,
       o.status,
       o.id,
       o.srcid,
       o.srcname,
       o.assetid,
       o.targs,
       o.catid,
       o.started,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.minx,
       o.cap,
       o.stock,
       o.avail,
       o.nstart,
       o.nend,
       (o.icon IS NOT NULL) AS icon,
       (o.pic IS NOT NULL) AS pic,
       (o.m1 IS NOT NULL) AS m1,
       (o.m2 IS NOT NULL) AS m2,
       (o.m3 IS NOT NULL) AS m3,
       (o.m4 IS NOT NULL) AS m4,
       o.ops
FROM public.lots o;


ALTER TABLE public.lots_vw OWNER TO postgres;

--
-- Name: orgs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.orgs (
                             id integer NOT NULL,
                             prtid integer,
                             ctrid integer,
                             ext character varying(12),
                             legal character varying(20),
                             regid smallint,
                             addr character varying(30),
                             x double precision,
                             y double precision,
                             tel character varying(11),
                             trust boolean,
                             link character varying(30),
                             specs jsonb,
                             icon bytea,
                             pic bytea,
                             m1 bytea,
                             m2 bytea,
                             m3 bytea,
                             m4 bytea
)
    INHERITS (public.entities);


ALTER TABLE public.orgs OWNER TO postgres;

--
-- Name: orgs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.orgs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.orgs_id_seq OWNER TO postgres;

--
-- Name: orgs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.orgs_id_seq OWNED BY public.orgs.id;


--
-- Name: orgs_vw; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.orgs_vw AS
SELECT o.typ,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.oker,
       o.oked,
       o.status,
       o.id,
       o.prtid,
       o.ctrid,
       o.ext,
       o.legal,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.trust,
       o.link,
       o.specs,
       (o.icon IS NOT NULL) AS icon,
       (o.pic IS NOT NULL) AS pic,
       (o.m1 IS NOT NULL) AS m1,
       (o.m2 IS NOT NULL) AS m2,
       (o.m3 IS NOT NULL) AS m3,
       (o.m4 IS NOT NULL) AS m4
FROM public.orgs o;


ALTER TABLE public.orgs_vw OWNER TO postgres;

--
-- Name: regs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.regs (
                             id smallint NOT NULL,
                             idx smallint,
                             num smallint
)
    INHERITS (public.entities);


ALTER TABLE public.regs OWNER TO postgres;

--
-- Name: tests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tests (
                              id integer NOT NULL,
                              orgid integer,
                              level integer
)
    INHERITS (public.entities);


ALTER TABLE public.tests OWNER TO postgres;

--
-- Name: tests_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tests_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tests_id_seq OWNER TO postgres;

--
-- Name: tests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tests_id_seq OWNED BY public.tests.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
                              id integer NOT NULL,
                              tel character varying(11) NOT NULL,
                              addr character varying(50),
                              im character varying(28),
                              credential character varying(32),
                              admly smallint DEFAULT 0 NOT NULL,
                              srcid smallint,
                              srcly smallint DEFAULT 0 NOT NULL,
                              shpid integer,
                              shply smallint,
                              vip integer[],
                              refer integer,
                              icon bytea,
                              CONSTRAINT users_vip_chk CHECK ((array_length(vip, 1) <= 4))
)
    INHERITS (public.entities);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.users_id_seq OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: users_vw; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.users_vw AS
SELECT o.typ,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.oked,
       o.oker,
       o.status,
       o.id,
       o.tel,
       o.addr,
       o.im,
       o.credential,
       o.admly,
       o.srcid,
       o.srcly,
       o.shpid,
       o.shply,
       o.vip,
       o.refer,
       (o.icon IS NOT NULL) AS icon
FROM public.users o;


ALTER TABLE public.users_vw OWNER TO postgres;

--
-- Name: _accts status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public._accts ALTER COLUMN status SET DEFAULT 1;


--
-- Name: assets status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.assets ALTER COLUMN status SET DEFAULT 1;


--
-- Name: assets id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.assets ALTER COLUMN id SET DEFAULT nextval('public.assets_id_seq'::regclass);


--
-- Name: bookclrs status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.bookclrs ALTER COLUMN status SET DEFAULT 1;


--
-- Name: books status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books ALTER COLUMN status SET DEFAULT '-1'::integer;


--
-- Name: books id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books ALTER COLUMN id SET DEFAULT nextval('public.books_id_seq'::regclass);


--
-- Name: buyclrs id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buyclrs ALTER COLUMN id SET DEFAULT nextval('public.buyclrs_id_seq'::regclass);


--
-- Name: buys status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buys ALTER COLUMN status SET DEFAULT 1;


--
-- Name: buys id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buys ALTER COLUMN id SET DEFAULT nextval('public.buys_id_seq'::regclass);


--
-- Name: cats status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cats ALTER COLUMN status SET DEFAULT 1;


--
-- Name: gens id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gens ALTER COLUMN id SET DEFAULT nextval('public.gens_id_seq'::regclass);


--
-- Name: items status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.items ALTER COLUMN status SET DEFAULT 1;


--
-- Name: items id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.items ALTER COLUMN id SET DEFAULT nextval('public.items_id_seq'::regclass);


--
-- Name: lots status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lots ALTER COLUMN status SET DEFAULT 1;


--
-- Name: lots id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lots ALTER COLUMN id SET DEFAULT nextval('public.lots_id_seq'::regclass);


--
-- Name: orgs status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orgs ALTER COLUMN status SET DEFAULT 1;


--
-- Name: orgs id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orgs ALTER COLUMN id SET DEFAULT nextval('public.orgs_id_seq'::regclass);


--
-- Name: regs status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.regs ALTER COLUMN status SET DEFAULT 1;


--
-- Name: tests status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tests ALTER COLUMN status SET DEFAULT 1;


--
-- Name: tests id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tests ALTER COLUMN id SET DEFAULT nextval('public.tests_id_seq'::regclass);


--
-- Name: users status; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN status SET DEFAULT 1;


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: assets_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.assets_id_seq', 4, true);


--
-- Name: books_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.books_id_seq', 171, true);


--
-- Name: buyclrs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.buyclrs_id_seq', 57, true);


--
-- Name: buys_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.buys_id_seq', 75, true);


--
-- Name: gens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.gens_id_seq', 2, true);


--
-- Name: items_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.items_id_seq', 56, true);


--
-- Name: lots_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.lots_id_seq', 136, true);


--
-- Name: orgs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.orgs_id_seq', 38, true);


--
-- Name: tests_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.tests_id_seq', 1, false);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 63, true);


--
-- Name: assets assets_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.assets
    ADD CONSTRAINT assets_pk PRIMARY KEY (id);


SET default_tablespace = sup;

--
-- Name: books books_pk; Type: CONSTRAINT; Schema: public; Owner: postgres; Tablespace: sup
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_pk PRIMARY KEY (id);


SET default_tablespace = '';

--
-- Name: buyaggs_itemid buyaggs_itemid_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buyaggs_itemid
    ADD CONSTRAINT buyaggs_itemid_pk PRIMARY KEY (orgid, dt, typ);


--
-- Name: buyaggs_typ buyaggs_typ_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buyaggs_typ
    ADD CONSTRAINT buyaggs_typ_pk PRIMARY KEY (orgid, dt, typ);


--
-- Name: buyclrs buyclrs_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buyclrs
    ADD CONSTRAINT buyclrs_pk PRIMARY KEY (id);


SET default_tablespace = rtl;

--
-- Name: buys buys_pk; Type: CONSTRAINT; Schema: public; Owner: postgres; Tablespace: rtl
--

ALTER TABLE ONLY public.buys
    ADD CONSTRAINT buys_pk PRIMARY KEY (id);


SET default_tablespace = '';

--
-- Name: cats cats_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.cats
    ADD CONSTRAINT cats_pk PRIMARY KEY (id);


--
-- Name: gens gens_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gens
    ADD CONSTRAINT gens_pk PRIMARY KEY (id);


--
-- Name: items items_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_pk PRIMARY KEY (id);


--
-- Name: lots lots_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lots
    ADD CONSTRAINT lots_pk PRIMARY KEY (id);


--
-- Name: orgs orgs_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orgs
    ADD CONSTRAINT orgs_pk PRIMARY KEY (id);


--
-- Name: regs regs_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.regs
    ADD CONSTRAINT regs_pk PRIMARY KEY (id);


--
-- Name: tests tests_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tests
    ADD CONSTRAINT tests_pk PRIMARY KEY (id);


--
-- Name: users users_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pk PRIMARY KEY (id);


--
-- Name: assets_orgidstatus_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX assets_orgidstatus_idx ON public.assets USING btree (orgid, status);


SET default_tablespace = sup;

--
-- Name: books_ctridstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE INDEX books_ctridstatus_idx ON public.books USING btree (ctrid, status);


--
-- Name: books_mktidstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE INDEX books_mktidstatus_idx ON public.books USING btree (mktid, status);


--
-- Name: books_shpidstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE INDEX books_shpidstatus_idx ON public.books USING btree (shpid, status);


--
-- Name: books_single_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE UNIQUE INDEX books_single_idx ON public.books USING btree (shpid, status) WHERE (status = '-1'::integer);


--
-- Name: books_srcidstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: sup
--

CREATE INDEX books_srcidstatus_idx ON public.books USING btree (srcid, status);


SET default_tablespace = rtl;

--
-- Name: buys_mktidstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE INDEX buys_mktidstatus_idx ON public.buys USING btree (mktid, status);


--
-- Name: buys_shpidstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE INDEX buys_shpidstatus_idx ON public.buys USING btree (shpid, status);


--
-- Name: buys_single_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE UNIQUE INDEX buys_single_idx ON public.buys USING btree (shpid, typ, status) WHERE ((typ = 1) AND (status = '-1'::integer));


--
-- Name: buys_uidstatus_idx; Type: INDEX; Schema: public; Owner: postgres; Tablespace: rtl
--

CREATE INDEX buys_uidstatus_idx ON public.buys USING btree (uid, status);


SET default_tablespace = '';

--
-- Name: items_catid_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX items_catid_idx ON public.items USING btree (catid);


--
-- Name: lots_catid_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX lots_catid_idx ON public.lots USING btree (catid);


--
-- Name: lots_nend_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX lots_nend_idx ON public.lots USING btree (nend);


--
-- Name: lots_srcidstatus_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX lots_srcidstatus_idx ON public.lots USING btree (srcid, status);


--
-- Name: users_admly_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX users_admly_idx ON public.users USING btree (admly) WHERE (admly > 0);


--
-- Name: users_im_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_im_idx ON public.users USING btree (im);


--
-- Name: users_shpid_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX users_shpid_idx ON public.users USING btree (shpid) WHERE (shpid > 0);


--
-- Name: users_srcid_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX users_srcid_idx ON public.users USING btree (srcid) WHERE (srcid > 0);


--
-- Name: users_tel_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_tel_idx ON public.users USING btree (tel);


--
-- Name: users_vip_idx; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX users_vip_idx ON public.users USING gin (vip);


--
-- Name: buys buys_trig; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER buys_trig AFTER INSERT OR UPDATE OF status ON public.buys FOR EACH ROW EXECUTE PROCEDURE public.buys_trig_func();


--
-- Name: books books_ctrid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_ctrid_fk FOREIGN KEY (ctrid) REFERENCES public.orgs(id);


--
-- Name: books books_lotid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_lotid_fk FOREIGN KEY (lotid) REFERENCES public.lots(id);


--
-- Name: books books_mktid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_mktid_fk FOREIGN KEY (mktid) REFERENCES public.orgs(id);


--
-- Name: books books_shpid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_shpid_fk FOREIGN KEY (shpid) REFERENCES public.orgs(id);


--
-- Name: books books_srcid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.books
    ADD CONSTRAINT books_srcid_fk FOREIGN KEY (srcid) REFERENCES public.orgs(id);


--
-- Name: buys buys_mkt_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buys
    ADD CONSTRAINT buys_mkt_fk FOREIGN KEY (mktid) REFERENCES public.orgs(id);


--
-- Name: buys buys_shpid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buys
    ADD CONSTRAINT buys_shpid_fk FOREIGN KEY (shpid) REFERENCES public.orgs(id);


--
-- Name: buys buys_uid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.buys
    ADD CONSTRAINT buys_uid_fk FOREIGN KEY (uid) REFERENCES public.users(id);


--
-- Name: items items_catid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_catid_fk FOREIGN KEY (catid) REFERENCES public.cats(id);


--
-- Name: items items_lotid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_lotid_fk FOREIGN KEY (lotid) REFERENCES public.lots(id);


--
-- Name: items items_shpid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_shpid_fk FOREIGN KEY (shpid) REFERENCES public.orgs(id);


--
-- Name: lots lots_catsid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.lots
    ADD CONSTRAINT lots_catsid_fk FOREIGN KEY (catid) REFERENCES public.cats(id);


--
-- Name: orgs orgs_ctrid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orgs
    ADD CONSTRAINT orgs_ctrid_fk FOREIGN KEY (ctrid) REFERENCES public.orgs(id);


--
-- Name: orgs orgs_prtid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orgs
    ADD CONSTRAINT orgs_prtid_fk FOREIGN KEY (prtid) REFERENCES public.orgs(id);


--
-- Name: orgs orgs_regid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.orgs
    ADD CONSTRAINT orgs_regid_fk FOREIGN KEY (regid) REFERENCES public.regs(id);


--
-- Name: tests tests_orgid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tests
    ADD CONSTRAINT tests_orgid_fk FOREIGN KEY (orgid) REFERENCES public.orgs(id);


--
-- Name: users users_refer_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_refer_fk FOREIGN KEY (refer) REFERENCES public.users(id);


--
-- Name: users users_shpid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_shpid_fk FOREIGN KEY (shpid) REFERENCES public.orgs(id);


--
-- Name: users users_srcid_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_srcid_fk FOREIGN KEY (srcid) REFERENCES public.orgs(id);


--
-- PostgreSQL database dump complete
--

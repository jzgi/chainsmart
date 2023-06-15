create sequence evals_id_seq;

alter sequence evals_id_seq owner to postgres;

create sequence purs_id_seq;

alter sequence purs_id_seq owner to postgres;

create sequence fabs_id_seq;

alter sequence fabs_id_seq owner to postgres;

create type stockop as
(
    dt    timestamp(0),
    qty   integer,
    avail integer,
    tip   varchar(10),
    by    varchar(10)
);

alter type stockop owner to postgres;

create type buyitem as
(
    itemid integer,
    lotid  integer,
    name   varchar(12),
    unit   varchar(4),
    unitx  smallint,
    price  money,
    "off"  money,
    qty    numeric(6, 1)
);

alter type buyitem owner to postgres;

create table entities
(
    typ     smallint           not null,
    name    varchar(12)        not null,
    tip     varchar(40),
    created timestamp(0),
    creator varchar(12),
    adapted timestamp(0),
    adapter varchar(10),
    oked    timestamp(0),
    oker    varchar(10),
    status  smallint default 1 not null
);

alter table entities
    owner to postgres;

create table cats
(
    id   smallint not null
        constraint cats_pk
            primary key,
    idx  smallint,
    size smallint
)
    inherits (entities);

alter table cats
    owner to postgres;

create table regs
(
    id  smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (entities);

alter table regs
    owner to postgres;

create table orgs
(
    id      serial
        constraint orgs_pk
            primary key,
    prtid   integer
        constraint orgs_prtid_fk
            references orgs,
    ctrid   integer
        constraint orgs_ctrid_fk
            references orgs,
    ext     varchar(12),
    legal   varchar(20),
    regid   smallint
        constraint orgs_regid_fk
            references regs,
    addr    varchar(30),
    x       double precision,
    y       double precision,
    tel     varchar(11),
    trust   boolean,
    link    varchar(30),
    credit  smallint,
    reserve integer,
    specs   jsonb,
    icon    bytea,
    pic     bytea,
    m1      bytea,
    m2      bytea,
    m3      bytea,
    m4      bytea
)
    inherits (entities);

alter table orgs
    owner to postgres;

create table users
(
    id         serial
        constraint users_pk
            primary key,
    tel        varchar(11)        not null,
    addr       varchar(50),
    im         varchar(28),
    credential varchar(32),
    admly      smallint default 0 not null,
    supid      smallint
        constraint users_supid_fk
            references orgs,
    suply      smallint default 0 not null,
    rtlid      integer
        constraint users_rtlid_fk
            references orgs,
    rtlly      smallint,
    vip        integer[],
    refer      integer
        constraint users_refer_fk
            references users,
    icon       bytea
)
    inherits (entities);

alter table users
    owner to postgres;

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create unique index users_tel_idx
    on users (tel);

create index users_rtlid_idx
    on users (rtlid)
    where (rtlid > 0);

create index users_supid_idx
    on users (supid)
    where (supid > 0);

create index users_vip_idx
    on users using gin (vip);

create table evals
(
    id    integer default nextval('tests_id_seq'::regclass) not null
        constraint evals_pk
            primary key,
    orgid integer
        constraint evals_orgid_fk
            references orgs,
    level integer
)
    inherits (entities);

alter table evals
    owner to postgres;

alter sequence evals_id_seq owned by evals.id;

create table lots
(
    id      serial
        constraint lots_pk
            primary key,
    orgid   integer,
    orgname varchar(12),
    fabid   integer,
    targs   integer[],
    catid   smallint
        constraint lots_catsid_fk
            references cats,
    started date,
    unit    varchar(4),
    unitx   smallint,
    price   money,
    "off"   money,
    cap     integer,
    stock   integer,
    avail   integer,
    nstart  integer,
    nend    integer,
    minx    smallint,
    maxx    smallint,
    flashx  smallint,
    ops     stockop[],
    icon    bytea,
    pic     bytea,
    m1      bytea,
    m2      bytea,
    m3      bytea,
    m4      bytea,
    constraint lots_chk
        check ((typ >= 1) AND (typ <= 2) AND (avail >= 0) AND (unitx >= 1))
)
    inherits (entities);

alter table lots
    owner to postgres;

create table purs
(
    id      bigint default nextval('books_id_seq'::regclass) not null
        constraint purs_pk
            primary key,
    rtlid   integer                                          not null
        constraint purs_rtlid_fk
            references orgs,
    rtlname varchar(12),
    mktid   integer                                          not null
        constraint purs_mktid_fk
            references orgs,
    ctrid   integer                                          not null
        constraint purs_ctrid_fk
            references orgs,
    supid   integer                                          not null
        constraint purs_supid_fk
            references orgs,
    supname varchar(12),
    lotid   integer
        constraint purs_lotid_fk
            references lots,
    unit    varchar(4),
    unitx   smallint,
    price   money,
    "off"   money,
    qty     integer,
    topay   money,
    pay     money,
    ret     integer,
    refund  money,
    constraint typ_chk
        check ((typ >= 1) AND (typ <= 2))
)
     inherits
(
    entities
)tablespace sup;

alter table purs
    owner to postgres;

alter sequence purs_id_seq owned by purs.id;

create unique index purs_uidx
    on purs (rtlid, status)
    where (status = '-1'::integer) tablespace sup;

create index purs_ctridstatus_idx
    on purs (ctrid, status)
    tablespace sup;

create index purs_rtlidstatus_idx
    on purs (rtlid, status)
    tablespace sup;

create index purs_supidstatus_idx
    on purs (supid, status)
    tablespace sup;

create index purs_mktidstatus_idx
    on purs (mktid, status)
    tablespace sup;

create index lots_srcidstatus_idx
    on lots (orgid, status);

create index lots_catid_idx
    on lots (catid);

create index lots_nend_idx
    on lots (nend);

create table fabs
(
    id     integer default nextval('assets_id_seq'::regclass) not null
        constraint fabs_pk
            primary key,
    orgid  integer,
    rank   smallint,
    remark varchar(100),
    co2ekg money,
    co2ep  money,
    x      double precision,
    y      double precision,
    specs  jsonb,
    icon   bytea,
    pic    bytea,
    m1     bytea,
    m2     bytea,
    m3     bytea,
    m4     bytea
)
    inherits (entities);

alter table fabs
    owner to postgres;

alter sequence fabs_id_seq owned by fabs.id;

create index fabs_orgidstatus_idx
    on fabs (orgid, status);

create table buys
(
    id     bigserial
        constraint buys_pk
            primary key,
    rtlid  integer not null
        constraint buys_rtlid_fk
            references orgs,
    mktid  integer not null
        constraint buys_mkt_fk
            references orgs,
    uid    integer
        constraint buys_uid_fk
            references users,
    uname  varchar(12),
    utel   varchar(11),
    ucom   varchar(12),
    uaddr  varchar(30),
    uim    varchar(28),
    topay  money,
    pay    money,
    ret    numeric(6, 1),
    refund money,
    items  buyitem[],
    constraint buys_chk
        check ((typ >= 1) AND (typ <= 3))
)
   inherits
(
    entities
) tablespace rtl ;

alter table buys
    owner to postgres;

create index buys_uidstatus_idx
    on buys (uid, status)
    tablespace rtl;

create index buys_rtlidstatus_idx
    on buys (rtlid, status)
    tablespace rtl;

create index buys_mktidstatus_idx
    on buys (mktid, status)
    tablespace rtl;

create unique index buys_uidx
    on buys (uid, typ, status)
    where ((typ = 1) AND (status = '-1'::smallint)) tablespace rtl;

create table items
(
    id     serial
        constraint items_pk
            primary key,
    orgid  integer            not null
        constraint items_rtlid_fk
            references orgs,
    lotid  integer
        constraint items_lotid_fk
            references lots,
    catid  smallint
        constraint items_catid_fk
            references cats,
    unit   varchar(4),
    unitx  smallint,
    price  money,
    "off"  money,
    maxx   smallint,
    stock  smallint default 0 not null,
    avail  smallint default 0 not null
        constraint items_avail_chk
            check (avail >= 0),
    flashx smallint default 0 not null,
    ops    stockop[],
    icon   bytea,
    pic    bytea
)
    inherits (entities);

alter table items
    owner to postgres;

create index items_catid_idx
    on items (catid);

create table buyaggs_typ
(
    orgid  integer not null,
    dt     date    not null,
    typ    integer not null,
    name   varchar(12),
    corgid integer,
    trans  integer,
    amt    money,
    constraint buyaggs_typ_pk
        primary key (orgid, dt, typ)
)
    tablespace rtl;

alter table buyaggs_typ
    owner to postgres;

create table buyaggs_itemid
(
    orgid  integer not null,
    dt     date    not null,
    typ    integer not null,
    name   varchar(12),
    corgid integer,
    trans  integer,
    amt    money,
    constraint buyaggs_itemid_pk
        primary key (orgid, dt, typ)
)
    tablespace rtl;

alter table buyaggs_itemid
    owner to postgres;

create table gens
(
    id      smallserial
        constraint gens_pk
            primary key,
    typ     smallint,
    till    date,
    started timestamp(0),
    ended   timestamp(0),
    opr     varchar(12)
);

alter table gens
    owner to postgres;

create table purclrs
(
    orgid integer not null,
    dt    date    not null,
    trans integer,
    amt   money,
    rate  smallint,
    topay money,
    pay   money
)
   inherits
(
    entities
) tablespace sup ;

alter table purclrs
    owner to postgres;

create table buyclrs
(
    id     serial
        constraint buyclrs_pk
            primary key,
    orgid  integer not null,
    dt     date    not null,
    typ    smallint,
    name   varchar(12),
    trans  integer,
    amt    money,
    rate   smallint,
    topay  money,
    pay    money,
    acct   varchar(20),
    status smallint
)
    tablespace rtl;

alter table buyclrs
    owner to postgres;

create table puraggs_typ
(
    orgid   integer not null,
    dt      date,
    typ     integer not null,
    name    varchar(12),
    corgid  integer,
    trans   integer,
    qty     integer,
    amt     money,
    created timestamp(0),
    creator varchar(12)
)
    tablespace sup;

alter table puraggs_typ
    owner to postgres;

create table puraggs_lotid
(
    orgid   integer not null,
    dt      date,
    typ     integer not null,
    name    varchar(12),
    corgid  integer,
    trans   integer,
    qty     integer,
    amt     money,
    created timestamp(0),
    creator varchar(12)
)
    tablespace sup;

alter table puraggs_lotid
    owner to postgres;

create table vans
(
    id     serial,
    orgid  integer,
    rank   smallint,
    remark varchar(100),
    co2ekg money,
    co2ep  money,
    x      double precision,
    y      double precision,
    specs  jsonb,
    icon   bytea,
    pic    bytea,
    m1     bytea,
    m2     bytea,
    m3     bytea,
    m4     bytea
)
    inherits (entities);

alter table vans
    owner to postgres;

create view orgs_vw
            (typ, name, tip, created, creator, adapted, adapter, oker, oked, status, id, prtid, ctrid, ext, legal,
             regid, addr, x, y, tel, trust, link, credit, specs, icon, pic, m1, m2, m3, m4)
as
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
       o.credit,
       o.specs,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM orgs o;

alter table orgs_vw
    owner to postgres;

create view users_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, tel, addr, im, credential,
             admly, supid, suply, rtlid, rtlly, vip, refer, icon)
as
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
       o.supid,
       o.suply,
       o.rtlid,
       o.rtlly,
       o.vip,
       o.refer,
       o.icon IS NOT NULL AS icon
FROM users o;

alter table users_vw
    owner to postgres;

create view fabs_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, rank, remark, co2ekg,
             co2ep, x, y, specs, icon, pic, m1, m2, m3, m4)
as
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
       o.remark,
       o.co2ekg,
       o.co2ep,
       o.x,
       o.y,
       o.specs,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM fabs o;

alter table fabs_vw
    owner to postgres;

create view lots_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, orgname, fabid, targs,
             catid, started, unit, unitx, price, "off", cap, stock, avail, nstart, nend, minx, maxx, flashx, icon, pic,
             m1, m2, m3, m4, ops)
as
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
       o.orgname,
       o.fabid,
       o.targs,
       o.catid,
       o.started,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.capx,
       o.stock,
       o.avail,
       o.nstart,
       o.nend,
       o.minx,
       o.maxx,
       o.flashx,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4,
       o.ops
FROM lots o;

alter table lots_vw
    owner to postgres;

create view items_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, lotid, catid, unit,
             unitx, price, "off", maxx, stock, avail, flashx, ops, icon, pic)
as
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
       o.lotid,
       o.catid,
       o.unit,
       o.step,
       o.price,
       o.off,
       o.max,
       o.stock,
       o.avail,
       o.flash,
       o.ops,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic
FROM items o;

alter table items_vw
    owner to postgres;

create function first_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as
$$SELECT $1$$;

alter function first_agg(anyelement, anyelement) owner to postgres;

create function last_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as
$$SELECT $2$$;

alter function last_agg(anyelement, anyelement) owner to postgres;

create function pursgen(till date, opr character varying) returns void
    language plpgsql
as
$$
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

alter function pursgen(date, varchar) owner to postgres;

create function buys_trig_func() returns trigger
    language plpgsql
as
$$
DECLARE
    itm buyitem;
BEGIN
    -- update avail values
    IF (TG_OP = 'INSERT' AND NEW.status = 4) THEN

        FOREACH itm IN ARRAY NEW.items LOOP -- oked
        UPDATE items SET avail = avail - itm.qty, stock = stock - itm.qty, flash = CASE WHEN flash > 0 THEN flash - (itm.qty / itm.unitw) ELSE flash END WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 1 AND OLD.status < 1) THEN -- paid

        FOREACH itm IN ARRAY NEW.items LOOP
                UPDATE items SET avail = avail - itm.qty, flash = CASE WHEN flash > 0 THEN flash - (itm.qty / itm.unitw) ELSE flash END WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 4 AND OLD.status < 4) THEN -- sent

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

alter function buys_trig_func() owner to postgres;

create trigger buys_trig
    after insert or update
        of status
    on buys
    for each row
execute procedure buys_trig_func();

create function buyitem_agg_func(ret buyaggs_itemid[], items buyitem[], orgid integer, dt date, corgid integer) returns buyaggs_itemid[]
    language plpgsql
as
$$
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

alter function buyitem_agg_func(buyaggs_itemid[], buyitem[], integer, date, integer) owner to postgres;

create function buysgen() returns void
    language plpgsql
as
$$
DECLARE
    curr timestamp(0) = localtimestamp(0);
    till date;
    tillstamp timestamp(0);
    last date;
    laststamp timestamp(0);

    TYP_PLAT constant int = 1;
    TYP_RTL constant int = 2;
    TYP_MKT constant int = 3;

    BASE constant int = 100;
    RATE_PLAT constant int = 1;
    RATE_RTL constant int = 97;
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
    SELECT rtlid,
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
        rtlid, created::date, typ;

    -- aggregate buys by itemid

    INSERT INTO buyaggs_itemid
    SELECT
        (unnest(buys_agg(items,rtlid, created::date,mktid))).*
    FROM buys
    WHERE
            created >= laststamp AND created < tillstamp AND status > 0
    GROUP BY rtlid, created::date;


    INSERT INTO buyclrs
    (orgid, dt, typ, name, trans, amt, rate, topay)
    SELECT
        orgid, dt, TYP_RTL, first(name), sum(trans), sum(amt), RATE_RTL, sum(amt * RATE_RTL / BASE)
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

alter function buysgen() owner to postgres;

create aggregate first(anyelement) (
    sfunc = first_agg,
    stype = anyelement,
    parallel = safe
    );

alter aggregate first(anyelement) owner to postgres;

create aggregate last(anyelement) (
    sfunc = last_agg,
    stype = anyelement,
    parallel = safe
    );

alter aggregate last(anyelement) owner to postgres;

create aggregate buys_agg(items buyitem[], orgid integer, dt date, corgid integer) (
    sfunc = buyitem_agg_func,
    stype = buyaggs_itemid[]
    );

alter aggregate buys_agg(items buyitem[], orgid integer, dt date, corgid integer) owner to postgres;


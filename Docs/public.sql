create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type buydetail as
(
    wareid integer,
    itemid integer,
    name varchar(12),
    unit varchar(4),
    unitx numeric(6,1),
    price money,
    "off" money,
    qty numeric(6,1)
);

alter type buydetail owner to postgres;

create type wareop as
(
    dt timestamp(0),
    typ smallint,
    qty numeric(6,1),
    avail numeric(6,1),
    by varchar(12)
);

alter type wareop owner to postgres;

create table entities
(
    typ smallint not null,
    state smallint,
    name varchar(12) not null,
    tip varchar(40),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10),
    oked timestamp(0),
    oker varchar(10),
    status smallint default 1 not null
);

alter table entities owner to postgres;

create table _ldgs_
(
    seq integer,
    acct varchar(20),
    name varchar(12),
    amt integer,
    bal integer,
    cs uuid,
    blockcs uuid,
    stamp timestamp(0)
);

alter table _ldgs_ owner to postgres;

create table _peerldgs_
(
    peerid smallint
)
    inherits (_ldgs_);

alter table _peerldgs_ owner to postgres;

create table _peers_
(
    id smallint not null
        constraint peers__pk
            primary key,
    weburl varchar(50),
    secret varchar(16)
)
    inherits (entities);

alter table _peers_ owner to postgres;

create table _accts_
(
    no varchar(20),
    v integer
)
    inherits (entities);

alter table _accts_ owner to postgres;

create table cats
(
    idx smallint,
    size smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (entities);

alter table cats owner to postgres;

create table regs
(
    id smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (entities);

alter table regs owner to postgres;

create table orgs
(
    id serial not null
        constraint orgs_pk
            primary key,
    prtid integer
        constraint orgs_prtid_fk
            references orgs,
    ctrid integer
        constraint orgs_ctrid_fk
            references orgs,
    ext varchar(12),
    legal varchar(20),
    regid smallint
        constraint orgs_regid_fk
            references regs,
    addr varchar(30),
    x double precision,
    y double precision,
    tel varchar(11),
    trust boolean,
    link varchar(30),
    specs jsonb,
    icon bytea,
    pic bytea,
    m1 bytea,
    m2 bytea,
    m3 bytea,
    m4 bytea
)
    inherits (entities);

alter table orgs owner to postgres;

create table users
(
    id serial not null
        constraint users_pk
            primary key,
    tel varchar(11) not null,
    addr varchar(50),
    im varchar(28),
    credential varchar(32),
    admly smallint default 0 not null,
    srcid smallint
        constraint users_srcid_fk
            references orgs,
    srcly smallint default 0 not null,
    shpid integer
        constraint users_shpid_fk
            references orgs,
    shply smallint,
    vip integer[]
        constraint users_vip_chk
            check (array_length(vip, 1) <= 4),
    icon bytea
)
    inherits (entities);

alter table users owner to postgres;

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create unique index users_tel_idx
    on users (tel);

create index users_shpid_idx
    on users (shpid)
    where (shpid > 0);

create index users_srcid_idx
    on users (srcid)
    where (srcid > 0);

create index users_vip_idx
    on users (vip);

create table tests
(
    id serial not null
        constraint tests_pk
            primary key,
    orgid integer
        constraint tests_orgid_fk
            references orgs,
    level integer
)
    inherits (entities);

alter table tests owner to postgres;

create table items
(
    id serial not null
        constraint items_pk
            primary key,
    srcid integer
        constraint items_srcid_fk
            references orgs,
    origin varchar(12),
    store smallint,
    duration smallint,
    specs jsonb,
    icon bytea,
    pic bytea,
    m1 bytea,
    m2 bytea,
    m3 bytea,
    m4 bytea,
    m5 bytea,
    m6 bytea,
    constraint items_typ_fk
        foreign key (typ) references cats
)
    inherits (entities);

alter table items owner to postgres;

create table wares
(
    id serial not null
        constraint wares_pk
            primary key,
    shpid integer not null
        constraint wares_shpid_fk
            references orgs,
    itemid integer
        constraint wares_itemid_fk
            references items,
    unit varchar(4),
    unitx numeric(6,1),
    price money,
    "off" money,
    min smallint,
    max smallint,
    avail numeric(6,1) default 0.0 not null,
    icon bytea,
    pic bytea,
    ops wareop[]
)
    inherits (entities);

alter table wares owner to postgres;

create index items_srcidstatus_idx
    on items (srcid, status);

create table lots
(
    id serial not null
        constraint lots_pk
            primary key,
    srcid integer,
    srcname varchar(12),
    zonid integer not null
        constraint lots_zonid_fk
            references orgs,
    targs integer[],
    dated date,
    term smallint,
    itemid integer
        constraint lots_itemid_fk
            references items,
    unit varchar(4),
    unitx numeric(6,1),
    price money,
    "off" money,
    min integer,
    step integer,
    max integer,
    cap integer,
    avail integer,
    nstart integer,
    nend integer,
    m1 bytea,
    m2 bytea,
    m3 bytea,
    m4 bytea,
    constraint lots_typ_fk
        foreign key (typ) references cats
)
    inherits (entities);

alter table lots owner to postgres;

create table books
(
    id bigserial not null
        constraint books_pk
            primary key,
    shpid integer not null
        constraint books_shpid_fk
            references orgs,
    shpname varchar(12),
    mktid integer not null
        constraint books_mktid_fk
            references orgs,
    ctrid integer not null
        constraint books_ctrid_fk
            references orgs,
    srcid integer not null
        constraint books_srcid_fk
            references orgs,
    srcname varchar(12),
    zonid integer not null
        constraint books_zonly_fk
            references orgs,
    itemid integer
        constraint books_itemid_fk
            references items,
    lotid integer
        constraint books_lotid_fk
            references lots,
    unit varchar(4),
    unitx numeric(6,1),
    price money,
    "off" money,
    qty integer,
    topay money,
    pay money,
    ret numeric(6,1),
    refund money
)
    inherits (entities);

alter table books owner to postgres;

create unique index books_single_idx
    on books (shpid, status)
    where (status = 0);

create index lots_nend_idx
    on lots (nend);

create index lots_srcidstatus_idx
    on lots (srcid, status);

create table buys
(
    id bigserial not null
        constraint buys_pk
            primary key,
    shpid integer not null
        constraint buys_shpid_fk
            references orgs,
    mktid integer not null
        constraint buys_mkt_fk
            references orgs,
    uid integer not null
        constraint buys_uid_fk
            references users,
    uname varchar(12),
    utel varchar(11),
    uaddr varchar(30),
    uim varchar(28),
    details buydetail[],
    topay money,
    pay money,
    refund money
)
    inherits (entities);

alter table buys owner to postgres;

create unique index buys_single_idx
    on buys (shpid, status)
    where (status = 0);

create index buys_uidstatus_idx
    on buys (uid, status);

create index buys_shpidstatus_idx
    on buys (shpid, status);

create table global
(
    buysgen date,
    booksgen date,
    pk boolean not null
        constraint global_pk
            primary key
);

alter table global owner to postgres;

create table bookaggs
(
    typ smallint not null,
    orgid integer not null,
    dt date,
    acct integer not null,
    prtid integer,
    trans integer,
    qty numeric(8,1),
    amt money,
    created timestamp(0),
    creator varchar(12)
);

alter table bookaggs owner to postgres;

create index rpts_main_idx
    on bookaggs (typ, orgid, dt);

create table bookclrs
(
    id serial not null
        constraint clears_pk
            primary key,
    orgid integer not null,
    till date,
    trans integer,
    amt money,
    rate smallint,
    topay money,
    pay money
)
    inherits (entities);

alter table bookclrs owner to postgres;

create table buyaggs
(
    typ smallint not null,
    orgid integer not null,
    dt date,
    acct integer not null,
    prtid integer,
    trans integer,
    qty numeric(8,1),
    amt money,
    created timestamp(0),
    creator varchar(12)
);

alter table buyaggs owner to postgres;

create table buyclrs
(
    id serial not null
        constraint buyclrs_pk
            primary key,
    orgid integer not null,
    till date,
    trans integer,
    amt money,
    rate smallint,
    topay money,
    pay money
)
    inherits (entities);

alter table buyclrs owner to postgres;

create view items_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, srcid, origin, store, duration, specs, icon, pic, m1, m2, m3, m4, m5, m6) as
SELECT o.typ,
       o.state,
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
       o.origin,
       o.store,
       o.duration,
       o.specs,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4,
       o.m5 IS NOT NULL   AS m5,
       o.m6 IS NOT NULL   AS m6
FROM items o;

alter table items_vw owner to postgres;

create view lots_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, srcid, srcname, zonid, targs, dated, term, itemid, unit, unitx, price, "off", min, step, max, cap, avail, nstart, nend, m1, m2, m3, m4) as
SELECT o.typ,
       o.state,
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
       o.zonid,
       o.targs,
       o.dated,
       o.term,
       o.itemid,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.min,
       o.step,
       o.max,
       o.cap,
       o.avail,
       o.nstart,
       o.nend,
       o.m1 IS NOT NULL AS m1,
       o.m2 IS NOT NULL AS m2,
       o.m3 IS NOT NULL AS m3,
       o.m4 IS NOT NULL AS m4
FROM lots o;

alter table lots_vw owner to postgres;

create view orgs_vw(typ, state, name, tip, created, creator, adapted, adapter, oker, oked, status, id, prtid, ctrid, ext, legal, regid, addr, x, y, tel, trust, link, specs, icon, pic, m1, m2, m3, m4) as
SELECT o.typ,
       o.state,
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
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM orgs o;

alter table orgs_vw owner to postgres;

create view users_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, tel, addr, im, credential, admly, srcid, srcly, shpid, shply, vip, icon) as
SELECT u.typ,
       u.state,
       u.name,
       u.tip,
       u.created,
       u.creator,
       u.adapted,
       u.adapter,
       u.oked,
       u.oker,
       u.status,
       u.id,
       u.tel,
       u.addr,
       u.im,
       u.credential,
       u.admly,
       u.srcid,
       u.srcly,
       u.shpid,
       u.shply,
       u.vip,
       u.icon IS NOT NULL AS icon
FROM users u;

alter table users_vw owner to postgres;

create view wares_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, shpid, itemid, unit, unitx, price, "off", min, max, avail, icon, pic, ops) as
SELECT o.typ,
       o.state,
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
       o.itemid,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.min,
       o.max,
       o.avail,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.ops
FROM wares o;

alter table wares_vw owner to postgres;

create function first_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as $$
SELECT $1
$$;

alter function first_agg(anyelement, anyelement) owner to postgres;

create function last_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as $$
SELECT $2
$$;

alter function last_agg(anyelement, anyelement) owner to postgres;

create function buysgen(till date, opr character varying) returns void
    language plpgsql
as $$
DECLARE
    past date;
    now timestamp(0) = localtimestamp(0);
    tillstamp timestamp(0);
    paststamp timestamp(0);

    TYP_PLAT constant int = 1;
    TYP_GATEWAY constant int = 2;
    TYP_SHP constant int = 3;
    TYP_MKT constant int = 4;

    BASE constant int = 1000;
    RATE_PLAT constant int = 4;
    RATE_GATEWAY constant int = 6;
    RATE_SHP constant int = 970;
    RATE_MKT constant int = 20;

BEGIN

    -- adjust parameters

    tillstamp = (till + interval '1 day')::timestamp(0);

    opr = coalesce(opr, 'SYS');

    SELECT coalesce(
                   buysgen, '2000-01-01'::date)FROM global WHERE pk INTO past;
    paststamp = (past + interval '1 day')::timestamp(0);

    -- buys for shop

    INSERT INTO buyaggs (typ, orgid, acct, dt, prtid, trans, amt, created, creator)
    SELECT 1,
           shpid,
           typ,
           oked::date,
           first(mktid),
           count(pay),
           sum(pay - coalesce(refund, 0::money)),
           now,
           opr
    FROM buys
    WHERE status = 4 AND oked >= paststamp AND oked < tillstamp
    GROUP BY shpid, typ, oked::date;

    INSERT INTO buyclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
    SELECT TYP_SHP,
           first(creator),
           now,
           opr,
           orgid,
           till,
           sum(trans),
           sum(amt),
           RATE_SHP,
           sum(amt * RATE_SHP / BASE)
    FROM buyaggs
    WHERE typ = 1 AND dt > past AND dt <= till GROUP BY orgid;


    INSERT INTO buyclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
    SELECT TYP_MKT,
           first(creator),
           now,
           opr,
           prtid,
           till,
           sum(trans),
           sum(amt),
           RATE_MKT,
           sum(amt * RATE_MKT / BASE)
    FROM buyaggs
    WHERE typ = 1 AND dt > past AND dt <= till GROUP BY prtid;



    UPDATE global SET buysgen = till WHERE pk;
END
$$;

alter function buysgen(date, varchar) owner to postgres;

create function booksgen(till date, opr character varying) returns void
    language plpgsql
as $$
DECLARE

    past date;
    now timestamp(0) = localtimestamp(0);
    tillstamp timestamp(0);
    paststamp timestamp(0);


    TYP_PLAT constant int = 1;
    TYP_GATEWAY constant int = 2;
    TYP_SRC constant int = 5;
    TYP_ZON constant int = 6;
    TYP_CTR constant int = 7;

--     rates in thousandth

    BASE constant int = 1000;
    RATE_PLAT constant int = 4;
    RATE_GATEWAY constant int = 6;
    RATE_SRC constant int = 970;
    RATE_ZON constant int = 4;
    RATE_CTR constant int = 16;

BEGIN

    opr = coalesce(opr, 'SYS');

    SELECT coalesce(booksgen, '2000-01-01'::date)FROM global WHERE pk INTO past;
    tillstamp = (till + interval '1 day')::timestamp(0);

    opr = coalesce(opr, 'SYS');

    paststamp = (past + interval '1 day')::timestamp(0);



    -- books for source

    INSERT INTO bookaggs (typ, orgid, acct, dt, prtid, trans, amt, created, creator)
    SELECT 2,
           srcid,
           itemid,
           oked::date,
           first(zonid),
           count(pay),
           sum(pay - coalesce(refund, 0::money)),
           now,
           opr
    FROM books
    WHERE status = 4 AND oked >= paststamp AND oked < tillstamp
    GROUP BY srcid, itemid, oked::date;

    -- books for center

    INSERT INTO bookaggs (typ, orgid, acct, dt, prtid, trans, amt, created, creator)
    SELECT 3,
           ctrid,
           itemid,
           oked::date,
           NULL,
           count(pay),
           sum(pay - coalesce(refund, 0::money)),
           now,
           opr
    FROM books
    WHERE status = 4 AND oked >= paststamp AND oked < tillstamp
    GROUP BY ctrid, itemid, oked::date;



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
    WHERE typ = 2 AND dt > past AND dt <= till GROUP BY orgid;

    INSERT INTO bookclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
    SELECT TYP_ZON,
           first(creator),
           now,
           opr,
           prtid,
           till,
           sum(trans),
           sum(amt),
           RATE_ZON,
           sum(amt * RATE_ZON / BASE)
    FROM bookaggs
    WHERE typ = 2 AND dt > past AND dt <= till GROUP BY prtid;


    INSERT INTO bookclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
    SELECT TYP_CTR,
           first(creator),
           now,
           opr,
           orgid,
           till,
           sum(trans),
           sum(amt),
           RATE_CTR,
           sum(amt * RATE_CTR / BASE)
    FROM bookaggs
    WHERE typ = 3 AND dt > past AND dt <= till GROUP BY orgid;


    UPDATE global SET booksgen = till WHERE pk;

END
$$;

alter function booksgen(date, varchar) owner to postgres;

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


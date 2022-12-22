create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type buyline as
(
    wareid integer,
    itemid integer,
    name varchar(12),
    unit varchar(4),
    price money,
    "off" money,
    qty smallint
);

alter type buyline owner to postgres;

create table entities
(
    typ smallint not null,
    state smallint,
    name varchar(12) not null,
    tip varchar(50),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10),
    oked timestamp(0),
    oker varchar(10),
    status smallint default 1 not null
);

alter table entities owner to postgres;

create table ledgers_
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

alter table ledgers_ owner to postgres;

create table peerledgs_
(
    peerid smallint
)
    inherits (ledgers_);

alter table peerledgs_ owner to postgres;

create table peers_
(
    id smallint not null
        constraint peers__pk
            primary key,
    weburl varchar(50),
    secret varchar(16)
)
    inherits (entities);

alter table peers_ owner to postgres;

create table accts_
(
    no varchar(20),
    v integer
)
    inherits (entities);

alter table accts_ owner to postgres;

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
    alias varchar(12),
    fully varchar(20),
    regid smallint
        constraint orgs_regid_fk
            references regs,
    addr varchar(30),
    x double precision,
    y double precision,
    tel varchar(11),
    trust boolean,
    link varchar(12),
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
    orgid smallint
        constraint users_orgid_fk
            references orgs,
    orgly smallint default 0 not null,
    orgext boolean,
    vip integer,
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

create index users_orgid_idx
    on users (orgid)
    where (orgid IS NOT NULL);

create table wares
(
    id serial not null
        constraint wares_pk
            primary key,
    shpid integer not null
        constraint wares_shpid_fk
            references orgs,
    itemid integer,
    unit varchar(4),
    unitstd varchar(4),
    unitx money,
    price money,
    "off" money,
    min smallint,
    max smallint,
    step smallint,
    icon bytea,
    pic bytea
)
    inherits (entities);

alter table wares owner to postgres;

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

create table buys
(
    id bigserial not null
        constraint buys_pk
            primary key,
    shpid integer not null
        constraint buys_shpid_fk
            references orgs,
    shpname varchar(12),
    mktid integer not null
        constraint buys_mkt_fk
            references orgs,
    uid integer not null
        constraint buys_uid_fk
            references users,
    uname varchar(12),
    utel varchar(11),
    uaddr varchar(20),
    uim varchar(28),
    pay money,
    deliv money,
    hand money,
    lines buyline[]
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

create table rpts
(
    orgid integer,
    dt date,
    itemid smallint,
    count integer,
    amt money,
    qty integer
)
    inherits (entities);

alter table rpts owner to postgres;

create table clears
(
    id serial not null
        constraint clears_pk
            primary key,
    till date,
    orgid integer not null
        constraint clears_orgid_fk
            references orgs,
    sprid integer not null,
    orders integer,
    amt money,
    rate money,
    pay integer
)
    inherits (entities);

alter table clears owner to postgres;

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
    ctrid integer not null
        constraint lots_ctrid_fk
            references orgs,
    mktids integer[],
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
    remain integer,
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

create view wares_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, shpid, itemid, unit, unitstd, unitx, price, "off", min, max, step, icon, pic) as
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
       o.unitstd,
       o.unitx,
       o.price,
       o.off,
       o.min,
       o.max,
       o.step,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic
FROM wares o;

alter table wares_vw owner to postgres;

create view orgs_vw(typ, state, name, tip, created, creator, adapted, adapter, oker, oked, status, id, prtid, ctrid, alias, fully, regid, addr, x, y, tel, trust, link, specs, icon, pic, m1, m2, m3, m4) as
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
       o.alias,
       o.fully,
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

create view users_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, tel, addr, im, credential, admly, orgid, orgly, orgext, vip, icon) as
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
       u.orgid,
       u.orgly,
       u.orgext,
       u.vip,
       u.icon IS NOT NULL AS icon
FROM users u;

alter table users_vw owner to postgres;

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

create view lots_vw(typ, state, name, tip, created, creator, adapted, adapter, oked, oker, status, id, srcid, srcname, zonid, ctrid, mktids, dated, term, itemid, unit, unitx, price, "off", min, step, max, cap, remain, nstart, nend, m1, m2, m3, m4) as
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
       o.ctrid,
       o.mktids,
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

create function recalc(timestamp without time zone) returns void
    language plpgsql
as $$
BEGIN

    -- clear
    DELETE FROM clears WHERE status = 0;

    -- by shop
    INSERT INTO clears (typ, orgid, till, amt)
    SELECT 1,
           shpid,
           $1,
           sum((pay - coalesce(refund, 0::money)) * 0.88)
    FROM buys
    WHERE status IN (1,2) AND oked < $1 GROUP BY shpid;

    -- mkt commission
    INSERT INTO clears (typ, orgid, till, amt)
    SELECT 4,
           shpid,
           $1,
           sum(fee * 0.88)
    FROM buys WHERE status IN (1, 2) AND oked < $1 GROUP BY shpid;

END
$$;

alter function recalc(timestamp) owner to postgres;

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


create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type buyln_type as
(
    stockid integer,
    name varchar(12),
    wareid smallint,
    price money,
    qty smallint,
    qtyre smallint
);

alter type buyln_type owner to postgres;

create table entities
(
    typ smallint not null,
    status smallint default 0 not null,
    name varchar(12) not null,
    tip varchar(30),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10)
);

alter table entities owner to postgres;

create table regs
(
    id smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (entities);

comment on column regs.num is 'sub resources';

alter table regs owner to postgres;

create table dailys
(
    orgid integer,
    dt date,
    itemid smallint,
    count integer,
    amt money,
    qty integer
)
    inherits (entities);

alter table dailys owner to postgres;

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
        constraint peers_pk
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

create table clears
(
    id serial not null
        constraint clears_pk
            primary key,
    dt date,
    orgid integer not null,
    sprid integer not null,
    orders integer,
    total money,
    rate money,
    pay integer
)
    inherits (entities);

alter table clears owner to postgres;

create table cats
(
    idx smallint,
    size smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (entities);

comment on column cats.size is 'sub resources';

alter table cats owner to postgres;

create table stocks
(
    id serial not null
        constraint stocks_pk
            primary key,
    shpid integer,
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

alter table stocks owner to postgres;

create table items
(
    id serial not null
        constraint items_pk
            primary key,
    srcid integer,
    store smallint,
    duration smallint,
    agt boolean,
    unit varchar(4),
    unitpkg varchar(4),
    unitx smallint[],
    icon bytea,
    pic bytea,
    m1 bytea,
    m2 bytea,
    m3 bytea,
    m4 bytea
)
    inherits (entities);

alter table items owner to postgres;

create table rules
(
    id serial not null
        constraint rules_pk
            primary key,
    oker varchar(12),
    oked timestamp(0),
    state smallint
)
    inherits (entities);

alter table rules owner to postgres;

create table buys
(
    id bigserial not null
        constraint buys_pk
            primary key,
    shpid integer not null,
    mktid integer not null,
    uid integer not null,
    uname varchar(10),
    utel varchar(11),
    uaddr varchar(20),
    uim varchar(28),
    lines buyln_type[],
    fee money,
    pay money,
    refund money,
    oker varchar(10),
    oked timestamp(0),
    state smallint
)
    inherits (entities);

alter table buys owner to postgres;

create index buys_orgs_state_idx
    on buys (mktid, shpid, state);

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
    orgid smallint,
    orgly smallint default 0 not null,
    idcard varchar(18),
    icon bytea
)
    inherits (entities);

alter table users owner to postgres;

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
    license varchar(20),
    trust boolean,
    regid smallint
        constraint orgs_regid_fk
            references regs
            on update cascade,
    addr varchar(30),
    x double precision,
    y double precision,
    tel varchar(11),
    sprid integer
        constraint orgs_sprid_fk
            references users,
    rvrid integer
        constraint orgs_rvrid_fk
            references users,
    icon bytea,
    oker varchar(10),
    oked timestamp(0),
    state smallint
)
    inherits (entities);

alter table orgs owner to postgres;

create table events
(
    id serial not null
        constraint events_pk
            primary key,
    orgid integer
        constraint events_forid_fk
            references orgs,
    credit integer
)
    inherits (entities);

alter table events owner to postgres;

create table lots
(
    id serial not null
        constraint lots_pk
            primary key,
    itemid integer
        constraint lots_items_fk
            references items,
    srcid integer
        constraint lots_srcid_fk
            references orgs,
    ctrid integer
        constraint lots_ctrid_fk
            references orgs,
    ctring boolean,
    price money,
    "off" money,
    cap integer,
    remain integer,
    min integer,
    max integer,
    step integer,
    oker varchar(10),
    oked timestamp(0),
    state smallint,
    nstart integer,
    nend integer,
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
    shpid integer not null,
    mktid integer not null,
    ctrid integer not null,
    srcid integer not null,
    zonid integer not null,
    itemid integer
        constraint books_itemid_fk
            references items,
    lotid integer
        constraint books_lotid_fk
            references lots,
    unit varchar(4),
    unitx smallint,
    unitstd varchar(4),
    price money,
    "off" money,
    qty integer,
    cut integer,
    pay money,
    refund money,
    oker varchar(10),
    oked timestamp(0),
    state smallint
)
    inherits (entities);

alter table books owner to postgres;

create index lots_nstart_nend_idx
    on lots (nstart, nend);

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create unique index users_tel_idx
    on users (tel);

create index users_orgidorgly_idx
    on users (orgid, orgly)
    where (orgly > 0);

create view users_vw(typ, status, name, tip, created, creator, adapted, adapter, id, tel, addr, im, credential, admly, orgid, orgly, idcard, icon) as
SELECT u.typ,
       u.status,
       u.name,
       u.tip,
       u.created,
       u.creator,
       u.adapted,
       u.adapter,
       u.id,
       u.tel,
       u.addr,
       u.im,
       u.credential,
       u.admly,
       u.orgid,
       u.orgly,
       u.idcard,
       u.icon IS NOT NULL AS icon
FROM users u;

alter table users_vw owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, adapted, adapter, id, prtid, ctrid, license, trust, regid, addr, x, y, tel, sprid, sprname, sprtel, sprim, rvrid, icon, oker, oked, state) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.id,
       o.prtid,
       o.ctrid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.sprid,
       m.name             AS sprname,
       m.tel              AS sprtel,
       m.im               AS sprim,
       o.rvrid,
       o.icon IS NOT NULL AS icon,
       o.oker,
       o.oked,
       o.state
FROM orgs o
         LEFT JOIN users m
                   ON o.sprid =
                      m.id;

alter table orgs_vw owner to postgres;

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


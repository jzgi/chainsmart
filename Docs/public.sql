create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type act_type as
(
    "user" varchar(10),
    role varchar(10),
    party varchar(10),
    op varchar(10),
    stamp timestamp(0)
);

alter type act_type owner to postgres;

create type purchop_type as
(
    state smallint,
    label varchar(12),
    orgid integer,
    uid integer,
    uname varchar(12),
    utel varchar(11),
    stamp timestamp(0)
);

alter type purchop_type owner to postgres;

create type buyln_type as
(
    prodid integer,
    prodname varchar(12),
    itemid smallint,
    price money,
    qty smallint,
    qtyre smallint
);

alter type buyln_type owner to postgres;

create table infos
(
    typ smallint not null,
    state smallint default 0 not null,
    name varchar(12) not null,
    tip varchar(30),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10)
);

alter table infos owner to postgres;

create table regs
(
    id smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (infos);

comment on column regs.num is 'sub resources';

alter table regs owner to postgres;

create table items
(
    id integer not null,
    unit varchar(4),
    unitip varchar(10),
    icon bytea
)
    inherits (infos);

comment on table items is 'standard items';

alter table items owner to postgres;

create table orgs
(
    id serial not null
        constraint orgs_pk
            primary key,
    fork smallint,
    sprid integer,
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
    tag varchar(4),
    mgrid integer,
    toctrs integer[],
    img bytea
)
    inherits (infos);

alter table orgs owner to postgres;

create table users
(
    id serial not null
        constraint users_pk
            primary key,
    tel varchar(11) not null,
    im varchar(28),
    credential varchar(32),
    admly smallint default 0 not null,
    orgid smallint
        constraint users_orgid_fk
            references orgs,
    orgly smallint default 0 not null,
    idcard varchar(18)
)
    inherits (infos);

alter table users owner to postgres;

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create index users_orgid_idx
    on users (orgid)
    where (orgid > 0);

create unique index users_tel_idx
    on users (tel);

create table dailys
(
    orgid integer,
    dt date,
    itemid smallint,
    count integer,
    amt money,
    qty integer
)
    inherits (infos);

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
    inherits (infos);

alter table peers_ owner to postgres;

create table accts_
(
    no varchar(20),
    v integer
)
    inherits (infos);

alter table accts_ owner to postgres;

create table notes
(
    id serial not null,
    fromid integer,
    toid integer
)
    inherits (infos);

comment on table notes is 'annoucements and notices';

alter table notes owner to postgres;

create table purchs
(
    id bigserial not null
        constraint purchs_pk
            primary key,
    bizid integer not null,
    srcid integer not null,
    prvid integer not null,
    ctrid integer not null,
    mrtid integer not null,
    prodid integer,
    itemid smallint,
    unit varchar(4),
    unitx smallint,
    mode smallint,
    price money,
    "off" money,
    qty smallint,
    pay money,
    qtyre smallint,
    payre money,
    ops purchop_type[],
    status smallint,
    bunit varchar(4),
    bunitx smallint,
    bprice money,
    bmin smallint,
    bmax smallint,
    bstep smallint
)
    inherits (infos);

comment on table purchs is 'purchases and resales';

comment on column purchs.unitx is 'times of standard unit';

comment on column purchs.qtyre is 'qty reduced';

comment on column purchs.payre is 'pay refunded';

alter table purchs owner to postgres;

create table buys
(
    id bigserial not null
        constraint buys_pk
            primary key,
    bizid integer not null,
    mrtid integer not null,
    uid integer not null,
    uname varchar(10),
    utel varchar(11),
    uaddr varchar(20),
    uim varchar(28),
    lns buyln_type[],
    pay money,
    payre money,
    status smallint
)
    inherits (infos);

comment on table buys is 'customer buys';

alter table buys owner to postgres;

create table prods
(
    id serial not null
        constraint prods_pk
            primary key,
    orgid integer,
    itemid integer,
    ext varchar(10),
    store smallint,
    duration smallint,
    toagt boolean,
    unit varchar(4),
    unitx smallint,
    price money,
    cap smallint,
    min smallint,
    max smallint,
    step smallint,
    starton timestamp(0),
    endon timestamp(0),
    "off" money,
    threshold smallint,
    present smallint,
    img bytea,
    pic bytea
)
    inherits (infos);

comment on table prods is 'products from sources';

comment on column prods.store is 'storage method';

comment on column prods.unitx is 'times of standard unit';

comment on column prods.present is 'group-purchase accumulative';

alter table prods owner to postgres;

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
    pay integer,
    status smallint
)
    inherits (infos);

alter table clears owner to postgres;

create view orgs_vw(typ, state, name, tip, created, creator, adapted, adapter, id, fork, tag, sprid, license, trust, regid, addr, x, y, tel, toctrs, mgrid, mgrname, mgrtel, mgrim, img) as
SELECT o.typ,
       o.state,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.id,
       o.fork,
       o.tag,
       o.sprid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.toctrs,
       o.mgrid,
       m.name            AS mgrname,
       m.tel             AS mgrtel,
       m.im              AS mgrim,
       o.img IS NOT NULL AS img
FROM orgs o
         LEFT JOIN users m
                   ON o.mgrid =
                      m.id;

alter table orgs_vw owner to postgres;


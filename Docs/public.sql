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
    ops stockop[]
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
    avail numeric(8,1),
    nstart integer,
    nend integer,
    m1 bytea,
    m2 bytea,
    m3 bytea,
    m4 bytea,
    ops stockop[],
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
    qty numeric(8,1),
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
    lns buyln[],
    topay money,
    pay money,
    ret numeric(6,1),
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


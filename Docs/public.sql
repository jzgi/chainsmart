create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

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
    stockid integer,
    name varchar(12),
    wareid smallint,
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

create table users
(
    id serial not null
        constraint users_pk
            primary key,
    tel varchar(11) not null,
    im varchar(28),
    credential varchar(32),
    admly smallint default 0 not null,
    orgid smallint,
    orgly smallint default 0 not null,
    idcard varchar(18),
    icon bytea
)
    inherits (infos);

alter table users owner to postgres;

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create unique index users_tel_idx
    on users (tel);

create index users_orgid_idx
    on users (orgid);

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

create table orgs
(
    id serial not null
        constraint orgs_pk
            primary key,
    fork smallint,
    sprid integer
        constraint orgs_sprid_fk
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
    mgrid integer
        constraint orgs_mgrid_fk
            references users,
    ctrties integer[],
    img bytea
)
    inherits (infos);

alter table orgs owner to postgres;

alter table users
    add constraint users_orgid_fk
        foreign key (orgid) references orgs;

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
    wareid integer,
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
    status smallint
)
    inherits (infos);

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

create table cats
(
    idx smallint,
    num smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (infos);

comment on column cats.num is 'sub resources';

alter table cats owner to postgres;

create table items
(
    id integer not null
        constraint items_pk
            primary key,
    unit varchar(4),
    unitip varchar(10),
    icon bytea,
    constraint items_typ_fk
        foreign key (typ) references cats
)
    inherits (infos);

comment on table mops is 'standard items';

alter table mops owner to postgres;

create table wares
(
    id serial not null
        constraint wares_pk
            primary key,
    srcid integer
        constraint wares_srcid_fk
            references orgs,
    itemid integer
        constraint wares_itemid_fk
            references mops,
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
    pic bytea,
    constraint wares_typ_fk
        foreign key (typ) references cats
)
    inherits (infos);

comment on table wares is 'sellable wares by sources';

comment on column wares.store is 'storage method';

comment on column wares.unitx is 'times of standard unit';

comment on column wares.present is 'group-purchase accumulative';

alter table wares owner to postgres;

create table stocks
(
    id serial not null
        constraint stocks_pk
            primary key,
    bizid integer,
    wareid integer,
    unit varchar(4),
    unitx smallint,
    price money,
    min smallint,
    max smallint,
    step smallint
)
    inherits (infos);

alter table stocks owner to postgres;

create view orgs_vw(typ, state, name, tip, created, creator, adapted, adapter, id, fork, sprid, license, trust, regid, addr, x, y, tel, ctrties, mgrid, mgrname, mgrtel, mgrim, img) as
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
       o.sprid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.ctrties,
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

create view users_vw(typ, state, name, tip, created, creator, adapted, adapter, id, tel, im, credential, admly, orgid, orgly, idcard, icon) as
SELECT u.typ,
       u.state,
       u.name,
       u.tip,
       u.created,
       u.creator,
       u.adapted,
       u.adapter,
       u.id,
       u.tel,
       u.im,
       u.credential,
       u.admly,
       u.orgid,
       u.orgly,
       u.idcard,
       u.icon IS NOT NULL AS icon
FROM users u;

alter table users_vw owner to postgres;

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


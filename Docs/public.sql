create table public.entities
(
    typ     smallint           not null,
    state   smallint,
    name    varchar(12)        not null,
    tip     varchar(40),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10),
    fixed   timestamp(0),
    fixer   varchar(10),
    status  smallint default 1 not null
);

alter table public.entities
    owner to postgres;

create table public._ledgs
(
    seq     integer,
    acct    varchar(20),
    name    varchar(12),
    amt     integer,
    bal     integer,
    cs      uuid,
    blockcs uuid,
    stamp   timestamp(0)
);

alter table public._ledgs
    owner to postgres;

create table public.cats
(
    idx  smallint,
    size smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (public.entities);

alter table public.cats
    owner to postgres;

create table public.regs
(
    id  smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (public.entities);

alter table public.regs
    owner to postgres;

create table public.orgs
(
    id    serial
        constraint orgs_pk
            primary key,
    prtid integer
        constraint orgs_prtid_fk
            references public.orgs,
    ctrid integer
        constraint orgs_ctrid_fk
            references public.orgs,
    ext   varchar(12),
    legal varchar(20),
    regid smallint
        constraint orgs_regid_fk
            references public.regs,
    addr  varchar(30),
    x     double precision,
    y     double precision,
    tel   varchar(11),
    trust boolean,
    link  varchar(30),
    specs jsonb,
    icon  bytea,
    pic   bytea,
    m1    bytea,
    m2    bytea,
    m3    bytea,
    m4    bytea
)
    inherits (public.entities);

alter table public.orgs
    owner to postgres;

create table public.users
(
    id         serial
        constraint users_pk
            primary key,
    tel        varchar(11)        not null,
    addr       varchar(50),
    im         varchar(28),
    credential varchar(32),
    admly      smallint default 0 not null,
    srcid      smallint
        constraint users_srcid_fk
            references public.orgs,
    srcly      smallint default 0 not null,
    shpid      integer
        constraint users_shpid_fk
            references public.orgs,
    shply      smallint,
    vip        integer[]
        constraint users_vip_chk
        check (array_length(vip, 1) <= 4),
    icon       bytea
)
    inherits (public.entities);

alter table public.users
    owner to postgres;

create index users_admly_idx
    on public.users (admly)
    where (admly > 0);

create unique index users_im_idx
    on public.users (im);

create unique index users_tel_idx
    on public.users (tel);

create index users_shpid_idx
    on public.users (shpid)
    where (shpid > 0);

create index users_srcid_idx
    on public.users (srcid)
    where (srcid > 0);

create index users_vip_idx
    on public.users using gin (vip);

create table public.tests
(
    id    serial
        constraint tests_pk
            primary key,
    orgid integer
        constraint tests_orgid_fk
            references public.orgs,
    level integer
)
    inherits (public.entities);

alter table public.tests
    owner to postgres;

create table public.buys
(
    id     bigserial
        constraint buys_pk
            primary key,
    shpid  integer not null
        constraint buys_shpid_fk
            references public.orgs,
    mktid  integer not null
        constraint buys_mkt_fk
            references public.orgs,
    uid    integer
        constraint buys_uid_fk
            references public.users,
    uname  varchar(12),
    utel   varchar(11),
    uaddr  varchar(30),
    uim    varchar(28),
    lns    buyln[],
    topay  money,
    pay    money,
    ret    numeric(6, 1),
    refund money
)
    tablespace rtl inherits
(
    public.entities
);

alter table public.buys
    owner to postgres;

create index buys_uidstatus_idx
    on public.buys (uid, status)
    tablespace rtl;

create index buys_shpidstatus_idx
    on public.buys (shpid, status)
    tablespace rtl;

create unique index buys_single_idx
    on public.buys (shpid, typ, status)
    where ((typ = 1) AND (status = 0));

create trigger buys_trig
    after insert or update
                        on public.buys
                        for each row
                        when (new.status = 1 OR new.status = 4 OR new.status = 8)
                        execute procedure public.buys_trig_upd();

create table public.global
(
    buysgen  date,
    booksgen date,
    pk       boolean not null
        constraint global_pk
            primary key
);

alter table public.global
    owner to postgres;

create table public.bookaggs
(
    typ     smallint not null,
    orgid   integer  not null,
    dt      date,
    acct    integer  not null,
    prtid   integer,
    trans   integer,
    qty     numeric(8, 1),
    amt     money,
    created timestamp(0),
    creator varchar(12)
);

alter table public.bookaggs
    owner to postgres;

create index rpts_main_idx
    on public.bookaggs (typ, orgid, dt);

create table public.bookclrs
(
    id    integer default nextval('clears_id_seq'::regclass) not null
        constraint clears_pk
            primary key,
    orgid integer                                            not null,
    till  date,
    trans integer,
    amt   money,
    rate  smallint,
    topay money,
    pay   money
)
    inherits (public.entities);

alter table public.bookclrs
    owner to postgres;

create table public.buyaggs
(
    typ     smallint not null,
    orgid   integer  not null,
    dt      date,
    acct    integer  not null,
    prtid   integer,
    trans   integer,
    qty     numeric(8, 1),
    amt     money,
    created timestamp(0),
    creator varchar(12)
);

alter table public.buyaggs
    owner to postgres;

create table public.buyclrs
(
    id    serial
        constraint buyclrs_pk
            primary key,
    orgid integer not null,
    till  date,
    trans integer,
    amt   money,
    rate  smallint,
    topay money,
    pay   money
)
    inherits (public.entities);

alter table public.buyclrs
    owner to postgres;

create table public._accts
(
    no      varchar(18),
    balance money
)
    inherits (public.entities);

alter table public._accts
    owner to postgres;

create table public._asks
(
    acct    varchar(20),
    name    varchar(12),
    amt     integer,
    bal     integer,
    cs      uuid,
    blockcs uuid,
    stamp   timestamp(0)
);

alter table public._asks
    owner to postgres;

create table public.assets
(
    id     serial
        constraint assets_pk
            primary key,
    orgid  integer,
    cap    integer,
    cern   varchar(12),
    factor double precision,
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
    inherits (public.entities);

alter table public.assets
    owner to postgres;

create table public.lots
(
    id      serial
        constraint lots_pk
            primary key,
    srcid   integer,
    srcname varchar(12),
    zonid   integer not null
        constraint lots_zonid_fk
            references public.orgs,
    assetid integer
        constraint lots_assetid_fk
            references public.assets,
    targs   integer[],
    term    smallint,
    dated   date,
    unit    varchar(4),
    unitx   numeric(6, 1),
    price   money,
    "off"   money,
    min     integer,
    step    integer,
    max     integer,
    cap     integer,
    avail   numeric(8, 1),
    nstart  integer,
    nend    integer,
    ops     stockop[],
    icon    bytea,
    pic     bytea,
    m1      bytea,
    m2      bytea,
    m3      bytea,
    m4      bytea
)
    inherits (public.entities);

alter table public.lots
    owner to postgres;

create table public.items
(
    id    integer       default nextval('wares_id_seq'::regclass) not null
        constraint items_pk
            primary key,
    shpid integer                                                 not null
        constraint items_shpid_fk
            references public.orgs,
    lotid integer
        constraint items_lotid_fk
            references public.lots,
    unit  varchar(4),
    unitx numeric(6, 1),
    price money,
    "off" money,
    min   smallint,
    max   smallint,
    avail numeric(6, 1) default 0.0                               not null,
    icon  bytea,
    pic   bytea,
    ops   stockop[]
)
    tablespace rtl inherits
(
    public.entities
);

alter table public.items
    owner to postgres;

create table public.books
(
    id      bigserial
        constraint books_pk
            primary key,
    shpid   integer not null
        constraint books_shpid_fk
            references public.orgs,
    shpname varchar(12),
    mktid   integer not null
        constraint books_mktid_fk
            references public.orgs,
    ctrid   integer not null
        constraint books_ctrid_fk
            references public.orgs,
    srcid   integer not null
        constraint books_srcid_fk
            references public.orgs,
    srcname varchar(12),
    zonid   integer not null
        constraint books_zonly_fk
            references public.orgs,
    lotid   integer
        constraint books_lotid_fk
            references public.lots,
    unit    varchar(4),
    unitx   numeric(6, 1),
    price   money,
    "off"   money,
    qty     numeric(8, 1),
    topay   money,
    pay     money,
    ret     numeric(6, 1),
    refund  money
)
    tablespace sup inherits
(
    public.entities
);

alter table public.books
    owner to postgres;

create unique index books_single_idx
    on public.books (shpid, status)
    where (status = 0) tablespace sup;

create index lots_nend_idx
    on public.lots (nend);

create index lots_srcidstatus_idx
    on public.lots (srcid, status);


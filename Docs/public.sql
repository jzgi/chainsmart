create table public.entities
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

comment on table public.entities is 'abstract entities';

alter table public.entities
    owner to postgres;

create table public.cats
(
    idx   smallint,
    style smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (public.entities);

comment on table public.cats is 'categories';

alter table public.cats
    owner to postgres;

create table public.regs
(
    id    smallint not null
        constraint regs_pk
            primary key,
    idx   smallint,
    style smallint
)
    inherits (public.entities);

comment on table public.regs is 'regions';

alter table public.regs
    owner to postgres;

create index regs_typidx_idx
    on public.regs (typ, idx);

create table public.orgs
(
    id           serial
        constraint orgs_pk
            primary key,
    parentid     integer
        constraint orgs_parentid_fk
            references public.orgs,
    hubid        integer
        constraint orgs_hubid_fk
            references public.orgs,
    cover        varchar(12),
    legal        varchar(20),
    regid        smallint not null
        constraint orgs_regid_fk
            references public.regs
            on update cascade,
    addr         varchar(30),
    x            double precision,
    y            double precision,
    tel          varchar(11),
    trust        boolean,
    descr        varchar(100),
    bankacctname varchar(15),
    bankacct     varchar(20),
    specs        jsonb,
    openat       time(0),
    closeat      time(0),
    rank         smallint,
    style        smallint,
    icon         bytea,
    pic          bytea,
    m1           bytea,
    m2           bytea,
    m3           bytea,
    scene        bytea,
    ties         integer[],
    cattyp       smallint,
    symtyp       smallint,
    tagtyp       smallint,
    envtyp       smallint,
    constraint orgs_chk
        check ((typ >= 1) AND (typ <= 27))
)
    inherits (public.entities);

comment on table public.orgs is 'organizational units';

alter table public.orgs
    owner to postgres;

create index orgs_parentidstu_idx
    on public.orgs (parentid, status);

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
    supid      smallint
        constraint users_supid_fk
            references public.orgs,
    suply      smallint default 0 not null,
    rtlid      integer
        constraint users_rtlid_fk
            references public.orgs,
    rtlly      smallint,
    vip        integer[],
    agreed     date,
    icon       bytea,
    orgid      integer
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

create index users_rtlid_idx
    on public.users (rtlid)
    where (rtlid > 0);

create index users_supid_idx
    on public.users (supid)
    where (supid > 0);

create index users_vip_idx
    on public.users using gin (vip);

create table public.purs
(
    id       bigint     default nextval('books_id_seq'::regclass) not null
        constraint purs_pk
            primary key,
    rtlid    integer                                              not null
        constraint purs_rtlid_fk
            references public.orgs,
    mktid    integer                                              not null
        constraint purs_mktid_fk
            references public.orgs,
    hubid    integer                                              not null
        constraint purs_hubid_fk
            references public.orgs,
    supid    integer                                              not null
        constraint purs_supid_fk
            references public.orgs,
    ctrid    integer                                              not null,
    lotid    integer,
    unit     varchar(4),
    unitip   varchar(8) default 0                                 not null,
    unitx    smallint,
    price    money,
    "off"    money,
    qty      integer,
    fee      money,
    topay    money,
    pay      money,
    ret      integer,
    refund   money,
    refunder varchar(10),
    constraint typ_chk
        check ((typ >= 1) AND (typ <= 2))
)
    inherits
        (public.entities)
    tablespace sup;

comment on table public.purs is 'supply purchases';

alter table public.purs
    owner to postgres;

create index purs_supidstatustyp_idx
    on public.purs (supid, status, typ)
    tablespace sup;

create index purs_mktidstatustyp_idx
    on public.purs (mktid, status, typ) tablespace sup
    where ((status = 2) OR (status = 4));

create index purs_rtlidstatustyp_idx
    on public.purs (rtlid, status, typ)
    tablespace sup;

create index purs_hubidstatustypmktid_idx
    on public.purs (hubid, status, typ, mktid) tablespace sup
    where ((typ = 1) AND ((status = 1) OR (status = 2)));

create index purs_gen_idx
    on public.purs (status, oked, supid) tablespace sup
    where (status = 4);

create table public.buys
(
    id       serial
        constraint buys_pk
            primary key,
    rtlid    integer not null
        constraint buys_rtlid_fk
            references public.orgs,
    mktid    integer not null
        constraint buys_mkt_fk
            references public.orgs,
    uid      integer
        constraint buys_uid_fk
            references public.users,
    uname    varchar(12),
    utel     varchar(11),
    ucom     varchar(12),
    uaddr    varchar(30),
    uim      varchar(28),
    fee      money,
    topay    money,
    pay      money,
    ret      numeric(6, 1),
    refund   money,
    refunder varchar(10),
    items    buyln[],
    constraint buys_chk
        check ((typ >= 1) AND (typ <= 3))
)
    inherits
        (public.entities)
    tablespace rtl;

comment on table public.buys is 'retail buys';

alter table public.buys
    owner to postgres;

create index buys_rtlidstatustyp_idx
    on public.buys (rtlid asc, status asc, typ asc, oked desc)
    tablespace rtl;

create index buys_gen_idx
    on public.buys (status asc, oked desc, rtlid asc, typ asc) tablespace rtl
    where ((status = 4) AND (typ = 1));

create index buys_uidstatus_idx
    on public.buys (uid, status)
    tablespace rtl;

create index buys_mktidstatustypucomoked_idx
    on public.buys (mktid asc, status asc, typ asc, ucom asc, oked desc) tablespace rtl
    where ((typ = 1) AND (adapter IS NOT NULL));

create trigger buys_trig
    after insert or update
        of status
    on public.buys
    for each row
execute procedure public.buys_trig_func();

create table public.items
(
    id     serial
        constraint items_pk
            primary key,
    orgid  integer               not null
        constraint items_rtlid_fk
            references public.orgs,
    srcid  integer,
    lotid  integer,
    unit   varchar(4),
    unitip varchar(10) default 0 not null,
    unitx  smallint,
    price  money,
    "off"  money,
    max    smallint,
    min    smallint    default 0 not null,
    stock  smallint    default 0 not null,
    ops    itemop[],
    icon   bytea,
    pic    bytea,
    promo  boolean,
    cattyp smallint,
    link   varchar(50),
    nstart integer,
    nend   integer,
    m1     bytea,
    m2     bytea,
    m3     bytea,
    m4     bytea,
    constraint items_chk
        check ((typ >= 1) AND (typ <= 2))
)
    inherits (public.entities);

comment on table public.items is 'retail items';

alter table public.items
    owner to postgres;

create index items_orgidstu_idx
    on public.items (orgid, status);

create table public.buyaps
(
    level  smallint not null,
    orgid  integer  not null,
    dt     date     not null,
    trans  integer,
    amt    money,
    rate   smallint,
    topay  money,
    xorgid integer,
    constraint buyaps_pk
        primary key (level, orgid, dt)
)
    tablespace rtl;

comment on table public.buyaps is 'buy accounts payable';

alter table public.buyaps
    owner to postgres;

create table public.ldgs
(
    orgid  integer not null,
    dt     date    not null,
    acct   integer not null,
    name   varchar(12),
    xorgid integer,
    trans  integer,
    qty    integer,
    amt    money
);

alter table public.ldgs
    owner to postgres;

create table public.buyldgs_itemid
(
    constraint buyldgs_itemid_pk
        primary key (orgid, dt, acct)
)
    inherits
        (public.ldgs)
    tablespace rtl;

comment on table public.buyldgs_itemid is 'buy ledgers by itemid';

alter table public.buyldgs_itemid
    owner to postgres;

create table public.buyldgs_typ
(
    constraint buyldgs_typ_pk
        primary key (orgid, dt, acct)
)
    inherits
        (public.ldgs)
    tablespace rtl;

comment on table public.buyldgs_typ is 'buy ledgers by type';

alter table public.buyldgs_typ
    owner to postgres;

create table public.purldgs_lotid
(
    constraint purldgs_lotid_pk
        primary key (orgid, acct, dt)
)
    inherits
        (public.ldgs)
    tablespace sup;

comment on table public.purldgs_lotid is 'purchase ledgers by lotid';

comment on column public.purldgs_lotid.orgid is 'supid of that provides the lot';

comment on column public.purldgs_lotid.xorgid is 'the parentid ';

alter table public.purldgs_lotid
    owner to postgres;

create table public.purldgs_typ
(
    constraint purldgs_typ_pk
        primary key (orgid, dt, acct)
) inherits (public.ldgs)
  tablespace sup;

comment on table public.purldgs_typ is 'purchase ledgers by type';

comment on column public.purldgs_typ.orgid is 'hubid that handles the purchase';

comment on column public.purldgs_typ.xorgid is 'supid of that provides the lot';

alter table public.purldgs_typ
    owner to postgres;

create table public.puraps
(
    level  smallint not null,
    orgid  integer  not null,
    dt     date     not null,
    trans  integer,
    amt    money,
    rate   smallint,
    topay  money,
    xorgid integer,
    constraint puraps_pk
        primary key (level, orgid, dt)
)
    tablespace sup;

comment on table public.puraps is 'purchase accounts payable';

alter table public.puraps
    owner to postgres;

create table public.buygens
(
    till    date not null
        constraint buygens_pk
            primary key,
    last    date not null,
    started timestamp(0),
    ended   timestamp(0),
    opr     varchar(12),
    amt     money
)
    tablespace rtl;

comment on table public.buygens is 'buy generations';

alter table public.buygens
    owner to postgres;

create table public.purgens
(
    till    date not null
        constraint purgens_pk
            primary key,
    last    date not null,
    started timestamp(0),
    ended   timestamp(0),
    opr     varchar(12),
    amt     money
)
    tablespace sup;

comment on table public.purgens is 'purchase generations';

alter table public.purgens
    owner to postgres;

create table public.tests
(
    id    serial
        constraint tests_pk
            primary key,
    estid integer not null
        constraint tests_parentid_fk
            references public.orgs,
    orgid integer not null
        constraint tests_orgid_fk
            references public.orgs,
    val   numeric,
    level smallint
)
    inherits (public.entities);

alter table public.tests
    owner to postgres;

create table public.codes
(
    id     integer default nextval('jobs_id_seq'::regclass) not null
        constraint codes_pk
            primary key,
    orgid  integer
        constraint codes_orgid_fk
            references public.orgs,
    num    integer,
    nstart integer,
    nend   integer,
    cnt    integer,
    aided  timestamp(0),
    aider  varchar(10)
)
    inherits (public.entities);

alter table public.codes
    owner to postgres;

create table public.peers
(
    uri        varchar(50),
    credential varchar(32)
)
    inherits (public.entities);

alter table public.peers
    owner to postgres;

create table public.flows
(
    id     integer default nextval('lotops_id_seq'::regclass) not null
        constraint flows_pk
            primary key,
    orgid  integer
        constraint flows_orgid_fk
            references public.orgs,
    itemid integer,
    hubid  integer
        constraint flows_hubid_fk
            references public.orgs,
    qty    integer,
    nstart integer,
    nend   integer,
    srcid  integer,
    tagtyp smallint
)
    inherits (public.entities);

comment on table public.flows is 'goods flow operations';

alter table public.flows
    owner to postgres;

create table public.syms
(
    idx   smallint,
    style smallint,
    constraint syms_pk
        primary key (typ)
)
    inherits (public.entities);

comment on table public.syms is 'symbols';

alter table public.syms
    owner to postgres;

create table public.lots
(
    id     integer default nextval('wares_id_seq'::regclass) not null
        constraint lots_pk
            primary key,
    orgid  integer                                           not null
        constraint lots_orgid_fk
            references public.orgs,
    itemid integer                                           not null,
    hubid  integer                                           not null
        constraint lots_hubid_fk
            references public.orgs,
    stock  integer,
    area   smallint
)
    inherits (public.entities);

alter table public.lots
    owner to postgres;

create table public.envs
(
    idx   smallint,
    style smallint,
    constraint envs_pk
        primary key (typ)
)
    inherits (public.entities);

alter table public.envs
    owner to postgres;

create table public.tags
(
    idx   smallint,
    style smallint,
    constraint tags_pk
        primary key (typ)
)
    inherits (public.entities);

alter table public.tags
    owner to postgres;

create table public.cers
(
    idx   smallint,
    style smallint,
    constraint cers_pk
        primary key (typ)
)
    inherits (public.entities);

comment on table public.cers is 'certifications';

alter table public.cers
    owner to postgres;


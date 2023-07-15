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
    idx  smallint,
    size smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (public.entities);

comment on table public.cats is 'categories';

alter table public.cats
    owner to postgres;

create table public.regs
(
    id     smallint not null
        constraint regs_pk
            primary key,
    idx    smallint,
    catmsk smallint
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
    upperid      integer
        constraint orgs_upperid_fk
            references public.orgs,
    hubid        integer
        constraint orgs_hubid_fk
            references public.orgs,
    cover        varchar(12),
    legal        varchar(20),
    regid        smallint not null
        constraint orgs_regid_fk
            references public.regs,
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
    carb         money,
    icon         bytea,
    pic          bytea,
    m1           bytea,
    m2           bytea,
    m3           bytea,
    scene        bytea
)
    inherits (public.entities);

comment on table public.orgs is 'organizational units';

alter table public.orgs
    owner to postgres;

create index orgs_upperidstu_idx
    on public.orgs (upperid, status);

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

create index users_rtlid_idx
    on public.users (rtlid)
    where (rtlid > 0);

create index users_supid_idx
    on public.users (supid)
    where (supid > 0);

create index users_vip_idx
    on public.users using gin (vip);

create table public.evals
(
    id    integer default nextval('tests_id_seq'::regclass) not null
        constraint evals_pk
            primary key,
    orgid integer
        constraint evals_orgid_fk
            references public.orgs,
    level integer
)
    inherits (public.entities);

alter table public.evals
    owner to postgres;

create table public.fabs
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
    inherits (public.entities);

comment on table public.fabs is 'fabrications of product lots';

alter table public.fabs
    owner to postgres;

create table public.lots
(
    id      serial
        constraint lots_pk
            primary key,
    orgid   integer,
    fabid   integer
        constraint lots_fabid_fk
            references public.fabs,
    cattyp  smallint
        constraint lots_cattyp_fk
            references public.cats
            on update cascade on delete restrict,
    started date,
    unit    varchar(4),
    unitw   smallint default 0 not null,
    unitx   smallint,
    price   money,
    "off"   money,
    capx    integer,
    minx    smallint,
    maxx    smallint,
    flashx  smallint,
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

comment on table public.lots is 'supply product lots';

comment on column public.lots.unitw is 'unit weight';

alter table public.lots
    owner to postgres;

create table public.purs
(
    id     bigint   default nextval('books_id_seq'::regclass) not null
        constraint purs_pk
            primary key,
    rtlid  integer                                            not null
        constraint purs_rtlid_fk
            references public.orgs,
    mktid  integer                                            not null
        constraint purs_mktid_fk
            references public.orgs,
    hubid  integer                                            not null
        constraint purs_ctrid_fk
            references public.orgs,
    supid  integer                                            not null
        constraint purs_supid_fk
            references public.orgs,
    ctrid  integer,
    lotid  integer
        constraint purs_lotid_fk
            references public.lots,
    unit   varchar(4),
    unitw  smallint default 0                                 not null,
    unitx  smallint,
    price  money,
    "off"  money,
    qty    integer,
    fee    money,
    topay  money,
    pay    money,
    ret    integer,
    refund money,
    constraint typ_chk
        check ((typ >= 1) AND (typ <= 2))
)
    inherits
(
    public.entities
)tablespace sup ;

comment on table public.purs is 'supply purchases';

comment on column public.purs.unitw is 'unit weight';

alter table public.purs
    owner to postgres;

create index purs_ctridstatus_idx
    on public.purs (hubid, status)
    tablespace sup;

create index purs_rtlidstatus_idx
    on public.purs (rtlid, status)
    tablespace sup;

create index purs_supidstatus_idx
    on public.purs (supid, status)
    tablespace sup;

create index purs_mktidstatus_idx
    on public.purs (mktid, status)
    tablespace sup;

create index lots_orgidstatustyp_idx
    on public.lots (orgid, status, typ);

create index lots_statuscattyp_idx
    on public.lots (status, cattyp);

create index fabs_orgidstatus_idx
    on public.fabs (orgid, status);

create table public.buys
(
    id     bigserial
        constraint buys_pk
            primary key,
    rtlid  integer not null
        constraint buys_rtlid_fk
            references public.orgs,
    mktid  integer not null
        constraint buys_mkt_fk
            references public.orgs,
    uid    integer
        constraint buys_uid_fk
            references public.users,
    uname  varchar(12),
    utel   varchar(11),
    ucom   varchar(12),
    uaddr  varchar(30),
    uim    varchar(28),
    items  buyitem[],
    fee    money,
    topay  money,
    pay    money,
    ret    numeric(6, 1),
    refund money,
    constraint buys_chk
        check ((typ >= 1) AND (typ <= 3))
)
    inherits
(
    public.entities
)tablespace rtl ;

comment on table public.buys is 'retail buys';

alter table public.buys
    owner to postgres;

create index buys_uidstatustyp_idx
    on public.buys (uid, status, typ)
    tablespace rtl;

create index buys_mktidstatustyp_idx
    on public.buys (mktid, status, typ)
    where (((status = 1) OR (status = 2)) AND (typ = 1)) tablespace rtl;

create index buys_rtlidtypstatus_idx
    on public.buys (rtlid asc, typ asc, status asc, oked desc);

create trigger buys_trig
    after insert or update
        of status
    on public.buys
    for each row
execute procedure public.buys_trig_func();

create table public.items
(
    id    serial
        constraint items_pk
            primary key,
    orgid integer            not null
        constraint items_rtlid_fk
            references public.orgs,
    lotid integer
        constraint items_lotid_fk
            references public.lots,
    rank  smallint,
    unit  varchar(4),
    unitw smallint default 0 not null,
    step  smallint,
    price money,
    "off" money,
    max   smallint,
    min   smallint default 0 not null,
    stock smallint default 0 not null,
    ops   stockop[],
    icon  bytea,
    pic   bytea,
    promo boolean,
    constraint items_chk
        check (typ = ANY (ARRAY [1, 2]))
)
    inherits (public.entities);

comment on table public.items is 'retail items';

comment on column public.items.unitw is 'unit weight';

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
    xorgid integer
)
    tablespace rtl;

comment on table public.buyaps is 'buy accounts payable';

alter table public.buyaps
    owner to postgres;

create table public.vans
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
    inherits (public.entities);

alter table public.vans
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
(
    public.ldgs
)tablespace rtl ;

comment on table public.buyldgs_itemid is 'buy ledgers by itemid';

alter table public.buyldgs_itemid
    owner to postgres;

create table public.buyldgs_typ
(
    constraint buyldgs_typ_pk
        primary key (orgid, dt, acct)
)
   inherits
(
    public.ldgs
) tablespace rtl ;

comment on table public.buyldgs_typ is 'buy ledgers by type';

alter table public.buyldgs_typ
    owner to postgres;

create table public.purldgs_lotid
(
    constraint purldgs_lotid_pk
        primary key (orgid, acct, dt)
)
    inherits
(
    public.ldgs
)tablespace sup ;

comment on table public.purldgs_lotid is 'purchase ledgers by lotid';

comment on column public.purldgs_lotid.orgid is 'supid of that provides the lot';

comment on column public.purldgs_lotid.xorgid is 'the parentid ';

alter table public.purldgs_lotid
    owner to postgres;

create table public.purldgs_hub_lotid
(
    constraint purldgs_typ_pk
        primary key (orgid, dt, acct)
)
    inherits
(
    public.ldgs
)tablespace sup ;

comment on table public.purldgs_hub_lotid is 'purchase ledgers by type';

comment on column public.purldgs_hub_lotid.orgid is 'hubid that handles the purchase';

comment on column public.purldgs_hub_lotid.xorgid is 'supid of that provides the lot';

alter table public.purldgs_hub_lotid
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
    xorgid integer
)
    tablespace sup;

comment on table public.puraps is 'purchaes accounts payable';

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
);

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
);

comment on table public.purgens is 'purchase generations';

alter table public.purgens
    owner to postgres;

create table public.lotinvs
(
    lotid integer not null
        constraint lotinvs_lotid_fk
            references public.lots,
    hubid integer not null
        constraint lotinvs_hubid_fk
            references public.orgs,
    stock integer not null,
    constraint lotinvs_pk
        primary key (lotid, hubid)
);

alter table public.lotinvs
    owner to postgres;

create table public.carbs
(
    orgid integer not null,
    dt    date    not null,
    typ   integer,
    amt   money,
    rate  smallint,
    topay money
);

alter table public.carbs
    owner to postgres;

create table public.progs
(
    userid integer not null,
    bal    money,
    constraint progs_pk
        primary key (userid, typ)
)
    inherits (public.entities);

alter table public.progs
    owner to postgres;


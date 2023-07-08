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
    id   smallint not null
        constraint cats_pk
            primary key,
    idx  smallint,
    size smallint
)
    inherits (public.entities);

comment on table public.cats is 'categories';

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

comment on table public.regs is 'regions';

alter table public.regs
    owner to postgres;

create table public.orgs
(
    id       serial
        constraint orgs_pk
            primary key,
    extid    integer
        constraint orgs_prtid_fk
            references public.orgs,
    ctrid    integer
        constraint orgs_ctrid_fk
            references public.orgs,
    ext      varchar(12),
    legal    varchar(20),
    regid    smallint
        constraint orgs_regid_fk
            references public.regs,
    addr     varchar(30),
    x        double precision,
    y        double precision,
    tel      varchar(11),
    trust    boolean,
    link     varchar(30),
    credit   smallint,
    bankacct varchar(20),
    specs    jsonb,
    icon     bytea,
    pic      bytea,
    m1       bytea,
    m2       bytea,
    m3       bytea,
    scene    bytea,
    opened   time(0),
    closed   time(0)
)
    inherits (public.entities);

comment on table public.orgs is 'organizational units';

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

create table public.lots
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
            references public.cats,
    started date,
    unit    varchar(4),
    unitw   smallint default 0 not null,
    unitx   smallint,
    price   money,
    "off"   money,
    capx    integer,
    stock   integer,
    avail   integer,
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
    id      bigint   default nextval('books_id_seq'::regclass) not null
        constraint purs_pk
            primary key,
    rtlid   integer                                            not null
        constraint purs_rtlid_fk
            references public.orgs,
    rtlname varchar(12),
    mktid   integer                                            not null
        constraint purs_mktid_fk
            references public.orgs,
    ctrid   integer                                            not null
        constraint purs_ctrid_fk
            references public.orgs,
    supid   integer                                            not null
        constraint purs_supid_fk
            references public.orgs,
    supname varchar(12),
    lotid   integer
        constraint purs_lotid_fk
            references public.lots,
    unit    varchar(4),
    unitw   smallint default 0                                 not null,
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
    public.entities
)tablespace sup ;

comment on table public.purs is 'supply purchases';

comment on column public.purs.unitw is 'unit weight';

alter table public.purs
    owner to postgres;

create unique index purs_uidx
    on public.purs (rtlid, status)
    where (status = '-1'::integer) tablespace sup;

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

create index lots_srcidstatus_idx
    on public.lots (orgid, status);

create index lots_catid_idx
    on public.lots (catid);

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
    public.entities
)tablespace rtl ;

comment on table public.buys is 'retail buys';

alter table public.buys
    owner to postgres;

create index buys_uidstatus_idx
    on public.buys (uid, status)
    tablespace rtl;

create index buys_rtlidstatus_idx
    on public.buys (rtlid, status)
    tablespace rtl;

create index buys_mktidstatus_idx
    on public.buys (mktid, status)
    tablespace rtl;

create unique index buys_uidx
    on public.buys (uid, typ, status)
    where ((typ = 1) AND (status = '-1'::smallint)) tablespace rtl;

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
    catid smallint
        constraint items_catid_fk
            references public.cats,
    unit  varchar(4),
    unitw smallint default 0 not null,
    step  smallint,
    price money,
    "off" money,
    max   smallint,
    stock smallint default 0 not null,
    avail smallint default 0 not null,
    flash smallint default 0 not null,
    ops   stockop[],
    icon  bytea,
    pic   bytea
)
    inherits (public.entities);

comment on table public.items is 'retail items';

comment on column public.items.unitw is 'unit weight';

alter table public.items
    owner to postgres;

create index items_catid_idx
    on public.items (rank);

create table public.buygens
(
    till    date not null
        constraint buygens_pk
            primary key,
    started timestamp(0),
    ended   timestamp(0),
    opr     varchar(12),
    amt     money
);

comment on table public.buygens is 'buy generations';

alter table public.buygens
    owner to postgres;

create table public.buyaps
(
    level smallint not null,
    orgid integer  not null,
    dt    date     not null,
    trans integer,
    amt   money,
    rate  smallint,
    topay money
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

create table public.carbs
(
    userid integer not null,
    dt     date    not null,
    typ    smallint,
    v      money,
    opr    varchar(10),
    constraint carbs_pk
        primary key (userid, dt)
);

comment on table public.carbaps is 'carbon credits operations';

alter table public.carbaps
    owner to postgres;

create table public.purgens
(
    till    date not null
        constraint purgens_pk
            primary key,
    started timestamp(0),
    ended   timestamp(0),
    opr     varchar(12),
    amt     money
);

comment on table public.purgens is 'purchase generations';

alter table public.purgens
    owner to postgres;

create table public.mcards
(
);

alter table public.mcards
    owner to postgres;

create table public.ldgs
(
    orgid   integer not null,
    dt      date    not null,
    acct    integer not null,
    name    varchar(12),
    coorgid integer,
    trans   integer,
    qty     integer,
    amt     money
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
) tablespace rtl ;

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

alter table public.purldgs_lotid
    owner to postgres;

create table public.purldgs_typ
(
    constraint purldgs_typ_pk
        primary key (orgid, dt, acct)
)
    inherits
(
    public.ldgs
)tablespace sup ;

comment on table public.purldgs_hub_lotid is 'purchase ledgers by type';

alter table public.purldgs_hub_lotid
    owner to postgres;

create table public.puraps
(
    level smallint not null,
    orgid integer  not null,
    dt    date     not null,
    trans integer,
    amt   money,
    rate  smallint,
    topay money
)
    tablespace sup;

comment on table public.puraps is 'purchaes accounts payable';

alter table public.puraps
    owner to postgres;


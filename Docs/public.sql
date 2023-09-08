create sequence evals_id_seq;

alter sequence evals_id_seq owner to postgres;

create sequence purs_id_seq;

alter sequence purs_id_seq owner to postgres;

create sequence srcs_id_seq;

alter sequence srcs_id_seq owner to postgres;

create type stockop as
(
    dt    timestamp(0),
    qty   integer,
    stock integer,
    typ   smallint,
    by    varchar(10),
    hub   integer
);

alter type stockop owner to postgres;

create type buyitem as
(
    itemid integer,
    lotid  integer,
    name   varchar(12),
    unit   varchar(4),
    unitw  smallint,
    price  money,
    "off"  money,
    qty    numeric(6, 1)
);

alter type buyitem owner to postgres;

create table entities
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

comment on table entities is 'abstract entities';

alter table entities
    owner to postgres;

create table cats
(
    idx  smallint,
    size smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (entities);

comment on table cats is 'categories';

alter table cats
    owner to postgres;

create table regs
(
    id     smallint not null
        constraint regs_pk
            primary key,
    idx    smallint,
    catmsk smallint
)
    inherits (entities);

comment on table regs is 'regions';

alter table regs
    owner to postgres;

create index regs_typidx_idx
    on regs (typ, idx);

create table orgs
(
    id           serial
        constraint orgs_pk
            primary key,
    upperid      integer
        constraint orgs_upperid_fk
            references orgs,
    hubid        integer
        constraint orgs_hubid_fk
            references orgs,
    cover        varchar(12),
    legal        varchar(20),
    regid        smallint not null
        constraint orgs_regid_fk
            references regs,
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
    scene        bytea,
    constraint orgs_chk
        check ((typ >= 1) AND (typ <= 14))
)
    inherits (entities);

comment on table orgs is 'organizational units';

alter table orgs
    owner to postgres;

create index orgs_upperidstu_idx
    on orgs (upperid, status);

create table users
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
            references orgs,
    suply      smallint default 0 not null,
    rtlid      integer
        constraint users_rtlid_fk
            references orgs,
    rtlly      smallint,
    vip        integer[],
    agreed     date,
    icon       bytea
)
    inherits (entities);

alter table users
    owner to postgres;

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create unique index users_tel_idx
    on users (tel);

create index users_rtlid_idx
    on users (rtlid)
    where (rtlid > 0);

create index users_supid_idx
    on users (supid)
    where (supid > 0);

create index users_vip_idx
    on users using gin (vip);

create table evals
(
    id      integer default nextval('tests_id_seq'::regclass) not null
        constraint evals_pk
            primary key,
    orgid   integer
        constraint evals_orgid_fk
            references orgs,
    level   integer,
    upperid integer
)
    inherits (entities);

alter table tests
    owner to postgres;

alter sequence evals_id_seq owned by tests.id;

create table srcs
(
    id     integer default nextval('assets_id_seq'::regclass) not null
        constraint srcs_pk
            primary key,
    orgid  integer,
    rank   smallint,
    remark varchar(200),
    co2ekg money,
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
    inherits (entities);

comment on table srcs is 'sources of product lots';

alter table srcs
    owner to postgres;

alter sequence srcs_id_seq owned by srcs.id;

create table lots
(
    id     serial
        constraint lots_pk
            primary key,
    orgid  integer,
    srcid  integer
        constraint lots_srcid_fk
            references srcs,
    cattyp smallint
        constraint lots_cattyp_fk
            references cats
            on update cascade on delete restrict,
    shipon date,
    unit   varchar(4),
    unitw  smallint default 0 not null,
    unitx  smallint,
    price  money,
    "off"  money,
    stock  integer,
    min    integer,
    max    integer,
    cap    integer,
    nstart integer,
    nend   integer,
    ops    stockop[],
    icon   bytea,
    pic    bytea,
    m1     bytea,
    m2     bytea,
    m3     bytea,
    m4     bytea,
    linka  varchar(60),
    linkb  varchar(60),
    constraint lots_chk
        check ((typ >= 1) AND (typ <= 2))
)
    inherits (entities);

comment on table lots is 'supply product lots';

comment on column lots.unitw is 'unit weight';

alter table lots
    owner to postgres;

create table purs
(
    id       bigint   default nextval('books_id_seq'::regclass) not null
        constraint purs_pk
            primary key,
    rtlid    integer                                            not null
        constraint purs_rtlid_fk
            references orgs,
    mktid    integer                                            not null
        constraint purs_mktid_fk
            references orgs,
    hubid    integer                                            not null
        constraint purs_ctrid_fk
            references orgs,
    supid    integer                                            not null
        constraint purs_supid_fk
            references orgs,
    ctrid    integer                                            not null,
    lotid    integer
        constraint purs_lotid_fk
            references lots,
    unit     varchar(4),
    unitw    smallint default 0                                 not null,
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
(
    entities
)tablespace sup ;

comment on table purs is 'supply purchases';

comment on column purs.unitw is 'unit weight';

alter table purs
    owner to postgres;

alter sequence purs_id_seq owned by purs.id;

create index purs_supidstatustyp_idx
    on purs (supid, status, typ)
    tablespace sup;

create index purs_hubidstatusmktid_idx
    on purs (hubid, status, mktid) tablespace sup
    where ((typ = 1) AND ((status = 1) OR (status = 2)));

create index purs_mktidstatustyp_idx
    on purs (mktid, status, typ) tablespace sup
    where ((status = 2) OR (status = 4));

create index purs_rtlidstatustyp_idx
    on purs (rtlid, status, typ)
    tablespace sup;

create index lots_orgidstatustyp_idx
    on lots (orgid, status, typ);

create index lots_statuscattyp_idx
    on lots (status, cattyp);

create index srcs_orgidstatus_idx
    on srcs (orgid, status);

create table buys
(
    id       serial
        constraint buys_pk
            primary key,
    rtlid    integer not null
        constraint buys_rtlid_fk
            references orgs,
    mktid    integer not null
        constraint buys_mkt_fk
            references orgs,
    uid      integer
        constraint buys_uid_fk
            references users,
    uname    varchar(12),
    utel     varchar(11),
    ucom     varchar(12),
    uaddr    varchar(30),
    uim      varchar(28),
    items    buyitem[],
    fee      money,
    topay    money,
    pay      money,
    ret      numeric(6, 1),
    refund   money,
    refunder varchar(10),
    constraint buys_chk
        check ((typ >= 1) AND (typ <= 2))
)
    inherits
(
    entities
)tablespace rtl ;

comment on table buys is 'retail buys';

alter table buys
    owner to postgres;

create index buys_rtlidstatustyp_idx
    on buys (rtlid asc, status asc, typ asc, oked desc)
    tablespace rtl;

create index buys_gen_idx
    on buys (status asc, oked desc, rtlid asc, typ asc) tablespace rtl
    where ((status = 4) AND (typ = 1));

create index buys_uidstatus_idx
    on buys (uid, status)
    tablespace rtl;

create index buys_mktidstatustypucomoked_idx
    on buys (mktid asc, status asc, typ asc, ucom asc, oked desc) tablespace rtl
    where ((typ = 1) AND (adapter IS NOT NULL));

create table items
(
    id    serial
        constraint items_pk
            primary key,
    orgid integer            not null
        constraint items_rtlid_fk
            references orgs,
    lotid integer
        constraint items_lotid_fk
            references lots,
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
        check ((typ >= 1) AND (typ <= 2))
)
    inherits (entities);

comment on table items is 'retail items';

comment on column items.unitw is 'unit weight';

alter table items
    owner to postgres;

create index items_orgidstu_idx
    on items (orgid, status);

create table buyaps
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

comment on table buyaps is 'buy accounts payable';

alter table buyaps
    owner to postgres;

create table ldgs
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

alter table ldgs
    owner to postgres;

create table buyldgs_itemid
(
    constraint buyldgs_itemid_pk
        primary key (orgid, dt, acct)
)
    inherits
(
    ldgs
)tablespace rtl ;

comment on table buyldgs_itemid is 'buy ledgers by itemid';

alter table buyldgs_itemid
    owner to postgres;

create table buyldgs_typ
(
    constraint buyldgs_typ_pk
        primary key (orgid, dt, acct)
)
    inherits
(
    ldgs
)tablespace rtl ;

comment on table buyldgs_typ is 'buy ledgers by type';

alter table buyldgs_typ
    owner to postgres;

create table purldgs_lotid
(
    constraint purldgs_lotid_pk
        primary key (orgid, acct, dt)
)
    inherits
(
    ldgs
)tablespace sup ;

comment on table purldgs_lotid is 'purchase ledgers by lotid';

comment on column purldgs_lotid.orgid is 'supid of that provides the lot';

comment on column purldgs_lotid.xorgid is 'the parentid ';

alter table purldgs_lotid
    owner to postgres;

create table purldgs_typ
(
    constraint purldgs_typ_pk
        primary key (orgid, dt, acct)
)
     inherits
(
    ldgs
)tablespace sup;

comment on table purldgs_typ is 'purchase ledgers by type';

comment on column purldgs_typ.orgid is 'hubid that handles the purchase';

comment on column purldgs_typ.xorgid is 'supid of that provides the lot';

alter table purldgs_typ
    owner to postgres;

create table puraps
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

comment on table puraps is 'purchaes accounts payable';

alter table puraps
    owner to postgres;

create table buygens
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

comment on table buygens is 'buy generations';

alter table buygens
    owner to postgres;

create table purgens
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

comment on table purgens is 'purchase generations';

alter table purgens
    owner to postgres;

create table lotinvs
(
    lotid integer not null
        constraint lotinvs_lotid_fk
            references lots,
    hubid integer not null
        constraint lotinvs_hubid_fk
            references orgs,
    stock integer not null,
    constraint lotinvs_pk
        primary key (lotid, hubid)
);

alter table lotinvs
    owner to postgres;

create table carbs
(
    orgid integer not null,
    dt    date    not null,
    typ   integer,
    amt   money,
    rate  smallint,
    topay money
);

alter table carbs
    owner to postgres;

create table progs
(
    userid integer not null,
    bal    money,
    constraint progs_pk
        primary key (userid, typ)
)
    inherits (entities);

alter table progs
    owner to postgres;

create view users_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, tel, addr, im, credential,
             admly, supid, suply, rtlid, rtlly, vip, agreed, icon)
as
SELECT o.typ,
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
       o.tel,
       o.addr,
       o.im,
       o.credential,
       o.admly,
       o.supid,
       o.suply,
       o.rtlid,
       o.rtlly,
       o.vip,
       o.agreed,
       o.icon IS NOT NULL AS icon
FROM users o;

alter table users_vw
    owner to postgres;

create view orgs_vw
            (typ, name, tip, created, creator, adapted, adapter, oker, oked, status, id, upperid, hubid, cover, legal,
             regid, addr, x, y, tel, trust, descr, bankacctname, bankacct, specs, openat, closeat, rank, carb, icon,
             pic, m1, m2, m3, scene)
as
SELECT o.typ,
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
       o.upperid,
       o.hubid,
       o.cover,
       o.legal,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.trust,
       o.descr,
       o.bankacctname,
       o.bankacct,
       o.specs,
       o.openat,
       o.closeat,
       o.rank,
       o.carb,
       o.icon IS NOT NULL  AS icon,
       o.pic IS NOT NULL   AS pic,
       o.m1 IS NOT NULL    AS m1,
       o.m2 IS NOT NULL    AS m2,
       o.m3 IS NOT NULL    AS m3,
       o.scene IS NOT NULL AS scene
FROM orgs o;

alter table orgs_vw
    owner to postgres;

create view items_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, lotid, rank, unit,
             unitw, price, "off", promo, step, max, min, stock, ops, icon, pic)
as
SELECT o.typ,
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
       o.orgid,
       o.lotid,
       o.rank,
       o.unit,
       o.unitw,
       o.price,
       o.off,
       o.promo,
       o.step,
       o.max,
       o.min,
       o.stock,
       o.ops,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic
FROM items o;

alter table items_vw
    owner to postgres;

create view lots_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, srcid, cattyp, shipon,
             unit, unitw, unitx, price, "off", stock, min, max, cap, nstart, nend, linka, linkb, ops, icon, pic, m1, m2,
             m3, m4)
as
SELECT o.typ,
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
       o.orgid,
       o.srcid,
       o.cattyp,
       o.shipon,
       o.unit,
       o.unitw,
       o.unitx,
       o.price,
       o.off,
       o.stock,
       o.min,
       o.max,
       o.cap,
       o.nstart,
       o.nend,
       o.linka,
       o.linkb,
       o.ops,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM lots o;

alter table lots_vw
    owner to postgres;

create view srcs_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, rank, remark, co2ekg, x,
             y, specs, icon, pic, m1, m2, m3, m4)
as
SELECT o.typ,
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
       o.orgid,
       o.rank,
       o.remark,
       o.co2ekg,
       o.x,
       o.y,
       o.specs,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM srcs o;

alter table srcs_vw
    owner to postgres;

create function first_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as
$$SELECT $1$$;

alter function first_agg(anyelement, anyelement) owner to postgres;

create function last_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as
$$SELECT $2$$;

alter function last_agg(anyelement, anyelement) owner to postgres;

create function buys_trig_func() returns trigger
    language plpgsql
as
$$
DECLARE
    itm buyitem;
BEGIN
    -- update stock values
    IF (TG_OP = 'INSERT' AND NEW.status = 4) THEN

        FOREACH itm IN ARRAY NEW.items LOOP -- oked
        UPDATE items SET stock = stock - itm.qty WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 1 AND OLD.status < 1) THEN -- paid

        FOREACH itm IN ARRAY NEW.items LOOP
                UPDATE items SET stock = stock - itm.qty WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 0 AND OLD.status > 0) THEN -- voided

        FOREACH itm IN ARRAY NEW.items LOOP
                UPDATE items SET stock = stock + itm.qty WHERE id = itm.itemid;
            END LOOP;

    END IF;

    RETURN NEW;
END
$$;

alter function buys_trig_func() owner to postgres;

create trigger buys_trig
    after insert or update
        of status
    on buys
    for each row
execute procedure buys_trig_func();

create function buygen(till date, opr character varying) returns void
    language plpgsql
as
$$
DECLARE
    now timestamp(0) = localtimestamp(0);

    last date;
    tillstamp timestamp(0);
    laststamp timestamp(0);

    LVL_BIZ constant int = 1;
    LVL_PRT constant int = 2;

    RATE constant int = 97;

BEGIN

    -- apply default parameter values if needed
    opr = coalesce(opr, 'SYS');
    till = coalesce(till, now::date - interval '1 day');

    -- adjust parameters
    SELECT coalesce(max(buygens.till), '2000-01-01'::date) FROM buygens INTO last;
    IF (till <= last) THEN
        RETURN;
    END IF;

    laststamp = (last + interval '1 day')::timestamp(0);
    tillstamp = (till + interval '1 day')::timestamp(0);

    -- aggregate by typ
    INSERT INTO buyldgs_typ
    SELECT rtlid,
           till,
           typ,
           CASE WHEN typ = 1 THEN '网售' WHEN typ = 2 THEN '场售' END,
           first(mktid),
           count(*),
           NULL,
           sum(CASE WHEN pay = coalesce(refund, 0::money) THEN 0::money ELSE pay - coalesce(refund, 0::money) - coalesce(fee, 0::money) END)
    FROM buys
    WHERE
            status = 4 AND oked >= laststamp AND oked < tillstamp
    GROUP BY rtlid, typ;

    -- aggregate by itemid
    INSERT INTO buyldgs_itemid
    SELECT
        (unnest(buys_agg(items,rtlid, till,mktid))).*
    FROM buys
    WHERE
            status = 4 AND oked >= laststamp AND oked < tillstamp
    GROUP BY rtlid;

    -- close the buys 
    UPDATE buys
    SET status = 8
    WHERE
            status = 4 AND oked >= laststamp AND oked < tillstamp;

    -- post to accounts payable level 1
    INSERT INTO buyaps
    SELECT
        LVL_BIZ,
        orgid,
        till,
        sum(trans),
        sum(amt),
        RATE,
        sum(amt * RATE / 100),
        first(xorgid)
    FROM buyldgs_typ
    WHERE
            acct = 1 AND dt > last AND dt <= till
    GROUP BY orgid;


    INSERT INTO buygens
    (till, last, started, ended, opr)
    VALUES
        (till, last, now, localtimestamp(0), opr);
END
$$;

alter function buygen(date, varchar) owner to postgres;

create function purgen(till date, opr character varying) returns void
    language plpgsql
as
$$
DECLARE

    now timestamp(0) = localtimestamp(0);

    last date;
    tillstamp timestamp(0);
    laststamp timestamp(0);

    LVL_BIZ constant int = 1;
    LVL_FEE constant int = 2;

    RATE constant int = 97;

BEGIN

    -- apply default parameter values if needed
    opr = coalesce(opr, 'SYS');
    till = coalesce(till, now::date - interval '1 day');

    -- adjust parameters
    SELECT coalesce(max(purgens.till), '2000-01-01'::date) FROM purgens INTO last;
    IF (till <= last) THEN
        RETURN;
    END IF;

    laststamp = (last + interval '1 day')::timestamp(0);
    tillstamp = (till + interval '1 day')::timestamp(0);


    -- aggregate by lotid
    INSERT INTO purldgs_typ
    SELECT supid,
           till,
           typ,
           CASE WHEN typ = 1 THEN '品控仓' WHEN typ = 2 THEN '产源' END,
           first(ctrid),
           count(*),
           sum(qty - ret),
           sum(CASE WHEN pay = coalesce(refund, 0::money) THEN 0::money ELSE pay - coalesce(refund, 0::money) - coalesce(fee, 0::money) END)
    FROM purs
    WHERE
            status = 4 AND oked >= laststamp AND oked < tillstamp
    GROUP BY
        supid, typ;

    -- aggregate by lotid
    INSERT INTO purldgs_lotid
    SELECT supid,
           till,
           lotid,
           first(name),
           first(ctrid),
           count(*),
           sum(qty - ret),
           sum(CASE WHEN pay = coalesce(refund, 0::money) THEN 0::money ELSE pay - coalesce(refund, 0::money) - coalesce(fee, 0::money) END)
    FROM purs
    WHERE
            status = 4 AND oked >= laststamp AND oked < tillstamp
    GROUP BY
        supid, lotid;

    -- close the purchases 
    UPDATE purs
    SET status = 8
    WHERE
            status = 4 AND oked >= laststamp AND oked < tillstamp;

    -- accounts payable level 1
    INSERT INTO puraps
    SELECT
        LVL_BIZ,
        orgid,
        till,
        sum(trans),
        sum(amt),
        RATE,
        sum(amt * RATE / 100),
        first(xorgid)
    FROM
        purldgs_typ
    WHERE
            dt > last AND dt <= till
    GROUP BY
        orgid;

    INSERT INTO purgens (till, last, started, ended, opr)
    VALUES (till, last, now, localtimestamp(0), opr);

END
$$;

alter function purgen(date, varchar) owner to postgres;

create function buys_agg_func(ret ldgs[], items buyitem[], orgid integer, dt date, xorgid integer) returns ldgs[]
    language plpgsql
as
$$
DECLARE
    agg ldgs;
    itm buyitem;
    fnd bool;
BEGIN

    FOREACH itm IN ARRAY items LOOP

            fnd = FALSE;

            IF ret IS NOT NULL THEN
                FOREACH agg IN ARRAY ret LOOP
                        IF agg.acct = itm.itemid THEN -- found
                            agg.trans = agg.trans + 1;
                            agg.qty = agg.qty + itm.qty;
                            agg.amt = agg.amt + (itm.price - itm.off) * itm.qty;
                            fnd = TRUE;
                            CONTINUE;
                        END IF;
                    END LOOP;
            END IF;

            IF ret IS NULL OR NOT fnd THEN
                agg = (orgid, dt, itm.itemid, itm.name, xorgid, 1, itm.qty, (itm.price - itm.off) * itm.qty);
                ret = ret || agg;
            end if;
        END LOOP;

    RETURN ret;
END;
$$;

alter function buys_agg_func(ldgs[], buyitem[], integer, date, integer) owner to postgres;

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

create aggregate buys_agg(items buyitem[], orgid integer, dt date, xorgid integer) (
    sfunc = buys_agg_func,
    stype = ldgs[]
    );

alter aggregate buys_agg(items buyitem[], orgid integer, dt date, xorgid integer) owner to postgres;


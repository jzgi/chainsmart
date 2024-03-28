create sequence purs_id_seq;

alter sequence purs_id_seq owner to postgres;

create sequence tags_id_seq;

alter sequence codes_id_seq owner to postgres;

create sequence lotops_id_seq;

alter sequence bats_id_seq owner to postgres;

create sequence lots_id_seq;

alter sequence lots_id_seq owner to postgres;

create type itemop as
(
    dt    timestamp(0),
    qty   integer,
    stock integer,
    typ   smallint,
    by    varchar(10)
);

alter type itemop owner to postgres;

create type buyln as
(
    itemid integer,
    lotid  integer,
    name   varchar(12),
    unit   varchar(4),
    unitip varchar(8),
    price  money,
    "off"  money,
    qty    numeric(6, 1)
);

alter type buyln owner to postgres;

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
    idx   smallint,
    style smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (entities);

comment on table cats is 'categories';

alter table cats
    owner to postgres;

create table regs
(
    id    smallint not null
        constraint regs_pk
            primary key,
    idx   smallint,
    style smallint
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
    parentid     integer
        constraint orgs_parentid_fk
            references orgs,
    hubid        integer
        constraint orgs_hubid_fk
            references orgs,
    cover        varchar(12),
    legal        varchar(20),
    regid        smallint not null
        constraint orgs_regid_fk
            references regs
            on update cascade,
    addr         varchar(30),
    x            double precision,
    y            double precision,
    tel          varchar(11),
    trust        boolean,
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
    m4           bytea,
    ties         integer[],
    cattyp       smallint,
    symtyp       smallint,
    tagtyp       smallint,
    envtyp       smallint,
    constraint orgs_chk
        check ((typ >= 1) AND (typ <= 27))
)
    inherits (entities);

comment on table orgs is 'organizational units';

alter table orgs
    owner to postgres;

create index orgs_parentidstu_idx
    on orgs (parentid, status);

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
    icon       bytea,
    orgid      integer
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
    on users (mktid)
    where (mktid > 0);

create index users_supid_idx
    on users (supid)
    where (supid > 0);

create index users_vip_idx
    on users using gin (vip);

create table purs
(
    id       bigint     default nextval('books_id_seq'::regclass) not null
        constraint purs_pk
            primary key,
    rtlid    integer                                              not null
        constraint purs_rtlid_fk
            references orgs,
    mktid    integer                                              not null
        constraint purs_mktid_fk
            references orgs,
    hubid    integer                                              not null
        constraint purs_hubid_fk
            references orgs,
    supid    integer                                              not null
        constraint purs_supid_fk
            references orgs,
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
(
    entities
)tablespace sup ;

comment on table purs is 'supply purchases';

alter table purs
    owner to postgres;

alter sequence purs_id_seq owned by purs.id;

create index purs_supidstatustyp_idx
    on purs (supid, status, typ)
    tablespace sup;

create index purs_mktidstatustyp_idx
    on purs (mktid, status, typ) tablespace sup
    where ((status = 2) OR (status = 4));

create index purs_rtlidstatustyp_idx
    on purs (orgid, status, typ)
    tablespace sup;

create index purs_hubidstatustypmktid_idx
    on purs (hubid, status, typ, mktid) tablespace sup
    where ((typ = 1) AND ((status = 1) OR (status = 2)));

create index purs_gen_idx
    on purs (status, oked, supid) tablespace sup
    where (status = 4);

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
(
    entities
)tablespace rtl;

comment on table buys is 'retail buys';

alter table buys
    owner to postgres;

create index buys_rtlidstatustyp_idx
    on buys (orgid asc, status asc, typ asc, oked desc)
    tablespace rtl;

create index buys_gen_idx
    on buys (status asc, oked desc, orgid asc, typ asc) tablespace rtl
    where ((status = 4) AND (typ = 1));

create index buys_uidstatus_idx
    on buys (uid, status)
    tablespace rtl;

create index buys_mktidstatustypucomoked_idx
    on buys (mktid asc, status asc, typ asc, ucom asc, oked desc) tablespace rtl
    where ((typ = 1) AND (adapter IS NOT NULL));

create table items
(
    id     serial
        constraint items_pk
            primary key,
    orgid  integer            not null
        constraint items_rtlid_fk
            references orgs,
    srcid  integer,
    cattyp smallint,
    unit   varchar(4),
    unitip varchar(10),
    unitx  smallint,
    price  money,
    "off"  money,
    max    smallint,
    min    smallint default 0 not null,
    stock  smallint default 0 not null,
    icon   bytea,
    pic    bytea,
    promo  boolean,
    link   varchar(50),
    m1     bytea,
    m2     bytea,
    m3     bytea,
    m4     bytea,
    constraint items_chk
        check ((typ >= 1) AND (typ <= 2))
)
    inherits (entities);

comment on table items is 'retail items';

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
) tablespace rtl ;

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
) tablespace rtl;

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
) tablespace sup;

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
) tablespace sup ;

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

comment on table puraps is 'purchase accounts payable';

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

create table tests
(
    id    serial
        constraint tests_pk
            primary key,
    estid integer not null
        constraint tests_parentid_fk
            references orgs,
    orgid integer not null
        constraint tests_orgid_fk
            references orgs,
    val   numeric,
    level smallint
)
    inherits (entities);

alter table tests
    owner to postgres;

create table codes
(
    id     integer default nextval('jobs_id_seq'::regclass) not null
        constraint codes_pk
            primary key,
    orgid  integer
        constraint codes_orgid_fk
            references orgs,
    num    integer,
    nstart integer,
    nend   integer,
    cnt    integer,
    aided  timestamp(0),
    aider  varchar(10)
)
    inherits (entities);

alter table codes
    owner to postgres;

alter sequence codes_id_seq owned by codes.id;

create table peers
(
    uri        varchar(50),
    credential varchar(32)
)
    inherits (entities);

alter table peers
    owner to postgres;

create table bats
(
    id     integer default nextval('lotops_id_seq'::regclass) not null
        constraint bats_pk
            primary key,
    orgid  integer
        constraint bats_orgid_fk
            references orgs,
    itemid integer,
    hubid  integer
        constraint bats_hubid_fk
            references orgs,
    qty    integer,
    nstart integer,
    nend   integer,
    srcid  integer,
    tagtyp smallint
)
    inherits (entities);

comment on table bats is 'goods flow operations';

alter table bats
    owner to postgres;

alter sequence bats_id_seq owned by bats.id;

create table syms
(
    idx   smallint,
    style smallint,
    constraint syms_pk
        primary key (typ)
)
    inherits (entities);

comment on table syms is 'symbols';

alter table syms
    owner to postgres;

create table lots
(
    id     integer default nextval('wares_id_seq'::regclass) not null
        constraint lots_pk
            primary key,
    orgid  integer                                           not null
        constraint lots_orgid_fk
            references orgs,
    itemid integer                                           not null,
    hubid  integer                                           not null
        constraint lots_hubid_fk
            references orgs,
    stock  integer,
    area   smallint
)
    inherits (entities);

alter table lots
    owner to postgres;

alter sequence lots_id_seq owned by lots.id;

create table envs
(
    idx   smallint,
    style smallint,
    constraint envs_pk
        primary key (typ)
)
    inherits (entities);

alter table envs
    owner to postgres;

create table tags
(
    idx   smallint,
    style smallint,
    constraint tags_pk
        primary key (typ)
)
    inherits (entities);

alter table tags
    owner to postgres;

create table cers
(
    idx   smallint,
    style smallint,
    constraint cers_pk
        primary key (typ)
)
    inherits (entities);

comment on table cers is 'certifications';

alter table cers
    owner to postgres;

create view users_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, tel, addr, im, credential,
             admly, supid, suply, rtlid, rtlly, vip, agreed, orgid, icon)
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
       o.mktid,
       o.mktly,
       o.vip,
       o.agreed,
       o.orgid,
       o.icon IS NOT NULL AS icon
FROM users o;

alter table users_vw
    owner to postgres;

create view items_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, srcid, cattyp, unit,
             unitip, unitx, price, "off", promo, max, min, stock, link, icon, pic, m1, m2, m3, m4)
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
       o.unit,
       o.unitip,
       o.unitx,
       o.price,
       o.off,
       o.promo,
       o.max,
       o.min,
       o.stock,
       o.link,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM items o;

alter table items_vw
    owner to postgres;

create view orgs_vw
            (typ, name, tip, created, creator, adapted, adapter, oker, oked, status, id, parentid, hubid, cover, legal,
             regid, addr, x, y, tel, trust, bankacctname, bankacct, specs, openat, closeat, rank, style, cattyp, symtyp,
             tagtyp, envtyp, ties, icon, pic, m1, m2, m3, m4)
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
       o.parentid,
       o.hubid,
       o.cover,
       o.legal,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.trust,
       o.bankacctname,
       o.bankacct,
       o.specs,
       o.openat,
       o.closeat,
       o.rank,
       o.style,
       o.cattyp,
       o.symtyp,
       o.tagtyp,
       o.envtyp,
       o.ties,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM orgs o;

alter table orgs_vw
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
    itm buyln;
BEGIN
    -- update stock values
    IF (TG_OP = 'INSERT' AND NEW.status = 4) THEN -- pos create

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
           CASE WHEN typ = 1 THEN '网售' WHEN typ = 2 THEN '现金' WHEN typ = 3 THEN '其他' END,
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
           CASE WHEN typ = 1 THEN '云仓' WHEN typ = 2 THEN '产源' END,
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

create function buys_agg_func(ret ldgs[], items buyln[], orgid integer, dt date, xorgid integer) returns ldgs[]
    language plpgsql
as
$$
DECLARE
    agg ldgs;
    itm buyln;
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

alter function buys_agg_func(ldgs[], buyln[], integer, date, integer) owner to postgres;

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

create aggregate buys_agg(items buyln[], orgid integer, dt date, xorgid integer) (
    sfunc = buys_agg_func,
    stype = ldgs[]
    );

alter aggregate buys_agg(items buyln[], orgid integer, dt date, xorgid integer) owner to postgres;


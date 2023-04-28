create sequence tests_id_seq;

alter sequence tests_id_seq owner to postgres;

create type stockop as
(
    dt    timestamp(0),
    tip   varchar(20),
    qty   integer,
    avail integer,
    by    varchar(12)
);

alter type stockop owner to postgres;

create type buyitem as
(
    itemid integer,
    lotid  integer,
    name   varchar(12),
    unit   varchar(4),
    unitx  smallint,
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

alter table entities
    owner to postgres;

create table _ledgs
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

alter table _ledgs
    owner to postgres;

create table cats
(
    id   smallint not null
        constraint cats_pk
            primary key,
    idx  smallint,
    size smallint
)
    inherits (entities);

alter table cats
    owner to postgres;

create table regs
(
    id  smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (entities);

alter table regs
    owner to postgres;

create table orgs
(
    id    serial
        constraint orgs_pk
            primary key,
    prtid integer
        constraint orgs_prtid_fk
            references orgs,
    ctrid integer
        constraint orgs_ctrid_fk
            references orgs,
    ext   varchar(12),
    legal varchar(20),
    regid smallint
        constraint orgs_regid_fk
            references regs,
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
    inherits (entities);

alter table orgs
    owner to postgres;

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
    srcid      smallint
        constraint users_srcid_fk
            references orgs,
    srcly      smallint default 0 not null,
    shpid      integer
        constraint users_shpid_fk
            references orgs,
    shply      smallint,
    vip        integer[]
        constraint users_vip_chk
            check (array_length(vip, 1) <= 4),
    refer      integer
        constraint users_refer_fk
            references users,
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

create index users_shpid_idx
    on users (rtlid)
    where (rtlid > 0);

create index users_srcid_idx
    on users (supid)
    where (supid > 0);

create index users_vip_idx
    on users using gin (vip);

create table evals
(
    id    integer default nextval('tests_id_seq'::regclass) not null
        constraint evals_pk
            primary key,
    orgid integer
        constraint evals_orgid_fk
            references orgs,
    level integer
)
    inherits (entities);

alter table credits
    owner to postgres;

alter sequence tests_id_seq owned by credits.id;

create table _accts
(
    no      varchar(18),
    balance money
)
    inherits (entities);

alter table _accts
    owner to postgres;

create table _asks
(
    acct    varchar(20),
    name    varchar(12),
    amt     integer,
    bal     integer,
    cs      uuid,
    blockcs uuid,
    stamp   timestamp(0)
);

alter table _asks
    owner to postgres;

create table lots
(
    id      serial
        constraint lots_pk
            primary key,
    srcid   integer,
    srcname varchar(12),
    assetid integer,
    targs   integer[],
    catid   smallint
        constraint lots_catsid_fk
            references cats,
    started date,
    unit    varchar(4),
    unitx   smallint,
    price   money,
    "off"   money,
    minx    smallint,
    cap     integer,
    stock   integer,
    avail   integer,
    nstart  integer,
    nend    integer,
    ops     stockop[],
    icon    bytea,
    pic     bytea,
    m1      bytea,
    m2      bytea,
    m3      bytea,
    m4      bytea,
    constraint lots_typ_chk
        check ((typ >= 1) AND (typ <= 2) AND (avail >= 0))
)
    inherits (entities);

alter table lots
    owner to postgres;

create table books
(
    id      bigserial
        constraint books_pk
            primary key,
    shpid   integer not null
        constraint books_shpid_fk
            references orgs,
    shpname varchar(12),
    mktid   integer not null
        constraint books_mktid_fk
            references orgs,
    ctrid   integer not null
        constraint books_ctrid_fk
            references orgs,
    srcid   integer not null
        constraint books_srcid_fk
            references orgs,
    srcname varchar(12),
    lotid   integer
        constraint books_lotid_fk
            references lots,
    unit    varchar(4),
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
    entities
)tablespace sup ;

alter table purs
    owner to postgres;

create unique index books_single_idx
    on purs (rtlid, status)
    where (status = '-1'::integer) tablespace sup;

create index books_ctridstatus_idx
    on purs (ctrid, status)
    tablespace sup;

create index books_shpidstatus_idx
    on purs (rtlid, status)
    tablespace sup;

create index books_srcidstatus_idx
    on purs (supid, status)
    tablespace sup;

create index books_mktidstatus_idx
    on purs (mktid, status)
    tablespace sup;

create index lots_nend_idx
    on lots (nend);

create index lots_srcidstatus_idx
    on lots (supid, status);

create index lots_catid_idx
    on lots (catid);

create table assets
(
    id     serial
        constraint assets_pk
            primary key,
    orgid  integer,
    rank   smallint,
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
    inherits (entities);

alter table prods
    owner to postgres;

create index assets_orgidstatus_idx
    on prods (orgid, status);

create table buys
(
    id     bigserial
        constraint buys_pk
            primary key,
    shpid  integer not null
        constraint buys_shpid_fk
            references orgs,
    mktid  integer not null
        constraint buys_mkt_fk
            references orgs,
    uid    integer
        constraint buys_uid_fk
            references users,
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
    constraint typ_chk
        check ((typ >= 1) AND (typ <= 3))
)
    inherits
(
    entities
)tablespace rtl ;

alter table buys
    owner to postgres;

create index buys_uidstatus_idx
    on buys (uid, status)
    tablespace rtl;

create index buys_shpidstatus_idx
    on buys (rtlid, status)
    tablespace rtl;

create unique index buys_single_idx
    on buys (rtlid, typ, status)
    where ((typ = 1) AND (status = '-1'::integer)) tablespace rtl;

create index buys_mktidstatus_idx
    on buys (mktid, status)
    tablespace rtl;

create table items
(
    id    serial
        constraint items_pk
            primary key,
    shpid integer  not null
        constraint items_shpid_fk
            references orgs,
    lotid integer
        constraint items_lotid_fk
            references lots,
    catid smallint
        constraint items_catid_fk
            references cats,
    unit  varchar(4),
    unitx smallint,
    price money,
    "off" money,
    minx  smallint,
    stock smallint,
    avail smallint not null
        constraint items_avail_chk
            check (avail >= 0),
    ops   stockop[],
    icon  bytea,
    pic   bytea
)
    inherits (entities);

alter table items
    owner to postgres;

create index items_catid_idx
    on items (catid);

create table buyaggs_typ
(
    orgid  integer not null,
    dt     date    not null,
    typ    integer not null,
    name   varchar(12),
    corgid integer,
    trans  integer,
    amt    money,
    constraint buyaggs_typ_pk
        primary key (orgid, dt, typ)
)
    tablespace rtl;

alter table buyaggs_typ
    owner to postgres;

create table buyaggs_itemid
(
    orgid  integer not null,
    dt     date    not null,
    typ    integer not null,
    name   varchar(12),
    corgid integer,
    trans  integer,
    amt    money,
    constraint buyaggs_itemid_pk
        primary key (orgid, dt, typ)
)
    tablespace rtl;

alter table buyaggs_itemid
    owner to postgres;

create table gens
(
    id      smallserial
        constraint gens_pk
            primary key,
    typ     smallint,
    till    date,
    started timestamp(0),
    ended   timestamp(0),
    opr     varchar(12)
);

alter table gens
    owner to postgres;

create table bookclrs
(
    orgid integer not null,
    dt    date    not null,
    trans integer,
    amt   money,
    rate  smallint,
    topay money,
    pay   money
)
    inherits
(
    entities
)tablespace sup ;

alter table purclrs
    owner to postgres;

create table buyclrs
(
    id     serial
        constraint buyclrs_pk
            primary key,
    orgid  integer not null,
    dt     date    not null,
    typ    smallint,
    name   varchar(12),
    trans  integer,
    amt    money,
    rate   smallint,
    topay  money,
    pay    money,
    acct   varchar(20),
    status smallint
)
    tablespace rtl;

alter table buyclrs
    owner to postgres;

create table bookaggs_typ
(
    orgid   integer not null,
    dt      date,
    typ     integer not null,
    name    varchar(12),
    corgid  integer,
    trans   integer,
    qty     integer,
    amt     money,
    created timestamp(0),
    creator varchar(12)
)
    tablespace sup;

alter table puraggs_typ
    owner to postgres;

create table bookaggs_lotid
(
    orgid   integer not null,
    dt      date,
    typ     integer not null,
    name    varchar(12),
    corgid  integer,
    trans   integer,
    qty     integer,
    amt     money,
    created timestamp(0),
    creator varchar(12)
)
    tablespace sup;

alter table puraggs_lotid
    owner to postgres;

create view orgs_vw
            (typ, name, tip, created, creator, adapted, adapter, oker, oked, status, id, prtid, ctrid, ext, legal,
             regid, addr, x, y, tel, trust, link, specs, icon, pic, m1, m2, m3, m4)
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
       o.prtid,
       o.ctrid,
       o.ext,
       o.legal,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.trust,
       o.link,
       o.specs,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM orgs o;

alter table orgs_vw
    owner to postgres;

create view users_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, tel, addr, im, credential,
             admly, srcid, srcly, shpid, shply, vip, refer, icon)
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
       o.refer,
       o.icon IS NOT NULL AS icon
FROM users o;

alter table users_vw
    owner to postgres;

create view assets_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, orgid, rank, cap, cern, factor,
             x, y, specs, icon, pic, m1, m2, m3, m4)
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
       o.cap,
       o.cern,
       o.factor,
       o.x,
       o.y,
       o.specs,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4
FROM prods o;

alter table prods_vw
    owner to postgres;

create view lots_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, srcid, srcname, assetid, targs,
             catid, started, unit, unitx, price, "off", minx, cap, stock, avail, nstart, nend, icon, pic, m1, m2, m3,
             m4, ops)
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
       o.supid,
       o.supname,
       o.prodid,
       o.targs,
       o.catid,
       o.started,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.minx,
       o.cap,
       o.stock,
       o.avail,
       o.nstart,
       o.nend,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic,
       o.m1 IS NOT NULL   AS m1,
       o.m2 IS NOT NULL   AS m2,
       o.m3 IS NOT NULL   AS m3,
       o.m4 IS NOT NULL   AS m4,
       o.ops
FROM lots o;

alter table lots_vw
    owner to postgres;

create view items_vw
            (typ, name, tip, created, creator, adapted, adapter, oked, oker, status, id, shpid, lotid, catid, unit,
             unitx, price, "off", minx, stock, avail, ops, icon, pic)
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
       o.rtlid,
       o.lotid,
       o.catid,
       o.unit,
       o.unitx,
       o.price,
       o.off,
       o.minx,
       o.stock,
       o.avail,
       o.ops,
       o.icon IS NOT NULL AS icon,
       o.pic IS NOT NULL  AS pic
FROM items o;

alter table items_vw
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

create function booksgen(till date, opr character varying) returns void
    language plpgsql
as
$$
DECLARE

    past date;
    now timestamp(0) = localtimestamp(0);
    tillstamp timestamp(0);
    paststamp timestamp(0);

    TYP_PLAT constant int = 1;
    TYP_SRC constant int = 2;
    TYP_CTR constant int = 3;

--     rates in thousandth

    BASE constant int = 100;
    RATE_PLAT constant int = 1;
    RATE_SRC constant int = 97;
    RATE_CTR constant int = 2;

BEGIN

    opr = coalesce(opr, 'SYS');

    SELECT coalesce(booksgen, '2000-01-01'::date)FROM global WHERE pk INTO past;
    tillstamp = (till + interval '1 day')::timestamp(0);

    opr = coalesce(opr, 'SYS');

    paststamp = (past + interval '1 day')::timestamp(0);

    -- books for source

    INSERT INTO bookaggs (orgid, dt, typ, coid, trans, amt, created, creator)
    SELECT supid,
           oked::date,
           typ,
           first(ctrid),
           count(pay),
           sum(pay - coalesce(refund, 0::money)),
           now,
           opr
    FROM purs
    WHERE status = 4 AND oked >= paststamp AND oked < tillstamp
    GROUP BY supid, oked::date, typ;


    INSERT INTO purclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
    SELECT TYP_SRC,
           first(creator),
           now,
           opr,
           orgid,
           till,
           sum(trans),
           sum(amt),
           RATE_SRC,
           sum(amt * RATE_SRC / BASE)
    FROM bookaggs
    WHERE typ = 1 AND dt > past AND dt <= till GROUP BY orgid;

    INSERT INTO purclrs (typ, name, created, creator, orgid, till, trans, amt, rate, topay)
    SELECT TYP_CTR,
           first(creator),
           now,
           opr,
           coid,
           till,
           sum(trans),
           sum(amt),
           RATE_CTR,
           sum(amt * RATE_CTR / BASE)
    FROM bookaggs
    WHERE typ = 1 AND dt > past AND dt <= till GROUP BY coid;

    UPDATE global SET booksgen = till WHERE pk;

END
$$;

alter function pursgen(date, varchar) owner to postgres;

create function buys_trig_func() returns trigger
    language plpgsql
as
$$
DECLARE
    itm buyitem;
BEGIN
    -- update ware avail values
    IF (TG_OP = 'INSERT' AND NEW.status = 4) THEN

        FOREACH itm IN ARRAY NEW.items LOOP -- oked
        UPDATE items SET avail = avail - itm.qty, stock = stock - itm.qty WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 1 AND OLD.status < 1) THEN -- paid

        FOREACH itm IN ARRAY NEW.items LOOP
                UPDATE items SET avail = avail - itm.qty WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 4 AND OLD.status < 4) THEN -- delivered

        FOREACH itm IN ARRAY NEW.items LOOP
                UPDATE items SET stock = stock - itm.qty WHERE id = itm.itemid;
            END LOOP;

    ELSEIF (TG_OP = 'UPDATE' AND NEW.status = 0 AND OLD.status > 0) THEN -- voided

        FOREACH itm IN ARRAY NEW.items LOOP
                UPDATE items SET avail = avail + itm.qty, stock = stock + itm.qty WHERE id = itm.itemid;
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

create function buyitem_agg_func(ret buyaggs_itemid[], items buyitem[], orgid integer, dt date, corgid integer) returns buyaggs_itemid[]
    language plpgsql
as
$$
DECLARE
    agg buyaggs_itemid;
    itm buyitem;
    fnd bool;
BEGIN

    FOREACH itm IN ARRAY items LOOP

            fnd = FALSE;

            IF ret IS NOT NULL THEN
                FOREACH agg IN ARRAY ret LOOP
                        IF agg.typ = itm.itemid THEN -- found
                            agg.trans = agg.trans + itm.qty;
                            agg.amt = agg.amt + (itm.price - itm.off) * itm.qty;
                            fnd = TRUE;
                            CONTINUE;
                        END IF;
                    END LOOP;
            END IF;

            IF ret IS NULL OR NOT fnd THEN
                agg = (orgid, dt, itm.itemid, itm.name, corgid, itm.qty, (itm.price - itm.off) * itm.qty);
                ret = ret || agg;
            end if;
        END LOOP;

    RETURN ret;
END;
$$;

alter function buyitem_agg_func(buyaggs_itemid[], buyitem[], integer, date, integer) owner to postgres;

create function buysgen() returns void
    language plpgsql
as
$$
DECLARE
    curr timestamp(0) = localtimestamp(0);
    till date;
    tillstamp timestamp(0);
    last date;
    laststamp timestamp(0);

    TYP_PLAT constant int = 1;
    TYP_SHP constant int = 2;
    TYP_MKT constant int = 3;

    BASE constant int = 100;
    RATE_PLAT constant int = 1;
    RATE_SHP constant int = 97;
    RATE_MKT constant int = 2;

BEGIN

    -- adjust parameters

    SELECT coalesce(max(gens.till), '2000-01-01'::date) FROM gens WHERE typ = 1 INTO last;
    laststamp = (last + interval '1 day')::timestamp(0);

    till = curr::date - interval '1 day';
    tillstamp = (till + interval '1 day')::timestamp(0);

    IF (till <= last) THEN
        RETURN;
    end if;

    -- aggregate buys by typ

    INSERT INTO buyaggs_typ
    SELECT shpid,
           created::date,
           typ,
           first(name),
           first(mktid),
           count(*),
           sum(pay - coalesce(refund, 0::money))
    FROM buys
    WHERE
            created >= laststamp AND created < tillstamp AND status > 0
    GROUP BY
        shpid, created::date, typ;

    -- aggregate buys by itemid

    INSERT INTO buyaggs_itemid
    SELECT
        (unnest(buys_agg(items,shpid, created::date,mktid))).*
    FROM buys
    WHERE
            created >= laststamp AND created < tillstamp AND status > 0
    GROUP BY shpid, created::date;


    INSERT INTO buyclrs
    (orgid, dt, typ, name, trans, amt, rate, topay)
    SELECT
        orgid, dt, TYP_SHP, first(name), sum(trans), sum(amt), RATE_SHP, sum(amt * RATE_SHP / BASE)
    FROM buyaggs_typ
    WHERE
            typ = 1 AND dt > last AND dt <= till GROUP BY orgid, dt;

    INSERT INTO
        buyclrs (orgid, dt, typ, name, trans, amt, rate, topay)
    SELECT
        corgid, dt, TYP_MKT, first(name), sum(trans), sum(amt), RATE_MKT, sum(amt * RATE_MKT / BASE)
    FROM buyaggs_typ
    WHERE
            typ = 1 AND dt > last AND dt <= till GROUP BY corgid, dt;


    INSERT INTO gens (typ, till, started, ended)
    VALUES
        (1, till, curr, localtimestamp(0));
END
$$;

alter function buysgen() owner to postgres;

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

create aggregate buys_agg(items buyitem[], orgid integer, dt date, corgid integer) (
    sfunc = buyitem_agg_func,
    stype = buyaggs_itemid[]
    );

alter aggregate buys_agg(items buyitem[], orgid integer, dt date, corgid integer) owner to postgres;


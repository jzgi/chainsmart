create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table users
(
	id serial not null
		constraint users_pk
			primary key,
	typ smallint default 0 not null,
	status smallint default 0 not null,
	name varchar(8) not null,
	tel varchar(11) not null,
	created timestamp(0) default ('now'::text)::timestamp without time zone not null,
	credential varchar(16),
	admly smallint default 0 not null,
	orgid smallint,
	orgly smallint default 0 not null,
	acct varchar(20),
	im varchar(28) not null,
	refid integer
		constraint users_refid_fk
			references users
);

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

create table learns
(
	id varchar(8),
	major smallint,
	minor smallint,
	target smallint,
	a smallint,
	b smallint,
	c smallint,
	d smallint,
	e smallint
);

alter table learns owner to postgres;

create table plans
(
	dt date not null
		constraint plans_pk
			primary key,
	detox1 smallint[],
	detox2 smallint[],
	regular1 smallint[],
	regular2 smallint[],
	home1 smallint[],
	home2 smallint[]
);

alter table plans owner to postgres;

create table _arts
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20)
);

alter table _arts owner to postgres;

create table _docs
(
	typ smallint not null,
	status smallint default 0 not null,
	orgid smallint not null,
	issued date,
	ended date,
	span smallint,
	name varchar(20) not null,
	tag varchar(8)
);

alter table _docs owner to postgres;

create table items
(
	id smallserial not null
		constraint items_pk
			primary key,
	progg smallint,
	price money not null,
	ingrs jsonb
)
inherits (_arts);

alter table items owner to postgres;

create table mats
(
	id smallserial not null
		constraint mats_pk
			primary key,
	unit varchar(2),
	calory smallint,
	carb money,
	fat money,
	protein money,
	sugar money,
	mineral smallint,
	vitamin money
)
inherits (_arts);

alter table mats owner to postgres;

create table regs
(
	id smallint not null
		constraint regs_pk
			primary key,
	sort smallint not null
)
inherits (_arts);

alter table regs owner to postgres;

create table orgs
(
	id smallserial not null
		constraint orgs_pk
			primary key,
	regid smallint
		constraint orgs_regid_fk
			references regs,
	addr varchar(20),
	x double precision,
	y double precision,
	mgrid integer,
	cttid integer,
	suppid smallint
		constraint orgs_superid_fk
			references orgs,
	icon bytea,
	extern boolean default false not null
)
inherits (_arts);

alter table orgs owner to postgres;

create table diets
(
	id smallserial not null
		constraint diets_pk
			primary key,
	trackg char(4) not null,
	span smallint not null,
	level smallint,
	price money
)
inherits (_arts);

alter table diets owner to postgres;

create table orders
(
	status smallint default 0 not null,
	id serial not null
		constraint orders_pk
			primary key,
	dietid smallint not null
		constraint orders_dietid_fk
			references diets,
	trackg char(4),
	level smallint,
	ptid smallint not null
		constraint orders_ptid_fk
			references orgs,
	uid integer not null
		constraint orders_uid_fk
			references users,
	uname varchar(8) not null,
	utel varchar(11) not null,
	uim varchar(28),
	price money,
	pay money,
	refund money,
	compl smallint,
	tag varchar(8),
	constraint orders_orgid_fk
		foreign key (orgid) references orgs
)
inherits (_docs);

alter table orders owner to postgres;

create table orderlgs
(
	orderid integer not null
		constraint orderlgs_orderid_fk
			references orders,
	dt date not null
		constraint orderlgs_dt_fk
			references plans,
	track smallint,
	status smallint default 0 not null,
	fit smallint,
	digest smallint,
	rest smallint,
	blood smallint,
	sugar smallint,
	style smallint,
	constraint orderlgs_pk
		primary key (orderid, dt)
);

alter table orderlgs owner to postgres;

create index orders_orgidstatus_idx
	on orders (orgid, status);

create index orders_uidstatus_idx
	on orders (uid, status);

create index orders_ptidstatus_idx
	on orders (ptid, status);

create table clears
(
	id serial not null
		constraint clears_pkey
			primary key,
	typ smallint default 0 not null,
	status smallint,
	orgid smallint not null
		constraint clears_orgid_fkey
			references orgs,
	till date,
	serv smallint,
	compl smallint,
	amt money,
	created timestamp(0),
	pay money default 0 not null,
	payer varchar(8)
);

alter table clears owner to postgres;

create table lots
(
	id serial not null
		constraint lots_pk
			primary key,
	tip varchar(50),
	unit varchar(4),
	unitip varchar(10),
	price money,
	min smallint default 1 not null,
	max smallint default 1 not null,
	least smallint default 1 not null,
	step smallint default 1 not null,
	extern boolean,
	addr varchar(20),
	start timestamp(0),
	author varchar(8),
	icon bytea,
	img bytea,
	qtys smallint default 0 not null,
	pays money default 0 not null,
	constraint lots_orgid_fk
		foreign key (orgid) references orgs
)
inherits (_docs);

alter table lots owner to postgres;

create table lotjns
(
	lotid integer not null
		constraint lotjns_lotid_fk
			references lots,
	uid integer not null
		constraint lotjns_uid_fk
			references users,
	status smallint,
	uname varchar(10) not null,
	uacct varchar(20),
	utel varchar(11) not null,
	uim varchar(28) not null,
	uaddr varchar(30),
	ptid smallint
		constraint lotjns_ptid_fk
			references orgs,
	qty smallint,
	inited timestamp(0),
	stamp timestamp(0),
	pay money,
	credit money,
	constraint lotjns_pk
		primary key (lotid, uid)
);

alter table lotjns owner to postgres;

create index lotjns_ptid_idx
	on lotjns (ptid)
	where (ptid > 0);

create index lots_orgidstatusid_idx
	on lots (orgid asc, status asc, id desc);

create view orgs_vw(typ, status, name, tip, id, regid, addr, x, y, mgrid, cttid, suppid, extern, icon, mgrname, mgrtel, mgrim, cttname, ctttel, cttim) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.id,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.mgrid,
       o.cttid,
       o.suppid,
       o.extern,
       o.icon IS NOT NULL AS icon,
       m.name             AS mgrname,
       m.tel              AS mgrtel,
       m.im               AS mgrim,
       c.name             AS cttname,
       c.tel              AS ctttel,
       c.im               AS cttim
FROM orgs o
         LEFT JOIN users m
                   ON o.mgrid =
                      m.id
         LEFT JOIN users c
                   ON o.cttid =
                      c.id;

alter table orgs_vw owner to postgres;

create view lots_vw(typ, status, orgid, issued, ended, span, name, tag, id, tip, unit, unitip, price, min, max, least, step, extern, addr, start, author, qtys, pays, icon, img) as
SELECT o.typ,
       o.status,
       o.orgid,
       o.issued,
       o.ended,
       o.span,
       o.name,
       o.tag,
       o.id,
       o.tip,
       o.unit,
       o.unitip,
       o.price,
       o.min,
       o.max,
       o."least",
       o.step,
       o.extern,
       o.addr,
       o.start,
       o.author,
       o.qtys,
       o.pays,
       o.icon IS NOT NULL AS icon,
       o.img IS NOT NULL  AS img
FROM lots o;

alter table lots_vw owner to postgres;

create function recalc(timestamp without time zone) returns void
	language plpgsql
as $$
BEGIN

    -- clear
    DELETE FROM cashes WHERE status = 0;

    -- by shop
    INSERT INTO cashes (typ, orgid, till, amt)
    SELECT 1,
           orgid,
           $1,
           sum((pay - coalesce(refund, 0::money)) * 0.88)
    FROM orders
    WHERE status IN (1,2) AND ended < $1 GROUP BY orgid;

    -- by pt
    INSERT INTO cashes (typ, orgid, till, amt)
    SELECT 3,
           ptid,
           $1,
           sum((pay - coalesce(refund, 0::money)) * 0.10)
    FROM orders
    WHERE status IN (1,2) AND ended < $1 GROUP BY ptid;

    -- merchant entrust
    INSERT INTO cashes (typ, orgid, till, amt)
    SELECT 4,
           orgid,
           $1,
           sum(pays * 0.88)
    FROM lots WHERE status IN (1, 2) AND ended < $1 AND global = FALSE GROUP BY orgid;

    -- merchant self-delivery
    INSERT INTO cashes (typ, orgid, till, amt)
    SELECT 5,
           orgid,
           $1,
           sum(pays * 0.98)
    FROM lots WHERE status IN (1, 2) AND ended < $1 AND global = TRUE GROUP BY orgid;

    -- pt goods
    INSERT INTO cashes (typ, orgid, till, amt)
    SELECT 4,
           d.ptid,
           $1,
           sum(pay * 0.10)
    FROM lots m, lotjns d WHERE m.id = d.lotid AND m.status IN (1, 2) AND m.ended < $1 ANd d.ptid IS NOT NULL GROUP BY d.ptid;

END
$$;

alter function recalc(timestamp) owner to postgres;

create function reckon(timestamp without time zone) returns void
	language plpgsql
as $$
BEGIN

    -- cashes
    UPDATE cashes SET status = 1 WHERE status = 0;

    -- orders
    UPDATE orders SET status = 3 
    WHERE status IN (1,2) AND ended < $1;

    -- lots
    UPDATE lots SET status = 3
    WHERE status IN (1,2) AND ended < $1;

END
$$;

alter function reckon(timestamp) owner to postgres;


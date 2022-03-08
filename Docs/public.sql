create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type act_type as
(
	"user" varchar(10),
	role varchar(10),
	party varchar(10),
	op varchar(10),
	stamp timestamp(0)
);

alter type act_type owner to postgres;

create type wareln_type as
(
	wareid integer,
	warename varchar(20),
	itemid smallint,
	price money,
	qty smallint,
	total money
);

alter type wareln_type owner to postgres;

create table infos
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(12) not null,
	tip varchar(30),
	created timestamp(0),
	creator varchar(10),
	adapted timestamp(0),
	adapter varchar(10)
);

alter table infos owner to postgres;

create table regs
(
	id smallint not null
		constraint regs_pk
			primary key,
	idx smallint
)
inherits (infos);

alter table regs owner to postgres;

create table agrees
(
	id serial not null,
	orgid smallint,
	started date,
	ended date,
	content jsonb
)
inherits (infos);

alter table agrees owner to postgres;

create table items
(
	id integer not null,
	unit varchar(4),
	unitip varchar(10),
	icon bytea
)
inherits (infos);

alter table items owner to postgres;

create table orgs
(
	id serial not null
		constraint orgs_pk
			primary key,
	fork smallint,
	sprid integer,
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
	zone smallint,
	mgrid integer,
	ctras integer[],
	img bytea
)
inherits (infos);

alter table orgs owner to postgres;

create table users
(
	id serial not null
		constraint users_pk
			primary key,
	tel varchar(11) not null,
	im varchar(28),
	credential varchar(32),
	admly smallint default 0 not null,
	orgid smallint
		constraint users_orgid_fk
			references orgs,
	orgly smallint default 0 not null,
	idcard varchar(18)
)
inherits (infos);

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

create table clears
(
	id serial not null
		constraint clears_pkey
			primary key,
	orgid integer not null,
	dt date,
	sprid integer not null,
	count integer,
	amt money
)
inherits (infos);

alter table clears owner to postgres;

create table books
(
	id bigserial not null
		constraint books_pk
			primary key,
	bizid integer not null,
	bizname varchar(10),
	mrtid integer not null,
	mrtname varchar(10),
	ctrid integer not null,
	ctrname varchar(10),
	prvid integer not null,
	prvname varchar(10),
	srcid integer not null,
	srcname varchar(10),
	itemid integer,
	wareid integer,
	warename integer,
	price money,
	qty smallint,
	fee money,
	pay money,
	cs varchar(32),
	state smallint,
	trace act_type[],
	peerid_ smallint,
	coid_ bigint,
	seq_ integer,
	cs_ varchar(32),
	blockcs_ varchar(32)
)
inherits (infos);

alter table books owner to postgres;

create table buys
(
	id bigserial not null
		constraint buys_pk
			primary key,
	bizid integer not null,
	bizname varchar(10),
	mrtid integer not null,
	mrtname varchar(10),
	uid integer not null,
	uname varchar(10),
	utel varchar(11),
	uaddr varchar(20),
	uim varchar(28),
	totalp money,
	fee money,
	pay money,
	wares wareln_type[]
)
inherits (infos);

alter table buys owner to postgres;

create table products
(
	id integer not null
		constraint products_pk
			primary key,
	fillg smallint,
	fillon date,
	orgid integer
		constraint products_orgid_fk
			references orgs,
	itemid integer,
	ext varchar(10),
	unit varchar(4),
	unitx smallint,
	min smallint,
	max smallint,
	step smallint,
	price money,
	cap integer,
	mrtg smallint,
	mrtprice money,
	rankg smallint,
	img bytea,
	cert bytea
)
inherits (infos);

alter table products owner to postgres;

create table pieces
(
	id serial not null
		constraint pieces_pk
			primary key,
	productid integer,
	orgid integer,
	itemid integer,
	ext varchar(10),
	unit varchar(4),
	unitx smallint,
	min smallint,
	max smallint,
	step smallint,
	price money,
	cap integer
)
inherits (infos);

alter table pieces owner to postgres;

create table peers
(
	id smallint not null
		constraint peers_pk
			primary key,
	domain varchar(50),
	secure boolean,
	fedkey varchar(32)
)
inherits (infos);

alter table peers owner to postgres;

create table carbons
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

alter table carbons owner to postgres;

create table peercarbons
(
	peerid smallint
)
inherits (carbons);

alter table peercarbons owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, adapted, adapter, id, fork, zone, sprid, license, trust, regid, addr, x, y, tel, ctras, mgrid, mgrname, mgrtel, mgrim, img) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.id,
       o.fork,
       o.zone,
       o.sprid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.ctras,
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


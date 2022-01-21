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

create table _infos
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(30),
	created timestamp(0),
	creator varchar(10),
	adapted timestamp(0),
	adapter varchar(10)
);

alter table _infos owner to postgres;

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
	idcard varchar(18)
)
inherits (_infos);

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

create table regs
(
	id smallint not null
		constraint regs_pk
			primary key,
	idx smallint
)
inherits (_infos);

alter table regs owner to postgres;

create table agrees
(
	id serial not null,
	orgid smallint,
	started date,
	ended date,
	content jsonb
)
inherits (_infos);

alter table agrees owner to postgres;

create table items
(
	id serial not null
		constraint items_pk
			primary key,
	cat smallint,
	unit varchar(4),
	unitip varchar(10),
	feertl money,
	feesup money,
	icon bytea
)
inherits (_infos);

alter table items owner to postgres;

create table _deals
(
	itemid integer,
	artid integer,
	artname integer,
	price money,
	qty smallint,
	pay money,
	cs varchar(32),
	state smallint,
	trace act_type[]
)
inherits (_infos);

alter table _deals owner to postgres;

create table userlgs
(
	userid integer not null,
	cont jsonb
);

alter table userlgs owner to postgres;

create table _wares
(
	orgid integer,
	itemid integer,
	cat smallint,
	ext varchar(10),
	unit varchar(4),
	unitx smallint,
	min smallint,
	max smallint,
	step smallint,
	price money,
	cap integer
)
inherits (_infos);

alter table _wares owner to postgres;

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
	mgrid integer
		constraint orgs_mgrid_fk
			references users,
	rank smallint,
	icon bytea,
	cert bytea,
	tel varchar(11)
)
inherits (_infos);

alter table orgs owner to postgres;

alter table users
	add constraint users_orgid_fk
		foreign key (orgid) references orgs;

create table products
(
	id serial not null
		constraint products_pk
			primary key,
	fillon date,
	mrtg smallint,
	mrtprice money,
	rankg smallint,
	img bytea,
	cert bytea,
	constraint products_orgid_fk
		foreign key (orgid) references orgs,
	constraint products_itemid_fk
		foreign key (itemid) references items
)
inherits (_wares);

alter table products owner to postgres;

create table pieces
(
	id serial not null
		constraint pieces_pk
			primary key,
	productid integer
)
inherits (_wares);

alter table pieces owner to postgres;

create table routes
(
	ctrid integer
		constraint routes_ctrid_fk
			references orgs,
	ptid integer
		constraint routes_ptid_fk
			references orgs
)
inherits (_infos);

alter table routes owner to postgres;

create index routes_typctrid_idx
	on routes (typ, ctrid);

create index routes_typptid_idx
	on routes (typ, ptid);

create table peers_
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(30),
	created timestamp(0),
	creator varchar(10),
	id smallint not null
		constraint peers_pk
			primary key,
	domain varchar(50),
	secure boolean
)
inherits (_infos);

alter table peers_ owner to postgres;

create table dailys
(
	orgid integer,
	dt date,
	itemid smallint,
	count integer,
	amt money,
	qty integer
)
inherits (_infos);

alter table dailys owner to postgres;

create table books_
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
	peerid_ smallint,
	coid_ bigint,
	seq_ integer,
	cs_ varchar(32),
	blockcs_ varchar(32)
)
inherits (_deals);

alter table books_ owner to postgres;

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
	uim varchar(28)
)
inherits (_deals);

alter table buys owner to postgres;

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
inherits (_infos);

alter table clears owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, adapted, adapter, id, fork, sprid, rank, license, trust, regid, addr, tel, x, y, mgrid, mgrname, mgrtel, mgrim, icon) as
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
       o.sprid,
       o.rank,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.tel,
       o.x,
       o.y,
       o.mgrid,
       m.name             AS mgrname,
       m.tel              AS mgrtel,
       m.im               AS mgrim,
       o.icon IS NOT NULL AS icon
FROM orgs o
         LEFT JOIN users m
                   ON o.mgrid =
                      m.id;

alter table orgs_vw owner to postgres;


create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table clears
(
	id smallserial not null
);

alter table clears owner to postgres;

create table _docs
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(30),
	created timestamp(0),
	creator varchar(10),
	modified timestamp(0),
	modifier varchar(10)
);

alter table _docs owner to postgres;

create table items
(
	id serial not null
		constraint items_pk
			primary key,
	unit varchar(4),
	unitip varchar(10),
	icon bytea
)
inherits (_docs);

alter table items owner to postgres;

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
	license varchar(20),
	idcard varchar(18),
	icon bytea
)
inherits (_docs);

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
inherits (_docs);

alter table regs owner to postgres;

create table agrees
(
	id serial not null,
	orgid smallint,
	started date,
	ended date,
	content jsonb
)
inherits (_docs);

alter table agrees owner to postgres;

create table distribs
(
	id serial not null
		constraint distribs_pk
			primary key,
	bizid smallint not null,
	ctrid integer,
	planid smallint,
	itemid smallint not null,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money
)
inherits (_docs);

alter table distribs owner to postgres;

create table yields
(
	id serial not null
		constraint yields_pk
			primary key,
	srcid integer,
	itemid smallint,
	test bytea
)
inherits (_docs);

alter table yields owner to postgres;

create table orgs
(
	id serial not null
		constraint orgs_pk
			primary key,
	sprid integer,
	ctrid integer,
	license varchar(20),
	trust boolean,
	regid smallint not null,
	addr varchar(30),
	x double precision,
	y double precision,
	mgrid integer,
	icon bytea,
	forkie smallint
)
inherits (_docs);

alter table orgs owner to postgres;

create table plans
(
	id serial not null
		constraint supplys_pk
			primary key,
	ctrid integer not null,
	itemid integer not null,
	started date,
	ended date,
	filled date,
	bmode smallint,
	bunit varchar(4),
	bx smallint,
	bmin smallint,
	bmax smallint,
	bstep smallint,
	bprice money,
	boff money,
	dmode smallint,
	dunit varchar(4),
	dx smallint,
	dmin smallint,
	dmax smallint,
	dstep smallint,
	dprice money,
	doff money,
	smode smallint,
	sunit varchar(4),
	sx smallint,
	smin smallint,
	smax smallint,
	sstep smallint,
	sprice money,
	soff money
)
inherits (_docs);

alter table plans owner to postgres;

create table posts
(
	id integer not null
		constraint posts_pk
			primary key,
	bizid integer,
	itemid integer,
	planid integer,
	unit varchar(4),
	unitx smallint,
	min smallint,
	max smallint,
	step smallint,
	price money,
	"off" money
)
inherits (_docs);

alter table posts owner to postgres;

create table needs
(
	id serial not null
		constraint needs_pk
			primary key,
	mrtid smallint not null,
	bizid integer not null,
	postid smallint not null,
	itemid smallint,
	uid integer not null,
	uname varchar(10),
	utel varchar(11),
	uim varchar(28),
	price money,
	pay money,
	refund money
)
inherits (_docs);

alter table needs owner to postgres;

create table subscribs
(
	id serial not null
		constraint subscribs_pk
			primary key,
	ctrid integer,
	srcid integer not null,
	frmid integer not null,
	itemid integer not null,
	planid integer,
	yieldid integer,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money,
	codend integer,
	codes smallint
)
inherits (_docs);

alter table subscribs owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, modified, modifier, id, fork, sprid, ctrid, license, trust, regid, addr, x, y, mgrid, mgrname, mgrtel, mgrim, icon) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.modified,
       o.modifier,
       o.id,
       o.fork,
       o.sprid,
       o.ctrid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
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


create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table regs_
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id smallserial not null
		constraint regs_pk
			primary key,
	idx smallint
);

alter table regs_ owner to postgres;

create table items
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id smallserial not null,
	unit varchar(4),
	unitip varchar(10),
	icon bytea,
	img bytea,
	extra bytea,
	price money,
	"off" money
);

alter table items owner to postgres;

create table users
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id serial not null
		constraint users_pk
			primary key,
	tel varchar(11) not null,
	im varchar(28) not null,
	credential varchar(16),
	admly smallint default 0 not null,
	orgid smallint,
	orgly smallint default 0 not null
)
inherits ();

alter table users owner to postgres;

create table orgs_
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id smallserial not null
		constraint orgs_pk
			primary key,
	coid integer
		constraint orgs_coid_fk
			references orgs_,
	ctrid integer
		constraint orgs_ctrid_fk
			references orgs_,
	regid smallint
		constraint orgs_regid_fk
			references regs_,
	addr varchar(20),
	x double precision,
	y double precision,
	mgrid integer
		constraint orgs_mgrid_fk
			references users,
	icon bytea,
	license bytea,
	perm bytea,
	"grant" boolean
);

alter table orgs_ owner to postgres;

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

create table clears
(
	id smallserial not null
);

alter table clears owner to postgres;

create table books
(
	typ smallint not null,
	status smallint default 0 not null,
	partyid smallint not null,
	ctrid integer,
	created timestamp(0),
	creator varchar(10),
	id serial not null,
	no bigint,
	planid smallint,
	itemid smallint not null,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money
);

alter table books owner to postgres;

create table purchs
(
	typ smallint not null,
	status smallint default 0 not null,
	partyid smallint not null,
	ctrid integer,
	created timestamp(0),
	creator varchar(10),
	id serial not null,
	no bigint,
	planid smallint,
	itemid smallint not null,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money,
	codestart integer,
	codes smallint,
	prodid integer
)
inherits ();

alter table purchs owner to postgres;

create table agrmts_
(
	id serial not null,
	orgid smallint,
	typ smallint not null,
	start date,
	"end" date,
	signed timestamp(0),
	created timestamp(0),
	signer varchar(10),
	creator varchar(10),
	status smallint
);

alter table agrmts_ owner to postgres;

create table supplys
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id serial not null,
	itemid smallint not null,
	bunit varchar(4),
	bunitx smallint,
	bmin smallint,
	bmax smallint,
	bstep smallint,
	bprice money,
	boff money,
	punit varchar(4),
	punitx smallint,
	pmin smallint,
	pmax smallint,
	pstep smallint,
	pprice money,
	poff money,
	started date,
	ended date,
	delivered date
);

alter table supplys owner to postgres;

create table yields
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id serial not null,
	srcid integer,
	itemid smallint,
	test bytea
);

alter table yields owner to postgres;

create table cats_
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id smallserial not null,
	idx smallint,
	stamp_ timestamp
);

alter table cats_ owner to postgres;

create table shards_
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10),
	id varchar(4),
	idx smallint,
	stamp_ timestamp
);

alter table shards_ owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, id, coid, ctrid, regid, addr, x, y, "grant", mgrid, mgrname, mgrtel, mgrim, icon, license, perm) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.id,
       o.coid,
       o.ctrid,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o."grant",
       o.mgrid,
       m.name                AS mgrname,
       m.tel                 AS mgrtel,
       m.im                  AS mgrim,
       o.icon IS NOT NULL    AS icon,
       o.license IS NOT NULL AS license,
       o.perm IS NOT NULL    AS perm
FROM orgs o
         LEFT JOIN users m
                   ON o.mgrid =
                      m.id;

alter table orgs_vw owner to postgres;


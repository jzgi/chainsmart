create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table arts_
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10)
);

alter table arts_ owner to postgres;

create table regs
(
	id smallserial not null
		constraint regs_pk
			primary key,
	idx smallint
)
inherits (arts_);

alter table regs owner to postgres;

create table items
(
	id smallserial not null,
	unit varchar(4),
	unitip varchar(10),
	icon bytea,
	img bytea,
	extra bytea
)
inherits (arts_);

alter table items owner to postgres;

create table flows_
(
	typ smallint not null,
	status smallint default 0 not null,
	partyid smallint not null,
	ctrid integer,
	created timestamp(0),
	creator varchar(10)
);

alter table flows_ owner to postgres;

create table users
(
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
inherits (arts_);

alter table users owner to postgres;

create table orgs
(
	id smallserial not null
		constraint orgs_pk
			primary key,
	coid integer
		constraint orgs_coid_fk
			references orgs,
	ctrid integer
		constraint orgs_ctrid_fk
			references orgs,
	regid smallint
		constraint orgs_regid_fk
			references regs,
	addr varchar(20),
	x double precision,
	y double precision,
	mgrid integer
		constraint orgs_mgrid_fk
			references users,
	icon bytea,
	license bytea,
	perm bytea
)
inherits (arts_);

alter table orgs owner to postgres;

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

create table buys
(
	id serial not null,
	no bigint,
	planid smallint,
	itemid smallint not null,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money
)
inherits (flows_);

alter table buys owner to postgres;

create table purchs
(
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
inherits (flows_);

alter table purchs owner to postgres;

create table agrmts
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

alter table agrmts owner to postgres;

create table plans
(
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
	poff money
)
inherits (arts_);

alter table plans owner to postgres;

create table chans
(
	status smallint default 0 not null,
	prodid smallint,
	orgid smallint
)
inherits (arts_);

alter table chans owner to postgres;

create table msgs
(
	id serial not null
)
inherits (arts_);

alter table msgs owner to postgres;

create table prods
(
	id serial not null,
	srcid integer,
	itemid smallint,
	test bytea
)
inherits (arts_);

alter table prods owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, id, coid, ctrid, regid, addr, x, y, mgrid, mgrname, mgrtel, mgrim, icon, license, perm) as
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


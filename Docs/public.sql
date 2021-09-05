create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table _arts
(
	typ smallint not null,
	status smallint default 0 not null,
	name varchar(10) not null,
	tip varchar(20),
	created timestamp(0),
	creator varchar(10)
);

alter table _arts owner to postgres;

create table regs
(
	id smallserial not null,
	sort smallint
)
inherits (_arts);

alter table regs owner to postgres;

create table orgs
(
	id serial not null
		constraint orgs_pk
			primary key,
	grpid integer,
	ctrid integer,
	regid smallint,
	addr varchar(20),
	x double precision,
	y double precision,
	mgrid integer,
	icon bytea,
	license bytea,
	perm bytea
)
inherits (_arts);

alter table orgs owner to postgres;

create table items
(
	id serial not null,
	unit varchar(4),
	unitip varchar(10),
	upc varchar(15),
	icon bytea
)
inherits (_arts);

alter table items owner to postgres;

create table _docs
(
	typ smallint not null,
	status smallint default 0 not null,
	partyid smallint not null,
	no integer,
	ctrid integer,
	created timestamp(0),
	creator varchar(10)
);

alter table _docs owner to postgres;

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
inherits (_arts);

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

create table buys
(
	id serial not null,
	itemid smallint not null,
	price money,
	discount money,
	qty integer,
	pay money,
	refound money
)
inherits (_docs);

alter table buys owner to postgres;

create table purchases
(
	id serial not null,
	itemid smallint not null,
	price money,
	discount money,
	qty integer,
	pay money,
	refound money
)
inherits (_docs);

alter table purchases owner to postgres;

create table supplies
(
	id smallserial not null,
	itemid smallint not null,
	srcid smallint,
	idx smallint,
	dmin smallint,
	dmax smallint,
	dstep smallint,
	dprice money,
	doff money,
	agts smallint[],
	umin smallint,
	umax smallint,
	ustep smallint,
	uprice money,
	uoff money,
	img bytea,
	testa bytea,
	testb bytea
)
inherits (_arts);

alter table supplies owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, id, grpid, ctrid, regid, addr, x, y, mgrid, mgrname, mgrtel, mgrim, icon, license, perm) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.id,
       o.teamid              AS grpid,
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


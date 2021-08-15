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

create table itemagts
(
	itemid integer,
	bizid integer
);

alter table itemagts owner to postgres;

create table cats
(
)
inherits (_arts);

alter table cats owner to postgres;

create table regs
(
	id smallserial not null
)
inherits (_arts);

alter table regs owner to postgres;

create table orgs
(
	id serial not null
		constraint orgs_pk
			primary key,
	grpid integer,
	refid integer,
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
	cat varchar(10),
	unit varchar(4),
	unitip varchar(10),
	upc varchar(15),
	price money,
	discount money,
	pic bytea
)
inherits (_arts);

alter table items owner to postgres;

create table _docs
(
	typ smallint not null,
	status smallint default 0 not null,
	partyid smallint not null,
	tag varchar(8),
	ctrid integer
);

alter table _docs owner to postgres;

create table dsitems
(
	itemid smallint not null,
	price money,
	discount money
)
inherits (_arts);

alter table dsitems owner to postgres;

create table usitems
(
	itemid smallint not null,
	price money,
	discount money
)
inherits (_arts);

alter table usitems owner to postgres;

create table dsords
(
	id serial not null,
	itemid smallint not null,
	price money,
	discount money,
	payno integer
)
inherits (_docs);

alter table dsords owner to postgres;

create table usords
(
	id serial not null,
	itemid smallint not null,
	price money,
	discount money,
	payno integer
)
inherits (_docs);

alter table usords owner to postgres;


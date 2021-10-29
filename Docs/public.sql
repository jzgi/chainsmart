create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table clears
(
	id smallserial not null
);

alter table clears owner to postgres;

create table deals
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
);

alter table deals owner to postgres;

create table _beans
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

alter table _beans owner to postgres;

create table supplys_
(
	id serial not null
		constraint supplys_pk
			primary key,
	ctrid integer not null,
	itemid integer not null,
	started date,
	ended date,
	delivered date,
	smode smallint,
	sunit varchar(4),
	sunitx smallint,
	smin smallint,
	smax smallint,
	sstep smallint,
	sprice money,
	soff money,
	mmode smallint,
	munit varchar(4),
	munitx smallint,
	mmin smallint,
	mmax smallint,
	mstep smallint,
	mprice money,
	moff money
)
inherits (_beans);

alter table supplys_ owner to postgres;

create table items_
(
	id serial not null
		constraint items_pk
			primary key,
	unit varchar(4),
	unitip varchar(10),
	icon bytea
)
inherits (_beans);

alter table items_ owner to postgres;

create table orgs_
(
	id serial not null
		constraint orgs_pk
			primary key,
	sprid integer,
	ctrid integer,
	mgrid integer,
	cttid integer,
	entrust boolean,
	license varchar(20),
	regid varchar(4),
	addr varchar(30),
	x double precision,
	y double precision,
	icon bytea,
	shard_ varchar(4),
	seq_ bigint
)
inherits (_beans);

alter table orgs_ owner to postgres;

create table users_
(
	id serial not null
		constraint users_pk
			primary key,
	tel varchar(11) not null,
	im varchar(28),
	credential varchar(32),
	admly smallint default 0 not null,
	orgid smallint,
	orgly smallint default 0 not null
)
inherits (_beans);

alter table users_ owner to postgres;

create index users_admly_idx
	on users_ (admly)
	where (admly > 0);

create unique index users_im_idx
	on users_ (im);

create index users_orgid_idx
	on users_ (orgid)
	where (orgid > 0);

create unique index users_tel_idx
	on users_ (tel);

create table cats_
(
	id integer not null
		constraint cats_pk
			primary key,
	idx smallint,
	shard_ varchar(4),
	stamp_ timestamp
)
inherits (_beans);

alter table cats_ owner to postgres;

create table regs_
(
	id varchar(4) not null
		constraint regs_pk
			primary key,
	idx smallint
)
inherits (_beans);

alter table regs_ owner to postgres;

create table agrmts_
(
	id serial not null,
	orgid smallint,
	started date,
	ended date,
	content jsonb
)
inherits (_beans);

alter table agrmts_ owner to postgres;

create table books
(
	id serial not null
		constraint books_pk
			primary key,
	bizid smallint not null,
	ctrid integer,
	supplyid smallint,
	itemid smallint not null,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money
)
inherits (_beans);

alter table books owner to postgres;

create table yields
(
	id serial not null
		constraint yields_pk
			primary key,
	srcid integer,
	itemid smallint,
	test bytea
)
inherits (_beans);

alter table yields owner to postgres;


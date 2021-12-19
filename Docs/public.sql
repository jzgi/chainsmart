create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create table clears
(
	id smallserial not null
);

alter table clears owner to postgres;

create table _arts
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

alter table _arts owner to postgres;

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

create table regs
(
	id smallint not null
		constraint regs_pk
			primary key,
	idx smallint
)
inherits (_arts);

alter table regs owner to postgres;

create table agrees
(
	id serial not null,
	orgid smallint,
	started date,
	ended date,
	content jsonb
)
inherits (_arts);

alter table agrees owner to postgres;

create table items
(
	id serial not null
		constraint items_pk
			primary key,
	cat smallint,
	unit varchar(4),
	unitip varchar(10),
	unitfee money,
	icon bytea
)
inherits (_arts);

alter table items owner to postgres;

create table _docs
(
	sprid integer,
	fromid integer,
	toid integer,
	ccid integer,
	handled timestamp(0),
	handler varchar(10)
)
inherits (_arts);

alter table _docs owner to postgres;

create table books
(
	id bigserial not null
		constraint distribs_pk
			primary key,
	itemid smallint not null,
	planid smallint,
	price money,
	"off" money,
	qty integer,
	pay money,
	refund money
)
inherits (_docs);

alter table books owner to postgres;

create table buys
(
	id bigserial not null
		constraint needs_pk
			primary key,
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

alter table buys owner to postgres;

create table bids_
(
	id bigserial not null
		constraint bids_pk
			primary key,
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

alter table bids_ owner to postgres;

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
inherits (_arts);

alter table _wares owner to postgres;

create table orgs
(
	id serial not null
		constraint orgs_pk
			primary key,
	fork smallint,
	sprid integer
		constraint orgs_sprid_fk
			references orgs,
	license varchar(20),
	trust boolean,
	regid smallint
		constraint orgs_regid_fk
			references regs,
	addr varchar(30),
	x double precision,
	y double precision,
	mgrid integer
		constraint orgs_mgrid_fk
			references users,
	icon bytea,
	cert bytea
)
inherits (_arts);

alter table orgs owner to postgres;

alter table users
	add constraint users_orgid_fk
		foreign key (orgid) references orgs;

create table pieces
(
	id integer not null
		constraint produces_pk
			primary key,
	constraint produces_orgid_fk
		foreign key (orgid) references orgs,
	constraint produces_itemid_fk
		foreign key (itemid) references items
)
inherits (_wares);

alter table pieces owner to postgres;

create table plans
(
	id serial not null
		constraint plans_pk
			primary key,
	pieceid integer,
	starton date,
	endon date,
	fillon date,
	postg smallint,
	postprice money,
	icon bytea,
	img bytea,
	cert bytea
)
inherits (_wares);

alter table plans owner to postgres;

create table posts
(
	id serial not null
		constraint posts_pk
			primary key,
	planid integer
)
inherits (_wares);

alter table posts owner to postgres;

create table orglnks
(
	ctrid integer
		constraint routes_ctrid_fk
			references orgs,
	mrtid integer
		constraint routes_mrtid_fk
			references orgs
)
inherits (_arts);

alter table orglnks owner to postgres;

create index routes_ctrid_idx
	on orglnks (ctrid);

create index routes_mrtid_idx
	on orglnks (mrtid);

create table planagts
(
	planid integer
		constraint planagts_planid_fk
			references plans,
	bizid integer
		constraint planagts_bizid_fk
			references orgs
)
inherits (_arts);

alter table planagts owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, adapted, adapter, id, fork, sprid, license, trust, regid, addr, x, y, mgrid, mgrname, mgrtel, mgrim, icon) as
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


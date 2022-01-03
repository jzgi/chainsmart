create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

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
	license varchar(20),
	idcard varchar(18),
	icon bytea
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
	unitfee money,
	icon bytea
)
inherits (_infos);

alter table items owner to postgres;

create table _deals
(
	itemid integer,
	artid integer,
	artname integer,
	handled timestamp(0),
	handler varchar(10),
	price money,
	qty smallint,
	pay money,
	refund money
)
inherits (_infos);

alter table _deals owner to postgres;

create table userlgs
(
	userid integer not null,
	cont jsonb
);

alter table userlgs owner to postgres;

create table _articles
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

alter table _articles owner to postgres;

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
	postg smallint,
	postprice money,
	rank smallint,
	icon bytea,
	img bytea,
	cert bytea,
	constraint products_orgid_fk
		foreign key (orgid) references orgs,
	constraint products_itemid_fk
		foreign key (itemid) references items
)
inherits (_articles);

alter table products owner to postgres;

create table posts
(
	id serial not null
		constraint posts_pk
			primary key,
	productid integer
)
inherits (_articles);

alter table posts owner to postgres;

create table clears
(
	id serial not null
		constraint clears_pk
			primary key,
	till timestamp(0),
	recs integer,
	amount money,
	pay money
)
inherits (_infos);

alter table clears owner to postgres;

create table buys
(
	id bigserial not null
		constraint buys_pk
			primary key,
	bizid smallint not null,
	bizname varchar(10),
	mrtid smallint not null,
	mrtname varchar(10),
	uid integer not null,
	uname varchar(10),
	utel varchar(11),
	uim varchar(28)
)
inherits (_deals);

alter table buys owner to postgres;

create table books_
(
	id bigserial not null
		constraint books_pk
			primary key,
	bizid smallint not null,
	bizname varchar(10),
	mrtid smallint not null,
	mrtname varchar(10),
	ctrid smallint not null,
	ctrname varchar(10),
	prvid smallint not null,
	prvname varchar(10),
	srcid smallint not null,
	srcname varchar(10),
	seq_ integer,
	cs_ varchar(32),
	blockcs_ varchar(32),
	peer_ smallint not null,
	rec_ bigint
)
inherits (_deals);

alter table books_ owner to postgres;

create table links
(
	ctrid integer
		constraint links_ctrid_fk
			references orgs,
	ptid integer
		constraint links_ptid_fk
			references orgs
)
inherits (_infos);

alter table links owner to postgres;

create index links_typctrid_idx
	on links (typ, ctrid);

create index links_typptid_idx
	on links (typ, ptid);

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


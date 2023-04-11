#!/bin/bash
mkdir /sup
chown -R postgres:postgres /sup
mkdir /rtl
chown -R postgres:postgres /rtl

service postgresql start
su - postgres -c "psql -f /db/init.db.sql"
su - postgres -c "psql -dsmart -f /db/smart.sql"
rm -rf /db
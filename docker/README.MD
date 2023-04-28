## database
```bash
cat ./db/init-sql/smart-schema.sql > ./db/init-db/smart.sql
cat ./db/init-sql/smart-table.sql >> ./db/init-db/smart.sql
cat ./db/init-sql/smart-data.sql >> ./db/init-db/smart.sql
```

## build image
```bash
docker build -t chainsmart .
```

## run container
```bash
docker run -itd -p 80:80 -p 81:81 -p 5432:5432 --restart=always -v $PWD/nginx/sites-enabled:/etc/nginx/sites-enabled chainsmart 
```

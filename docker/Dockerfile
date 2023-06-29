FROM ubuntu:20.04
LABEL Author="CalcYu@live.com"

RUN sed -i s@/archive.ubuntu.com/@/mirrors.aliyun.com/@g /etc/apt/sources.list \
&& sed -i s@/security.ubuntu.com/@/mirrors.aliyun.com/@g /etc/apt/sources.list \
&& apt-get clean \
&& apt-get update \
&& apt-get -y install \
wget 

RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
&& dpkg -i packages-microsoft-prod.deb \ 
&& rm packages-microsoft-prod.deb

RUN apt-get update \
&& apt-get install -y \
dotnet-runtime-7.0 \
nginx


RUN apt-get install -y \
gnupg2
RUN wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add - 
RUN echo "deb http://apt.postgresql.org/pub/repos/apt/ focal-pgdg main" | tee /etc/apt/sources.list.d/postgresql-pgdg.list > /dev/null
RUN apt-get update -y \
&& apt-get install -y \
postgresql-9.6

# DB
COPY ./db/conf/. /etc/postgresql/9.6/main/.
WORKDIR /db
COPY ./db/init-sql/. ./
RUN chmod +x ./setup.sh && ./setup.sh

# Web
WORKDIR /app
COPY app/. ./
# nginx
VOLUME [ "/etc/nginx/sites-enabled" ]
EXPOSE 80
EXPOSE 81
EXPOSE 443
EXPOSE 5432
COPY ./entrypoint.sh .
ENTRYPOINT ["/bin/bash","entrypoint.sh"]
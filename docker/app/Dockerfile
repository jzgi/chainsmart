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
dotnet-runtime-7.0

# Web
WORKDIR /app
COPY chainsmart/. ./

EXPOSE 80
EXPOSE 81
EXPOSE 443
ENTRYPOINT ["dotnet","lib/ChainSmart.dll"]
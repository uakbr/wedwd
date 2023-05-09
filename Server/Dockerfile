FROM ubuntu:focal

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://*:5000"

RUN \
  apt-get -y update && \
  apt-get -y install \
  apt-utils \
  wget \
  apt-transport-https \
  unzip \
  acl \
  libssl1.0

RUN \
  wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb && \
  dpkg -i packages-microsoft-prod.deb && \
  apt-get -y update && \
  apt-get -y install aspnetcore-runtime-5.0

RUN \
  mkdir -p /var/www/iControl && \
  mkdir /config && \
  wget -q https://github.com/lucent-sea/iControl/releases/latest/download/iControl_Server_Linux-x64.zip && \
  unzip -o iControl_Server_Linux-x64.zip -d /var/www/iControl && \
  rm iControl_Server_Linux-x64.zip

RUN \
  mkdir -p /iControl-data && \
  sed -i 's/DataSource=iControl.db/DataSource=\/iControl-data\/iControl.db/' /var/www/iControl/appsettings.json

VOLUME "/iControl-data"

WORKDIR /var/www/iControl

COPY DockerMain.sh /

RUN chmod 755 /DockerMain.sh

ENTRYPOINT ["/DockerMain.sh"]
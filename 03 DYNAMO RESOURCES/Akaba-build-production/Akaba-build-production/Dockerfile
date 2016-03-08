###
# Akaba services docker build file
FROM ubuntu:14.04
MAINTAINER Giles Hall
LABEL SERVICENAME=web

###
# Base Environment :: locale
RUN locale-gen en_US.UTF-8  
ENV LANG en_US.UTF-8  
ENV LANGUAGE en_US:en  
ENV LC_ALL en_US.UTF-8  

###
# Base Environment :: apt-get
RUN apt-get update
RUN apt-get -u dist-upgrade -y

###
# Akaba Service :: togo
RUN \
    apt-get install -y software-properties-common && \
    add-apt-repository -y ppa:nginx/stable && \
    apt-get update && \
    apt-get install -y nginx && \
    echo "\ndaemon off;" >> /etc/nginx/nginx.conf
ADD . /var/www/html/
RUN \
    chown -R www-data:www-data /var/www/html/

ENTRYPOINT ["nginx"]
EXPOSE 80

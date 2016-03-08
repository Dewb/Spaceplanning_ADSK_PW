###
# Akaba services docker build file
FROM ubuntu:14.04
MAINTAINER Giles Hall
LABEL SERVICENAME=lsystem

###
# Base Environment :: locale
RUN locale-gen en_US.UTF-8  
ENV LANG en_US.UTF-8  
ENV LANGUAGE en_US:en  
ENV LC_ALL en_US.UTF-8  

###
# Base Environment :: apt-get
RUN \
    apt-get update && \
    apt-get -u dist-upgrade -y && \
    apt-get install -y software-properties-common

###
# Base Environment :: environment variables and directories
ENV AKABA_HOME /opt/akaba
ENV AKABA_SERVICES_HOME ${AKABA_HOME}/services
ENV AKABA_SRC_HOME ${AKABA_HOME}/src
RUN \
    mkdir -p ${AKABA_HOME} && \
    mkdir -p ${AKABA_SERVICES_HOME} && \
    mkdir -p ${AKABA_SRC_HOME}

###
# Akaba Service :: casablanca dependency
#   as of right now (Apr 9th, 2015), Casablanca is only available in 
#   the Vivid Veret release of Ubuntu (15.04), but it also maintained
#   in a PPA (https://launchpad.net/~vslavik/+archive/ubuntu/poedit)
#   for older versions of Ubuntu.
RUN \
    add-apt-repository -y ppa:vslavik/poedit && \
    apt-get update && \
    apt-get install -y libcpprest-dev

###
# Akaba Service :: build essential
RUN apt-get install -y build-essential libboost-all-dev cmake clang

###
# Akaba Service :: lsystem
ADD . ${AKABA_SRC_HOME}/lsystem
RUN \
    mkdir ${AKABA_SRC_HOME}/lsystem/build && \
    cd ${AKABA_SRC_HOME}/lsystem/build && \
    cmake -DCMAKE_INSTALL_PREFIX:PATH=/opt/akaba/services/lsystem .. && \
    make && \
    make install
EXPOSE 34569
ENTRYPOINT ["/opt/akaba/services/lsystem/bin/AkabaGeneratorService", "-a", "0.0.0.0", "-p", "34569"]

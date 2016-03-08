###
# Akaba services docker build file
FROM ubuntu:14.04
MAINTAINER Giles Hall
LABEL SERVICENAME=togo

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
# Akaba Service :: togo
RUN \
    add-apt-repository -y ppa:pypy/ppa && \
    apt-get update && \
    apt-get install -y python-software-properties python python-dev python-pip graphviz python-setuptools python-virtualenv pypy
ADD . ${AKABA_SRC_HOME}/togo
RUN \
    mkdir -p /opt/akaba/services/togo && \
    virtualenv -p /usr/bin/pypy /opt/akaba/services/togo && \
    /opt/akaba/services/togo/bin/pip install pqdict graphviz && \
    cd ${AKABA_SRC_HOME}/togo && /opt/akaba/services/togo/bin/python setup.py install
EXPOSE 34568
ENTRYPOINT ["/opt/akaba/services/togo/bin/run_venv.sh", "/opt/akaba/services/togo/", "/opt/akaba/services/togo/bin/togo_service.py", "-a", "0.0.0.0", "-p", "34568"]

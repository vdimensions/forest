apt-get -y update
apt-get -y install curl g++ pkg-config libgdiplus libunwind8 libssl-dev make mono-complete gettext libssl-dev libcurl4-openssl-dev zlib1g libicu-dev uuid-dev unzip libgtk2.0-dev libglib2.0-dev

export CFLAGS=`pkg-config --cflags glib-2.0` 
export LDLIBS=`pkg-config --libs glib-2.0`
export LINK=g++

npm i tjanczuk/edge
npm cache clean

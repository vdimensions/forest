# A wrapper around paket.exe and paket.bootstrapper.exe, located in ./.paket/ directory,
# relative to the location of this file.
# The purpose of this file is to allow correct cross-platform invocation of paket 

EXEC='${0%/*}/paket.exe'

if [ "$(uname)" == "Darwin" ]; then
  EXEC="mono ${EXEC}";
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
  EXEC="mono ${EXEC}";
fi

eval "${EXEC} \"$@\""

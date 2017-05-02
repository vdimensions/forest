EXEC='${0%/*}/../.paket/paket.exe'

if [ ! -f ${EXEC} ]; then
  if [ "$(uname)" == "Darwin" ]; then
    eval "mono ${EXEC%.exe}.bootstrapper.exe";
  elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    eval "mono ${EXEC%.exe}.bootstrapper.exe";
  elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW32_NT" ]; then
    eval "${EXEC%.exe}.bootstrapper.exe";
  elif [ "$(expr substr $(uname -s) 1 10)" == "MINGW64_NT" ]; then
    eval "${EXEC%.exe}.bootstrapper.exe";
  fi
fi

if [ "$(uname)" == "Darwin" ]; then
  EXEC="mono ${EXEC}";
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
  EXEC="mono ${EXEC}";
fi

eval "${EXEC} \"$@\""

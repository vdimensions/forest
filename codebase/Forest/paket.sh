PAKET='../../.paket/paket.exe'
if [ ! -f $PAKET ]; then
  eval "${PAKET%.exe}.bootstrapper.exe"
fi
$PAKET "$@"

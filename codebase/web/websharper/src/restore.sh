paket='.paket/paket.sh'
project='Forest.Web.WebSharper'

$paket update
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi
rm -rf obj/
dotnet restore $project.fsproj
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi

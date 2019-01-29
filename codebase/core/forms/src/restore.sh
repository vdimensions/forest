paket='.paket/paket.sh'
project='Forest.Forms'
project_format='csproj'

$paket update && rm -rf obj/ && dotnet restore $project.$project_format
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi
$paket simplify
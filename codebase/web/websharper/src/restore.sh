paket='.paket/paket.sh'
project='Forest.Web.WebSharper'
project_format='fsproj'

rm -rf paket-files/ && $paket update && rm -rf obj/ && mkdir obj/ && dotnet restore $project.$project_format
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi
$paket simplify

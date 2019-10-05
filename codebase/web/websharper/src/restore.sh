project='Forest.Web.WebSharper'
project_format='fsproj'

dotnet restore $project.$project_format
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi

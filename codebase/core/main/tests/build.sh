project='Forest.Core.Tests'
project_format='csproj'

 ./restore.sh

dotnet clean $project.$project_format && dotnet build $project.$project_format
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi

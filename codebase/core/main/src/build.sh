project='Forest.Core'

./restore.sh

dotnet msbuild $project.fsproj
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi
dotnet msbuild $project.dist.csproj
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi
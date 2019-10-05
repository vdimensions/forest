msbuild="../../../../submodules/btw/msbuild.sh"
project='Forest.Engine'
project_format='csproj'

./restore.sh

dotnet clean $project.$project_format && $msbuild $project.$project_format && dotnet pack $project.$project_format --no-build
if [ $? -ne 0 ]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi

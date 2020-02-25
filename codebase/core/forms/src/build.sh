project_name='Forest.Forms'
project_format='csproj'
project="${project_name}.${project_format}"

./restore.sh

dotnet clean ${project} && dotnet build ${project} && dotnet pack ${project} --no-build
if [[ $? -ne 0 ]]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi

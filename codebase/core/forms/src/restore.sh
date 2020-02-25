project_name='Forest.Forms'
project_format='csproj'
project="${project_name}.${project_format}"

dotnet restore ${project}
if [[ $? -ne 0 ]]; then
  read -rsp "Press [Enter] to quit"
  echo ""
  exit
fi
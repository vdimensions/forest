dir=`pwd`
if [ ! -z "$1" ]; then
  dir=$(echo $1|sed 's/\\/\//g')
fi
packages_dir=""
if [ ! -z "$3" ]; then
  packages_dir=$(echo $3|sed 's/\\/\//g');
  mkdir -p "$packages_dir"
  echo "Packages will be copied to $packages_dir"
fi
cd $dir

echo "Removing existing packages"
rm -rf *.nupkg
echo "----------------"
if [ -z "$2" ]; then
  for file in $(ls *.nuspec); do
    nuget pack "$file"
    echo "----------------"
  done
else
  for file in $(ls *.nuspec | grep "$2"); do
    nuget pack "$file"
    echo "----------------"
  done
fi

for file in $(ls *.nupkg); do
  if [ ! -z "$packages_dir" ]; then
    cp -rfv $file $packages_dir/
  fi
  #nuget push "$file" -s http://localhost/nuget.vdimensions.com/ VDimensions
  #echo "----------------"
  #nuget push "$file" -s http://nuget.vdimensions.net/nuget-repo/ VDimensions
  #echo "----------------"
done

echo "Cleaning up..."
rm -rf *.nupkg
echo "Done."
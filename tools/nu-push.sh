for file in $(ls ../packs/*.nupkg); do
  nuget push "$file" -s http://localhost/nuget.vdimensions.com/ VDimensions
  echo "----------------"
  nuget push "$file" -s http://nuget.vdimensions.net/nuget-repo/ VDimensions
  echo "----------------"
done

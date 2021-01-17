dirs=(
    "codebase/core/main"
    "codebase/ui/main"
)
base_dir=`pwd`
version="2.8.3"
registry="http://192.168.1.178:8373"

for dir in ${dirs[@]}; do
  cd "$base_dir/$dir"
  npm version $version
  npm publish --registry $registry
  cd $base_dir
done
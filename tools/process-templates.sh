function get_path() {
    pushd `dirname $0` > /dev/null
    local scpth=`pwd -P`
    popd > /dev/null
    echo "$scpth";
}
function normalize_path() {
  echo $(echo $1|sed 's/\\/\//g')
}

function export_vars() {
  IFS=$'\n'
  for line in $(cat $1); do
    IFS=$'|'
    data=(${line/\=/\|});
    IFS=$'\n'
    eval "export ${data[0]}=${data[1]}";
  done
  IFS=$' '
  #read -rsp $'Press [Enter] to continue...\n'
}
function print_vars() {
  echo "Environment setup:"
  IFS=$'\n'
  for line in $(cat $1); do
    IFS=$'|'
    data=(${line/\=/\|});
    IFS=$'\n'
    eval "echo \"  ${data[0]} \$${data[0]}\"";
  done
  IFS=$' '
  echo "------"
}
function unset_vars() {
  IFS=$'\n'
  for line in $(cat $1); do
    IFS=$'|'
    data=(${line/\=/\|});
    IFS=$'\n'
    eval "unset ${data[0]}";
  done
  IFS=$' '
}
function substitute() {
  echo 'cat <<END_OF_TEXT' >  temp.swp
  cat "$1" >> temp.swp
  echo "" >> temp.swp
  echo 'END_OF_TEXT' >> temp.swp
  bash temp.swp > "$2"
  rm temp.swp
}

root=$1;
if [ -z "$root" ]; then
  root="$(get_path)";
  root="$root/.."
fi
root=$(normalize_path "$root");

workdir=$2;
if [ -z "$workdir" ]; then
  workdir="$root";
fi
workdir=$(normalize_path "$workdir");

echo "ROOT:     $root"
echo "WORKDIR:  $workdir"

############

for props in "$root/build.properties" "$workdir/build.properties"; do
  if [ -e "$props" ]; then
    export_vars "$props";
    print_vars "$props";
  fi
done

cd "$workdir"
echo "Processing files "
IFS=$'\n'
if [ "$workdir" == "$root" ]; then
  for file in $(find | grep "build\.properties\.template"); do
    target=${file%.template};
    echo -ne "> $target..."
    substitute "$file" "$target"
    echo " [ok]"
  done
else
  for file in $(find | grep "\.template"); do
    target=${file%.template};
    echo -ne "> $target..."
    if [ "$(basename $target)" == "build.properties" ]; then 
      echo " [skipped]"
    else
      substitute "$file" "$target"
      echo " [ok]"
    fi
  done
fi
IFS=$' '
cd "$root"

for props in "$root/build.properties" "$workdir/build.properties"; do
  if [ -e "$props" ]; then
    unset_vars "$props";
  fi
done

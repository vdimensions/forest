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
}
function rewrite_vars() {
  cp "$1" "$1.swp"
  IFS=$'\n'
  echo "" > "$1";
  for line in $(cat $1.swp); do
    IFS=$'|'
    data=(${line/\=/\|});
    IFS=$'\n'
    if [ "${data[0]}" == "$2" ]; then
      eval "echo ${data[0]}=\$${data[0]} >> $1";
    else 
      echo $line >> $1;
    fi
  done
  IFS=$' '
  rm -rf "$1.swp"
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

workdir=$1;
if [ -z "$workdir" ]; then
  root="$(get_path)";
fi
workdir=$(normalize_path "$workdir");

echo "WORKDIR:  $workdir"

############

for props in "$workdir/build.properties"; do
  if [ -e "$props" ]; then
    export_vars "$props";
  fi
done

eval "$2=\$((\$$2+1))"

rewrite_vars "$workdir/build.properties" "$2"
unset_vars "$workdir/build.properties";

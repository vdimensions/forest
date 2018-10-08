scriptdir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
remote=$1

if [ -z "$remote" ]; then
  echo "invalid remote"
  exit
fi

remote_uri="$(git config --get remote.$remote.url)"
echo "=== Removing remote directory ($remote_uri) ..."
echo ""
rm -rf "$remote_uri"
if [ $? -ne 0 ]; then
  read -rsp "! An error occurred. Press [Enter] to quit"
  echo ""
  exit
fi
echo ""
echo "=== Removing remote directory [Done]"
echo ""
echo ""
echo "=== Recreating remote ..."
echo ""
mkdir "$remote_uri"
cd "$remote_uri"
git init --bare ./
cd "$scriptdir/../"
echo "=== Recreating remote [Done]"
echo ""
echo ""
echo "=== Compressing and pushing ... "
echo ""
git gc && git push $remote
echo "=== Compressing and pushing [Done] "
echo ""

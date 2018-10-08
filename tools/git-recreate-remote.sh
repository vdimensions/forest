scriptdir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
remote=$1
branch=$2
if [ -z "$branch" ]; then
  branch='master'
fi

if [ -z "$remote" ]; then
  echo "invalid remote"
  exit
fi

remote_uri="$(git config --get remote.$remote.url)"
echo "=== Removing remote directory ($remote_uri) ..."
rm -rf "$remote_uri"
if [ $? -ne 0 ]; then
  read -rsp "! An error occurred. Press [Enter] to quit"
  echo ""
  exit
fi
echo "=== Removing remote directory [Done]"
echo ""
echo ""
echo "=== Recreating remote ..."
echo ""
mkdir "$remote_uri"
cd "$remote_uri"
git init --bare ./
echo ""
echo "=== Recreating remote [Done]"
echo ""
echo ""
echo "=== Compressing and pushing ... "
echo ""
cd "$scriptdir/../"
git gc && git push $remote $branch
echo "=== Compressing and pushing [Done] "
echo ""

scriptdir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
remote=$1
branch=$2
if [ -z "$remote" ]; then
  remote='origin'
fi
if [ -z "$branch" ]; then
  branch='master'
fi
cd "$scriptdir/../"
git fetch $remote && git pull $remote $branch
git submodule init && git submodule update && git submodule foreach git fetch origin && git submodule foreach git reset origin/master --hard

#!/bin/sh
remote=$1
if [ -z "$remote" ]; then
  remote='origin'
fi
docsDir='_site'
currentBranch=$(git symbolic-ref --short HEAD)

echo "Generating dedicated branch for documentation"
git checkout -B docgen

rm -rf ../$docsDir
docfx 
cd ../

echo "Committing generated documentation in brach 'docgen'"
git add $docsDir && git commit -m "Pushing docfx generated documentation"
#git subtree push --prefix $docsDir $remote gh-pages

echo "Pushing subtree with documentation from $remote:docgen into $remote:gh-pages branch"
git push $remote `git subtree split --prefix $docsDir docgen 2> /dev/null`:gh-pages --force

echo "Restoring current branch $currentBranch"
git checkout $currentBranch

echo "Deleting docgen branch"
git branch -D docgen
 
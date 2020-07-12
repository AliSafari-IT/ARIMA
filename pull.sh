printf "\n"

echo "."
echo ".."
echo "..."
echo "...."
echo "....."
echo "...... Updating  local branch repository!"
echo "....."
echo "...."
echo "..."
echo ".."
echo "."
printf "\n"

currentBranch=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

echo "Git Pull to the current branch ($currentBranch):";
git pull

set +e
git diff-index --quiet HEAD

if [ $? == 1 ] ; then
  set -e
  echo "Git local working directory is clean from any changes or any script"
else
  set -e
  echo "Git local working directory is clean"
fi

printf "\n \n"

echo "Press any key to continue"
while [ true ] ; do
read -t 2 -n 1
if [ $? = 0 ] ; then
exit ;
else
echo -n "."
fi
done

printf "\n \n"


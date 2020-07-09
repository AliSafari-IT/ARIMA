printf "\n"
echo" Updating  remote repository...."
printf "\n"
echo "."
echo ".."
echo "..."
echo "...."
echo "....."
echo "...... Updating  remote repository!"
echo "....."
echo "...."
echo "..."
echo ".."
echo "."

currentBranch=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')



git add .
timestamp=$(date +"%D %T")

git commit -m "Power Energy Modeling: Branch $currentBranch ($timestamp)"

git push

if [[ "$(git push --porcelain)" == *"Done"* ]]
then
  echo "Git Push was successful!"
fi


sleep 5
printf "\n"
echo" Power Energy Modeling"
printf "\n"
echo "."
echo ".."
echo "..."
echo "...."
echo "....."
echo "...... Git Checkout a Remote Branch"
echo "....."
echo "...."
echo "..."
echo ".."
echo "."

git branch -a

currentBranch=$(git branch | sed -n -e 's/^\* \(.*\)/\1/p')

echo "Enter the new branch name"
read newBranch

git checkout -b $newBranch $currentBranch
git push -u origin HEAD

sleep 5
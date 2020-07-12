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
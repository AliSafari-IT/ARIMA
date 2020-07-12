printf "\n \n \n"
echo "........ Power Energy Modeling ......."
printf "\n"
echo "."
echo ".."
echo "..."
echo "...."
echo "....."
echo "...... Check if pull needed in Git!"
echo "....."
echo "...."
echo "..."
echo ".."
echo "."
printf "\n \n"
echo "Current state:" 
UPSTREAM=${1:-'@{u}'}
LOCAL=$(git rev-parse @)
REMOTE=$(git rev-parse "$UPSTREAM")
BASE=$(git merge-base @ "$UPSTREAM")

if [ $LOCAL = $REMOTE ]; then
    echo "Up-to-date"
elif [ $LOCAL = $BASE ]; then
    echo "Need to pull"
elif [ $REMOTE = $BASE ]; then
    echo "Need to push"
else
    echo "Diverged"
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
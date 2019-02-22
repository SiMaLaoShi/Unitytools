#!/Program Files/git/bin/bash
function modifyMD5(){
	dir=$1
	files=`ls -A $dir`
	for file in $files; do
		path="$dir/$file"
		if [[ -d $path ]]; then
			modifyMD5 $path
		elif [[ -f $path ]]; then
			line=`md5sum.exe $path`
			echo $line >> md5.txt
		fi
	done
}

# touch md5.txt
# modifyMD5 $(dirname $(readlink -f "$0"))

# coding:utf-8

import os
import re

def out_replace_file(component):
	# return r"replace_component(i,r'GetComponent("+'"' + component + '")'+"'"+',r"GetComponent(typeof(' + component + '))")\n'
	return r'replace_component(i,r"GetComponent('+"'" + component + "')"+'"'+",r'GetComponent(typeof(" + component + "))')\n"
	# return "replace_component(i,r'""+component+'"',"typeof("+component+")")"

def check_component(filePath):

	savePath = "1.txt"
	file = open(savePath,"wb")
	file.write("")
	file.close()
	with open(savePath, 'a') as outFile:
		with open(filePath,'rb') as inFile:
			while True:
				content = inFile.readline()
				# if re.findall(r"_GT(typeof((.*?)))"):
				# 	outFile.write(content)

				# p1 = re.compile(r'[typeof(](.*?)[))]', re.S)  #最小匹配

				# print(re.findall(p1, content))
				# if re.findall(p1,content):
				# 	outFile.write(re.findall(p1,content))
				str = content.strip()
				str = (out_replace_file(str).strip("\n"))
				# print(out_replace_file(str))
				print(str)
				outFile.write(str + "\n")
				# print(out_replace_file(content))
				if not content:
					return
				pass

	# with open(filePath,"rb") as inFile:
	# 	while True:
	# 		content = inFile.readline()
	# 		print(content)
	# 		if not content:
	# 			return
	# 		pass


if __name__ == '__main__':
	# check_component("F://XGame//Assets//Editor//Custom//CustomSettings.cs")
	check_component("Component.txt")
	# print(out_replace_file("UILabel"))

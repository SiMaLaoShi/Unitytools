# coding:utf-8
# outher 司马老师 
import os
import re
import sys
reload(sys)
sys.setdefaultencoding('utf8')

#读文件
def start_find_chinese(filePath):
    find_count = 0;
    file = open("chinese.txt", "a")
    file.write("\n//-------------------"+ filePath +"-------------------\n")
    file.close()
    with open("chinese.txt", 'a') as outfile:
        with open(filePath, 'rb') as infile:
            while True:
                content = infile.readline()
                if re.findall(r'"(.*?)"', content):
                    if check_contain_chinese(content) :
                        if check_str_log(content):
                            outfile.write(content)
                            find_count += 1;  
                            pass  
                        pass
                if not content:
                    return find_count 
#判断字符是不是日志类型
def check_str_log(str):
    if str.find("print") != -1:
        return False
        pass
    if str.find("error") != -1:
        return False
        pass
    if str.find("warn") != -1:
        return False
        pass
    if str.find("log") != -1:
        return False
        pass
    return True
    pass
#判断是否中文+
def check_contain_chinese(check_str):
     for ch in check_str.decode('utf-8'):
         if u'\u4e00' <= ch <= u'\u9fff':
           return True
     return False
#获取所有文件
def get_files(path, rule=".lua"):
    all = []
    for fpathe,dirs,fs in os.walk(path):   # os.walk是获取所有的目录
        for f in fs:
            filename = os.path.join(fpathe,f)
            if filename.endswith(rule):  # 判断是否是"xxx"结尾
                all.append(filename)
    return all

#清空文本文件
def clear_txt_file(filePath):
	 file = open(filePath,"wb")
     file.write("")
     file.close()
	pass

#在文本末追加换行标识
def txt_append_line(filePath):
	file = open("chinese.txt", "a")
    file.write("\n//-------------------"+ filePath +"-------------------\n")
    file.close()
	pass

# main
if __name__ == '__main__':
     file = open("chinese.txt","wb")
     file.write("")
     file.close()
     files = get_files(r"F:\\XGame\\Assets\\Lua")
     for i in files:
        start_find_chinese(i)
        print("search \t" + i + "over \t")

# coding:utf-8
#############################################
# 查找文件中出现的中文字符串                  #
#############################################

import os
import re
import sys
reload(sys)
sys.setdefaultencoding('utf8')

#输出查找结果的路径
savePath = "chinese.txt"
#需要查找的根文件夹
resPath = r"F:\\XGame\\Assets\\Lua\\System\\Chat"
#查找的匹配模式（单引号，双引号）
patterns = [r"'(.*?)'",r'"(.*?)"']

#读文件
def start_find_chinese(filePath):
    find_count = 0;
    if check_file_need_replace(filePath):
        file = open(savePath, "a")
        file.write("\n//-------------------" + filePath + "-------------------\n")
        file.close()
    count = 0
    with open(savePath, 'a') as outfile:
        with open(filePath, 'rb') as infile:
            while True:
                content = infile.readline()
                count = count + 1
                # if re.findall(patterns[0], content) or re.findall(patterns[1],content):
                #     if check_contain_chinese(content):
                #         if check_str_log(content):
                #             outfile.write(content)
                #             find_count += 1;
                #             pass
                #         pass
                #     pass
                if content.find("FindNode") != -1:
                	print content
                	print count
                	pass
                if not content:
                    return find_count

#剪裁文本文件是否有需要替换的文本
def check_file_need_replace(filePath):
    find_count = 0
    with open(filePath,"rb") as infile:
        while True:
            content = infile.readline()
            if re.findall(patterns[0],content) or re.findall(patterns[1],content):
                if check_contain_chinese(content):
                    if check_str_log(content):
                        find_count += 1
                        break
            if not content:
                break
    return find_count > 0
    pass

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

# main
if __name__ == '__main__':
     file = open(savePath,"wb")
     file.write("")
     file.close()
     files = get_files(resPath)
     for i in files:
        start_find_chinese(i)
        print("search \t" + i + "\tover")
     print("查找结束")

#coding=utf-8
import sys
import os
resPath = r"F:\\XGame\\Assets\\Lua"
# 获取所有文件
def get_files(path, rule=".lua"):
    all = []
    for fpathe,dirs,fs in os.walk(path):   # os.walk是获取所有的目录
        for f in fs:
            filename = os.path.join(fpathe,f)
            if filename.endswith(rule):  # 判断是否是"xxx"结尾
                all.append(filename)
    return all

def replace_component(file_path,old_str,new_str):
    try:
        f = open(file_path, 'r+')
        all_lines = f.readlines()
        f.seek(0)
        f.truncate()
        for line in all_lines:
            line = line.replace(old_str, new_str)
            f.write(line)
        f.close()
    except Exception, e:
        print e

if __name__ == '__main__':
     files = get_files(resPath)
     for i in files:
        print("search \t" + i + "\tover")

     print("查找结束")

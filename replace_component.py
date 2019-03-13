#coding=utf-8
import sys
import os
resPath = r"F:\\XGame\\Assets\\Lua\\System\\WorldBoss"
########################################
## 批量替换tolua中使用字符串获取组件的方式为改用泛型获取，效率更高
########################################

# 获取所有文件
def get_files(path, rule=".lua"):
    all = []
    for fpathe,dirs,fs in os.walk(path):   # os.walk是获取所有的目录
        for f in fs:
            filename = os.path.join(fpathe,f)
            if filename.endswith(rule):  # 判断是否是"xxx"结尾
                all.append(filename)
    return all

# 替换字符串
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
        replace_component(i,r':GetComponent("UILabel")',r":GetComponent(typeof(UILabel))")
        replace_component(i,r":GetComponent('UILabel')",r":GetComponent(typeof(UILabel))")
        # todo 替换所有组件，从CustomSettings中注册中的
        print(i)
     print("替换结束")

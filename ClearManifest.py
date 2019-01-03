# -*- coding: UTF-8 -*-

'''
Created on 2017年1月1日

@author: ckdroid
'''

import xml.etree.ElementTree as ET
import sys
import shutil

class PCParser(ET.XMLTreeBuilder):
    def __init__(self):
        ET.XMLTreeBuilder.__init__(self)
        # assumes ElementTree 1.2.X
        self._parser.CommentHandler = self.handle_comment

    def handle_comment(self, data):
        self._target.start(ET.Comment, {})
        self._target.data(data)
        self._target.end(ET.Comment)

# 备份源文件
shutil.copy("AndroidManifest.xml", "AndroidManifest_backup.xml")

try:
    # 定义namespace，这一步必须在parse之前做
    ET.register_namespace('android', "http://schemas.android.com/apk/res/android")
    ET.register_namespace('tools', "http://schemas.android.com/tools")
    ET.register_namespace('app', "http://schemas.android.com/apk/res-auto")
    parser = PCParser()
    tree = ET.parse("AndroidManifest.xml",parser)     #打开xml文档 
    root = tree.getroot()         #获得root节点  
except Exception, e: 
    print "Error:cannot parse file:AndroidManifest.xml."
    print e 
    sys.exit(1) 


print('root.tag =', root.tag)

#获得 android 的 name space
name_space="{http://schemas.android.com/apk/res/android}"

mdict = { };

print('----- clear uses-permission -----')

for element in root.findall('uses-permission'):
    rank = element.get(name_space+'name')
    if(mdict.has_key(rank)):
        root.remove(element)
        print(rank)
    else:
        mdict[rank]=element
    
print('----- clear done -----')

tree.write('AndroidManifest.xml',encoding="utf-8", xml_declaration=True,  method='xml')


import os
import shutil
import locale
import sys

# 设置输出字符集为 UTF-8
locale.setlocale(locale.LC_ALL, "")
sys.stdout = open(sys.stdout.fileno(), mode="w", encoding="utf-8", buffering=1)
sys.stderr = open(sys.stderr.fileno(), mode="w", encoding="utf-8", buffering=1)

#获取当前目录
curr_dir = os.getcwd()

out_dir = os.path.join(os.getcwd(), "out-csharp")
if not os.path.exists(out_dir):
    os.makedirs(out_dir)
print('输出目录：'+out_dir)

#遍历curr_dir/proto目录下的所有.proto文件
for file in os.listdir(curr_dir+'/proto'):
    if file.endswith(".proto"):
        proto_path = os.path.join(curr_dir+'/proto', file)
        print('proto_path:'+proto_path)
        # 执行bin/protoc -I=proto --csharp_out=out-csharp {proto_path}
        cmd = f'{curr_dir}/bin/protoc -I=proto --csharp_out=out-csharp {proto_path}'
        print(cmd)
        os.system(cmd)




# 复制生成的文件到指定目录
src_dir = out_dir
dst_dir1 = "C:/Users/Hello/source/repos/mmo-server/Common/Summer/Proto"
dst_dir2 = "D:/WorkSpace/UnityProjects/MMOGAME/Assets/Plugins/Summer/Proto"

for file in os.listdir(src_dir):
    if file.endswith(".cs"):
        src_file = os.path.join(src_dir, file)
        dst_file1 = os.path.join(dst_dir1, file)
        dst_file2 = os.path.join(dst_dir2, file)
        shutil.copy2(src_file, dst_file1)
        shutil.copy2(src_file, dst_file2)

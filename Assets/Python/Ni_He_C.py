import json
import os
import numpy as np
from hmmlearn import hmm
import joblib
import sys
import time
import multiprocessing
import threading
import win32pipe, win32file, pywintypes

# 定义管道名称，这是客户端和服务器通过管道通信的连接标识
PIPE_NAME = r'\\.\pipe\GesturePipe'

# 加载手势数据的方法
def load_gesture_data(file_path):
    try:
        # 打开文件并读取内容
        with open(file_path, 'r') as file:
            data = json.load(file)
        # 将文件中的不同数据字段合并成一个列表
        return (
            data['Palm_Movement_List'] + 
            data['Finger_Extension_List'] + 
            data['Palm_Angle_List'] + 
            data['Movement_Rate_List']
        )
    except Exception as e:
        print(f"Error loading file {file_path}: {e}")
        return []

# 数据预处理方法：标准化数据（让数据平均值为0，标准差为1）
def preprocess_data(data):
    data_array = np.array(data)  # 转换为numpy数组
    mean, std = np.mean(data_array), np.std(data_array)  # 计算均值和标准差
    # 标准化处理，返回标准化后的数据
    return (data_array - mean) / std

# 计算模型的Viterbi分数，用来评估该数据与模型的匹配度
def calculate_viterbi_score(model, test_data):
    preprocessed_data = preprocess_data(test_data)  # 先对数据进行标准化
    data_matrix = np.array(preprocessed_data).reshape(-1, 1)  # 将数据转换为适合HMM模型输入的格式
    try:
        log_prob, _ = model.decode(data_matrix, algorithm='viterbi')  # 使用Viterbi算法计算分数
        return log_prob  # 返回对数概率分数
    except ValueError:
        print("Error decoding data.")
        return float('-inf')  # 如果解码出错，返回负无穷

# 识别手势：加载数据并与预训练模型进行比对
def recognize_gesture(file_path):
    test_data = load_gesture_data(file_path)  # 加载手势数据
    if not test_data:
        return "NoData"  # 如果没有数据，返回“NoData”

    # 模型存放的目录路径
    model_dir = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Models/"
    label_scores = {}  # 存储每个模型的分数

    # 遍历目录中的所有模型文件
    for model_file in os.listdir(model_dir):
        if model_file.endswith("_hmm_model.pkl"):  # 只处理HMM模型文件
            label = model_file.replace("_hmm_model.pkl", "")  # 提取模型的标签
            try:
                # 加载HMM模型
                model = joblib.load(os.path.join(model_dir, model_file))
                # 计算该模型与测试数据的Viterbi分数
                label_scores[label] = calculate_viterbi_score(model, test_data)
            except FileNotFoundError:
                print(f"Model {model_file} not found.")
                continue

    # 如果没有找到任何模型，返回“NoModel”
    if not label_scores:
        return "NoModel"

    # 找到得分最高的标签（即最匹配的手势）
    best_label = max(label_scores, key=label_scores.get)
    return best_label

# 管道服务器，负责接收来自客户端的请求，并处理
def pipe_server():
    while True:
        try:
            # 创建命名管道
            pipe = win32pipe.CreateNamedPipe(
                PIPE_NAME,
                win32pipe.PIPE_ACCESS_DUPLEX,  # 双向通信
                win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
                1, 65536, 65536, 0, None
            )

            print("Waiting for Unity connection...")
            win32pipe.ConnectNamedPipe(pipe, None)  # 等待客户端（Unity）连接

            while True:
                try:
                    # 从管道读取数据（文件路径）
                    _, data = win32file.ReadFile(pipe, 65536)
                    file_path = data.decode().strip()  # 解码文件路径
                    print(f"Received request for: {file_path}")

                    # 调用手势识别函数，处理请求
                    result = recognize_gesture(file_path)
                    # 将结果写回管道，发送给客户端
                    win32file.WriteFile(pipe, result.encode())

                except pywintypes.error:
                    break

            # 关闭管道
            win32file.CloseHandle(pipe)

        except Exception as e:
            print(f"Pipe error: {e}")
            time.sleep(1)

# 主程序入口，启动管道服务器线程 
if __name__ == "__main__":
    pipe_thread = threading.Thread(target=pipe_server, daemon=True)
    pipe_thread.start()  # 启动管道服务器线程
    pipe_thread.join()  # 等待线程结束

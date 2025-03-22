import json
import os
import numpy as np
from hmmlearn import hmm
import joblib
import time
import threading
import win32pipe, win32file, pywintypes

# 管道名称，供 Unity 客户端连接
PIPE_NAME = r'\\.\pipe\GesturePipe'

# 处理Vector3和Quaternion数据
def process_vector3(vector3_list):
    return np.array([[v['x'], v['y'], v['z']] for v in vector3_list])

def process_quaternion(quaternion_list):
    return np.array([[q['x'], q['y'], q['z'], q['w']] for q in quaternion_list])

# 读取单个JSON文件并提取数据
def load_gesture_data(file_path):
    try:
        with open(file_path, 'r') as file:
            data = json.load(file)

        palm_movement = np.array(data['Palm_Movement_List']).reshape(-1, 1)
        finger_extension = np.array(data['Finger_Extension_List']).reshape(-1, 1)
        movement_rate = np.array(data['Movement_Rate_List']).reshape(-1, 1)
        palm_movement_direction = process_vector3(data['Palm_Movement_Direction_List'])
        palm_rotation_quaternion = process_quaternion(data['Palm_Rotation_Quaternion_List'])

        features = np.hstack((palm_movement, finger_extension, movement_rate, palm_movement_direction, palm_rotation_quaternion))

        return features
    except Exception as e:
        print(f"Error loading file {file_path}: {e}")
        return None

# 数据标准化
def normalize_data(data_list):
    """针对每个手势单独进行标准化"""
    normalized_data = []
    for data in data_list:
        mean = np.mean(data, axis=0)
        std = np.std(data, axis=0)
        std[std == 0] = 1e-8  # 避免除零错误
        normalized_data.append((data - mean) / std)
    return normalized_data

# 计算测试数据的 Viterbi 评分
def calculate_viterbi_score(model, test_data):
    scores = []
    for data in test_data:
        try:
            log_prob, _ = model.decode(data, algorithm='viterbi')
            scores.append(log_prob)
        except ValueError:
            print("Error decoding data. Skipping this sample.")
            scores.append(float('-inf'))  # 失败返回极小值
    return max(scores)  # 取最高分

# 处理 Unity 请求和返回结果
def handle_unity_request(pipe):
    while True:
        try:
            # 读取管道数据
            _, data = win32file.ReadFile(pipe, 65536)
            file_path = data.decode().strip()  # 获取文件路径
            print(f"Received request for: {file_path}")

            # 读取手势数据并标准化
            gesture_data = load_gesture_data(file_path)
            if gesture_data is None:
                result = "NoData"
            else:
                # 标准化数据
                gesture_data = normalize_data([gesture_data])[0]

                # 预测手势
                label_scores = {}

                # 加载模型
                for label in os.listdir("D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Models/"):
                    if label.endswith("_hmm_model.pkl"):
                        model_file = os.path.join("D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Models/", label)
                        try:
                            hmm_model = joblib.load(model_file)
                            score = calculate_viterbi_score(hmm_model, [gesture_data])
                            label_scores[label.replace("_hmm_model.pkl", "")] = score
                        except FileNotFoundError:
                            print(f"Model file {model_file} not found.")
                            continue

                if label_scores:
                    best_label = max(label_scores, key=label_scores.get)
                    result = f"{best_label}"
                else:
                    result = "NoModel"

            # 返回结果到Unity
            win32file.WriteFile(pipe, result.encode())
        except pywintypes.error:
            break

# 管道服务器
def pipe_server():
    while True:
        try:
            # 创建管道
            pipe = win32pipe.CreateNamedPipe(
                PIPE_NAME,
                win32pipe.PIPE_ACCESS_DUPLEX,
                win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
                1, 65536, 65536, 0, None
            )

            print("Waiting for Unity connection...")
            win32pipe.ConnectNamedPipe(pipe, None)  

            handle_unity_request(pipe)

            # 关闭管道
            win32file.CloseHandle(pipe)

        except Exception as e:
            print(f"Pipe error: {e}")
            time.sleep(1)

# 启动管道服务器线程
if __name__ == "__main__":
    pipe_thread = threading.Thread(target=pipe_server, daemon=True)
    pipe_thread.start()
    pipe_thread.join()

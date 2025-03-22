import json
import os
import numpy as np
from hmmlearn import hmm
import joblib
import matplotlib.pyplot as plt

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

# 加载指定文件夹下的所有JSON文件，并记录手势类别
def load_all_gesture_data_with_labels(main_folder_path):
    all_data = []
    labels = []
    for gesture_name in os.listdir(main_folder_path):
        gesture_path = os.path.join(main_folder_path, gesture_name)
        if os.path.isdir(gesture_path):
            for file_name in os.listdir(gesture_path):
                if file_name.endswith('.json'):
                    file_path = os.path.join(gesture_path, file_name)
                    gesture_data = load_gesture_data(file_path)
                    if gesture_data is not None:
                        all_data.append(gesture_data)
                        labels.append(gesture_name)
    return all_data, labels

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

if __name__ == "__main__":
    train_folder = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem"
    test_folder = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem_Test"

    # 加载训练数据
    X_train, y_train = load_all_gesture_data_with_labels(train_folder)
    if not X_train:
        print("No training data found. Please ensure the training folder contains valid JSON files.")
        exit(1)

    # 标准化训练数据
    X_train = normalize_data(X_train)

    # HMM 训练格式转换
    X_train_lengths = [len(seq) for seq in X_train]
    X_train_flattened = np.vstack(X_train)  # 合并为单个序列

    # 训练并保存 HMM 模型
    unique_labels = set(y_train)
    for label in unique_labels:
        label_data = [X_train[i] for i in range(len(y_train)) if y_train[i] == label]
        label_data_lengths = [len(seq) for seq in label_data]
        label_data_flattened = np.vstack(label_data)

        model = hmm.GaussianHMM(n_components=5, covariance_type="diag", n_iter=1000)
        model.fit(label_data_flattened, label_data_lengths)
        
        joblib.dump(model, f"{label}_hmm_model.pkl")

    # 加载测试数据
    X_test = []
    for file_name in os.listdir(test_folder):
        if file_name.endswith('.json'):
            file_path = os.path.join(test_folder, file_name)
            test_data = load_gesture_data(file_path)
            if test_data is not None:
                X_test.append(test_data)

    if not X_test:
        print("No test data found. Please ensure the test folder contains valid JSON files.")
        exit(1)

    # 标准化测试数据
    X_test = normalize_data(X_test)

    # 计算测试数据的匹配情况
    label_scores = {}

    for label in unique_labels:
        model_file = f"{label}_hmm_model.pkl"
        try:
            hmm_model = joblib.load(model_file)
            score = calculate_viterbi_score(hmm_model, X_test)
            label_scores[label] = score
        except FileNotFoundError:
            print(f"Model file {model_file} not found.")
            continue

    # 选取得分最高的手势
    if label_scores:
        best_label = max(label_scores, key=label_scores.get)
        print(f"The test data is most likely to belong to label: {best_label}")
    else:
        print("No suitable match found.")

    # 绘制匹配结果
    plt.figure(figsize=(10, 6))
    labels = list(label_scores.keys())
    scores = list(label_scores.values())
    plt.bar(labels, scores, color='b')

    if label_scores:
        best_score = label_scores[best_label]
        plt.axhline(y=best_score, color='r', linestyle='--', label=f'Best Match: {best_label}')

    plt.xlabel('Gesture Labels')
    plt.ylabel('Log Viterbi Score')
    plt.title('Log Viterbi Scores for Each Gesture')
    plt.xticks(rotation=45)
    plt.legend()
    plt.tight_layout()
    plt.show()

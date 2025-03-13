import json
import os
import numpy as np
from hmmlearn import hmm
import joblib

# 读取单个JSON文件并提取数据
def load_gesture_data(file_path):
    """
    该函数用于读取单个JSON文件，并从中提取所需的手势数据。
    :param file_path: 要读取的JSON文件的路径
    :return: 提取并组合后的手势数据列表，如果读取文件出错则返回空列表
    """
    try:
        with open(file_path, 'r') as file:
            data = json.load(file)
        palm_movement = data['Palm_Movement_List']
        finger_extension = data['Finger_Extension_List']
        palm_angle = data['Palm_Angle_List']
        movement_rate = data['Movement_Rate_List']
        combined_data = palm_movement + finger_extension + palm_angle + movement_rate
        return combined_data
    except Exception as e:
        print(f"Error loading file {file_path}: {e}")
        return []

# 加载指定文件夹下的所有JSON文件的数据，并记录手势类别
def load_all_gesture_data_with_labels(main_folder_path):
    """
    该函数用于加载指定主文件夹下所有手势文件夹中的JSON文件数据，并记录每个数据对应的手势类别。
    :param main_folder_path: 包含所有手势文件夹的主文件夹路径
    :return: 所有手势数据的列表和对应的标签列表
    """
    all_data = []
    labels = []
    gesture_folders = [os.path.join(main_folder_path, f) for f in os.listdir(main_folder_path) if os.path.isdir(os.path.join(main_folder_path, f))]
    for folder_path in gesture_folders:
        gesture_name = os.path.basename(folder_path)
        for file_name in os.listdir(folder_path):
            if file_name.endswith('.json'):
                file_path = os.path.join(folder_path, file_name)
                gesture_data = load_gesture_data(file_path)
                if gesture_data:
                    all_data.append(gesture_data)
                    labels.append(gesture_name)
    return all_data, labels

# 数据预处理，标准化数据
def preprocess_data(data):
    """
    该函数用于对单个手势数据进行标准化处理。
    :param data: 要处理的单个手势数据列表
    :return: 标准化后的手势数据数组
    """
    data_array = np.array(data)
    mean = np.mean(data_array)
    std = np.std(data_array)
    normalized_data = (data_array - mean) / std
    return normalized_data

# 对所有手势数据进行预处理，直接在训练时进行
def preprocess_all_data(train_data):
    """
    该函数用于对所有训练数据进行标准化处理。
    :param train_data: 所有训练数据的列表，每个元素为一个手势数据列表
    :return: 标准化后的所有训练数据列表
    """
    return [preprocess_data(gesture_data) for gesture_data in train_data]

# 训练单个HMM模型
def train_single_hmm_model(train_data, n_states=50, max_iter=100):
    """
    该函数用于训练单个隐马尔可夫模型（HMM）。
    :param train_data: 训练数据的列表，每个元素为一个手势数据列表
    :param n_states: HMM模型的隐藏状态数，默认为50
    :param max_iter: 模型训练的最大迭代次数，默认为100
    :return: 训练好的HMM模型
    """
    preprocessed_train_data = preprocess_all_data(train_data)
    data_matrix_list = [np.array(d).reshape(-1, 1) for d in preprocessed_train_data]
    lengths = [len(d) for d in preprocessed_train_data]
    train_data_matrix = np.concatenate(data_matrix_list)
    model = hmm.GaussianHMM(n_components=n_states, n_iter=max_iter)
    model.fit(train_data_matrix, lengths)
    return model

if __name__ == "__main__":
    main_folder_path = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem"
    all_data, labels = load_all_gesture_data_with_labels(main_folder_path)
    if not all_data:
        print("No gesture data loaded. Exiting...")
        exit()

    unique_labels = set(labels)
    for label in unique_labels:
        label_data = [all_data[i] for i, l in enumerate(labels) if l == label]
        # 训练对应标签的HMM模型
        model = train_single_hmm_model(label_data, n_states=20, max_iter=200)
        # 保存模型，文件名包含标签名
        model_file_name = f"{label}_hmm_model.pkl"
        joblib.dump(model, model_file_name)
        print(f"Model for {label} saved as {model_file_name}")
        
import json
import os
import numpy as np
import matplotlib.pyplot as plt
from hmmlearn import hmm
import joblib

# 处理Vector3和Quaternion数据
def process_vector3(vector3_list):
    return np.array([[v['x'], v['y'], v['z']] for v in vector3_list])

def process_quaternion(quaternion_list):
    return np.array([[q['x'], q['y'], q['z'], q['w']] for q in quaternion_list])

def load_gesture_data(file_path):
    """加载单个手势JSON文件"""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)

        palm_movement = np.array(data['Palm_Movement_List']).reshape(-1, 1)
        finger_extension = np.array(data['Finger_Extension_List']).reshape(-1, 1)
        movement_rate = np.array(data['Movement_Rate_List']).reshape(-1, 1)
        palm_movement_direction = process_vector3(data['Palm_Movement_Direction_List'])
        palm_rotation_quaternion = process_quaternion(data['Palm_Rotation_Quaternion_List'])

        # 按列堆叠数据，确保每一行是一个完整的特征向量
        features = np.hstack((
            palm_movement, finger_extension, movement_rate,
            palm_movement_direction, palm_rotation_quaternion
        ))

        return features
    except Exception as e:
        print(f"Error loading {file_path}: {str(e)}")
        return None

def load_dataset(main_folder):
    """加载整个数据集"""
    X, y = [], []
    for gesture_name in os.listdir(main_folder):
        gesture_dir = os.path.join(main_folder, gesture_name)
        if os.path.isdir(gesture_dir):
            for fname in os.listdir(gesture_dir):
                if fname.endswith('.json'):
                    features = load_gesture_data(os.path.join(gesture_dir, fname))
                    if features is not None:
                        X.append(features)
                        y.append(gesture_name)
    return X, y

def normalize_data(data_list):
    """数据标准化处理"""
    if not data_list:
        return []
    
    all_data = np.vstack(data_list)  # 合并所有数据用于计算均值和方差
    mean = np.mean(all_data, axis=0)
    std = np.std(all_data, axis=0)
    std[std == 0] = 1e-8  # 避免除零错误

    return [(seq - mean) / std for seq in data_list]

def train_hmm_with_validation(sequences, states_range, max_iter=200):
    """带验证的HMM训练"""
    sequences = normalize_data(sequences)
    if len(sequences) < 3:
        return None, None, None
    
    # 准备训练数据
    X = np.vstack(sequences)  # 合并所有样本
    lengths = [len(seq) for seq in sequences]
    
    bic_scores = []
    loglikes = []
    
    for n_states in states_range:
        try:
            model = hmm.GaussianHMM(n_components=n_states, n_iter=max_iter, random_state=42)
            model.fit(X, lengths=lengths)

            loglike = model.score(X, lengths=lengths)
            n_params = n_states**2 + 2*n_states - 1  # BIC 计算
            bic = -2 * loglike + n_params * np.log(len(sequences))

            loglikes.append(loglike)
            bic_scores.append(bic)
            print(f"States: {n_states:2d} | Log-Likelihood: {loglike:.2f} | BIC: {bic:.2f}")

        except Exception as e:
            print(f"Training failed for {n_states} states: {str(e)}")
            bic_scores.append(np.inf)
            loglikes.append(-np.inf)
    
    return bic_scores, loglikes, states_range

def visualize_results(gesture_name, states_range, bic_scores, loglikes):
    """可视化评估指标"""
    plt.figure(figsize=(12, 5))

    plt.subplot(1, 2, 1)
    plt.plot(states_range, bic_scores, 'bo-')
    plt.xlabel('Number of States')
    plt.ylabel('BIC Score')
    plt.title(f'BIC Score for {gesture_name}')
    plt.grid(True)

    plt.subplot(1, 2, 2)
    plt.plot(states_range, loglikes, 'go-')
    plt.xlabel('Number of States')
    plt.ylabel('Log-Likelihood')
    plt.title(f'Log-Likelihood for {gesture_name}')
    plt.grid(True)

    plt.tight_layout()
    plt.savefig(f"{gesture_name}_validation.png")
    plt.close()

def main():
    DATA_PATH = "D:/RuanJian/Unity/Unity/Projects/LeapMotion_Design/Assets/Dynamic_Gesture_Tem"
    STATES_RANGE = range(3, 16)  
    MAX_ITER = 300
    
    X, y = load_dataset(DATA_PATH)
    if not X:
        print("No data loaded!")
        return
    
    for gesture in set(y):
        print(f"\n{'='*40}\nProcessing gesture: {gesture}\n{'='*40}")
        
        gesture_data = [X[i] for i, label in enumerate(y) if label == gesture]
        if len(gesture_data) < 3:
            print(f"跳过 {gesture} (只有 {len(gesture_data)} 个样本)")
            continue
        
        bic_scores, loglikes, states_range = train_hmm_with_validation(gesture_data, STATES_RANGE, MAX_ITER)
        if bic_scores is None:
            continue
        
        visualize_results(gesture, states_range, bic_scores, loglikes)

        best_idx = np.argmin(bic_scores)
        best_states = STATES_RANGE[best_idx]
        print(f"\n最佳状态数: {best_states}")

        sequences = normalize_data(gesture_data)
        X_train = np.vstack(sequences)
        lengths = [len(seq) for seq in sequences]

        final_model = hmm.GaussianHMM(n_components=best_states, n_iter=MAX_ITER, random_state=42)
        final_model.fit(X_train, lengths=lengths)

        model_name = f"{gesture}_hmm_model.pkl"
        joblib.dump(final_model, model_name)
        print(f"已保存模型到 {model_name}")

if __name__ == "__main__":
    main()

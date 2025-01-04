import pandas as pd
from scipy.stats import skew, kurtosis

def compute_statistics(dataset):
    numeric_cols = dataset.select_dtypes(include=['float64', 'int64']).columns.tolist()
    statistics = {}
    for col in numeric_cols:
        max_val = dataset[col].max()
        min_val = dataset[col].min()
        mean_val = dataset[col].mean()
        median_val = dataset[col].median()
        var_val = dataset[col].var()
        skew_val = skew(dataset[col].dropna())  # 确保没有NaN值
        kurt_val = kurtosis(dataset[col].dropna())  # 确保没有NaN值

        statistics[col] = {
            '最大值': max_val,
            '最小值': min_val,
            '均值': mean_val,
            '中位数': median_val,
            '方差': var_val,
            '偏度': skew_val,
            '峰度': kurt_val
        }

    return statistics

if __name__ == "__main__":
    file_path = 'preprocessed_dataset.csv'
    dataset = pd.read_csv(file_path)
    stats = compute_statistics(dataset)
    for col, stat in stats.items():
        print(f"\n{col}列的统计数据：")
        for stat_name, value in stat.items():
            print(f"{stat_name}: {value}")

import pandas as pd

def preprocess(file_path):
    # 加载数据集
    dataset = pd.read_csv(file_path)

    # 数据缺失处理
    dataset['bmi'] = dataset['bmi'].fillna(dataset['bmi'].median())
    dataset['smoking_status'] = dataset['smoking_status'].fillna('unknown')

    # 数据异常处理
    numeric_cols = dataset.select_dtypes(include=['float64', 'int64']).columns.tolist()
    Q1_numeric = dataset[numeric_cols].quantile(0.25)
    Q3_numeric = dataset[numeric_cols].quantile(0.75)
    IQR_numeric = Q3_numeric - Q1_numeric
    outliers_mask = ((dataset[numeric_cols] < (Q1_numeric - 1.5 * IQR_numeric)) |
                     (dataset[numeric_cols] > (Q3_numeric + 1.5 * IQR_numeric)))
    dataset = dataset[~outliers_mask.any(axis=1)]

    # 添加打印语句来查看预处理后的数据
    print("数据集的前5行：")
    print(dataset.head())

    print("\n数据集的基本信息：")
    print(dataset.info())

    print("\n缺失值统计：")
    print(dataset.isnull().sum())

    # 如果你想查看特定列的处理结果
    print("\nbmi列的前5个值：")
    print(dataset['bmi'].head())

    print("\nsmoking_status列的值分布：")
    print(dataset['smoking_status'].value_counts())

    return dataset

if __name__ == "__main__":
    file_path = 'dataset.csv'
    preprocessed_dataset = preprocess(file_path)
    preprocessed_dataset.to_csv('preprocessed_dataset.csv', index=False)

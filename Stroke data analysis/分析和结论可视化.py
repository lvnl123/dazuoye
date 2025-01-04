import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.linear_model import LinearRegression
from sklearn.svm import SVR
from sklearn.cluster import KMeans
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestRegressor
from sklearn.metrics import mean_squared_error, accuracy_score, classification_report, silhouette_score
from sklearn.preprocessing import StandardScaler

# 加载数据集
dataset = pd.read_csv('preprocessed_dataset.csv')

# 设置支持中文的字体
plt.rcParams['font.sans-serif'] = ['SimHei']  # 使用黑体
plt.rcParams['axes.unicode_minus'] = False  # 正确显示负号

# 线性回归分析
# 选择特征和目标变量
features_lr = ['age', 'bmi']
target_lr = 'avg_glucose_level'
# 分割数据集
X_lr = dataset[features_lr]
y_lr = dataset[target_lr]
X_train_lr, X_test_lr, y_train_lr, y_test_lr = train_test_split(X_lr, y_lr, test_size=0.2, random_state=42)
# 训练线性回归模型
lr = LinearRegression()
lr.fit(X_train_lr, y_train_lr)
# 预测
y_pred_lr = lr.predict(X_test_lr)
mse_lr = mean_squared_error(y_test_lr, y_pred_lr)
print(f"线性回归均方误差: {mse_lr:.2f}")

# 将回归问题转换为分类问题并计算准确率和分类报告
def regression_to_classification(y_true, y_pred, threshold=0.5):
    y_true_class = np.where(y_true > threshold, 1, 0)
    y_pred_class = np.where(y_pred > threshold, 1, 0)
    return y_true_class, y_pred_class

y_true_lr_class, y_pred_lr_class = regression_to_classification(y_test_lr, y_pred_lr)
accuracy_lr = accuracy_score(y_true_lr_class, y_pred_lr_class)
report_lr = classification_report(y_true_lr_class, y_pred_lr_class)

print(f"线性回归准确率: {accuracy_lr:.2f}")
print("线性回归分类报告（仅供参考）:")
print(report_lr)
# 绘制线性回归拟合线图
plt.figure(figsize=(10, 5))
plt.scatter(X_test_lr[features_lr[0]], y_test_lr, color='blue', label='实际值')
plt.scatter(X_test_lr[features_lr[0]], y_pred_lr, color='red', label='预测值')
plt.title('线性回归拟合线')
plt.xlabel(features_lr[0])
plt.ylabel(target_lr)
plt.legend()
plt.show()

# 支持向量回归分析
scaler_svr = StandardScaler()
X_train_svr_scaled = scaler_svr.fit_transform(X_train_lr)
X_test_svr_scaled = scaler_svr.transform(X_test_lr)
svr = SVR(kernel='linear')
svr.fit(X_train_svr_scaled, y_train_lr)
y_pred_svr = svr.predict(X_test_svr_scaled)
mse_svr = mean_squared_error(y_test_lr, y_pred_svr)
print(f"支持向量回归均方误差: {mse_svr:.2f}")

y_true_svr_class, y_pred_svr_class = regression_to_classification(y_test_lr, y_pred_svr)
accuracy_svr = accuracy_score(y_true_svr_class, y_pred_svr_class)
report_svr = classification_report(y_true_svr_class, y_pred_svr_class)

print(f"支持向量回归准确率: {accuracy_svr:.2f}")
print("支持向量回归分类报告（仅供参考）:")
print(report_svr)
# 绘制SVR预测与实际值对比图
plt.figure(figsize=(10, 5))
plt.scatter(y_test_lr, y_pred_svr, color='green', label='SVR 预测值与实际值')
plt.plot([y_test_lr.min(), y_test_lr.max()], [y_test_lr.min(), y_test_lr.max()], 'k--', lw=2, label='理想线')
plt.title('SVR: 预测值与实际值')
plt.xlabel('实际血糖水平')
plt.ylabel('预测血糖水平')
plt.legend()
plt.show()

# 聚类分析
features_km = ['age', 'bmi', 'avg_glucose_level']
X_km = dataset[features_km]
scaler_km = StandardScaler()
X_km_scaled = scaler_km.fit_transform(X_km)
kmeans = KMeans(n_clusters=3, random_state=42)
clusters = kmeans.fit_predict(X_km_scaled)
print(f"聚类结果：")
for i in range(3):
    print(f"  类别 {i}: {np.sum(clusters == i)}个样本")

silhouette_avg = silhouette_score(X_km_scaled, clusters)
print(f"KMeans聚类轮廓系数: {silhouette_avg:.2f}")
# 绘制聚类结果的分布图
plt.figure(figsize=(10, 5))
plt.scatter(X_km_scaled[:, 0], X_km_scaled[:, 1], c=clusters, cmap='viridis', marker='o', label='数据点')
plt.scatter(kmeans.cluster_centers_[:, 0], kmeans.cluster_centers_[:, 1], s=300, c='red', label='质心', marker='*')
plt.title('聚类结果')
plt.xlabel('年龄（标准化）')
plt.ylabel('BMI（标准化）')
plt.legend()
plt.show()

# 随机森林回归分析
feature_columns = dataset.columns.drop(['id', 'avg_glucose_level'])
X = dataset[feature_columns]
y = dataset['avg_glucose_level']
X = pd.get_dummies(X)
X_train_rf, X_test_rf, y_train_rf, y_test_rf = train_test_split(X, y, test_size=0.2, random_state=42)
rf_regressor = RandomForestRegressor(n_estimators=100, random_state=42)
rf_regressor.fit(X_train_rf, y_train_rf)
y_pred_rf = rf_regressor.predict(X_test_rf)
mse_rf = mean_squared_error(y_test_rf, y_pred_rf)
print(f"随机森林回归均方误差: {mse_rf:.2f}")

y_true_rf_class, y_pred_rf_class = regression_to_classification(y_test_rf, y_pred_rf)
accuracy_rf = accuracy_score(y_true_rf_class, y_pred_rf_class)
report_rf = classification_report(y_true_rf_class, y_pred_rf_class)

print(f"随机森林回归准确率: {accuracy_rf:.2f}")
print("随机森林回归分类报告（仅供参考）:")
print(report_rf)
plt.figure(figsize=(10, 6))
plt.scatter(y_test_rf, y_pred_rf, color='blue', alpha=0.5)
plt.plot([y.min(), y.max()], [y.min(), y.max()], color='red', lw=2)
plt.xlabel('实际血糖水平')
plt.ylabel('预测血糖水平')
plt.title('实际值与预测值对比（随机森林）')
plt.show()


bins = [0, 10, 20, 30, 40, 50, 60, float('inf')]
labels = ['0-10岁', '11-20岁', '21-30岁', '31-40岁', '41-50岁', '51-60岁', '60岁以上']
# 使用pandas的cut函数将年龄分组
age_bins = pd.cut(dataset['age'], bins=bins, labels=labels, right=False)
age_group_counts = age_bins.value_counts().reindex(labels)
# 创建图表
fig, axs = plt.subplots(2, 2, figsize=(15, 10))

# 1. 年龄分布的饼图
wedges, texts, autotexts = axs[0, 0].pie(age_group_counts, autopct=lambda p: '{:.1f}%'.format(p) if p > 0 else '', startangle=140, labels=labels, wedgeprops=dict(width=1.0))
# 添加具体人数到标签
labels_with_counts = [f"{label} ({count})" for label, count in zip(age_group_counts.index, age_group_counts)]
# 更新标签
for label, text in zip(labels_with_counts, texts):
    text.set_text(label)
axs[0, 0].set_title('年龄分布饼图')
axs[0, 0].set_ylabel('')  # 隐藏y轴标签

# 2. BMI分布的直方图
axs[0, 1].hist(dataset['bmi'], bins=30, color='green', label='BMI分布')
axs[0, 1].set_title('BMI分布直方图')
axs[0, 1].set_xlabel('BMI')
axs[0, 1].set_ylabel('频数')
axs[0, 1].legend()

# 3. 血糖水平（avg_glucose_level）的直方图
axs[1, 0].hist(dataset['avg_glucose_level'], bins=30, color='red', label='血糖水平分布')
axs[1, 0].set_title('血糖水平分布直方图')
axs[1, 0].set_xlabel('血糖水平')
axs[1, 0].set_ylabel('频数')
axs[1, 0].legend()

# 4. 年龄与BMI的散点图
axs[1, 1].scatter(dataset['age'], dataset['bmi'], color='purple', alpha=0.5)
axs[1, 1].set_title('年龄与BMI的散点图')
axs[1, 1].set_xlabel('年龄')
axs[1, 1].set_ylabel('BMI')

# 调整布局并显示图表
plt.tight_layout()

# 显示图表
plt.show()
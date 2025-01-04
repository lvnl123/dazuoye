import sys
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.linear_model import LinearRegression
from sklearn.svm import SVR
from sklearn.cluster import KMeans
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, silhouette_score
from sklearn.preprocessing import StandardScaler
from PyQt5.QtWidgets import QApplication, QMainWindow, QPushButton, QVBoxLayout, QWidget, QGraphicsScene, QGraphicsView
from PyQt5.QtGui import QPixmap
from matplotlib.backends.backend_qt5agg import FigureCanvasQTAgg as FigureCanvas

# 你的数据集路径（请根据实际情况修改）
dataset = pd.read_csv('preprocessed_dataset.csv')


# 可视化函数：线性回归图
def generate_linear_regression_plot():
    features_lr = ['age', 'bmi']
    target_lr = 'avg_glucose_level'
    X_lr = dataset[features_lr]
    y_lr = dataset[target_lr]
    X_train_lr, X_test_lr, y_train_lr, y_test_lr = train_test_split(X_lr, y_lr, test_size=0.2, random_state=42)

    lr = LinearRegression()
    lr.fit(X_train_lr, y_train_lr)
    y_pred_lr = lr.predict(X_test_lr)

    fig, ax = plt.subplots(figsize=(10, 5))
    ax.scatter(X_test_lr[features_lr[0]], y_test_lr, color='blue', label='实际值')
    ax.scatter(X_test_lr[features_lr[0]], y_pred_lr, color='red', label='预测值')
    ax.set_title('线性回归拟合线')
    ax.set_xlabel(features_lr[0])
    ax.set_ylabel(target_lr)
    ax.legend()

    return fig


# 可视化函数：支持向量回归（SVR）图
def generate_svr_plot():
    features_lr = ['age', 'bmi']
    target_lr = 'avg_glucose_level'
    X_lr = dataset[features_lr]
    y_lr = dataset[target_lr]
    X_train_lr, X_test_lr, y_train_lr, y_test_lr = train_test_split(X_lr, y_lr, test_size=0.2, random_state=42)

    scaler_svr = StandardScaler()
    X_train_svr_scaled = scaler_svr.fit_transform(X_train_lr)
    X_test_svr_scaled = scaler_svr.transform(X_test_lr)

    svr = SVR(kernel='linear')
    svr.fit(X_train_svr_scaled, y_train_lr)
    y_pred_svr = svr.predict(X_test_svr_scaled)

    fig, ax = plt.subplots(figsize=(10, 5))
    ax.scatter(y_test_lr, y_pred_svr, color='green', label='SVR 预测值与实际值')
    ax.plot([y_test_lr.min(), y_test_lr.max()], [y_test_lr.min(), y_test_lr.max()], 'k--', lw=2, label='理想线')
    ax.set_title('SVR: 预测值与实际值')
    ax.set_xlabel('实际血糖水平')
    ax.set_ylabel('预测血糖水平')
    ax.legend()

    return fig


# 可视化函数：KMeans聚类图
def generate_kmeans_plot():
    features_km = ['age', 'bmi', 'avg_glucose_level']
    X_km = dataset[features_km]
    scaler_km = StandardScaler()
    X_km_scaled = scaler_km.fit_transform(X_km)

    kmeans = KMeans(n_clusters=3, random_state=42)
    clusters = kmeans.fit_predict(X_km_scaled)

    fig, ax = plt.subplots(figsize=(10, 5))
    scatter = ax.scatter(X_km_scaled[:, 0], X_km_scaled[:, 1], c=clusters, cmap='viridis', marker='o', label='数据点')
    ax.scatter(kmeans.cluster_centers_[:, 0], kmeans.cluster_centers_[:, 1], s=300, c='red', label='质心', marker='*')
    ax.set_title('KMeans聚类结果')
    ax.set_xlabel('年龄（标准化）')
    ax.set_ylabel('BMI（标准化）')
    ax.legend()

    return fig


# 可视化函数：随机森林回归图
def generate_rf_plot():
    feature_columns = dataset.columns.drop(['id', 'avg_glucose_level'])
    X = dataset[feature_columns]
    y = dataset['avg_glucose_level']
    X = pd.get_dummies(X)
    X_train_rf, X_test_rf, y_train_rf, y_test_rf = train_test_split(X, y, test_size=0.2, random_state=42)

    from sklearn.ensemble import RandomForestRegressor
    rf_regressor = RandomForestRegressor(n_estimators=100, random_state=42)
    rf_regressor.fit(X_train_rf, y_train_rf)
    y_pred_rf = rf_regressor.predict(X_test_rf)

    fig, ax = plt.subplots(figsize=(10, 6))
    ax.scatter(y_test_rf, y_pred_rf, color='blue', alpha=0.5)
    ax.plot([y.min(), y.max()], [y.min(), y.max()], color='red', lw=2)
    ax.set_xlabel('实际血糖水平')
    ax.set_ylabel('预测血糖水平')
    ax.set_title('实际值与预测值对比（随机森林）')

    return fig


# PyQt5 GUI类
class VisualizationWindow(QMainWindow):
    def __init__(self):
        super().__init__()

        self.setWindowTitle("数据分析与可视化")
        self.setGeometry(100, 100, 800, 600)

        # 主布局
        layout = QVBoxLayout()

        # 用于显示图像的标签
        self.canvas_area = QWidget(self)
        layout.addWidget(self.canvas_area)

        # 添加按钮
        self.linear_regression_button = QPushButton('显示线性回归图', self)
        self.linear_regression_button.clicked.connect(self.show_linear_regression_plot)
        layout.addWidget(self.linear_regression_button)

        self.svr_button = QPushButton('显示SVR图', self)
        self.svr_button.clicked.connect(self.show_svr_plot)
        layout.addWidget(self.svr_button)

        self.kmeans_button = QPushButton('显示KMeans聚类图', self)
        self.kmeans_button.clicked.connect(self.show_kmeans_plot)
        layout.addWidget(self.kmeans_button)

        self.rf_button = QPushButton('显示随机森林回归图', self)
        self.rf_button.clicked.connect(self.show_rf_plot)
        layout.addWidget(self.rf_button)

        # 主窗口中心部件
        container = QWidget()
        container.setLayout(layout)
        self.setCentralWidget(container)

    def show_linear_regression_plot(self):
        fig = generate_linear_regression_plot()
        self.show_plot(fig)

    def show_svr_plot(self):
        fig = generate_svr_plot()
        self.show_plot(fig)

    def show_kmeans_plot(self):
        fig = generate_kmeans_plot()
        self.show_plot(fig)

    def show_rf_plot(self):
        fig = generate_rf_plot()
        self.show_plot(fig)

    def show_plot(self, fig):
        # 清空原有内容
        for i in reversed(range(self.canvas_area.layout().count())):
            widget = self.canvas_area.layout().itemAt(i).widget()
            if widget is not None:
                widget.deleteLater()

        # 将图像嵌入到窗口中
        canvas = FigureCanvas(fig)
        self.canvas_area.layout().addWidget(canvas)


# 主程序
if __name__ == '__main__':
    app = QApplication(sys.argv)
    window = VisualizationWindow()
    window.show()
    sys.exit(app.exec_())

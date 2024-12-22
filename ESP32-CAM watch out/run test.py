import cv2
import numpy as np
import requests
from io import BytesIO
import paho.mqtt.client as mqtt
import json
import tkinter as tk
from tkinter import messagebox

# MQTT设置
mqtt_broker = '192.168.172.62'  # 替换为你的MQTT服务器地址
mqtt_port = 1883  # MQTT服务器端口
mqtt_topic = 'object/detection'  # 发布消息的主题
mqtt_alert_count_topic = 'object/alert_count'  # 弹窗次数主题

# 创建MQTT客户端
client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2)

# 连接到MQTT服务器
client.connect(mqtt_broker, mqtt_port)

# Load YOLO model
net = cv2.dnn.readNet("yolov3.weights", "yolov3.cfg")

# Load class names
with open("coco.names", "r") as f:
    classes = f.read().strip().split('\n')

# 检查类列表中是否只包含"person"
if len(classes) != 1 or classes[0] != "person":
    raise ValueError("coco.names文件应仅包含一个类名: 'person'")

# Set confidence threshold and non-maximum suppression threshold
conf_threshold = 0.5
nms_threshold = 0.4

# Create color for the 'person' class
color = (0, 255, 0)  # 使用绿色作为检测框的颜色

# 创建一个弹窗函数
def alert_window():
    root = tk.Tk()
    root.withdraw()  # 隐藏主窗口
    messagebox.showinfo("警告", "三次检测到人，请离开")
    root.destroy()  # 关闭弹窗

# 初始化连续检测到人的次数
person_count = 0
alert_count = 0  # 弹窗次数计数器

while True:
    # Fetch the live image from the ESP32-CAM feed URL
    url = "http://192.168.172.71/"
    response = requests.get(url)
    if response.status_code == 200:
        image_bytes = BytesIO(response.content)
        image = None
        try:
            # 尝试读取并解码图像
            image = cv2.imdecode(np.frombuffer(image_bytes.getvalue(), np.uint8), cv2.IMREAD_COLOR)
            if image is None:
                raise ValueError("Failed to decode image.")
        except Exception as e:
            print(f"Error reading image: {e}")
            continue  # 跳过当前循环迭代

        blob = cv2.dnn.blobFromImage(image, 1 / 255.0, (320,320), swapRB=True, crop=False)
        net.setInput(blob)
        layer_names = net.getUnconnectedOutLayersNames()
        outs = net.forward(layer_names)

        boxes = []
        confidences = []
        person_detected = False

        for out in outs:
            for detection in out:
                scores = detection[5:]
                class_id = np.argmax(scores)
                confidence = scores[class_id]

                # 仅处理person类（类ID为0）
                if class_id == 0 and confidence > conf_threshold:
                    person_detected = True
                    center_x = int(detection[0] * image.shape[1])
                    center_y = int(detection[1] * image.shape[0])
                    w = int(detection[2] * image.shape[1])
                    h = int(detection[3] * image.shape[0])

                    x = int(center_x - w / 2)
                    y = int(center_y - h / 2)

                    boxes.append([x, y, w, h])
                    confidences.append(float(confidence))

        indexes = cv2.dnn.NMSBoxes(boxes, confidences, conf_threshold, nms_threshold)

        if len(indexes) > 0:
            for i in indexes.flatten():
                x, y, w, h = boxes[i]
                confidence = confidences[i]

                # 绘制检测框
                cv2.rectangle(image, (x, y), (x + w, y + h), color, 2)
                text = f"person ({confidence:.2f})"
                cv2.putText(image, text, (x, y - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)

                # 打印检测到的对象信息
                print(f"识别结果: person, 准确率: {confidence:.2f}, (x: {x}, y: {y}, w: {w}, h: {h})")

                # 创建JSON格式的检测结果
                detection_data = {
                    "label": "person",
                    "confidence": confidence,
                    "x": x,
                    "y": y,
                    "w": w,
                    "h": h
                }
                detection_json = json.dumps(detection_data)

                # 发布JSON格式的检测结果到MQTT
                client.publish(mqtt_topic, detection_json)

        # 如果检测到人，则增加计数
        if person_detected:
            person_count += 1
        else:
            # 如果没有检测到人，则重置计数
            person_count = 0

        # 如果连续三次检测到人，则弹出窗口并重置计数
        if person_count >= 3:
            alert_window()
            alert_count += 1  # 增加弹窗次数计数器
            person_count = 0  # 重置连续检测到人的次数

            # 创建JSON格式的弹窗次数数据
            alert_data = {
                "alert_count": alert_count
            }
            alert_json = json.dumps(alert_data)

            # 发布JSON格式的弹窗次数数据到MQTT的独立主题
            client.publish(mqtt_alert_count_topic, alert_json)

        cv2.imshow("Object Detection", image)

        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

# 断开MQTT连接
client.disconnect()

cv2.destroyAllWindows()

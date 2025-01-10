import cv2
from ultralytics import YOLO
import mouse
from screeninfo import get_monitors
import time

# 获取屏幕的宽度和高度
monitor = get_monitors()[0]
screen_width, screen_height = monitor.width, monitor.height

# 加载YOLO模型进行手部关键点检测
keypoint_model = YOLO('../runs/pose/train/weights/best.pt')
# 加载YOLO模型进行手势分类
gesture_model = YOLO('../YOLOv10x_gestures.pt')

# 打开摄像头（0 是默认摄像头）
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("错误：无法打开摄像头。")
    exit()

mouse_pressed = False  # 用于标记鼠标是否按下
start_pos = None  # 记录鼠标按下时的初始位置

last_double_click_time = 0
double_click_delay = 1  # 双击的时间间隔

while True:
    ret, frame = cap.read()

    if not ret:
        print("错误：无法捕获帧")
        break

    frame = cv2.flip(frame, 1)  # 翻转图像，模拟镜像效果
    frame = cv2.convertScaleAbs(frame, alpha=0.5, beta=0)  # 调整图像亮度和对比度
    gesture_class = None

    # 对当前帧进行手势分类
    gesture_results = gesture_model(frame)

    if len(gesture_results) > 0 and gesture_results[0].boxes is not None:
        boxes = gesture_results[0].boxes
        if len(boxes) > 0:
            gesture_class_idx = int(boxes[0].cls[0])
            gesture_class = gesture_results[0].names[gesture_class_idx]
            print(f"检测到手势: {gesture_class}")
            cv2.putText(frame, f'Gesture: {gesture_class}', (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
    else:
        gesture_class = None

    # 对当前帧进行手部关键点检测
    keypoint_results = keypoint_model(frame)

    if len(keypoint_results) > 0:
        frame_with_results = keypoint_results[0].plot()
        frame_with_results = cv2.add(frame, frame_with_results)

        if gesture_class == 'palm':
            keypoints = keypoint_results[0].keypoints[0]
            print("keypoints.xy 的形状:", keypoints.xy.shape)
            print("keypoints.xy 的内容:", keypoints.xy)
            for i, point in enumerate(keypoints.xy[0]):
                if i == 13:  # 手腕位置
                    mapped_x = int(point[0] / frame.shape[1] * screen_width)
                    mapped_y = int(point[1] / frame.shape[0] * screen_height)
                    mouse.move(mapped_x, mapped_y)
            print("检测到手势: palm，执行移动操作")

        if gesture_class == 'fist':
            if not mouse_pressed:
                mouse_pressed = True
                keypoints = keypoint_results[0].keypoints[0]
                print("keypoints.xy 的形状:", keypoints.xy.shape)
                print("keypoints.xy 的内容:", keypoints.xy)
                for i, point in enumerate(keypoints.xy[0]):
                    if i == 13:  # 使用中指根部作为拖动起点
                        mapped_x = int(point[0] / frame.shape[1] * screen_width)
                        mapped_y = int(point[1] / frame.shape[0] * screen_height)
                        mouse.press()
                        start_pos = (mapped_x, mapped_y)
                        break
            print("检测到手势: fist，执行拖动操作")
        elif gesture_class != 'fist' and mouse_pressed:
            mouse.release()
            mouse_pressed = False
            start_pos = None

        if mouse_pressed and start_pos:
            keypoints = keypoint_results[0].keypoints[0]
            print("keypoints.xy 的形状:", keypoints.xy.shape)
            print("keypoints.xy 的内容:", keypoints.xy)
            for i, point in enumerate(keypoints.xy[0]):
                if i == 13:  # 中指根部继续作为参考
                    mapped_x = int(point[0] / frame.shape[1] * screen_width)
                    mapped_y = int(point[1] / frame.shape[0] * screen_height)
                    mouse.move(mapped_x, mapped_y)

        if gesture_class == 'ok':
            current_time = time.time()
            if current_time - last_double_click_time >= double_click_delay:
                mouse.double_click()
                last_double_click_time = current_time
            print("检测到手势: ok，执行双击操作")

        if gesture_class == 'peace':
            mouse.right_click()
            print("检测到手势: peace，执行右键点击操作")

    cv2.imshow('摄像头', frame_with_results)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()

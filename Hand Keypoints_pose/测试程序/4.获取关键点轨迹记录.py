import cv2
from ultralytics import YOLO

# 加载YOLO模型进行手部关键点检测
keypoint_model = YOLO('../runs/pose/train/weights/best.pt')

# 加载YOLO模型进行手势分类（请替换为正确的路径）
gesture_model = YOLO('../YOLOv10x_gestures.pt')

# 打开摄像头（0 是默认摄像头）
cap = cv2.VideoCapture(0)

if not cap.isOpened():
    print("错误：无法打开摄像头。")
    exit()

# 初始化关键点8的轨迹列表
trajectory = []

while True:
    # 按帧捕获
    ret, frame = cap.read()

    if not ret:
        print("错误：无法捕获帧")
        break
    frame = cv2.convertScaleAbs(frame, alpha=0.5, beta=0)

    # 对当前帧进行手部关键点检测
    keypoint_results = keypoint_model(frame)

    # 确保关键点检测结果有效
    if len(keypoint_results) == 0:
        print("错误：没有从模型中获取到关键点结果。")
        continue

    # 获取关键点数据（关键点的坐标）
    keypoints = keypoint_results[0].keypoints[0]  # 获取关键点数据对象
    # 打印出keypoints.xy的形状和内容以供调试
    print("keypoints.xy 的形状:", keypoints.xy.shape)
    print("keypoints.xy 的内容:", keypoints.xy)

    # 打印出每个关键点的坐标
    print("关键点位置：")
    for i, point in enumerate(keypoints.xy[0]):  # 访问keypoints.xy[0]数组
        x, y = point[0], point[1]
        print(f"关键点 {i}: (x: {x:.2f}, y: {y:.2f})")
        # 如果当前是关键点13，则保存其坐标
        if i == 13:
            trajectory.append((int(x), int(y)))

    # 绘制关键点8的轨迹
    for point in trajectory:
        cv2.circle(frame, point, 2, (255, 0, 0), -1)

    # 对当前帧进行手势分类
    gesture_results = gesture_model(frame)

    # 确保手势分类结果有效
    if len(gesture_results) == 0:
        print("错误：没有从分类模型中获取到手势结果。")
        continue

    # 初始化渲染后的帧，包含关键点结果
    frame_with_results = keypoint_results[0].plot()  # 渲染关键点

    # 检查是否包含手势分类框
    if gesture_results[0].boxes is not None:
        boxes = gesture_results[0].boxes
        # 检查是否检测到至少一个框
        if len(boxes) > 0:
            # 获取第一个检测框的类别ID
            gesture_class_idx = int(boxes[0].cls[0])  # 获取第一个检测框的类别索引
            gesture_class = gesture_results[0].names[gesture_class_idx]  # 获取类别名称

            # 打印手势分类标签到控制台
            print(f"检测到手势: {gesture_class}")

            # 在图像的左上角显示手势分类标签
            cv2.putText(frame_with_results, f'Gesture: {gesture_class}', (10, 30),
                        cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)  # 显示手势标签

    # 将关键点绘制到原始帧上
    frame_with_results = cv2.add(frame, frame_with_results)

    # 显示包含关键点和手势分类文本的帧
    cv2.imshow('摄像头', frame_with_results)

    # 检查是否按下 'q' 键退出
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# 释放摄像头并关闭所有OpenCV窗口
cap.release()
cv2.destroyAllWindows()

import cv2  # 导入OpenCV库，用于处理图像和视频流
from ultralytics import YOLO  # 导入YOLO模型库，用于目标检测和图像分析
import mouse  # 导入鼠标控制库，用于模拟鼠标操作（如点击、移动等）
from screeninfo import get_monitors  # 导入screeninfo库，用于获取屏幕分辨率和显示器信息
import time  # 导入time模块，用于控制时间延迟

# 获取屏幕的宽度和高度
monitor = get_monitors()[0]  # 默认获取第一个显示器的信息（如果有多个显示器，这里选择第一个）
screen_width, screen_height = monitor.width, monitor.height  # 获取屏幕的宽度和高度
# 需要将手部关键点的坐标映射到屏幕坐标，知道屏幕的尺寸后才能进行映射

# 加载YOLO模型进行手部关键点检测
keypoint_model = YOLO('../runs/pose/train/weights/best.pt')  # 加载已经训练好的YOLO模型来进行手部关键点检测
# 加载YOLO模型进行手势分类（请替换为正确的路径）
gesture_model = YOLO('../YOLOv10x_gestures.pt')  # 加载另一个YOLO模型，用于手势分类。该模型用于识别不同的手势类型

#--------------------------------------------------------------#
# 打开摄像头（0 是默认摄像头）
cap = cv2.VideoCapture(0)  # 创建一个视频捕获对象，0表示使用默认摄像头。这个对象会读取摄像头的视频流

if not cap.isOpened():  # 检查摄像头是否成功打开
    print("错误：无法打开摄像头。")  # 如果摄像头打开失败，打印错误信息
    exit()  # 如果无法打开摄像头，退出程序

# 定义一个变量来跟踪鼠标是否处于按下状态
mouse_pressed = False  # 用一个布尔值来标记鼠标是否被按下，初始状态为未按下
start_pos = None  # 用来记录鼠标按下时的初始位置，初始化为None

# 记录上一次双击的时间
last_double_click_time = 0
double_click_delay = 1  # 双击之间的最小时间间隔（单位：秒）

while True:  # 无限循环，持续处理每一帧图像
    # 按帧捕获
    ret, frame = cap.read()  # 从摄像头捕获一帧图像，ret为True时表示成功捕获，frame为捕获到的图像

    if not ret:  # 如果没有成功捕获到图像（ret为False）
        print("错误：无法捕获帧")  # 打印错误信息
        break  # 退出循环，结束程序

    frame = cv2.flip(frame, 1)  # 将捕获的图像进行水平翻转，模拟镜像效果（便于与屏幕上的动作对应）
    frame = cv2.convertScaleAbs(frame, alpha=0.5, beta=0)  # 对图像进行简单的亮度和对比度调整，使其更适合处理
    gesture_class = None  # 每次循环开始时，初始化gesture_class为None，表示当前没有检测到手势

#--------------------------------------------------------------#
    # 对当前帧进行手势分类
    gesture_results = gesture_model(frame)  # 使用训练好的手势分类模型处理当前帧图像，输出分类结果

    # 确保手势分类结果有效
    if len(gesture_results) > 0 and gesture_results[0].boxes is not None:  # 检查手势分类是否有有效结果
        boxes = gesture_results[0].boxes  # 获取检测到的目标框
        if len(boxes) > 0:  # 如果至少检测到一个框
            gesture_class_idx = int(boxes[0].cls[0])  # 获取第一个框的类别索引（整数）
            gesture_class = gesture_results[0].names[gesture_class_idx]  # 根据类别索引获得类别名称（手势类型）
            print(f"检测到手势: {gesture_class}")  # 打印检测到的手势类别
            # 在图像左上角显示手势类别的标签 # 字体设置为绿色，大小为1，粗细为2
            cv2.putText(frame, f'Gesture: {gesture_class}', (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
    else:
        gesture_class = None  # 如果没有检测到手势，设置gesture_class为None

    # 对当前帧进行手部关键点检测
    keypoint_results = keypoint_model(frame)  # 使用训练好的YOLO模型进行手部关键点检测

    # 确保关键点检测结果有效
    if len(keypoint_results) > 0:
        # 渲染并绘制检测到的关键点
        frame_with_results = keypoint_results[0].plot()  # 该方法会返回一个绘制了关键点的图像

        # 将关键点绘制到原始帧上
        frame_with_results = cv2.add(frame, frame_with_results)  # 使用cv2.add将检测到的关键点叠加到原始图像上

        # 当检测到“palm”手势时，模拟鼠标移动
        if gesture_class == 'palm':  # 如果检测到手势为“palm”
            keypoints = keypoint_results[0].keypoints[0]  # 获取第一只手的关键点数据
            print("keypoints.xy 的形状:", keypoints.xy.shape)  # 打印关键点数据的形状（方便调试）
            print("keypoints.xy 的内容:", keypoints.xy)  # 打印关键点的坐标

            # 遍历所有关键点，找到对应的关键点（手腕位置为13）
            for i, point in enumerate(keypoints.xy[0]):
                if i == 13:  # 手腕位置对应关键点13
                    mapped_x = int(point[0] / frame.shape[1] * screen_width)  # 将关键点x坐标映射到屏幕宽度
                    mapped_y = int(point[1] / frame.shape[0] * screen_height)  # 将关键点y坐标映射到屏幕高度
                    mouse.move(mapped_x, mapped_y)  # 移动鼠标到计算出的坐标位置
            print("检测到手势: palm，执行移动操作")  # 打印调试信息

        # 当检测到“fist”手势时，模拟鼠标按下并拖动
        if gesture_class == 'fist':  # 如果检测到手势为“fist” (拳头)
            if not mouse_pressed:  # 如果鼠标当前没有按下
                mouse_pressed = True  # 标记为鼠标已按下
                keypoints = keypoint_results[0].keypoints[0]  # 获取关键点数据
                print("keypoints.xy 的形状:", keypoints.xy.shape)  # 打印关键点数据的形状（方便调试）
                print("keypoints.xy 的内容:", keypoints.xy)  # 打印关键点的坐标
                for i, point in enumerate(keypoints.xy[0]):
                    if i == 13:  # 使用中指根部位置作为拖动起点
                        mapped_x = int(point[0] / frame.shape[1] * screen_width)  # 映射到屏幕坐标
                        mapped_y = int(point[1] / frame.shape[0] * screen_height)  # 映射到屏幕坐标
                        mouse.press()  # 模拟鼠标按下
                        start_pos = (mapped_x, mapped_y)  # 记录鼠标按下的起始位置
                        break
            print("检测到手势: fist，执行拖动操作")  # 打印调试信息
        elif gesture_class != 'fist' and mouse_pressed:  # 如果手势不是“fist”，但鼠标已按下
            mouse.release()  # 模拟鼠标释放
            mouse_pressed = False  # 重置鼠标按下状态
            start_pos = None  # 清除起始位置

        # 如果鼠标已经按下，开始拖动
        if mouse_pressed and start_pos:
            keypoints = keypoint_results[0].keypoints[0]  # 获取关键点数据
            print("keypoints.xy 的形状:", keypoints.xy.shape)  # 打印关键点数据的形状（方便调试）
            print("keypoints.xy 的内容:", keypoints.xy)  # 打印关键点的坐标
            for i, point in enumerate(keypoints.xy[0]):
                if i == 13:  # 继续使用中指根部作为鼠标拖动的参考
                    mapped_x = int(point[0] / frame.shape[1] * screen_width)  # 映射到屏幕坐标
                    mapped_y = int(point[1] / frame.shape[0] * screen_height)
                    mouse.move(mapped_x, mapped_y)  # 移动鼠标到新的坐标

        # 如果检测到"ok"手势，模拟鼠标双击
        if gesture_class == 'ok':
            current_time = time.time()  # 获取当前时间
            if current_time - last_double_click_time >= double_click_delay:  # 确保双击间隔大于设定的时间
                mouse.double_click()  # 执行双击操作
                last_double_click_time = current_time  # 更新双击时间
            print("检测到手势: ok，执行双击操作")

        # 如果检测到"peace"手势，模拟右键点击
        if gesture_class == 'peace':
            mouse.right_click()  # 执行右键点击操作
            print("检测到手势: peace，执行右键点击操作")

    # 显示图像窗口
    cv2.imshow('摄像头', frame_with_results)  # 在窗口中显示当前帧图像（已标注关键点）

    # 按下“q”退出程序
    if cv2.waitKey(1) & 0xFF == ord('q'):  # 如果按下q键
        break  # 退出循环

# 释放摄像头资源
cap.release()  # 释放摄像头资源
cv2.destroyAllWindows()  # 关闭所有OpenCV创建的窗口

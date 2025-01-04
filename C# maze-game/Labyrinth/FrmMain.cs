using Emgu.CV.CvEnum;  // 引用 EmguCV 库中的枚举类型，用于图像处理
using Labyrinth.Common; // 引用 Labyrinth 项目的公共类库
using System.Diagnostics; // 引用用于调试的命名空间
using System.Numerics; // 引用处理大数和复数的命名空间
using System.Windows.Forms; // 引用窗体和控件的命名空间
using NAudio.Wave; // 引用 NAudio 库，用于音频处理
using System.IO; // 引用文件操作的命名空间

namespace Labyrinth // 定义 Labyrinth 命名空间
{
    public partial class FrmMain : Form // FrmMain 类继承自 Form 类，是游戏主窗体
    {
        // 成员变量定义部分
        private readonly Maze _maze; // 存储迷宫实例
        private System.Windows.Forms.Timer _timer; // 存储定时器，用于计时
        private bool _hasMoved = false; // 标记是否已按过方向键
        private bool _isPlayGame = false; // 标记游戏是否进行中
        private bool _isAutoMove = false; // 标记是否正在自动移动
        private long time = 0; // 游戏的计时器变量
        private bool isMuted = false;  // 标记是否静音
        private WaveOutEvent waveOut;  // 存储音频播放器实例
        private AudioFileReader audioFileReader;  // 存储音频文件读取器实例
        private Point _playerPosition = new Point(0, 0); // 存储玩家位置
        private string selectedDifficulty = "未知难度"; // 当前选择的游戏难度
        private readonly List<GameRecord> gameRecords = new List<GameRecord>(); // 存储游戏通关记录

        // 构造函数
        public FrmMain()
        {
            InitializeComponent();  // 初始化窗体控件
            this.SetStyle(ControlStyles.DoubleBuffer |  // 设置双缓冲以减少屏幕闪烁
                ControlStyles.UserPaint |  // 自定义绘制
                ControlStyles.AllPaintingInWmPaint,  // 启用所有绘制功能
                true);
            this.UpdateStyles();  // 更新控件的样式

            _maze = new Maze();  // 创建迷宫实例
            InitializeAudioPlayer();  // 初始化音频播放器
            this.btnMuteMusic.Click += new System.EventHandler(this.btnMuteMusic_Click);  // 绑定静音按钮点击事件
        }

        // 音频文件查找方法
        private string FindAudioFile()
        {
            string startupPath = Application.StartupPath; // 获取应用程序启动路径
            string projectRootPath = startupPath; // 初始路径为启动路径
            while (!Directory.Exists(Path.Combine(projectRootPath, "music"))) // 循环直到找到 "music" 文件夹
            {
                projectRootPath = Directory.GetParent(projectRootPath).FullName; // 向上查找父级目录
                if (string.IsNullOrEmpty(projectRootPath)) // 如果找不到父级目录
                {
                    MessageBox.Show("未找到music文件夹."); // 提示未找到音乐文件夹
                    return null; // 返回 null，表示未找到音频文件夹
                }
            }

            string musicFolderPath = Path.Combine(projectRootPath, "music"); // 获取 music 文件夹路径
            string[] audioExtensions = { ".wav", ".mp3", ".ogg", ".flac" }; // 支持的音频文件扩展名

            foreach (string extension in audioExtensions) // 遍历支持的音频格式
            {
                string[] audioFiles = Directory.GetFiles(musicFolderPath, $"*{extension}", SearchOption.TopDirectoryOnly); // 查找该扩展名的音频文件
                if (audioFiles.Length > 0) // 如果找到音频文件
                {
                    return audioFiles[0]; // 返回第一个找到的音频文件路径
                }
            }

            return null; // 如果没有找到音频文件，返回 null
        }

        // 初始化音频播放器
        private void InitializeAudioPlayer()
        {
            if (waveOut == null) // 如果音频播放器未初始化
            {
                waveOut = new WaveOutEvent(); // 创建 WaveOutEvent 音频播放器
                Debug.WriteLine("waveOut initialized"); // 输出调试信息
            }

            string audioFilePath = FindAudioFile(); // 查找音频文件

            if (audioFilePath != null && File.Exists(audioFilePath)) // 如果音频文件存在
            {
                audioFileReader = new AudioFileReader(audioFilePath); // 创建 AudioFileReader 来读取音频文件
                waveOut.Init(audioFileReader); // 初始化播放器
                waveOut.Play(); // 开始播放音频
                waveOut.Volume = 0.5f; // 设置音量为 50%

                waveOut.PlaybackStopped += (s, e) =>  // 音频播放完后重新开始播放
                {
                    audioFileReader.Position = 0;  // 将音频播放位置回到起点
                    waveOut.Play();  // 继续播放
                };
            }
            else
            {
                MessageBox.Show("未找到音频文件！"); // 如果没有找到音频文件，弹出提示
            }
        }

        // 静音按钮点击事件
        private void btnMuteMusic_Click(object sender, EventArgs e)
        {
            if (isMuted) // 如果当前已经静音
            {
                waveOut.Volume = 0.5f; // 恢复音量为 50%
                waveOut.Play(); // 继续播放
                isMuted = false; // 标记为未静音
                btnMuteMusic.Text = "Mute"; // 修改按钮文本为 "Mute"
                Debug.WriteLine("Music unmuted"); // 输出调试信息
            }
            else
            {
                waveOut.Volume = 0f; // 设置音量为 0
                waveOut.Pause(); // 暂停播放
                isMuted = true; // 标记为静音
                btnMuteMusic.Text = "Unmute"; // 修改按钮文本为 "Unmute"
                Debug.WriteLine("Music muted"); // 输出调试信息
            }
        }

        // 游戏重置按钮点击事件
        private void btnReset_Click(object sender, EventArgs e)
        {
            _playerPosition = new Point(0, 0); // 重置玩家位置到起点
            var result = _maze.Move(_playerPosition); // 调用迷宫的 Move 方法处理玩家移动
            RefreshResult(result); // 刷新游戏状态
            plGame.Invalidate(); // 强制重新绘制游戏面板
            time = 0; // 重置计时器
            lblTime.BeginInvoke(() => lblTime.Text = Compute(time)); // 更新时间显示
            plGame.Focus(); // 使游戏面板获取焦点
            _hasMoved = false; // 重置是否按过方向键的标记

            if (_timer != null) // 如果定时器已经存在
            {
                _timer.Stop(); // 停止计时器
            }
        }

        // 游戏是否进行中属性
        public bool IsPlayGame
        {
            get => _isPlayGame;  // 获取游戏是否正在进行的状态
            set
            {
                if (_isPlayGame == value) // 如果状态没有变化
                    return;

                _isPlayGame = value; // 更新游戏状态
                btnPlayGame.ExecBeginInvoke(() => // 更新按钮文本
                {
                    btnPlayGame.Text = value ? "重新开始" : "开启游戏"; // 如果游戏正在进行，按钮显示 "重新开始"
                });
            }
        }

        // 窗体加载事件
        private void FrmGame_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // 允许窗体处理键盘事件
            BindType(typeof(MazeType), this.cbMazeType, "Default"); // 将 MazeType 枚举绑定到下拉框

            cbphb.Items.Add("所有难度"); // 为排行榜下拉框添加选项
            cbphb.Items.Add("简单");
            cbphb.Items.Add("中等");
            cbphb.Items.Add("困难");
            cbphb.SelectedIndex = 0; // 默认选择 "所有难度"

            cbphb.SelectedIndexChanged += Cbphb_SelectedIndexChanged; // 为难度下拉框绑定选择变化事件

            UpdateListBox(); // 更新排行榜列表
        }

        // 绑定枚举类型到 ComboBox
        private void BindType(Type type, ComboBox comboBox, string defaultValue)
        {
            var enumValues = Enum.GetValues(type); // 获取枚举的所有值
            var list = new List<IdValues>(); // 创建列表存储枚举值
            int index = 0, curIndex = 0;
            foreach (Enum value in enumValues) // 遍历所有枚举值
            {
                int hc = value.GetHashCode(); // 获取哈希值
                list.Add(new IdValues // 将枚举值封装为 IdValues 对象
                {
                    Id = hc.ToString(),
                    Value = value.ToString(),
                    Standby = hc
                });
                if (value.ToString() == defaultValue)  // 如果当前枚举值与默认值相同
                    index = curIndex;  // 设置默认选中的索引

                curIndex++;  // 增加当前索引
            }

            comboBox.ValueMember = "Id";  // 设置 ComboBox 的值成员
            comboBox.DisplayMember = "Value";  // 设置 ComboBox 的显示成员
            comboBox.DataSource = list;  // 设置数据源
            comboBox.SelectedIndex = index;  // 设置默认选中的项
        }

        // 窗体关闭事件
        private void FrmGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            _maze.Dispose(); // 释放迷宫资源
            this.Dispose(); // 释放窗体资源
        }

        // 开始/重新开始游戏按钮点击事件
        private void btnPlayGame_Click(object sender, EventArgs e)
        {
            _hasMoved = false; // 重置方向键按下标记
            if (IsPlayGame)  // 如果当前正在进行游戏
            {
                // 如果正在游戏中，弹出确认框询问是否重新开始
                if (MessageBox.Show("正在游戏中，确认重新开始吗？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                {
                    plGame.Focus();  // 让游戏面板获取焦点
                    return;  // 取消重新开始游戏
                }
            }

            if (_timer != null)  // 如果定时器已存在
            {
                _timer.Stop();  // 停止定时器
                _timer.Dispose();  // 释放定时器资源
                _timer = null;  // 置空定时器
            }

            _isAutoMove = false;  // 停止自动移动
            IsPlayGame = true;  // 游戏开始
            _hasMoved = false;  // 重置按键标记
            time = 0;  // 重置时间
            lblTime.ExecBeginInvoke(() =>
            {
                lblTime.Text = "00:00";  // 显示时间为 00:00
            });

            int w, h;  // 定义迷宫宽度和高度
            if (rbEasy.Checked)  // 如果选择了简单难度
            {
                w = 30;
                h = 21;
                selectedDifficulty = "简单";  // 设置选择的难度
            }
            else if (rbMedium.Checked)  // 如果选择了中等难度
            {
                w = 66;
                h = 45;
                selectedDifficulty = "中等";
            }
            else  // 如果选择了困难难度
            {
                w = 100;
                h = 67;
                selectedDifficulty = "困难";
            }

            using var g = plGame.CreateGraphics();  // 获取游戏面板的绘图对象
            MazeType mazeType = (MazeType)(this.cbMazeType.Items[cbMazeType.SelectedIndex] as IdValues).Standby;  // 获取选中的迷宫类型
            _maze.CreateMaze(w, h, plGame.Width, plGame.Height, mazeType);  // 创建迷宫

            plGame.Controls.Clear();  // 清空面板上的控件
            g.Clear(plGame.BackColor);  // 清空画布
            _maze.Draw(g, plGame.Width, plGame.Height);  // 绘制迷宫

            _timer = new System.Windows.Forms.Timer();  // 创建定时器实例
            _timer.Interval = 1000;  // 设置定时器每秒触发一次
            time = 0;  // 重置计时器
            _timer.Tick += timer_Tick;  // 设置定时器的 Tick 事件

            plGame.Focus();  // 让游戏面板获取焦点
        }

        // 每秒触发计时器事件
        private void timer_Tick(object? sender, EventArgs e)
        {
            lblTime.ExecBeginInvoke(() =>  // 更新时间显示
            {
                lblTime.Text = Compute(++time);  // 递增时间并显示
            });
        }

        // 计算时间显示格式
        public string Compute(long time)
        {
            if (time < 60)  // 如果时间小于 60 秒
                return $"00:{ChangeString(time)}";  // 格式化为 "00:xx"
            long minute = time / 60;  // 计算分钟
            if (minute < 60)  // 如果分钟小于 60
                return $"{ChangeString(minute)}:{ChangeString(time % 60)}";  // 格式化为 "xx:xx"
            long hour = minute / 60;  // 计算小时
            return $"{ChangeString(hour)}:{Compute(time - hour * 3600)}";  // 格式化为 "xx:xx:xx"
        }

        private string ChangeString(long val)
        {
            return val.ToString("D2");  // 将数字格式化为两位数
        }

        // 游戏面板的绘制事件
        private void plGame_Paint(object sender, PaintEventArgs e)
        {
            plGame.Controls.Clear();  // 清空面板上的控件
            e.Graphics.Clear(plGame.BackColor);  // 清空绘图区域
            _maze.Draw(e.Graphics, plGame.Width, plGame.Height);  // 绘制迷宫
        }

        // 按键按下事件，用于处理玩家的方向键控制
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_hasMoved)  // 如果玩家还没有移动过
            {
                _hasMoved = true;  // 标记玩家已经移动过
                if (_timer != null)
                {
                    _timer.Start();  // 启动计时器
                }
            }

            if (_isAutoMove)  // 如果正在自动移动，忽略按键输入
                return;

            base.OnKeyDown(e);  // 调用基类的 OnKeyDown 方法
            var result = _maze.Move(e);  // 处理玩家的移动
            RefreshResult(result);  // 刷新游戏状态
        }

        // 刷新游戏状态，更新显示
        private void RefreshResult(MoveResult result)
        {
            if (result.IsInvalidate)  // 如果玩家位置发生变化
            {
                plGame.ExecInvoke(() =>
                {
                    plGame.Invalidate(result.OldRect);  // 无效区域需要重新绘制
                    plGame.Invalidate(result.NewRect);  // 新区域需要重新绘制
                });

                if (result.IsWin)  // 如果玩家通关
                {
                    IsPlayGame = false;  // 设置游戏状态为未进行中
                    if (_timer != null)
                    {
                        _timer.Stop();  // 停止计时器
                        _timer.Dispose();  // 释放定时器资源
                        _timer = null;  // 置空定时器
                    }

                    waveOut.Pause();  // 暂停背景音乐

                    string elapsedTime = Compute(time);  // 计算通关时间

                    DialogResult dialogResult = MessageBox.Show($"通关成功！\n难度：{selectedDifficulty}\n用时：{elapsedTime}",
                        "通关提示", MessageBoxButtons.OK, MessageBoxIcon.Information);  // 弹窗显示通关信息

                    if (dialogResult == DialogResult.OK)  // 如果用户点击 "OK"
                    {
                        waveOut.Play();  // 恢复背景音乐播放
                    }

                    // 保存通关记录到列表
                    GameRecord record = new GameRecord(selectedDifficulty, elapsedTime);
                    gameRecords.Add(record);  // 添加记录到游戏记录列表

                    UpdateListBox();  // 更新排行榜列表

                    _hasMoved = false;  // 重置玩家移动标记，避免回到起点时误启动计时器
                }
            }
        }

        // 更新排行榜 ListBox 数据
        private void UpdateListBox()
        {
            // Ensure this code runs on the UI thread
            if (listBoxphb.InvokeRequired)
            {
                listBoxphb.Invoke(new Action(UpdateListBox));
                return;
            }

            listBoxphb.Items.Clear();  // 清空现有的排行榜

            string selectedCategory = cbphb.SelectedItem.ToString();  // 获取当前选中的难度

            var filteredRecords = selectedCategory == "所有难度"  // 如果选择 "所有难度"
                ? gameRecords  // 不过滤记录
                : gameRecords.Where(r => r.Difficulty == selectedCategory).ToList();  // 根据难度过滤记录

            // 将筛选后的记录添加到 ListBox 中
            foreach (var record in filteredRecords)
            {
                listBoxphb.Items.Add($"{record.Difficulty}: {record.Time}");
            }
        }

        // 下拉框选择变化事件
        private void Cbphb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateListBox();  // 更新排行榜
        }

        // 游戏记录类
        public class GameRecord
        {
            public string Difficulty { get; set; }  // 游戏难度
            public string Time { get; set; }  // 通关时间

            public GameRecord(string difficulty, string time)
            {
                Difficulty = difficulty;  // 初始化难度
                Time = time;  // 初始化通关时间
            }
        }

        // 窗体激活时，确保聚焦到游戏面板
        private void FrmMain_Activated(object sender, EventArgs e)
        {
            plGame.Focus();  // 确保游戏面板获得焦点
        }

        // 提示按钮点击事件，用于显示迷宫提示
        private void btnPrompt_Click(object sender, EventArgs e)
        {
            if (_maze.MazeBitmap == null)  // 如果迷宫位图为空，直接返回
            {
                return;
            }

            Bitmap bmp = new Bitmap(plGame.Width, plGame.Height);  // 创建与游戏面板大小相同的位图
            plGame.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));  // 将游戏面板内容绘制到位图中
            int size = rbEasy.Checked ? 0 : rbMedium.Checked ? 1 : 2;  // 根据难度选择提示级别
            FrmPrompt frmPrompt = new FrmPrompt(_maze.DrawPath(bmp), size);  // 创建并显示提示窗体
            frmPrompt.Show();  // 显示提示窗口
            plGame.Focus();  // 确保游戏面板获得焦点
        }

        // 自动通关按钮点击事件
        private void btnPass_Click(object sender, EventArgs e)
        {
            if (!_maze.IsMove)  // 如果迷宫无法移动，直接返回
                return;

            _isAutoMove = true;  // 设置为自动移动
            Task.Run(() =>
            {
                var path = _maze.FindPath();  // 查找迷宫路径
                if (path != null)  // 如果找到路径
                {
                    Point point = new Point(0, 0);  // 设置起始位置
                    foreach (var item in path)  // 遍历路径
                    {
                        if (!_isAutoMove)  // 如果自动移动被取消，退出循环
                            break;

                        point.X = item.X;  // 更新当前位置
                        point.Y = item.Y;
                        var result = _maze.Move(point);  // 执行移动
                        RefreshResult(result);  // 刷新游戏状态
                        plGame.ExecInvoke(() =>
                        {
                            plGame.Update();  // 更新游戏面板
                        });

                        Thread.Sleep(50);  // 每步暂停 50 毫秒
                    }
                }
                _isAutoMove = false;  // 结束自动移动
            });
        }

        // 处理游戏面板的尺寸变化
        private void plGame_Resize(object sender, EventArgs e)
        {
            // 如果需要调整面板的大小时，可以在这里处理
        }

        // 定义 IdValues 类用于枚举值的处理
        public class IdValues
        {
            public string Id { get; set; }  // 存储枚举值的 Id
            public string Value { get; set; }  // 存储枚举值的显示文本
            public string Value2 { get; set; }  // 备用字段
            public string Value3 { get; set; }  // 备用字段
            public string Value4 { get; set; }  // 备用字段
            public string Value5 { get; set; }  // 备用字段
            public int Standby { get; set; }  // 存储备用字段的整型值

            public static bool operator ==(IdValues idValues, IdValues idValues2)
            {
                return idValues.Equals(idValues2);  // 判断两个 IdValues 是否相等
            }
            public static bool operator !=(IdValues idValues, IdValues idValues2)
            {
                return !idValues.Equals(idValues2);  // 判断两个 IdValues 是否不相等
            }

            public override int GetHashCode()
            {
                var code = (Id, Value, Value2, Value3, Value4, Value5, Standby).GetHashCode();  // 计算哈希码
                return code;
            }

            public override bool Equals(object? obj)
            {
                return obj?.GetHashCode() == GetHashCode();  // 判断是否相等
            }

            const int TARGET = 0x1F;  // 常量，用于哈希计算

            // 将连续字段的哈希代码左移两位或更多位来加权各个哈希代码
            // 在最佳情况下，超出位 31 的位应环绕，而不是被丢弃
            public int ShiftAndWrap(int value, int positions = 3)
            {
                positions &= TARGET;  // 确保位置不超过 31
                uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);  // 将整数转换为字节数组并再转换为无符号整型
                uint wrapped = number >> (32 - positions);  // 执行右移操作
                return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);  // 进行位运算后再转换回整数
            }
        }
    }
}



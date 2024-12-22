using Emgu.CV.CvEnum;  // ���� EmguCV ���е�ö�����ͣ�����ͼ����
using Labyrinth.Common; // ���� Labyrinth ��Ŀ�Ĺ������
using System.Diagnostics; // �������ڵ��Ե������ռ�
using System.Numerics; // ���ô�������͸����������ռ�
using System.Windows.Forms; // ���ô���Ϳؼ��������ռ�
using NAudio.Wave; // ���� NAudio �⣬������Ƶ����
using System.IO; // �����ļ������������ռ�

namespace Labyrinth // ���� Labyrinth �����ռ�
{
    public partial class FrmMain : Form // FrmMain ��̳��� Form �࣬����Ϸ������
    {
        // ��Ա�������岿��
        private readonly Maze _maze; // �洢�Թ�ʵ��
        private System.Windows.Forms.Timer _timer; // �洢��ʱ�������ڼ�ʱ
        private bool _hasMoved = false; // ����Ƿ��Ѱ��������
        private bool _isPlayGame = false; // �����Ϸ�Ƿ������
        private bool _isAutoMove = false; // ����Ƿ������Զ��ƶ�
        private long time = 0; // ��Ϸ�ļ�ʱ������
        private bool isMuted = false;  // ����Ƿ���
        private WaveOutEvent waveOut;  // �洢��Ƶ������ʵ��
        private AudioFileReader audioFileReader;  // �洢��Ƶ�ļ���ȡ��ʵ��
        private Point _playerPosition = new Point(0, 0); // �洢���λ��
        private string selectedDifficulty = "δ֪�Ѷ�"; // ��ǰѡ�����Ϸ�Ѷ�
        private readonly List<GameRecord> gameRecords = new List<GameRecord>(); // �洢��Ϸͨ�ؼ�¼

        // ���캯��
        public FrmMain()
        {
            InitializeComponent();  // ��ʼ������ؼ�
            this.SetStyle(ControlStyles.DoubleBuffer |  // ����˫�����Լ�����Ļ��˸
                ControlStyles.UserPaint |  // �Զ������
                ControlStyles.AllPaintingInWmPaint,  // �������л��ƹ���
                true);
            this.UpdateStyles();  // ���¿ؼ�����ʽ

            _maze = new Maze();  // �����Թ�ʵ��
            InitializeAudioPlayer();  // ��ʼ����Ƶ������
            this.btnMuteMusic.Click += new System.EventHandler(this.btnMuteMusic_Click);  // �󶨾�����ť����¼�
        }

        // ��Ƶ�ļ����ҷ���
        private string FindAudioFile()
        {
            string startupPath = Application.StartupPath; // ��ȡӦ�ó�������·��
            string projectRootPath = startupPath; // ��ʼ·��Ϊ����·��
            while (!Directory.Exists(Path.Combine(projectRootPath, "music"))) // ѭ��ֱ���ҵ� "music" �ļ���
            {
                projectRootPath = Directory.GetParent(projectRootPath).FullName; // ���ϲ��Ҹ���Ŀ¼
                if (string.IsNullOrEmpty(projectRootPath)) // ����Ҳ�������Ŀ¼
                {
                    MessageBox.Show("δ�ҵ�music�ļ���."); // ��ʾδ�ҵ������ļ���
                    return null; // ���� null����ʾδ�ҵ���Ƶ�ļ���
                }
            }

            string musicFolderPath = Path.Combine(projectRootPath, "music"); // ��ȡ music �ļ���·��
            string[] audioExtensions = { ".wav", ".mp3", ".ogg", ".flac" }; // ֧�ֵ���Ƶ�ļ���չ��

            foreach (string extension in audioExtensions) // ����֧�ֵ���Ƶ��ʽ
            {
                string[] audioFiles = Directory.GetFiles(musicFolderPath, $"*{extension}", SearchOption.TopDirectoryOnly); // ���Ҹ���չ������Ƶ�ļ�
                if (audioFiles.Length > 0) // ����ҵ���Ƶ�ļ�
                {
                    return audioFiles[0]; // ���ص�һ���ҵ�����Ƶ�ļ�·��
                }
            }

            return null; // ���û���ҵ���Ƶ�ļ������� null
        }

        // ��ʼ����Ƶ������
        private void InitializeAudioPlayer()
        {
            if (waveOut == null) // �����Ƶ������δ��ʼ��
            {
                waveOut = new WaveOutEvent(); // ���� WaveOutEvent ��Ƶ������
                Debug.WriteLine("waveOut initialized"); // ���������Ϣ
            }

            string audioFilePath = FindAudioFile(); // ������Ƶ�ļ�

            if (audioFilePath != null && File.Exists(audioFilePath)) // �����Ƶ�ļ�����
            {
                audioFileReader = new AudioFileReader(audioFilePath); // ���� AudioFileReader ����ȡ��Ƶ�ļ�
                waveOut.Init(audioFileReader); // ��ʼ��������
                waveOut.Play(); // ��ʼ������Ƶ
                waveOut.Volume = 0.5f; // ��������Ϊ 50%

                waveOut.PlaybackStopped += (s, e) =>  // ��Ƶ����������¿�ʼ����
                {
                    audioFileReader.Position = 0;  // ����Ƶ����λ�ûص����
                    waveOut.Play();  // ��������
                };
            }
            else
            {
                MessageBox.Show("δ�ҵ���Ƶ�ļ���"); // ���û���ҵ���Ƶ�ļ���������ʾ
            }
        }

        // ������ť����¼�
        private void btnMuteMusic_Click(object sender, EventArgs e)
        {
            if (isMuted) // �����ǰ�Ѿ�����
            {
                waveOut.Volume = 0.5f; // �ָ�����Ϊ 50%
                waveOut.Play(); // ��������
                isMuted = false; // ���Ϊδ����
                btnMuteMusic.Text = "Mute"; // �޸İ�ť�ı�Ϊ "Mute"
                Debug.WriteLine("Music unmuted"); // ���������Ϣ
            }
            else
            {
                waveOut.Volume = 0f; // ��������Ϊ 0
                waveOut.Pause(); // ��ͣ����
                isMuted = true; // ���Ϊ����
                btnMuteMusic.Text = "Unmute"; // �޸İ�ť�ı�Ϊ "Unmute"
                Debug.WriteLine("Music muted"); // ���������Ϣ
            }
        }

        // ��Ϸ���ð�ť����¼�
        private void btnReset_Click(object sender, EventArgs e)
        {
            _playerPosition = new Point(0, 0); // �������λ�õ����
            var result = _maze.Move(_playerPosition); // �����Թ��� Move ������������ƶ�
            RefreshResult(result); // ˢ����Ϸ״̬
            plGame.Invalidate(); // ǿ�����»�����Ϸ���
            time = 0; // ���ü�ʱ��
            lblTime.BeginInvoke(() => lblTime.Text = Compute(time)); // ����ʱ����ʾ
            plGame.Focus(); // ʹ��Ϸ����ȡ����
            _hasMoved = false; // �����Ƿ񰴹�������ı��

            if (_timer != null) // �����ʱ���Ѿ�����
            {
                _timer.Stop(); // ֹͣ��ʱ��
            }
        }

        // ��Ϸ�Ƿ����������
        public bool IsPlayGame
        {
            get => _isPlayGame;  // ��ȡ��Ϸ�Ƿ����ڽ��е�״̬
            set
            {
                if (_isPlayGame == value) // ���״̬û�б仯
                    return;

                _isPlayGame = value; // ������Ϸ״̬
                btnPlayGame.ExecBeginInvoke(() => // ���°�ť�ı�
                {
                    btnPlayGame.Text = value ? "���¿�ʼ" : "������Ϸ"; // �����Ϸ���ڽ��У���ť��ʾ "���¿�ʼ"
                });
            }
        }

        // ��������¼�
        private void FrmGame_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // �����崦������¼�
            BindType(typeof(MazeType), this.cbMazeType, "Default"); // �� MazeType ö�ٰ󶨵�������

            cbphb.Items.Add("�����Ѷ�"); // Ϊ���а����������ѡ��
            cbphb.Items.Add("��");
            cbphb.Items.Add("�е�");
            cbphb.Items.Add("����");
            cbphb.SelectedIndex = 0; // Ĭ��ѡ�� "�����Ѷ�"

            cbphb.SelectedIndexChanged += Cbphb_SelectedIndexChanged; // Ϊ�Ѷ��������ѡ��仯�¼�

            UpdateListBox(); // �������а��б�
        }

        // ��ö�����͵� ComboBox
        private void BindType(Type type, ComboBox comboBox, string defaultValue)
        {
            var enumValues = Enum.GetValues(type); // ��ȡö�ٵ�����ֵ
            var list = new List<IdValues>(); // �����б�洢ö��ֵ
            int index = 0, curIndex = 0;
            foreach (Enum value in enumValues) // ��������ö��ֵ
            {
                int hc = value.GetHashCode(); // ��ȡ��ϣֵ
                list.Add(new IdValues // ��ö��ֵ��װΪ IdValues ����
                {
                    Id = hc.ToString(),
                    Value = value.ToString(),
                    Standby = hc
                });
                if (value.ToString() == defaultValue)  // �����ǰö��ֵ��Ĭ��ֵ��ͬ
                    index = curIndex;  // ����Ĭ��ѡ�е�����

                curIndex++;  // ���ӵ�ǰ����
            }

            comboBox.ValueMember = "Id";  // ���� ComboBox ��ֵ��Ա
            comboBox.DisplayMember = "Value";  // ���� ComboBox ����ʾ��Ա
            comboBox.DataSource = list;  // ��������Դ
            comboBox.SelectedIndex = index;  // ����Ĭ��ѡ�е���
        }

        // ����ر��¼�
        private void FrmGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            _maze.Dispose(); // �ͷ��Թ���Դ
            this.Dispose(); // �ͷŴ�����Դ
        }

        // ��ʼ/���¿�ʼ��Ϸ��ť����¼�
        private void btnPlayGame_Click(object sender, EventArgs e)
        {
            _hasMoved = false; // ���÷�������±��
            if (IsPlayGame)  // �����ǰ���ڽ�����Ϸ
            {
                // ���������Ϸ�У�����ȷ�Ͽ�ѯ���Ƿ����¿�ʼ
                if (MessageBox.Show("������Ϸ�У�ȷ�����¿�ʼ��", "ϵͳ��ʾ", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                {
                    plGame.Focus();  // ����Ϸ����ȡ����
                    return;  // ȡ�����¿�ʼ��Ϸ
                }
            }

            if (_timer != null)  // �����ʱ���Ѵ���
            {
                _timer.Stop();  // ֹͣ��ʱ��
                _timer.Dispose();  // �ͷŶ�ʱ����Դ
                _timer = null;  // �ÿն�ʱ��
            }

            _isAutoMove = false;  // ֹͣ�Զ��ƶ�
            IsPlayGame = true;  // ��Ϸ��ʼ
            _hasMoved = false;  // ���ð������
            time = 0;  // ����ʱ��
            lblTime.ExecBeginInvoke(() =>
            {
                lblTime.Text = "00:00";  // ��ʾʱ��Ϊ 00:00
            });

            int w, h;  // �����Թ���Ⱥ͸߶�
            if (rbEasy.Checked)  // ���ѡ���˼��Ѷ�
            {
                w = 30;
                h = 21;
                selectedDifficulty = "��";  // ����ѡ����Ѷ�
            }
            else if (rbMedium.Checked)  // ���ѡ�����е��Ѷ�
            {
                w = 66;
                h = 45;
                selectedDifficulty = "�е�";
            }
            else  // ���ѡ���������Ѷ�
            {
                w = 100;
                h = 67;
                selectedDifficulty = "����";
            }

            using var g = plGame.CreateGraphics();  // ��ȡ��Ϸ���Ļ�ͼ����
            MazeType mazeType = (MazeType)(this.cbMazeType.Items[cbMazeType.SelectedIndex] as IdValues).Standby;  // ��ȡѡ�е��Թ�����
            _maze.CreateMaze(w, h, plGame.Width, plGame.Height, mazeType);  // �����Թ�

            plGame.Controls.Clear();  // �������ϵĿؼ�
            g.Clear(plGame.BackColor);  // ��ջ���
            _maze.Draw(g, plGame.Width, plGame.Height);  // �����Թ�

            _timer = new System.Windows.Forms.Timer();  // ������ʱ��ʵ��
            _timer.Interval = 1000;  // ���ö�ʱ��ÿ�봥��һ��
            time = 0;  // ���ü�ʱ��
            _timer.Tick += timer_Tick;  // ���ö�ʱ���� Tick �¼�

            plGame.Focus();  // ����Ϸ����ȡ����
        }

        // ÿ�봥����ʱ���¼�
        private void timer_Tick(object? sender, EventArgs e)
        {
            lblTime.ExecBeginInvoke(() =>  // ����ʱ����ʾ
            {
                lblTime.Text = Compute(++time);  // ����ʱ�䲢��ʾ
            });
        }

        // ����ʱ����ʾ��ʽ
        public string Compute(long time)
        {
            if (time < 60)  // ���ʱ��С�� 60 ��
                return $"00:{ChangeString(time)}";  // ��ʽ��Ϊ "00:xx"
            long minute = time / 60;  // �������
            if (minute < 60)  // �������С�� 60
                return $"{ChangeString(minute)}:{ChangeString(time % 60)}";  // ��ʽ��Ϊ "xx:xx"
            long hour = minute / 60;  // ����Сʱ
            return $"{ChangeString(hour)}:{Compute(time - hour * 3600)}";  // ��ʽ��Ϊ "xx:xx:xx"
        }

        private string ChangeString(long val)
        {
            return val.ToString("D2");  // �����ָ�ʽ��Ϊ��λ��
        }

        // ��Ϸ���Ļ����¼�
        private void plGame_Paint(object sender, PaintEventArgs e)
        {
            plGame.Controls.Clear();  // �������ϵĿؼ�
            e.Graphics.Clear(plGame.BackColor);  // ��ջ�ͼ����
            _maze.Draw(e.Graphics, plGame.Width, plGame.Height);  // �����Թ�
        }

        // ���������¼������ڴ�����ҵķ��������
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_hasMoved)  // �����һ�û���ƶ���
            {
                _hasMoved = true;  // �������Ѿ��ƶ���
                if (_timer != null)
                {
                    _timer.Start();  // ������ʱ��
                }
            }

            if (_isAutoMove)  // ��������Զ��ƶ������԰�������
                return;

            base.OnKeyDown(e);  // ���û���� OnKeyDown ����
            var result = _maze.Move(e);  // ������ҵ��ƶ�
            RefreshResult(result);  // ˢ����Ϸ״̬
        }

        // ˢ����Ϸ״̬��������ʾ
        private void RefreshResult(MoveResult result)
        {
            if (result.IsInvalidate)  // ������λ�÷����仯
            {
                plGame.ExecInvoke(() =>
                {
                    plGame.Invalidate(result.OldRect);  // ��Ч������Ҫ���»���
                    plGame.Invalidate(result.NewRect);  // ��������Ҫ���»���
                });

                if (result.IsWin)  // ������ͨ��
                {
                    IsPlayGame = false;  // ������Ϸ״̬Ϊδ������
                    if (_timer != null)
                    {
                        _timer.Stop();  // ֹͣ��ʱ��
                        _timer.Dispose();  // �ͷŶ�ʱ����Դ
                        _timer = null;  // �ÿն�ʱ��
                    }

                    waveOut.Pause();  // ��ͣ��������

                    string elapsedTime = Compute(time);  // ����ͨ��ʱ��

                    DialogResult dialogResult = MessageBox.Show($"ͨ�سɹ���\n�Ѷȣ�{selectedDifficulty}\n��ʱ��{elapsedTime}",
                        "ͨ����ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);  // ������ʾͨ����Ϣ

                    if (dialogResult == DialogResult.OK)  // ����û���� "OK"
                    {
                        waveOut.Play();  // �ָ��������ֲ���
                    }

                    // ����ͨ�ؼ�¼���б�
                    GameRecord record = new GameRecord(selectedDifficulty, elapsedTime);
                    gameRecords.Add(record);  // ��Ӽ�¼����Ϸ��¼�б�

                    UpdateListBox();  // �������а��б�

                    _hasMoved = false;  // ��������ƶ���ǣ�����ص����ʱ��������ʱ��
                }
            }
        }

        // �������а� ListBox ����
        private void UpdateListBox()
        {
            // Ensure this code runs on the UI thread
            if (listBoxphb.InvokeRequired)
            {
                listBoxphb.Invoke(new Action(UpdateListBox));
                return;
            }

            listBoxphb.Items.Clear();  // ������е����а�

            string selectedCategory = cbphb.SelectedItem.ToString();  // ��ȡ��ǰѡ�е��Ѷ�

            var filteredRecords = selectedCategory == "�����Ѷ�"  // ���ѡ�� "�����Ѷ�"
                ? gameRecords  // �����˼�¼
                : gameRecords.Where(r => r.Difficulty == selectedCategory).ToList();  // �����Ѷȹ��˼�¼

            // ��ɸѡ��ļ�¼��ӵ� ListBox ��
            foreach (var record in filteredRecords)
            {
                listBoxphb.Items.Add($"{record.Difficulty}: {record.Time}");
            }
        }

        // ������ѡ��仯�¼�
        private void Cbphb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateListBox();  // �������а�
        }

        // ��Ϸ��¼��
        public class GameRecord
        {
            public string Difficulty { get; set; }  // ��Ϸ�Ѷ�
            public string Time { get; set; }  // ͨ��ʱ��

            public GameRecord(string difficulty, string time)
            {
                Difficulty = difficulty;  // ��ʼ���Ѷ�
                Time = time;  // ��ʼ��ͨ��ʱ��
            }
        }

        // ���弤��ʱ��ȷ���۽�����Ϸ���
        private void FrmMain_Activated(object sender, EventArgs e)
        {
            plGame.Focus();  // ȷ����Ϸ����ý���
        }

        // ��ʾ��ť����¼���������ʾ�Թ���ʾ
        private void btnPrompt_Click(object sender, EventArgs e)
        {
            if (_maze.MazeBitmap == null)  // ����Թ�λͼΪ�գ�ֱ�ӷ���
            {
                return;
            }

            Bitmap bmp = new Bitmap(plGame.Width, plGame.Height);  // ��������Ϸ����С��ͬ��λͼ
            plGame.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));  // ����Ϸ������ݻ��Ƶ�λͼ��
            int size = rbEasy.Checked ? 0 : rbMedium.Checked ? 1 : 2;  // �����Ѷ�ѡ����ʾ����
            FrmPrompt frmPrompt = new FrmPrompt(_maze.DrawPath(bmp), size);  // ��������ʾ��ʾ����
            frmPrompt.Show();  // ��ʾ��ʾ����
            plGame.Focus();  // ȷ����Ϸ����ý���
        }

        // �Զ�ͨ�ذ�ť����¼�
        private void btnPass_Click(object sender, EventArgs e)
        {
            if (!_maze.IsMove)  // ����Թ��޷��ƶ���ֱ�ӷ���
                return;

            _isAutoMove = true;  // ����Ϊ�Զ��ƶ�
            Task.Run(() =>
            {
                var path = _maze.FindPath();  // �����Թ�·��
                if (path != null)  // ����ҵ�·��
                {
                    Point point = new Point(0, 0);  // ������ʼλ��
                    foreach (var item in path)  // ����·��
                    {
                        if (!_isAutoMove)  // ����Զ��ƶ���ȡ�����˳�ѭ��
                            break;

                        point.X = item.X;  // ���µ�ǰλ��
                        point.Y = item.Y;
                        var result = _maze.Move(point);  // ִ���ƶ�
                        RefreshResult(result);  // ˢ����Ϸ״̬
                        plGame.ExecInvoke(() =>
                        {
                            plGame.Update();  // ������Ϸ���
                        });

                        Thread.Sleep(50);  // ÿ����ͣ 50 ����
                    }
                }
                _isAutoMove = false;  // �����Զ��ƶ�
            });
        }

        // ������Ϸ���ĳߴ�仯
        private void plGame_Resize(object sender, EventArgs e)
        {
            // �����Ҫ�������Ĵ�Сʱ�����������ﴦ��
        }

        // ���� IdValues ������ö��ֵ�Ĵ���
        public class IdValues
        {
            public string Id { get; set; }  // �洢ö��ֵ�� Id
            public string Value { get; set; }  // �洢ö��ֵ����ʾ�ı�
            public string Value2 { get; set; }  // �����ֶ�
            public string Value3 { get; set; }  // �����ֶ�
            public string Value4 { get; set; }  // �����ֶ�
            public string Value5 { get; set; }  // �����ֶ�
            public int Standby { get; set; }  // �洢�����ֶε�����ֵ

            public static bool operator ==(IdValues idValues, IdValues idValues2)
            {
                return idValues.Equals(idValues2);  // �ж����� IdValues �Ƿ����
            }
            public static bool operator !=(IdValues idValues, IdValues idValues2)
            {
                return !idValues.Equals(idValues2);  // �ж����� IdValues �Ƿ����
            }

            public override int GetHashCode()
            {
                var code = (Id, Value, Value2, Value3, Value4, Value5, Standby).GetHashCode();  // �����ϣ��
                return code;
            }

            public override bool Equals(object? obj)
            {
                return obj?.GetHashCode() == GetHashCode();  // �ж��Ƿ����
            }

            const int TARGET = 0x1F;  // ���������ڹ�ϣ����

            // �������ֶεĹ�ϣ����������λ�����λ����Ȩ������ϣ����
            // ���������£�����λ 31 ��λӦ���ƣ������Ǳ�����
            public int ShiftAndWrap(int value, int positions = 3)
            {
                positions &= TARGET;  // ȷ��λ�ò����� 31
                uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);  // ������ת��Ϊ�ֽ����鲢��ת��Ϊ�޷�������
                uint wrapped = number >> (32 - positions);  // ִ�����Ʋ���
                return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);  // ����λ�������ת��������
            }
        }
    }
}



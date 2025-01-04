namespace Labyrinth
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            panel3 = new Panel();
            lblTime = new Label();
            panel2 = new Panel();
            btnReset = new Button();
            cbMazeType = new ComboBox();
            btnPass = new Button();
            rbMedium = new RadioButton();
            rbDifficulty = new RadioButton();
            rbEasy = new RadioButton();
            btnPlayGame = new Button();
            btnPrompt = new Button();
            panel1 = new Panel();
            cbphb = new ComboBox();
            listBoxphb = new ListBox();
            btnMuteMusic = new Button();
            plGame = new Panel();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.InsetDouble;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 257F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 1, 0);
            tableLayoutPanel1.Controls.Add(plGame, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(4);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1569, 815);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel2.Controls.Add(panel1, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(1313, 7);
            tableLayoutPanel2.Margin = new Padding(4);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 2;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(249, 801);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Controls.Add(panel3, 0, 1);
            tableLayoutPanel3.Controls.Add(panel2, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(6, 405);
            tableLayoutPanel3.Margin = new Padding(4);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 71F));
            tableLayoutPanel3.Size = new Size(237, 390);
            tableLayoutPanel3.TabIndex = 3;
            // 
            // panel3
            // 
            panel3.Controls.Add(lblTime);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(6, 321);
            panel3.Margin = new Padding(4);
            panel3.Name = "panel3";
            panel3.Size = new Size(225, 63);
            panel3.TabIndex = 1;
            // 
            // lblTime
            // 
            lblTime.Dock = DockStyle.Fill;
            lblTime.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
            lblTime.Location = new Point(0, 0);
            lblTime.Margin = new Padding(4, 0, 4, 0);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(225, 63);
            lblTime.TabIndex = 0;
            lblTime.Text = "00:00";
            lblTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnReset);
            panel2.Controls.Add(cbMazeType);
            panel2.Controls.Add(btnPass);
            panel2.Controls.Add(rbMedium);
            panel2.Controls.Add(rbDifficulty);
            panel2.Controls.Add(rbEasy);
            panel2.Controls.Add(btnPlayGame);
            panel2.Controls.Add(btnPrompt);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(6, 6);
            panel2.Margin = new Padding(4);
            panel2.Name = "panel2";
            panel2.Size = new Size(225, 305);
            panel2.TabIndex = 0;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(8, 222);
            btnReset.Margin = new Padding(4);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(206, 40);
            btnReset.TabIndex = 9;
            btnReset.Text = "回到起点";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // cbMazeType
            // 
            cbMazeType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMazeType.FormattingEnabled = true;
            cbMazeType.Location = new Point(9, 7);
            cbMazeType.Margin = new Padding(4);
            cbMazeType.Name = "cbMazeType";
            cbMazeType.Size = new Size(205, 28);
            cbMazeType.TabIndex = 8;
            // 
            // btnPass
            // 
            btnPass.Location = new Point(9, 174);
            btnPass.Margin = new Padding(4);
            btnPass.Name = "btnPass";
            btnPass.Size = new Size(206, 40);
            btnPass.TabIndex = 7;
            btnPass.Text = "一键通过";
            btnPass.UseVisualStyleBackColor = true;
            btnPass.Click += btnPass_Click;
            // 
            // rbMedium
            // 
            rbMedium.AutoSize = true;
            rbMedium.Location = new Point(78, 48);
            rbMedium.Margin = new Padding(4);
            rbMedium.Name = "rbMedium";
            rbMedium.Size = new Size(60, 24);
            rbMedium.TabIndex = 6;
            rbMedium.Text = "中等";
            rbMedium.UseVisualStyleBackColor = true;
            // 
            // rbDifficulty
            // 
            rbDifficulty.AutoSize = true;
            rbDifficulty.Location = new Point(150, 48);
            rbDifficulty.Margin = new Padding(4);
            rbDifficulty.Name = "rbDifficulty";
            rbDifficulty.Size = new Size(60, 24);
            rbDifficulty.TabIndex = 5;
            rbDifficulty.Text = "困难";
            rbDifficulty.UseVisualStyleBackColor = true;
            // 
            // rbEasy
            // 
            rbEasy.AutoSize = true;
            rbEasy.Checked = true;
            rbEasy.Location = new Point(8, 48);
            rbEasy.Margin = new Padding(4);
            rbEasy.Name = "rbEasy";
            rbEasy.Size = new Size(60, 24);
            rbEasy.TabIndex = 4;
            rbEasy.TabStop = true;
            rbEasy.Text = "简单";
            rbEasy.UseVisualStyleBackColor = true;
            // 
            // btnPlayGame
            // 
            btnPlayGame.Location = new Point(9, 80);
            btnPlayGame.Margin = new Padding(4);
            btnPlayGame.Name = "btnPlayGame";
            btnPlayGame.Size = new Size(206, 40);
            btnPlayGame.TabIndex = 1;
            btnPlayGame.Text = "开启游戏";
            btnPlayGame.UseVisualStyleBackColor = true;
            btnPlayGame.Click += btnPlayGame_Click;
            // 
            // btnPrompt
            // 
            btnPrompt.Location = new Point(9, 127);
            btnPrompt.Margin = new Padding(4);
            btnPrompt.Name = "btnPrompt";
            btnPrompt.Size = new Size(206, 40);
            btnPrompt.TabIndex = 2;
            btnPrompt.Text = "智能提示";
            btnPrompt.UseVisualStyleBackColor = true;
            btnPrompt.Click += btnPrompt_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(cbphb);
            panel1.Controls.Add(listBoxphb);
            panel1.Controls.Add(btnMuteMusic);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(6, 6);
            panel1.Margin = new Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new Size(237, 389);
            panel1.TabIndex = 0;
            // 
            // cbphb
            // 
            cbphb.FormattingEnabled = true;
            cbphb.Location = new Point(19, 25);
            cbphb.Name = "cbphb";
            cbphb.Size = new Size(151, 28);
            cbphb.TabIndex = 0;
            cbphb.Text = "查询你的成绩";
            // 
            // listBoxphb
            // 
            listBoxphb.FormattingEnabled = true;
            listBoxphb.ItemHeight = 20;
            listBoxphb.Location = new Point(19, 61);
            listBoxphb.Name = "listBoxphb";
            listBoxphb.Size = new Size(201, 304);
            listBoxphb.TabIndex = 3;
            // 
            // btnMuteMusic
            // 
            btnMuteMusic.Location = new Point(203, 3);
            btnMuteMusic.Name = "btnMuteMusic";
            btnMuteMusic.Size = new Size(31, 29);
            btnMuteMusic.TabIndex = 2;
            btnMuteMusic.Text = "♪";
            btnMuteMusic.UseVisualStyleBackColor = true;
            // 
            // plGame
            // 
            plGame.Dock = DockStyle.Fill;
            plGame.Location = new Point(42, 38);
            plGame.Margin = new Padding(39, 35, 39, 35);
            plGame.Name = "plGame";
            plGame.Size = new Size(1225, 739);
            plGame.TabIndex = 1;
            plGame.Paint += plGame_Paint;
            plGame.Resize += plGame_Resize;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1569, 815);
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "迷宫";
            Activated += FrmMain_Activated;
            FormClosing += FrmGame_FormClosing;
            Load += FrmGame_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Panel panel1;
        private TableLayoutPanel tableLayoutPanel3;
        private Panel panel2;
        private Button btnPrompt;
        private Button btnPlayGame;
        private Panel panel3;
        private Label lblTime;
        private RadioButton rbDifficulty;
        private RadioButton rbEasy;
        private Panel plGame;
        private RadioButton rbMedium;
        private Button btnPass;
        private ComboBox cbMazeType;
        private Button btnMuteMusic;
        private Button btnReset;
        private ComboBox cbphb;
        private ListBox listBoxphb;
    }
}
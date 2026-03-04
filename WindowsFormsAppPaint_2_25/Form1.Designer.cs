namespace WindowsFormsAppPaint_2_25
{
	partial class Form1
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.cbLineThickness = new System.Windows.Forms.ComboBox();
			this.cbLineColor = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnDrawLine = new System.Windows.Forms.Button();
			this.btnDrawRect = new System.Windows.Forms.Button();
			this.btnDrawEllipse = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.파일ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.열기ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.열기ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.저장ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btnUndo = new System.Windows.Forms.Button();
			this.btnRedo = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.button2 = new System.Windows.Forms.Button();
			this.cbDlMode = new System.Windows.Forms.ComboBox();
			this.tbMinArea = new System.Windows.Forms.TextBox();
			this.btnRunDl = new System.Windows.Forms.Button();
			this.btnSelectModel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::WindowsFormsAppPaint_2_25.Properties.Resources.cat;
			this.pictureBox1.Location = new System.Drawing.Point(31, 69);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(500, 400);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
			this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
			this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
			this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
			// 
			// cbLineThickness
			// 
			this.cbLineThickness.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbLineThickness.FormattingEnabled = true;
			this.cbLineThickness.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
			this.cbLineThickness.Location = new System.Drawing.Point(622, 69);
			this.cbLineThickness.Name = "cbLineThickness";
			this.cbLineThickness.Size = new System.Drawing.Size(166, 26);
			this.cbLineThickness.TabIndex = 1;
			this.cbLineThickness.SelectedIndexChanged += new System.EventHandler(this.cbLineThickness_SelectedIndexChanged);
			// 
			// cbLineColor
			// 
			this.cbLineColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbLineColor.FormattingEnabled = true;
			this.cbLineColor.Items.AddRange(new object[] {
            "검정색",
            "빨간색",
            "주황색",
            "노란색",
            "초록색",
            "파란색",
            "보라색",
            "흰색"});
			this.cbLineColor.Location = new System.Drawing.Point(622, 113);
			this.cbLineColor.Name = "cbLineColor";
			this.cbLineColor.Size = new System.Drawing.Size(166, 26);
			this.cbLineColor.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(538, 76);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(68, 18);
			this.label1.TabIndex = 3;
			this.label1.Text = "선 두께";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(541, 129);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(68, 18);
			this.label2.TabIndex = 4;
			this.label2.Text = "선 색상";
			// 
			// btnDrawLine
			// 
			this.btnDrawLine.Location = new System.Drawing.Point(544, 197);
			this.btnDrawLine.Name = "btnDrawLine";
			this.btnDrawLine.Size = new System.Drawing.Size(254, 73);
			this.btnDrawLine.TabIndex = 5;
			this.btnDrawLine.Text = "선 그리기";
			this.btnDrawLine.UseVisualStyleBackColor = true;
			this.btnDrawLine.Click += new System.EventHandler(this.btnDrawLine_Click);
			// 
			// btnDrawRect
			// 
			this.btnDrawRect.Location = new System.Drawing.Point(544, 276);
			this.btnDrawRect.Name = "btnDrawRect";
			this.btnDrawRect.Size = new System.Drawing.Size(254, 73);
			this.btnDrawRect.TabIndex = 6;
			this.btnDrawRect.Text = "사각형 그리기";
			this.btnDrawRect.UseVisualStyleBackColor = true;
			this.btnDrawRect.Click += new System.EventHandler(this.btnDrawRect_Click);
			// 
			// btnDrawEllipse
			// 
			this.btnDrawEllipse.Location = new System.Drawing.Point(544, 355);
			this.btnDrawEllipse.Name = "btnDrawEllipse";
			this.btnDrawEllipse.Size = new System.Drawing.Size(254, 73);
			this.btnDrawEllipse.TabIndex = 7;
			this.btnDrawEllipse.Text = "원형 그리기";
			this.btnDrawEllipse.UseVisualStyleBackColor = true;
			this.btnDrawEllipse.Click += new System.EventHandler(this.btnDrawEllipse_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.파일ToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1223, 33);
			this.menuStrip1.TabIndex = 8;
			this.menuStrip1.Text = "파일 열기 저장";
			// 
			// 파일ToolStripMenuItem
			// 
			this.파일ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.열기ToolStripMenuItem,
            this.열기ToolStripMenuItem1,
            this.저장ToolStripMenuItem});
			this.파일ToolStripMenuItem.Name = "파일ToolStripMenuItem";
			this.파일ToolStripMenuItem.Size = new System.Drawing.Size(64, 29);
			this.파일ToolStripMenuItem.Text = "파일";
			// 
			// 열기ToolStripMenuItem
			// 
			this.열기ToolStripMenuItem.Name = "열기ToolStripMenuItem";
			this.열기ToolStripMenuItem.Size = new System.Drawing.Size(210, 34);
			this.열기ToolStripMenuItem.Text = "새로 만들기";
			this.열기ToolStripMenuItem.Click += new System.EventHandler(this.열기ToolStripMenuItem_Click);
			// 
			// 열기ToolStripMenuItem1
			// 
			this.열기ToolStripMenuItem1.Name = "열기ToolStripMenuItem1";
			this.열기ToolStripMenuItem1.Size = new System.Drawing.Size(210, 34);
			this.열기ToolStripMenuItem1.Text = "열기";
			this.열기ToolStripMenuItem1.Click += new System.EventHandler(this.열기ToolStripMenuItem1_Click);
			// 
			// 저장ToolStripMenuItem
			// 
			this.저장ToolStripMenuItem.Name = "저장ToolStripMenuItem";
			this.저장ToolStripMenuItem.Size = new System.Drawing.Size(210, 34);
			this.저장ToolStripMenuItem.Text = "저장";
			this.저장ToolStripMenuItem.Click += new System.EventHandler(this.저장ToolStripMenuItem_Click);
			// 
			// btnUndo
			// 
			this.btnUndo.Enabled = false;
			this.btnUndo.Location = new System.Drawing.Point(853, 69);
			this.btnUndo.Name = "btnUndo";
			this.btnUndo.Size = new System.Drawing.Size(101, 39);
			this.btnUndo.TabIndex = 9;
			this.btnUndo.Text = "실행취소";
			this.btnUndo.UseVisualStyleBackColor = true;
			this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
			// 
			// btnRedo
			// 
			this.btnRedo.Enabled = false;
			this.btnRedo.Location = new System.Drawing.Point(853, 114);
			this.btnRedo.Name = "btnRedo";
			this.btnRedo.Size = new System.Drawing.Size(101, 33);
			this.btnRedo.TabIndex = 10;
			this.btnRedo.Text = "되돌리기";
			this.btnRedo.UseVisualStyleBackColor = true;
			this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(31, 37);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(500, 28);
			this.textBox1.TabIndex = 11;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(541, 41);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 12;
			this.button1.Text = "선택";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// comboBox1
			// 
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "컬러 → 모노 변환",
            "컬러 → HSV 변환",
            "Flip (뒤집기)",
            "Pyramid Down",
            "Resize (리사이즈)"});
			this.comboBox1.Location = new System.Drawing.Point(15, 27);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(257, 26);
			this.comboBox1.TabIndex = 13;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.button2);
			this.groupBox1.Controls.Add(this.comboBox1);
			this.groupBox1.Location = new System.Drawing.Point(544, 435);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(287, 159);
			this.groupBox1.TabIndex = 14;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "이미지 변환 방법 선택";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(162, 92);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(81, 41);
			this.button2.TabIndex = 14;
			this.button2.Text = "적용";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// cbDlMode
			// 
			this.cbDlMode.FormattingEnabled = true;
			this.cbDlMode.Items.AddRange(new object[] {
            "Det",
            "Seg",
            "Cla"});
			this.cbDlMode.Location = new System.Drawing.Point(31, 492);
			this.cbDlMode.Name = "cbDlMode";
			this.cbDlMode.Size = new System.Drawing.Size(151, 26);
			this.cbDlMode.TabIndex = 15;
			// 
			// tbMinArea
			// 
			this.tbMinArea.Location = new System.Drawing.Point(31, 559);
			this.tbMinArea.Name = "tbMinArea";
			this.tbMinArea.Size = new System.Drawing.Size(151, 28);
			this.tbMinArea.TabIndex = 16;
			// 
			// btnRunDl
			// 
			this.btnRunDl.Location = new System.Drawing.Point(243, 548);
			this.btnRunDl.Name = "btnRunDl";
			this.btnRunDl.Size = new System.Drawing.Size(153, 47);
			this.btnRunDl.TabIndex = 17;
			this.btnRunDl.Text = "입력 완료";
			this.btnRunDl.UseVisualStyleBackColor = true;
			this.btnRunDl.Click += new System.EventHandler(this.btnRunDl_Click);
			// 
			// btnSelectModel
			// 
			this.btnSelectModel.Location = new System.Drawing.Point(243, 492);
			this.btnSelectModel.Name = "btnSelectModel";
			this.btnSelectModel.Size = new System.Drawing.Size(153, 42);
			this.btnSelectModel.TabIndex = 18;
			this.btnSelectModel.Text = "모델 선택 버튼";
			this.btnSelectModel.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1223, 888);
			this.Controls.Add(this.btnSelectModel);
			this.Controls.Add(this.btnRunDl);
			this.Controls.Add(this.tbMinArea);
			this.Controls.Add(this.cbDlMode);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.btnRedo);
			this.Controls.Add(this.btnUndo);
			this.Controls.Add(this.btnDrawEllipse);
			this.Controls.Add(this.btnDrawRect);
			this.Controls.Add(this.btnDrawLine);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cbLineColor);
			this.Controls.Add(this.cbLineThickness);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.groupBox1);
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ComboBox cbLineThickness;
		private System.Windows.Forms.ComboBox cbLineColor;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnDrawLine;
		private System.Windows.Forms.Button btnDrawRect;
		private System.Windows.Forms.Button btnDrawEllipse;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem 파일ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 열기ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 열기ToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem 저장ToolStripMenuItem;
		private System.Windows.Forms.Button btnUndo;
		private System.Windows.Forms.Button btnRedo;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ComboBox cbDlMode;
		private System.Windows.Forms.TextBox tbMinArea;
		private System.Windows.Forms.Button btnRunDl;
		private System.Windows.Forms.Button btnSelectModel;
	}
}


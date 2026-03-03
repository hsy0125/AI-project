using System;
using System.Collections.Generic;
using System.Drawing;                 // Color, Bitmap, Pen, Graphics 등 사용
using System.Windows.Forms;
using OpenCvSharp;                    // Mat, Cv2 등
using OpenCvSharp.Extensions;         // BitmapConverter

namespace ExProject_Console
{
	public partial class Form1 : System.Windows.Forms.Form
	{
		// ===============================
		// (0) 그림판용 Enum
		// ===============================
		private enum DrawColor : int
		{
			ColorBlack = 0,
			ColorRed,
			ColorOrange,
			ColorYellow,
			ColorGreen,
			ColorBlue,
			ColorPurple,
			ColorWhite
		}

		private enum DrawType
		{
			DrawNone = 0,
			DrawLine,
			DrawRectangle,
			DrawCircle
		}

		// ===============================
		// (0-2) OpenCV 변환용 Enum
		// ===============================
		private enum TransformType
		{
			Gray = 0,
			Hsv,
			Flip,
			PyrDown,
			Resize
		}

		// ===============================
		// (1) 그림판 상태 (Point 충돌 방지: System.Drawing.Point 풀네임)
		// ===============================
		private System.Drawing.Point _startPos;
		private System.Drawing.Point _currentPos;
		private bool _isDrawing = false;

		private int _lineThickness = 2;
		private DrawType _drawType = DrawType.DrawNone;

		private System.Drawing.Bitmap _canvas;
		private Stack<System.Drawing.Bitmap> _undoStack = new Stack<System.Drawing.Bitmap>();
		private Stack<System.Drawing.Bitmap> _redoStack = new Stack<System.Drawing.Bitmap>();

		// ===============================
		// (1-2) OpenCV 상태
		// ===============================
		private bool _isImageMode = false;
		private Mat _srcMat;
		private Mat _dstMat;
		private string _imagePath;

		// ===============================
		// (2) 공통 컨트롤
		// ===============================
		private PictureBox pictureBox1;

		private OpenFileDialog openFileDialog1;
		private SaveFileDialog saveFileDialog1;

		// ===============================
		// (3) 그림판 컨트롤
		// ===============================
		private ComboBox cbLineThickness;
		private ComboBox cbLineColor;

		private Button btnDrawLine;
		private Button btnDrawRect;
		private Button btnDrawEllipse;

		private Button btnUndo;
		private Button btnRedo;
		private Button btnNewCanvas;

		// ===============================
		// (4) OpenCV 변환 컨트롤
		// ===============================
		private Button btnImageOpen;
		private TextBox txtImagePath;
		private ComboBox cbTransform;
		private Button btnApply;
		private Button btnSave;

		private GroupBox groupOptions;

		private GroupBox groupFlip;
		private RadioButton rbFlipH;
		private RadioButton rbFlipV;
		private RadioButton rbFlipHV;

		private GroupBox groupResize;
		private NumericUpDown nudW;
		private NumericUpDown nudH;

		// ===============================
		// (5) 생성자
		// ===============================
		public Form1()
		{
			BuildUi();
			HookEvents();
			InitCanvas();
		}

		// ===============================
		// (6) UI 구성 (Size/Point 충돌 방지: System.Drawing.Size/Point 풀네임)
		// ===============================
		private void BuildUi()
		{
			this.Text = "그림판 + OpenCV 이미지 변환";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.ClientSize = new System.Drawing.Size(1020, 620);
			this.KeyPreview = true;

			openFileDialog1 = new OpenFileDialog();
			openFileDialog1.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*";

			saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "PNG|*.png|JPG|*.jpg;*.jpeg|BMP|*.bmp|All Files|*.*";

			// ===== PictureBox =====
			pictureBox1 = new PictureBox();
			pictureBox1.Location = new System.Drawing.Point(20, 20);
			pictureBox1.Size = new System.Drawing.Size(520, 420);
			pictureBox1.BorderStyle = BorderStyle.FixedSingle;
			pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.Controls.Add(pictureBox1);

			// ===== 그림판 영역(오른쪽 상단) =====
			int px = 560;
			int py = 20;

			Label lbl1 = new Label();
			lbl1.Text = "그림판";
			lbl1.Font = new System.Drawing.Font(lbl1.Font, FontStyle.Bold);
			lbl1.AutoSize = true;
			lbl1.Location = new System.Drawing.Point(px, py);
			this.Controls.Add(lbl1);

			Label lblThickness = new Label();
			lblThickness.Text = "선 두께";
			lblThickness.AutoSize = true;
			lblThickness.Location = new System.Drawing.Point(px, py + 35);
			this.Controls.Add(lblThickness);

			cbLineThickness = new ComboBox();
			cbLineThickness.DropDownStyle = ComboBoxStyle.DropDownList;
			cbLineThickness.Location = new System.Drawing.Point(px + 70, py + 30);
			cbLineThickness.Size = new System.Drawing.Size(120, 24);
			cbLineThickness.Items.AddRange(new object[] { "1", "2", "3", "5", "8" });
			cbLineThickness.SelectedIndex = 1;
			this.Controls.Add(cbLineThickness);

			Label lblColor = new Label();
			lblColor.Text = "선 색상";
			lblColor.AutoSize = true;
			lblColor.Location = new System.Drawing.Point(px, py + 70);
			this.Controls.Add(lblColor);

			cbLineColor = new ComboBox();
			cbLineColor.DropDownStyle = ComboBoxStyle.DropDownList;
			cbLineColor.Location = new System.Drawing.Point(px + 70, py + 65);
			cbLineColor.Size = new System.Drawing.Size(120, 24);
			cbLineColor.Items.AddRange(new object[]
			{
				"Black", "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "White"
			});
			cbLineColor.SelectedIndex = 0;
			this.Controls.Add(cbLineColor);

			btnUndo = new Button();
			btnUndo.Text = "실행취소";
			btnUndo.Location = new System.Drawing.Point(px + 220, py + 30);
			btnUndo.Size = new System.Drawing.Size(90, 30);
			this.Controls.Add(btnUndo);

			btnRedo = new Button();
			btnRedo.Text = "되돌리기";
			btnRedo.Location = new System.Drawing.Point(px + 220, py + 65);
			btnRedo.Size = new System.Drawing.Size(90, 30);
			this.Controls.Add(btnRedo);

			btnNewCanvas = new Button();
			btnNewCanvas.Text = "새 캔버스";
			btnNewCanvas.Location = new System.Drawing.Point(px + 320, py + 30);
			btnNewCanvas.Size = new System.Drawing.Size(90, 65);
			this.Controls.Add(btnNewCanvas);

			btnDrawLine = new Button();
			btnDrawLine.Text = "선 그리기";
			btnDrawLine.Location = new System.Drawing.Point(px, py + 110);
			btnDrawLine.Size = new System.Drawing.Size(190, 45);
			this.Controls.Add(btnDrawLine);

			btnDrawRect = new Button();
			btnDrawRect.Text = "사각형 그리기";
			btnDrawRect.Location = new System.Drawing.Point(px, py + 160);
			btnDrawRect.Size = new System.Drawing.Size(190, 45);
			this.Controls.Add(btnDrawRect);

			btnDrawEllipse = new Button();
			btnDrawEllipse.Text = "원형 그리기";
			btnDrawEllipse.Location = new System.Drawing.Point(px, py + 210);
			btnDrawEllipse.Size = new System.Drawing.Size(190, 45);
			this.Controls.Add(btnDrawEllipse);

			// ===== OpenCV 변환 영역(오른쪽 하단) =====
			int tx = 560;
			int ty = 300;

			Label lbl2 = new Label();
			lbl2.Text = "OpenCV 이미지 변환";
			lbl2.Font = new System.Drawing.Font(lbl2.Font, FontStyle.Bold);
			lbl2.AutoSize = true;
			lbl2.Location = new System.Drawing.Point(tx, ty);
			this.Controls.Add(lbl2);

			btnImageOpen = new Button();
			btnImageOpen.Text = "이미지 열기";
			btnImageOpen.Location = new System.Drawing.Point(tx, ty + 30);
			btnImageOpen.Size = new System.Drawing.Size(120, 32);
			this.Controls.Add(btnImageOpen);

			txtImagePath = new TextBox();
			txtImagePath.Location = new System.Drawing.Point(tx + 130, ty + 35);
			txtImagePath.Size = new System.Drawing.Size(300, 24);
			txtImagePath.ReadOnly = true;
			this.Controls.Add(txtImagePath);

			cbTransform = new ComboBox();
			cbTransform.DropDownStyle = ComboBoxStyle.DropDownList;
			cbTransform.Location = new System.Drawing.Point(tx, ty + 75);
			cbTransform.Size = new System.Drawing.Size(430, 24);
			cbTransform.Items.Add("컬러 → 모노(Grayscale)");
			cbTransform.Items.Add("컬러 → HSV");
			cbTransform.Items.Add("Flip (뒤집기)");
			cbTransform.Items.Add("Pyramid Down");
			cbTransform.Items.Add("Resize (리사이즈)");
			cbTransform.SelectedIndex = 0;
			this.Controls.Add(cbTransform);

			groupOptions = new GroupBox();
			groupOptions.Text = "옵션";
			groupOptions.Location = new System.Drawing.Point(tx, ty + 110);
			groupOptions.Size = new System.Drawing.Size(430, 120);
			this.Controls.Add(groupOptions);

			// Flip 옵션
			groupFlip = new GroupBox();
			groupFlip.Text = "Flip 방향";
			groupFlip.Location = new System.Drawing.Point(10, 25);
			groupFlip.Size = new System.Drawing.Size(410, 55);

			rbFlipH = new RadioButton() { Text = "수평(좌우)", Location = new System.Drawing.Point(10, 22), AutoSize = true };
			rbFlipV = new RadioButton() { Text = "수직(상하)", Location = new System.Drawing.Point(150, 22), AutoSize = true };
			rbFlipHV = new RadioButton() { Text = "양방향", Location = new System.Drawing.Point(290, 22), AutoSize = true };
			rbFlipH.Checked = true;

			groupFlip.Controls.Add(rbFlipH);
			groupFlip.Controls.Add(rbFlipV);
			groupFlip.Controls.Add(rbFlipHV);
			groupOptions.Controls.Add(groupFlip);

			// Resize 옵션
			groupResize = new GroupBox();
			groupResize.Text = "Resize 비율(%)";
			groupResize.Location = new System.Drawing.Point(10, 25);
			groupResize.Size = new System.Drawing.Size(410, 55);

			Label lblW = new Label() { Text = "Width", Location = new System.Drawing.Point(10, 24), AutoSize = true };
			nudW = new NumericUpDown() { Location = new System.Drawing.Point(60, 20), Width = 70, Minimum = 1, Maximum = 500, Value = 100 };

			Label lblH = new Label() { Text = "Height", Location = new System.Drawing.Point(160, 24), AutoSize = true };
			nudH = new NumericUpDown() { Location = new System.Drawing.Point(215, 20), Width = 70, Minimum = 1, Maximum = 500, Value = 100 };

			groupResize.Controls.Add(lblW);
			groupResize.Controls.Add(nudW);
			groupResize.Controls.Add(lblH);
			groupResize.Controls.Add(nudH);
			groupOptions.Controls.Add(groupResize);

			btnApply = new Button();
			btnApply.Text = "적용";
			btnApply.Location = new System.Drawing.Point(tx, ty + 240);
			btnApply.Size = new System.Drawing.Size(120, 32);
			this.Controls.Add(btnApply);

			btnSave = new Button();
			btnSave.Text = "결과 저장";
			btnSave.Location = new System.Drawing.Point(tx + 130, ty + 240);
			btnSave.Size = new System.Drawing.Size(150, 32);
			this.Controls.Add(btnSave);

			UpdateOptionPanels();
		}

		// ===============================
		// (7) 이벤트 연결
		// ===============================
		private void HookEvents()
		{
			cbLineThickness.SelectedIndexChanged += cbLineThickness_SelectedIndexChanged;

			btnDrawLine.Click += btnDrawLine_Click;
			btnDrawRect.Click += btnDrawRect_Click;
			btnDrawEllipse.Click += btnDrawEllipse_Click;

			btnUndo.Click += btnUndo_Click;
			btnRedo.Click += btnRedo_Click;
			btnNewCanvas.Click += btnNewCanvas_Click;

			pictureBox1.MouseDown += pictureBox1_MouseDown;
			pictureBox1.MouseMove += pictureBox1_MouseMove;
			pictureBox1.MouseUp += pictureBox1_MouseUp;
			pictureBox1.Paint += pictureBox1_Paint;

			this.KeyDown += Form1_KeyDown;

			btnImageOpen.Click += btnImageOpen_Click;
			cbTransform.SelectedIndexChanged += cbTransform_SelectedIndexChanged;
			btnApply.Click += btnApply_Click;
			btnSave.Click += btnSave_Click;
		}

		// ===============================
		// (8) 캔버스 초기화
		// ===============================
		private void InitCanvas()
		{
			_canvas?.Dispose();
			_canvas = new System.Drawing.Bitmap(pictureBox1.Width, pictureBox1.Height);

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_canvas))
			{
				g.Clear(System.Drawing.Color.White);
			}

			pictureBox1.Image?.Dispose();
			pictureBox1.Image = (System.Drawing.Bitmap)_canvas.Clone();

			_undoStack.Clear();
			_redoStack.Clear();
			UpdateUndoRedoButtons();

			_isImageMode = false;
			_srcMat?.Dispose(); _srcMat = null;
			_dstMat?.Dispose(); _dstMat = null;
		}

		// ===============================
		// (9) 그림판 UI 이벤트
		// ===============================
		private void cbLineThickness_SelectedIndexChanged(object sender, EventArgs e)
		{
			_lineThickness = Convert.ToInt32(cbLineThickness.SelectedItem);
		}

		private void btnDrawLine_Click(object sender, EventArgs e) { _drawType = DrawType.DrawLine; }
		private void btnDrawRect_Click(object sender, EventArgs e) { _drawType = DrawType.DrawRectangle; }
		private void btnDrawEllipse_Click(object sender, EventArgs e) { _drawType = DrawType.DrawCircle; }

		private void btnUndo_Click(object sender, EventArgs e) { Undo(); }
		private void btnRedo_Click(object sender, EventArgs e) { Redo(); }
		private void btnNewCanvas_Click(object sender, EventArgs e) { InitCanvas(); }

		// ===============================
		// (10) 그림판 마우스 드로잉
		// ===============================
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			if (_isImageMode) return;
			if (e.Button != MouseButtons.Left) return;
			if (_drawType == DrawType.DrawNone) return;

			_startPos = e.Location;
			_currentPos = e.Location;
			_isDrawing = true;
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isImageMode) return;
			if (!_isDrawing) return;

			_currentPos = e.Location;
			pictureBox1.Invalidate();
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			if (_isImageMode) return;
			if (e.Button != MouseButtons.Left) return;
			if (!_isDrawing) return;

			_isDrawing = false;
			if (_canvas == null) return;

			PushUndoSnapshot();

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_canvas))
			using (System.Drawing.Pen pen = new System.Drawing.Pen(GetSelColor(), _lineThickness))
			{
				if (_drawType == DrawType.DrawLine)
					g.DrawLine(pen, _startPos, _currentPos);
				else if (_drawType == DrawType.DrawRectangle)
					g.DrawRectangle(pen, GetRectangle(_startPos, _currentPos));
				else if (_drawType == DrawType.DrawCircle)
					g.DrawEllipse(pen, GetRectangle(_startPos, _currentPos));
			}

			pictureBox1.Image?.Dispose();
			pictureBox1.Image = (System.Drawing.Bitmap)_canvas.Clone();
			pictureBox1.Invalidate();
		}

		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			if (_isImageMode) return;
			if (!_isDrawing) return;

			using (System.Drawing.Pen pen = new System.Drawing.Pen(GetSelColor(), _lineThickness))
			{
				if (_drawType == DrawType.DrawLine)
					e.Graphics.DrawLine(pen, _startPos, _currentPos);
				else if (_drawType == DrawType.DrawRectangle)
					e.Graphics.DrawRectangle(pen, GetRectangle(_startPos, _currentPos));
				else if (_drawType == DrawType.DrawCircle)
					e.Graphics.DrawEllipse(pen, GetRectangle(_startPos, _currentPos));
			}
		}

		// ===============================
		// (11) Undo / Redo
		// ===============================
		private void PushUndoSnapshot()
		{
			_undoStack.Push((System.Drawing.Bitmap)_canvas.Clone());
			_redoStack.Clear();
			UpdateUndoRedoButtons();
		}

		private void Undo()
		{
			if (_undoStack.Count == 0) return;

			_redoStack.Push((System.Drawing.Bitmap)_canvas.Clone());

			_canvas.Dispose();
			_canvas = _undoStack.Pop();

			pictureBox1.Image?.Dispose();
			pictureBox1.Image = (System.Drawing.Bitmap)_canvas.Clone();
			pictureBox1.Invalidate();

			UpdateUndoRedoButtons();
		}

		private void Redo()
		{
			if (_redoStack.Count == 0) return;

			_undoStack.Push((System.Drawing.Bitmap)_canvas.Clone());

			_canvas.Dispose();
			_canvas = _redoStack.Pop();

			pictureBox1.Image?.Dispose();
			pictureBox1.Image = (System.Drawing.Bitmap)_canvas.Clone();
			pictureBox1.Invalidate();

			UpdateUndoRedoButtons();
		}

		private void UpdateUndoRedoButtons()
		{
			btnUndo.Enabled = _undoStack.Count > 0;
			btnRedo.Enabled = _redoStack.Count > 0;
		}

		// ===============================
		// (12) 단축키(그림판)
		// ===============================
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.N)
			{
				InitCanvas();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.Z)
			{
				Undo();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.Y)
			{
				Redo();
				e.SuppressKeyPress = true;
			}
		}

		// ===============================
		// (13) OpenCV 변환 이벤트
		// ===============================
		private void btnImageOpen_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

			_imagePath = openFileDialog1.FileName;
			txtImagePath.Text = _imagePath;

			_srcMat?.Dispose();
			_dstMat?.Dispose();
			_dstMat = null;

			_srcMat = Cv2.ImRead(_imagePath, ImreadModes.Color);
			if (_srcMat.Empty())
			{
				MessageBox.Show("이미지를 불러오지 못했습니다.");
				return;
			}

			_isImageMode = true;

			pictureBox1.Image?.Dispose();
			pictureBox1.Image = BitmapConverter.ToBitmap(_srcMat);
		}

		private void cbTransform_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateOptionPanels();
		}

		private void UpdateOptionPanels()
		{
			TransformType type = (TransformType)cbTransform.SelectedIndex;
			groupFlip.Visible = (type == TransformType.Flip);
			groupResize.Visible = (type == TransformType.Resize);
		}

		private void btnApply_Click(object sender, EventArgs e)
		{
			if (_srcMat == null)
			{
				MessageBox.Show("먼저 이미지를 열어주세요.");
				return;
			}

			TransformType type = (TransformType)cbTransform.SelectedIndex;

			// 기준 이미지를 결정
			Mat baseMat = _dstMat != null ? _dstMat : _srcMat;

			Mat newMat = new Mat();

			switch (type)
			{
				case TransformType.Gray:
					Cv2.CvtColor(baseMat, newMat, ColorConversionCodes.BGR2GRAY);
					break;

				case TransformType.Hsv:
					Cv2.CvtColor(baseMat, newMat, ColorConversionCodes.BGR2HSV);
					break;

				case TransformType.Flip:
					FlipMode mode = FlipMode.Y;

					if (rbFlipH.Checked) mode = FlipMode.Y;
					else if (rbFlipV.Checked) mode = FlipMode.X;
					else if (rbFlipHV.Checked) mode = FlipMode.XY;

					Cv2.Flip(baseMat, newMat, mode);
					break;

				case TransformType.PyrDown:
					Cv2.PyrDown(baseMat, newMat);
					break;

				case TransformType.Resize:
					double wp = (double)nudW.Value / 100.0;
					double hp = (double)nudH.Value / 100.0;

					int newW = (int)(baseMat.Width * wp);
					int newH = (int)(baseMat.Height * hp);

					if (newW < 1) newW = 1;
					if (newH < 1) newH = 1;

					Cv2.Resize(baseMat, newMat, new OpenCvSharp.Size(newW, newH));
					break;
			}

			// 기존 결과 해제
			_dstMat?.Dispose();
			_dstMat = newMat;

			pictureBox1.Image?.Dispose();
			pictureBox1.Image = BitmapConverter.ToBitmap(_dstMat);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			if (_dstMat == null || _dstMat.Empty())
			{
				MessageBox.Show("저장할 결과가 없습니다. 먼저 적용하세요.");
				return;
			}

			if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
			Cv2.ImWrite(saveFileDialog1.FileName, _dstMat);
		}

		// ===============================
		// (14) 유틸 (Rectangle 충돌 방지: System.Drawing.Rectangle 풀네임)
		// ===============================
		private System.Drawing.Color GetSelColor()
		{
			DrawColor drawColor = (DrawColor)cbLineColor.SelectedIndex;

			switch (drawColor)
			{
				case DrawColor.ColorBlack: return System.Drawing.Color.Black;
				case DrawColor.ColorRed: return System.Drawing.Color.Red;
				case DrawColor.ColorOrange: return System.Drawing.Color.Orange;
				case DrawColor.ColorYellow: return System.Drawing.Color.Yellow;
				case DrawColor.ColorGreen: return System.Drawing.Color.Green;
				case DrawColor.ColorBlue: return System.Drawing.Color.Blue;
				case DrawColor.ColorPurple: return System.Drawing.Color.Purple;
				case DrawColor.ColorWhite: return System.Drawing.Color.White;
				default: return System.Drawing.Color.Black;
			}
		}

		private System.Drawing.Rectangle GetRectangle(System.Drawing.Point p1, System.Drawing.Point p2)
		{
			return new System.Drawing.Rectangle(
				Math.Min(p1.X, p2.X),
				Math.Min(p1.Y, p2.Y),
				Math.Abs(p2.X - p1.X),
				Math.Abs(p2.Y - p1.Y));
		}

		// ===============================
		// (15) 종료 정리
		// ===============================
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			_canvas?.Dispose();
			_srcMat?.Dispose();
			_dstMat?.Dispose();
			base.OnFormClosed(e);
		}
	}
}
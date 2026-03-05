using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;              // Mat, Cv2
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing.Drawing2D;

using SaigeVision.Net.V2;
using SaigeVision.Net.V2.Classification;
using SaigeVision.Net.V2.Detection;
using SaigeVision.Net.V2.Segmentation;
using System.Drawing.Imaging;

using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;
/*
 * 그림판 만들기 (기능 정리)
 * (1) 파일 - (새로 만들기, 열기, 저장)
 * (2) 선 두께 / 색상 선택
 * (3) 선 그리기 / 사각형 그리기 / 원형 그리기
 * (4) 실행취소 / 되돌리기 (단축키: Ctrl+N / Ctrl+O / Ctrl+S / Ctrl+Z / Ctrl+Y)
 */

namespace WindowsFormsAppPaint_2_25
{
	public partial class Form1 : Form
	{
		// ===============================
		// (0) Enum 정의
		// ===============================

		// 딥러닝 용
		private enum DlMode
		{
			Det = 0,
			Seg = 1,
			Cla = 2
		}
		enum DrawColor : int
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

		enum DrawType
		{
			DrawNone = 0,
			DrawLine,
			DrawRectangle,
			DrawCircle
		}
		private enum TransformType
		{
			Gray = 0,
			Hsv = 1,
			Flip = 2,
			PyrDown = 3,
			Resize = 4
		}

		// ===============================
		// (1) 필드 변수 영역 (State)
		// ===============================
		// 시작 점
		private System.Drawing.Point _startPos;
		private System.Drawing.Point _currentPos;

		private bool _isDrawing = false;

		private int _lineThickness = 2;
		private DrawType _drawType = DrawType.DrawNone;

		private Bitmap _canvas;
		private Stack<Bitmap> _undoStack = new Stack<Bitmap>();
		private Stack<Bitmap> _redoStack = new Stack<Bitmap>();


		// 캔버스(누적 그림) + Undo/Redo 스택
		private bool _isImageMode = false;   // 이미지 모드면 그림판 입력 막기
		private string _imagePath = null;

		private Mat _srcMat = null;          // 처음 불러온 원본
		private Mat _currentMat = null;      // ★ 누적 변환 대상(Apply 누를 때마다 갱신)

		// 옵션 UI(디자이너에 없어도 groupBox1 안에 코드로 생성)
		private Panel _panelFlip;
		private RadioButton _rbFlipH;
		private RadioButton _rbFlipV;
		private RadioButton _rbFlipHV;

		private Panel _panelResize;
		private NumericUpDown _nudW;
		private NumericUpDown _nudH;
		private Label _lblW;
		private Label _lblH;

		// seg/det/cla 공통 옵션
		private SegmentationResult _lastSegResult;
		private DetectionResult _lastDetResult;
		private ClassificationResult _lastClsResult;

		// 모델 경로 저장용 필드(원하는 방식으로 바꿔도 됨)
		private string _detModelPath = "";
		private string _segModelPath = "";
		private string _claModelPath = "";

		private string GetModelPathByMode(DlMode mode)
		{
			switch (mode)
			{
				case DlMode.Det: return _detModelPath;
				case DlMode.Seg: return _segModelPath;
				case DlMode.Cla: return _claModelPath;
				default: return "";
			}
		}
		// ===============================
		// (2) 생성자 / 초기화
		// ===============================
		public Form1()
		{
			InitializeComponent();
			InitCanvas(); // 시작 시 캔버스 생성
			
			this.KeyPreview = true;
			this.KeyDown += Form1_KeyDown;
			// (디자인에서 추가한 OpenCV UI 연결)
			// button1: 이미지 선택, button2: 적용, comboBox1: 기능선택
			//if (button1 != null) button1.Click += button1_Click;
			//if (button2 != null) button2.Click += button2_Click;
			//if (comboBox1 != null) comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

			cbDlMode.SelectedIndex = 0;

			// 옵션 패널 생성(Flip/Resize)
			BuildOptionPanels();
			UpdateOptionPanels();

		}

		private void ApplyMatToCanvas(Mat mat)
		{
			if (mat == null || mat.Empty()) return;

			// Mat -> Bitmap
			Bitmap bmp = BitmapConverter.ToBitmap(mat);

			// 캔버스 크기에 맞게 새 캔버스 생성
			_canvas?.Dispose();
			_canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);

			using (Graphics g = Graphics.FromImage(_canvas))
			{
				g.Clear(Color.White);
				g.DrawImage(bmp, 0, 0, pictureBox1.Width, pictureBox1.Height);
			}

			bmp.Dispose();

			pictureBox1.Image = _canvas;
			pictureBox1.Invalidate();

			// 이제 "이미지 위에 그리기" 가능해야 하니까 막지 않는다
			_isImageMode = false;
		}

		// 캔버스 초기화(새로 만들기)
		private void InitCanvas()
		{
			_canvas?.Dispose();
			_canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);

			using (Graphics g = Graphics.FromImage(_canvas))
			{
				g.Clear(Color.White);
			}

			pictureBox1.Image = _canvas;

			_undoStack.Clear();
			_redoStack.Clear();

			UpdateUndoRedoButtons();
			pictureBox1.Invalidate();

			// 그림판으로 돌아오면 이미지 모드 해제(원하면 유지해도 됨)
			_isImageMode = false;
			

			if (_currentMat != null) { _currentMat.Dispose(); _currentMat = null; }
			if (_srcMat != null) { _srcMat.Dispose(); _srcMat = null; }

			_imagePath = null;
			if (textBox1 != null) textBox1.Text = "";
		}
		private Mat EnsureBgr(Mat src)
		{
			if (src == null || src.Empty())
				return src;

			// 이미 3채널(BGR)
			if (src.Channels() == 3)
				return src;

			// 1채널(Gray) -> BGR
			if (src.Channels() == 1)
			{
				Mat bgr = new Mat();
				Cv2.CvtColor(src, bgr, ColorConversionCodes.GRAY2BGR);
				return bgr;
			}

			// 4채널(BGRA) -> BGR
			if (src.Channels() == 4)
			{
				Mat bgr = new Mat();
				Cv2.CvtColor(src, bgr, ColorConversionCodes.BGRA2BGR);
				return bgr;
			}

			// 그 외는 그냥 복사
			return src.Clone();
		}

		// ===============================
		// (3) 파일 메뉴 기능 (새로 만들기 / 열기 / 저장)
		// ===============================
		private void 열기ToolStripMenuItem_Click(object sender, EventArgs e) // (실제로는 새로 만들기 역할)
		{
			InitCanvas();
		}

		private void 열기ToolStripMenuItem1_Click(object sender, EventArgs e) // 파일 열기
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
				//{
				//	using (var loaded = new Bitmap(openFileDialog1.FileName))
				//	{
				//		_canvas?.Dispose();
				//		_canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);

				//		using (Graphics g = Graphics.FromImage(_canvas))
				//		{
				//			g.Clear(Color.White);
				//			// 캔버스 크기에 맞춰 이미지 그려넣기
				//			g.DrawImage(loaded, 0, 0, pictureBox1.Width, pictureBox1.Height);
				//		}

				//		pictureBox1.Image = _canvas;
				//	}

				//	_undoStack.Clear();
				//	_redoStack.Clear();
				//	UpdateUndoRedoButtons();
				//	pictureBox1.Invalidate();
				//}

				LoadImage(openFileDialog1.FileName);

		}
		// 열기 메뉴에서 이미지 선택 시 Mat으로도 로드해서 변환 기능에 활용할 수 있도록 하는 메서드	
		private void LoadImage(string path)
		{
			_srcMat?.Dispose();
			_currentMat?.Dispose();

			_srcMat = Cv2.ImRead(path, ImreadModes.Color);
			if (_srcMat.Empty())
			{
				MessageBox.Show("이미지 로드 실패");
				return;
			}

			_currentMat = _srcMat.Clone();

			// 여기 한 줄로 끝
			ApplyMatToCanvas(_currentMat);

			_undoStack.Clear();
			_redoStack.Clear();
			UpdateUndoRedoButtons();
		}

		private void 저장ToolStripMenuItem_Click(object sender, EventArgs e) // 저장
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				if (_isImageMode && _currentMat != null && !_currentMat.Empty())
				{
					Cv2.ImWrite(saveFileDialog1.FileName, _currentMat);
					return;
				}
				_canvas?.Save(saveFileDialog1.FileName);
			}
		}

		// ===============================
		// (4) UI 입력(ComboBox / Button) 이벤트
		// ===============================
		private void cbLineThickness_SelectedIndexChanged(object sender, EventArgs e)
		{
			_lineThickness = Convert.ToInt32(cbLineThickness.SelectedItem);
		}


		private void btnDrawLine_Click(object sender, EventArgs e)
		{
			_drawType = DrawType.DrawLine;
		}

		private void btnDrawRect_Click(object sender, EventArgs e)
		{
			_drawType = DrawType.DrawRectangle;
		}

		private void btnDrawEllipse_Click(object sender, EventArgs e)
		{
			_drawType = DrawType.DrawCircle;
		}

		private void btnUndo_Click(object sender, EventArgs e)
		{
			Undo();
		}

		private void btnRedo_Click(object sender, EventArgs e)
		{
			Redo();
		}

		// ===============================
		// (5) 마우스 드로잉 (Down / Move / Up) + Paint(미리보기)
		// ===============================
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			if (_isImageMode) return;                 // ★ 추가: 이미지 모드면 그림판 입력 막기
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

			using (Graphics g = Graphics.FromImage(_canvas))
			using (Pen pen = new Pen(GetSelColor(), _lineThickness))
			{
				switch (_drawType)
				{
					case DrawType.DrawLine:
						g.DrawLine(pen, _startPos, _currentPos);
						break;

					case DrawType.DrawRectangle:
						g.DrawRectangle(pen, GetRectangle(_startPos, _currentPos));
						break;

					case DrawType.DrawCircle:
						g.DrawEllipse(pen, GetRectangle(_startPos, _currentPos));
						break;
				}
			}

			pictureBox1.Invalidate();
		}

		// PictureBox 그리기(드래그 중 미리보기만)
		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			if (_isImageMode) return;
			if (!_isDrawing) return;

			using (Pen pen = new Pen(GetSelColor(), _lineThickness))
			{
				switch (_drawType)
				{
					case DrawType.DrawLine:
						e.Graphics.DrawLine(pen, _startPos, _currentPos);
						break;

					case DrawType.DrawRectangle:
						var rect = GetRectangle(_startPos, _currentPos);
						e.Graphics.DrawRectangle(pen, rect);
						break;

					case DrawType.DrawCircle:
						var ellipse = GetRectangle(_startPos, _currentPos);
						e.Graphics.DrawEllipse(pen, ellipse);
						break;
				}
			}
		}

		// ===============================
		// (6) Undo / Redo 로직
		// ===============================

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.N)
			{
				// Ctrl + N → 새로 만들기
				InitCanvas();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.O)
			{
				// Ctrl + O → 열기
				열기ToolStripMenuItem1_Click(null, null);
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.S)
			{
				// Ctrl + S → 저장
				저장ToolStripMenuItem_Click(null, null);
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.Z)
			{
				// Ctrl + Z → 실행취소
				Undo();
				e.SuppressKeyPress = true;
			}
			else if (e.Control && e.KeyCode == Keys.Y)
			{
				// Ctrl + Y → 다시 실행
				Redo();
				e.SuppressKeyPress = true;
			}
		}
		private void PushUndoSnapshot()
		{
			if (_canvas == null) return;

			_undoStack.Push((Bitmap)_canvas.Clone()); // 현재 상태 저장
			_redoStack.Clear();                       // 새 작업 시작하면 redo 초기화
			UpdateUndoRedoButtons();
		}

		private void Undo()
		{
			if (_undoStack.Count == 0 || _canvas == null) return;

			_redoStack.Push((Bitmap)_canvas.Clone());

			_canvas.Dispose();
			_canvas = _undoStack.Pop();

			pictureBox1.Image = _canvas;
			pictureBox1.Invalidate();

			UpdateUndoRedoButtons();
		}

		private void Redo()
		{
			if (_redoStack.Count == 0 || _canvas == null) return;

			_undoStack.Push((Bitmap)_canvas.Clone());

			_canvas.Dispose();
			_canvas = _redoStack.Pop();

			pictureBox1.Image = _canvas;
			pictureBox1.Invalidate();

			UpdateUndoRedoButtons();
		}

		private void UpdateUndoRedoButtons()
		{
			btnUndo.Enabled = _undoStack.Count > 0;
			btnRedo.Enabled = _redoStack.Count > 0;
		}

		// ===============================
		// (7) 유틸리티(색상/사각형 계산)
		// ===============================
		private Color GetSelColor()
		{
			Color clrLine = new Color();

			DrawColor drawColor = (DrawColor)cbLineColor.SelectedIndex;

			switch (drawColor)
			{
				case DrawColor.ColorBlack:
					clrLine = Color.Black; break;
				case DrawColor.ColorRed:
					clrLine = Color.Red; break;
				case DrawColor.ColorOrange:
					clrLine = Color.Orange; break;
				case DrawColor.ColorYellow:
					clrLine = Color.Yellow; break;
				case DrawColor.ColorGreen:
					clrLine = Color.Green; break;
				case DrawColor.ColorBlue:
					clrLine = Color.Blue; break;
				case DrawColor.ColorPurple:
					clrLine = Color.Purple; break;
				case DrawColor.ColorWhite:
					clrLine = Color.White; break;
				default:
					clrLine = Color.Black; break;
			}

			return clrLine;
		}

		private Rectangle GetRectangle(System.Drawing.Point p1, System.Drawing.Point p2)
		{
			return new Rectangle(
				Math.Min(p1.X, p2.X),
				Math.Min(p1.Y, p2.Y),
				Math.Abs(p2.X - p1.X),
				Math.Abs(p2.Y - p1.Y));
		}

		//선택 버튼

		private void button1_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
			_imagePath = openFileDialog1.FileName;
			if (textBox1 != null) { textBox1.Text = _imagePath; textBox1.ReadOnly = true; }

			LoadImage(_imagePath);
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateOptionPanels();
		}
		// 적용 버튼
		private void button2_Click(object sender, EventArgs e)
		{
			// ★ PictureBox에 이미지만 있고 _currentMat이 없을 때 자동 변환
			if ((_currentMat == null || _currentMat.Empty()) && pictureBox1.Image != null)
			{
				var bmp = pictureBox1.Image as Bitmap;
				if (bmp != null)
				{
					_currentMat?.Dispose();
					_currentMat = BitmapConverter.ToMat(bmp);
					_isImageMode = true;
				}
			}

			if (_currentMat == null || _currentMat.Empty())
			{
				MessageBox.Show("먼저 이미지를 선택하세요.");
				return;
			}

			if (comboBox1 == null || comboBox1.SelectedIndex < 0)
			{
				MessageBox.Show("변환 기능을 선택하세요.");
				return;
			}

			TransformType type = (TransformType)comboBox1.SelectedIndex;
			Mat dst = new Mat();

			switch (type)
			{
				case TransformType.Gray:
					// 이미 Gray면 그대로 복사
					if (_currentMat.Channels() == 1)
						dst = _currentMat.Clone();
					else
						Cv2.CvtColor(_currentMat, dst, ColorConversionCodes.BGR2GRAY);
					break;

				case TransformType.Hsv:
					using (Mat bgr = EnsureBgr(_currentMat))
					{
						Cv2.CvtColor(bgr, dst, ColorConversionCodes.BGR2HSV);
					}
					break;

				case TransformType.Flip:
					FlipMode mode = FlipMode.Y;
					if (_rbFlipV != null && _rbFlipV.Checked) mode = FlipMode.X;
					else if (_rbFlipHV != null && _rbFlipHV.Checked) mode = FlipMode.XY;

					Cv2.Flip(_currentMat, dst, mode);
					break;

				case TransformType.PyrDown:
					Cv2.PyrDown(_currentMat, dst);
					break;

				case TransformType.Resize:
					double wp = 1.0;
					double hp = 1.0;

					if (_nudW != null) wp = (double)_nudW.Value / 100.0;
					if (_nudH != null) hp = (double)_nudH.Value / 100.0;

					int newW = (int)(_currentMat.Width * wp);
					int newH = (int)(_currentMat.Height * hp);

					if (newW < 1) newW = 1;
					if (newH < 1) newH = 1;

					Cv2.Resize(_currentMat, dst, new OpenCvSharp.Size(newW, newH));
					break;
			}

			_currentMat.Dispose();
			_currentMat = dst;

			_isImageMode = true;
			ApplyMatToCanvas(_currentMat);
		}
		private void BuildOptionPanels()
		{
			if (groupBox1 == null) return;

			_panelFlip = new Panel();
			_panelFlip.Dock = DockStyle.Fill;
			_panelFlip.Visible = false;

			_rbFlipH = new RadioButton();
			_rbFlipH.Text = "수평(좌우)";
			_rbFlipH.AutoSize = true;
			_rbFlipH.Location = new System.Drawing.Point(10, 25);   // ★ 여기
			_rbFlipH.Checked = true;

			_rbFlipV = new RadioButton();
			_rbFlipV.Text = "수직(상하)";
			_rbFlipV.AutoSize = true;
			_rbFlipV.Location = new System.Drawing.Point(110, 25);  // ★ 여기

			_rbFlipHV = new RadioButton();
			_rbFlipHV.Text = "양방향";
			_rbFlipHV.AutoSize = true;
			_rbFlipHV.Location = new System.Drawing.Point(210, 25); // ★ 여기

			_panelFlip.Controls.Add(_rbFlipH);
			_panelFlip.Controls.Add(_rbFlipV);
			_panelFlip.Controls.Add(_rbFlipHV);

			_panelResize = new Panel();
			_panelResize.Dock = DockStyle.Fill;
			_panelResize.Visible = false;

			_lblW = new Label();
			_lblW.Text = "Width(%)";
			_lblW.AutoSize = true;
			_lblW.Location = new System.Drawing.Point(10, 25);      // ★ 여기

			_nudW = new NumericUpDown();
			_nudW.Minimum = 1;
			_nudW.Maximum = 500;
			_nudW.Value = 100;
			_nudW.Location = new System.Drawing.Point(80, 22);      // ★ 여기
			_nudW.Width = 70;

			_lblH = new Label();
			_lblH.Text = "Height(%)";
			_lblH.AutoSize = true;
			_lblH.Location = new System.Drawing.Point(170, 25);     // ★ 여기

			_nudH = new NumericUpDown();
			_nudH.Minimum = 1;
			_nudH.Maximum = 500;
			_nudH.Value = 100;
			_nudH.Location = new System.Drawing.Point(250, 22);     // ★ 여기
			_nudH.Width = 70;

			_panelResize.Controls.Add(_lblW);
			_panelResize.Controls.Add(_nudW);
			_panelResize.Controls.Add(_lblH);
			_panelResize.Controls.Add(_nudH);

			groupBox1.Controls.Add(_panelFlip);
			groupBox1.Controls.Add(_panelResize);
		}
		private void UpdateOptionPanels()
		{
			if (comboBox1 == null) return;

			TransformType type = (TransformType)comboBox1.SelectedIndex;

			if (_panelFlip != null) _panelFlip.Visible = (type == TransformType.Flip);
			if (_panelResize != null) _panelResize.Visible = (type == TransformType.Resize);
		}

		

		private void btnRunDl_Click(object sender, EventArgs e)
		{
			if (_canvas == null)
			{
				MessageBox.Show("이미지가 없습니다.");
				return;
			}

			// 현재 캔버스(그림 포함)를 Bitmap으로 전달
			Bitmap inputBmp = (Bitmap)_canvas.Clone();

			if (!int.TryParse(tbMinArea.Text, out int minArea))
			{
				MessageBox.Show("Area는 숫자로 입력하세요.");
				inputBmp.Dispose();
				return;
			}

			DlMode mode = (DlMode)cbDlMode.SelectedIndex;

			string modelPath = GetModelPathByMode(mode);
			if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
			{
				MessageBox.Show("선택된 모드의 모델 경로가 올바르지 않습니다.");
				inputBmp.Dispose();
				return;
			}

			try
			{
				switch (mode)
				{
					case DlMode.Det:
						RunDet(inputBmp, modelPath, minArea);
						break;

					case DlMode.Seg:
						RunSeg(inputBmp, modelPath, minArea);
						break;

					case DlMode.Cla:
						RunCla(inputBmp, modelPath);
						break;
				}
			}
			finally
			{
				inputBmp.Dispose();
			}
		}


		// ------------------------------
		// Saige SDK 실행 (공통)
		// ------------------------------
		private SrImage ToSrImage(Bitmap bmp)
		{
			// SrImage가 Bitmap 생성자를 지원하는 예제 기준
			// (너가 올린 Saige 예제에서 new SrImage(bmp) 사용했음)
			return new SrImage(bmp);
		}

		// ------------------------------
		// DET
		// ------------------------------
		private void RunDet(Bitmap inputBmp, string modelPath, int minArea)
		{
			if (inputBmp == null) return;

			using (var engine = new DetectionEngine(modelPath, 0))
			{
				DetectionOption option = engine.GetInferenceOption();
				option.CalcTime = true;
				engine.SetInferenceOption(option);

				using (SrImage sr = ToSrImage(inputBmp))
				{
					Stopwatch sw = Stopwatch.StartNew();
					DetectionResult result = engine.Inspection(sr);
					sw.Stop();

					_lastDetResult = result;

					using (Bitmap output = new Bitmap(inputBmp.Width, inputBmp.Height, PixelFormat.Format24bppRgb))
					{
						using (Graphics g = Graphics.FromImage(output))
						{
							g.DrawImage(inputBmp, 0, 0, output.Width, output.Height);
						}

						DrawDetResult_FilterByArea(result, output, minArea);
						ApplyBitmapToCanvas(output);
					}
				}
			}
		}

		// ------------------------------
		// SEG
		// ------------------------------
		private void RunSeg(Bitmap inputBmp, string modelPath, int minArea)
		{
			if (inputBmp == null) return;

			using (var engine = new SegmentationEngine(modelPath, 0))
			{
				SegmentationOption option = engine.GetInferenceOption();
				option.CalcTime = false;
				option.CalcObject = true;
				option.CalcScoremap = false;
				option.CalcMask = false;
				option.CalcObjectAreaAndApplyThreshold = true;
				option.CalcObjectScoreAndApplyThreshold = true;
				option.OversizedImageHandling = OverSizeImageFlags.crop_into_tiles;
				engine.SetInferenceOption(option);

				using (SrImage sr = ToSrImage(inputBmp))
				{
					Stopwatch sw = Stopwatch.StartNew();
					SegmentationResult result = engine.Inspection(sr);
					sw.Stop();

					_lastSegResult = result;

					using (Bitmap output = new Bitmap(inputBmp.Width, inputBmp.Height, PixelFormat.Format24bppRgb))
					{
						using (Graphics g = Graphics.FromImage(output))
						{
							g.DrawImage(inputBmp, 0, 0, output.Width, output.Height);
						}

						DrawSegResult_FilterByArea(result, output, minArea);
						ApplyBitmapToCanvas(output);
					}
				}
			}
		}

		// ------------------------------
		// CLA
		// ------------------------------
		private void RunCla(Bitmap inputBmp, string modelPath)
		{
			if (inputBmp == null) return;

			using (var engine = new ClassificationEngine(modelPath, 0))
			{
				ClassificationOption option = engine.GetInferenceOption();
				option.CalcTime = true;
				option.CalcClassActivationMap = false;
				engine.SetInferenceOption(option);

				using (SrImage sr = ToSrImage(inputBmp))
				{
					Stopwatch sw = Stopwatch.StartNew();
					ClassificationResult result = engine.Inspection(sr);
					sw.Stop();

					_lastClsResult = result;

					using (Bitmap output = new Bitmap(inputBmp.Width, inputBmp.Height, PixelFormat.Format24bppRgb))
					{
						using (Graphics g = Graphics.FromImage(output))
						{
							g.SmoothingMode = SmoothingMode.AntiAlias;
							g.DrawImage(inputBmp, 0, 0, output.Width, output.Height);

							string bestName = (result?.BestScoreClassInfo?.ClassInfo?.Name) ?? "N/A";
							string bestScore = (result?.BestScoreClassInfo != null)
								? result.BestScoreClassInfo.Score.ToString("N3")
								: "N/A";

							string text = $"CLS: {bestName} ({bestScore})";

							using (Font font = new Font("Arial", 18, FontStyle.Bold))
							using (Brush bg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
							using (Brush fg = Brushes.White)
							{
								SizeF sz = g.MeasureString(text, font);
								RectangleF rect = new RectangleF(10, 10, sz.Width + 20, sz.Height + 10);
								g.FillRectangle(bg, rect);
								g.DrawString(text, font, fg, 20, 15);
							}
						}

						ApplyBitmapToCanvas(output);
					}
				}
			}
		}
		private void DrawDetResult_FilterByArea(DetectionResult result, Bitmap bmp, int minArea)
		{
			if (result == null || result.DetectedObjects == null) return;

			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;

				foreach (var obj in result.DetectedObjects)
				{
					if (obj == null) continue;
					if (obj.Area < minArea) continue;

					using (Pen pen = new Pen(obj.ClassInfo.Color, 3))
					{
						float x = (float)obj.BoundingBox.X;
						float y = (float)obj.BoundingBox.Y;
						float w = (float)obj.BoundingBox.Width;
						float h = (float)obj.BoundingBox.Height;

						g.DrawRectangle(pen, x, y, w, h);
					}
				}
			}
		}
		
		// Seg 결과 그리기 + Area 필터
		private void DrawSegResult_FilterByArea(SegmentationResult result, Bitmap bmp, int minArea)
		{
			if (result == null || result.SegmentedObjects == null) return;

			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;

				foreach (var obj in result.SegmentedObjects)
				{
					if (obj == null) continue;
					if (obj.Area < minArea) continue;

					using (SolidBrush brush = new SolidBrush(Color.FromArgb(127, obj.ClassInfo.Color)))
					using (GraphicsPath gp = new GraphicsPath())
					{
						if (obj.Contour.Value == null || obj.Contour.Value.Count < 4) continue;

						gp.AddPolygon(obj.Contour.Value.ToArray());

						// inner contour(holes)
						foreach (var inner in obj.Contour.InnerValue)
						{
							if (inner == null || inner.Count < 4) continue;
							gp.AddPolygon(inner.ToArray());
						}

						g.FillPath(brush, gp);
					}
				}
			}
		}

		private void ApplyBitmapToCanvas(Bitmap src)
		{
			if (src == null) return;

			// Undo 스택에 현재 상태 저장(원하면)
			PushUndoSnapshot();

			using (Graphics g = Graphics.FromImage(_canvas))
			{
				g.Clear(Color.White);
				g.DrawImage(src, 0, 0, _canvas.Width, _canvas.Height);
			}

			pictureBox1.Image = _canvas;
			pictureBox1.Invalidate();
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			// 필요하면 초기화 코드 여기에
		}

		private void btnSelectModel_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Saige Model|*.saigecls;*.saigedet;*.saigeseg;*.saigeiad|All Files|*.*";
			if (ofd.ShowDialog() != DialogResult.OK)
				return;

			DlMode mode = (DlMode)cbDlMode.SelectedIndex;

			switch (mode)
			{
				case DlMode.Det:
					_detModelPath = ofd.FileName;
					break;

				case DlMode.Seg:
					_segModelPath = ofd.FileName;
					break;

				case DlMode.Cla:
					_claModelPath = ofd.FileName;
					break;
			}

			MessageBox.Show("모델 선택 완료:\n" + ofd.FileName);
		}
	}

	
}
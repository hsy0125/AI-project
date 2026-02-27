using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

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

		// ===============================
		// (1) 필드 변수 영역 (State)
		// ===============================
		// 시작 점
		private Point _startPos;

		// 현재 위치
		private Point _currentPos;

		// 드래그 여부
		private bool _isDrawing = false;

		// 선 두께
		private int _lineThickness = 2;

		// 도형 타입
		private DrawType _drawType = DrawType.DrawNone;

		// 캔버스(누적 그림) + Undo/Redo 스택
		private Bitmap _canvas;
		private Stack<Bitmap> _undoStack = new Stack<Bitmap>();
		private Stack<Bitmap> _redoStack = new Stack<Bitmap>();

		// ===============================
		// (2) 생성자 / 초기화
		// ===============================
		public Form1()
		{
			InitializeComponent();
			InitCanvas(); // 시작 시 캔버스 생성
			
			this.KeyPreview = true;
			this.KeyDown += Form1_KeyDown;
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
			{
				using (var loaded = new Bitmap(openFileDialog1.FileName))
				{
					_canvas?.Dispose();
					_canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);

					using (Graphics g = Graphics.FromImage(_canvas))
					{
						g.Clear(Color.White);
						// 캔버스 크기에 맞춰 이미지 그려넣기
						g.DrawImage(loaded, 0, 0, pictureBox1.Width, pictureBox1.Height);
					}

					pictureBox1.Image = _canvas;
				}

				_undoStack.Clear();
				_redoStack.Clear();
				UpdateUndoRedoButtons();
				pictureBox1.Invalidate();
			}
		}

		private void 저장ToolStripMenuItem_Click(object sender, EventArgs e) // 저장
		{
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
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
			if (e.Button != MouseButtons.Left) return;
			if (_drawType == DrawType.DrawNone) return;

			_startPos = e.Location;
			_currentPos = e.Location;
			_isDrawing = true;
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_isDrawing) return;

			_currentPos = e.Location;
			pictureBox1.Invalidate(); // Paint 다시 호출(미리보기)
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left) return;
			if (!_isDrawing) return;

			_isDrawing = false;
			if (_canvas == null) return;

			// 그리기 전 상태를 undo로 저장
			PushUndoSnapshot();

			// 캔버스에 실제로 그리기(누적 저장)
			using (Graphics g = Graphics.FromImage(_canvas))
			using (Pen pen = new Pen(GetSelColor(), _lineThickness))
			{
				switch (_drawType)
				{
					case DrawType.DrawLine:
						g.DrawLine(pen, _startPos, _currentPos);
						break;

					case DrawType.DrawRectangle:
						var rect = GetRectangle(_startPos, _currentPos);
						g.DrawRectangle(pen, rect);
						break;

					case DrawType.DrawCircle:
						var ellipse = GetRectangle(_startPos, _currentPos);
						g.DrawEllipse(pen, ellipse);
						break;
				}
			}

			pictureBox1.Invalidate();
		}

		// PictureBox 그리기(드래그 중 미리보기만)
		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
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

		private Rectangle GetRectangle(Point p1, Point p2)
		{
			return new Rectangle(
				Math.Min(p1.X, p2.X),
				Math.Min(p1.Y, p2.Y),
				Math.Abs(p2.X - p1.X),
				Math.Abs(p2.Y - p1.Y));
		}
	}
}
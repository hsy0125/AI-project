using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using SaigeVision.Net.V2;
using SaigeVision.Net.V2.Classification;
using SaigeVision.Net.V2.Detection;
using SaigeVision.Net.V2.Segmentation;

namespace WindowsFormsAppPaint_2_25
{
	internal class SaigeAiRunner
	{
		internal enum DlMode
		{
			Det = 0,
			Seg = 1,
			Cla = 2
		}

		//	internal sealed class SaigeAiRunner
		//	{
		//		// 결과를 "그려진 Bitmap"으로 반환 (최소 수정: Form1은 이 Bitmap을 캔버스에 올리기만 하면 됨)
		//		public Bitmap Run(Bitmap inputBmp, DlMode mode, string modelPath, int minArea, out string debugText)
		//		{
		//			debugText = "";
		//			if (inputBmp == null) throw new ArgumentNullException(nameof(inputBmp));

		//			var sw = Stopwatch.StartNew();

		//			switch (mode)
		//			{
		//				case DlMode.Det:
		//					{
		//						using (var engine = new DetectionEngine(modelPath, 0))
		//						{
		//							var opt = engine.GetInferenceOption();
		//							opt.CalcTime = true;
		//							engine.SetInferenceOption(opt);

		//							using (var sr = new SrImage(inputBmp))
		//							{
		//								var result = engine.Inspection(sr);
		//								sw.Stop();

		//								Bitmap output = Clone24bpp(inputBmp);
		//								DrawDet_FilterByArea(result, output, minArea);
		//								debugText = $"DET ok / {sw.ElapsedMilliseconds} ms / objs={(result?.DetectedObjects?.Count ?? 0)}";
		//								return output;
		//							}
		//						}
		//					}

		//				case DlMode.Seg:
		//					{
		//						using (var engine = new SegmentationEngine(modelPath, 0))
		//						{
		//							var opt = engine.GetInferenceOption();
		//							opt.CalcTime = true;
		//							opt.CalcObject = true;

		//							// 네 코드 옵션 유지(필요한 것만)
		//							opt.CalcScoremap = false;
		//							opt.CalcMask = false;
		//							opt.CalcObjectAreaAndApplyThreshold = true;
		//							opt.CalcObjectScoreAndApplyThreshold = true;
		//							opt.OversizedImageHandling = OverSizeImageFlags.crop_into_tiles;

		//							engine.SetInferenceOption(opt);

		//							using (var sr = new SrImage(inputBmp))
		//							{
		//								var result = engine.Inspection(sr);
		//								sw.Stop();

		//								Bitmap output = Clone24bpp(inputBmp);
		//								DrawSeg_FilterByArea(result, output, minArea);
		//								debugText = $"SEG ok / {sw.ElapsedMilliseconds} ms / objs={(result?.SegmentedObjects?.Count ?? 0)}";
		//								return output;
		//							}
		//						}
		//					}

		//				case DlMode.Cla:
		//					{
		//						using (var engine = new ClassificationEngine(modelPath, 0))
		//						{
		//							var opt = engine.GetInferenceOption();
		//							opt.CalcTime = true;
		//							opt.CalcClassActivationMap = false;
		//							engine.SetInferenceOption(opt);

		//							using (var sr = new SrImage(inputBmp))
		//							{
		//								var result = engine.Inspection(sr);
		//								sw.Stop();

		//								Bitmap output = Clone24bpp(inputBmp);
		//								DrawClsText(result, output);
		//								string bestName = (result?.BestScoreClassInfo?.ClassInfo?.Name) ?? "N/A";
		//								double bestScore = result?.BestScoreClassInfo?.Score ?? 0;
		//								debugText = $"CLA ok / {sw.ElapsedMilliseconds} ms / {bestName} ({bestScore:0.000})";
		//								return output;
		//							}
		//						}
		//					}

		//				default:
		//					throw new NotSupportedException("지원하지 않는 모드");
		//			}
		//		}

		//		private Bitmap Clone24bpp(Bitmap src)
		//		{
		//			// Saige/Graphics 안정성 위해 24bpp로 맞추기
		//			Bitmap dst = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
		//			using (Graphics g = Graphics.FromImage(dst))
		//			{
		//				g.DrawImage(src, 0, 0, dst.Width, dst.Height);
		//			}
		//			return dst;
		//		}

		//		private void DrawDet_FilterByArea(DetectionResult result, Bitmap bmp, int minArea)
		//		{
		//			if (result == null || result.DetectedObjects == null) return;

		//			using (Graphics g = Graphics.FromImage(bmp))
		//			{
		//				g.SmoothingMode = SmoothingMode.AntiAlias;

		//				foreach (var obj in result.DetectedObjects)
		//				{
		//					if (obj == null) continue;
		//					if (obj.Area < minArea) continue;

		//					using (Pen pen = new Pen(obj.ClassInfo.Color, 3))
		//					{
		//						float x = (float)obj.BoundingBox.X;
		//						float y = (float)obj.BoundingBox.Y;
		//						float w = (float)obj.BoundingBox.Width;
		//						float h = (float)obj.BoundingBox.Height;
		//						g.DrawRectangle(pen, x, y, w, h);
		//					}
		//				}
		//			}
		//		}

		//		private void DrawSeg_FilterByArea(SegmentationResult result, Bitmap bmp, int minArea)
		//		{
		//			if (result == null || result.SegmentedObjects == null) return;

		//			using (Graphics g = Graphics.FromImage(bmp))
		//			{
		//				g.SmoothingMode = SmoothingMode.AntiAlias;

		//				foreach (var obj in result.SegmentedObjects)
		//				{
		//					if (obj == null) continue;
		//					if (obj.Area < minArea) continue;
		//					if (obj.Contour.Value == null || obj.Contour.Value.Count < 4) continue;

		//					using (SolidBrush brush = new SolidBrush(Color.FromArgb(127, obj.ClassInfo.Color)))
		//					using (GraphicsPath gp = new GraphicsPath())
		//					{
		//						gp.AddPolygon(obj.Contour.Value.ToArray());

		//						if (obj.Contour.InnerValue != null)
		//						{
		//							foreach (var inner in obj.Contour.InnerValue)
		//							{
		//								if (inner == null || inner.Count < 4) continue;
		//								gp.AddPolygon(inner.ToArray());
		//							}
		//						}

		//						g.FillPath(brush, gp);
		//					}
		//				}
		//			}
		//		}

		//		private void DrawClsText(ClassificationResult result, Bitmap bmp)
		//		{
		//			using (Graphics g = Graphics.FromImage(bmp))
		//			{
		//				g.SmoothingMode = SmoothingMode.AntiAlias;

		//				string bestName = (result?.BestScoreClassInfo?.ClassInfo?.Name) ?? "N/A";
		//				string bestScore = (result?.BestScoreClassInfo != null)
		//					? result.BestScoreClassInfo.Score.ToString("N3")
		//					: "N/A";

		//				string text = $"CLS: {bestName} ({bestScore})";

		//				using (Font font = new Font("Arial", 18, FontStyle.Bold))
		//				using (Brush bg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
		//				using (Brush fg = Brushes.White)
		//				{
		//					SizeF sz = g.MeasureString(text, font);
		//					RectangleF rect = new RectangleF(10, 10, sz.Width + 20, sz.Height + 10);
		//					g.FillRectangle(bg, rect);
		//					g.DrawString(text, font, fg, 20, 15);
		//				}
		//			}
		//		}
		//	}
		//}
	}
}
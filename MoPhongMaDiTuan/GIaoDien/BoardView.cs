using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MoPhongMaDiTuan.MoHinh;

namespace MoPhongMaDiTuan.GiaoDien
{
    public class BoardView : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Board? BoardData { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentKnightX { get; set; } = 0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentKnightY { get; set; } = 0;

        // Đường đi hiện tại của quân mã (theo thứ tự các bước) để vẽ đường nối
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<(int x, int y)>? DuongDi { get; set; }

        // Ô vừa bị gỡ khi quay lui, được tô màu đỏ
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public (int x, int y)? OQuayLui { get; set; }

        // Khóa chọn ô xuất phát khi đang mô phỏng
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ChoPhepChon { get; set; } = true;

        // Sự kiện bắn ra ngoài khi người dùng click chọn một ô trên bàn cờ
        public event Action<int, int>? OnCellClicked;

        public BoardView()
        {
            this.DoubleBuffered = true; // Chống giật/nháy hình khi vẽ lại liên tục
            this.ResizeRedraw = true;
        }

        private void TinhKichThuoc(out int n, out int cellSize, out int offsetX, out int offsetY)
        {
            n = BoardData!.N;
            cellSize = Math.Max(1, Math.Min(this.Width, this.Height) / n);
            offsetX = (this.Width - cellSize * n) / 2;
            offsetY = (this.Height - cellSize * n) / 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BoardData == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            TinhKichThuoc(out int n, out int cellSize, out int offsetX, out int offsetY);

            using SolidBrush brushSang = new SolidBrush(Color.FromArgb(240, 217, 181));
            using SolidBrush brushToi = new SolidBrush(Color.FromArgb(181, 136, 99));
            using SolidBrush brushDaDi = new SolidBrush(Color.FromArgb(150, 205, 150));
            using SolidBrush brushQuayLui = new SolidBrush(Color.FromArgb(235, 105, 105));
            using Font fontSo = new Font("Segoe UI", Math.Max(7, cellSize / 5), FontStyle.Bold);
            using Font fontMa = new Font("Segoe UI Symbol", Math.Max(14, cellSize / 2), FontStyle.Bold);
            using StringFormat sfGiua = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using StringFormat sfGoc = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };

            // 1. Tô màu nền các ô
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    Rectangle rect = new Rectangle(offsetX + col * cellSize, offsetY + row * cellSize, cellSize, cellSize);
                    int giaTri = BoardData.MaTran[row, col];

                    Brush brushNen;
                    if (row == CurrentKnightX && col == CurrentKnightY)
                        brushNen = Brushes.Gold;                              // Ô quân Mã đang đứng
                    else if (OQuayLui.HasValue && OQuayLui.Value.x == row && OQuayLui.Value.y == col)
                        brushNen = brushQuayLui;                              // Ô vừa quay lui
                    else if (giaTri >= 0)
                        brushNen = brushDaDi;                                 // Ô đã đi qua
                    else
                        brushNen = ((row + col) % 2 == 0) ? brushSang : brushToi;

                    g.FillRectangle(brushNen, rect);
                    g.DrawRectangle(Pens.DimGray, rect);
                }
            }

            // 2. Vẽ đường đi nối tâm các ô theo thứ tự bước
            if (DuongDi != null && DuongDi.Count > 1)
            {
                using Pen butDuongDi = new Pen(Color.FromArgb(170, 30, 90, 200), Math.Max(2, cellSize / 14f));
                butDuongDi.StartCap = LineCap.Round;
                butDuongDi.EndCap = LineCap.Round;
                for (int i = 1; i < DuongDi.Count; i++)
                {
                    g.DrawLine(butDuongDi, TamO(DuongDi[i - 1], cellSize, offsetX, offsetY), TamO(DuongDi[i], cellSize, offsetX, offsetY));
                }
            }

            // 3. Vẽ số thứ tự bước (góc trên trái ô) và quân Mã (♞)
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    Rectangle rect = new Rectangle(offsetX + col * cellSize, offsetY + row * cellSize, cellSize, cellSize);
                    int giaTri = BoardData.MaTran[row, col];
                    if (row == CurrentKnightX && col == CurrentKnightY)
                    {
                        g.DrawString("♞", fontMa, Brushes.Black, rect, sfGiua);
                    }
                    if (giaTri >= 0)
                    {
                        Rectangle rectSo = new Rectangle(rect.X + 2, rect.Y + 1, rect.Width - 2, rect.Height - 1);
                        g.DrawString(giaTri.ToString(), fontSo, Brushes.DarkGreen, rectSo, sfGoc);
                    }
                }
            }
        }

        private static PointF TamO((int x, int y) o, int cellSize, int offsetX, int offsetY)
        {
            // x là hàng, y là cột (giống MaTran[x, y])
            return new PointF(offsetX + o.y * cellSize + cellSize / 2f, offsetY + o.x * cellSize + cellSize / 2f);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (BoardData == null || !ChoPhepChon) return;   // Đang mô phỏng thì không cho đổi ô xuất phát

            TinhKichThuoc(out int n, out int cellSize, out int offsetX, out int offsetY);
            if (e.X < offsetX || e.Y < offsetY) return;       // Click vào lề ngoài bàn cờ

            int col = (e.X - offsetX) / cellSize;
            int row = (e.Y - offsetY) / cellSize;

            if (row >= 0 && row < n && col >= 0 && col < n)
            {
                CurrentKnightX = row;
                CurrentKnightY = col;
                this.Invalidate(); // Vẽ lại bàn cờ ngay lập tức
                OnCellClicked?.Invoke(row, col);
            }
        }
    }
}

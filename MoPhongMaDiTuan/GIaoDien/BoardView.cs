using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MoPhongMaDiTuan.MoHinh;
using System.ComponentModel;

namespace MoPhongMaDiTuan.GiaoDien
{
    public class BoardView : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Board BoardData { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentKnightX { get; set; } = 0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentKnightY { get; set; } = 0;

        // Sự kiện bắn ra ngoài khi người dùng click chọn một ô trên bàn cờ
        public event Action<int, int> OnCellClicked;

        public BoardView()
        {
            this.DoubleBuffered = true; // Chống giật/nháy hình khi vẽ lại liên tục
            this.ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BoardData == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int n = BoardData.N;
            int cellSize = Math.Min(this.Width, this.Height) / n;
            int offsetX = (this.Width - cellSize * n) / 2;
            int offsetY = (this.Height - cellSize * n) / 2;

            Color mauOSang = Color.FromArgb(240, 217, 181);
            Color mauOToi = Color.FromArgb(181, 136, 99);
            Color mauDaDi = Color.FromArgb(130, 195, 130);

            using (Font fontSo = new Font("Segoe UI", Math.Max(8, cellSize / 4), FontStyle.Bold))
            using (Font fontMa = new Font("Segoe UI Symbol", Math.Max(14, cellSize / 2), FontStyle.Bold))
            {
                for (int row = 0; row < n; row++)
                {
                    for (int col = 0; col < n; col++)
                    {
                        Rectangle rect = new Rectangle(offsetX + col * cellSize, offsetY + row * cellSize, cellSize, cellSize);
                        int giaTri = BoardData.MaTran[row, col];

                        // 1. Tô màu nền ô cờ
                        Brush brushNen;
                        if (row == CurrentKnightX && col == CurrentKnightY)
                            brushNen = Brushes.Gold; // Ô quân Mã đang đứng
                        else if (giaTri >= 0)
                            brushNen = new SolidBrush(mauDaDi); // Ô đã đi qua
                        else
                            brushNen = ((row + col) % 2 == 0) ? new SolidBrush(mauOSang) : new SolidBrush(mauOToi);

                        g.FillRectangle(brushNen, rect);
                        g.DrawRectangle(Pens.DimGray, rect);

                        // 2. Vẽ quân Mã (♞) hoặc số thứ tự bước đi
                        StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        if (row == CurrentKnightX && col == CurrentKnightY)
                        {
                            g.DrawString("♞", fontMa, Brushes.Black, rect, sf);
                        }
                        else if (giaTri >= 0)
                        {
                            g.DrawString(giaTri.ToString(), fontSo, Brushes.DarkGreen, rect, sf);
                        }
                    }
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (BoardData == null) return;

            int n = BoardData.N;
            int cellSize = Math.Min(this.Width, this.Height) / n;
            int offsetX = (this.Width - cellSize * n) / 2;
            int offsetY = (this.Height - cellSize * n) / 2;

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
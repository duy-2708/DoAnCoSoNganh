using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MoPhongMaDiTuan.MoHinh;

namespace MoPhongMaDiTuan.GiaoDien
{
    public partial class MainForm : Form
    {
        private Board _board;
        private Knight _knight;
        private BoardView _boardView;
        private Label _lblToaDo;
        private Label _lblTrangThai;
        private ComboBox _cboKichThuoc;
        private ComboBox _cboThuatToan;
        private Button _btnBatDau;

        // Bộ điều khiển chạy từng bước (Generator + Timer)
        private System.Windows.Forms.Timer _timer;
        private IEnumerator<MoveStep> _stepEnumerator;
        private Solver _currentSolver;

        public MainForm()
        {
            InitializeComponent();
            KhoiTaoGiaoDien();
        }

        private void KhoiTaoGiaoDien()
        {
            this.Text = "Mô phỏng Bài toán Mã đi tuần - C# OOP WinForms";
            this.Size = new Size(850, 660);
            this.StartPosition = FormStartPosition.CenterScreen;

            _board = new Board(8);
            _knight = new Knight(0, 0);

            // Khởi tạo Timer điều khiển tốc độ chạy từng bước (mặc định 120ms/bước)
            _timer = new System.Windows.Forms.Timer { Interval = 120 };
            _timer.Tick += Timer_Tick;

            // --- PANEL ĐIỀU KHIỂN BÊN PHẢI ---
            Panel pnlRight = new Panel { Dock = DockStyle.Right, Width = 240, Padding = new Padding(10), BackColor = Color.WhiteSmoke };

            Label lblSize = new Label { Text = "1. Kích thước bàn cờ (N):", AutoSize = true, Location = new Point(15, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            _cboKichThuoc = new ComboBox { Location = new Point(15, 45), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboKichThuoc.Items.AddRange(new object[] { "5 x 5", "6 x 6", "7 x 7", "8 x 8" });
            _cboKichThuoc.SelectedIndex = 3;
            _cboKichThuoc.SelectedIndexChanged += (s, e) => DungVaResetBanCo();

            Label lblAlgo = new Label { Text = "2. Thuật toán giải:", AutoSize = true, Location = new Point(15, 85), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            _cboThuatToan = new ComboBox { Location = new Point(15, 110), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboThuatToan.Items.AddRange(new object[] { "Heuristic Warnsdorff", "Quay lui thuần (Backtracking)" });
            _cboThuatToan.SelectedIndex = 0;

            _lblToaDo = new Label { Text = "Xuất phát: (0, 0)\n(Click vào ô trên bàn cờ)", AutoSize = true, Location = new Point(15, 155), Font = new Font("Segoe UI", 9.5f) };

            _btnBatDau = new Button
            {
                Text = "▶ Bắt đầu chạy",
                Location = new Point(15, 210),
                Size = new Size(200, 42),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnBatDau.Click += BtnBatDau_Click;

            _lblTrangThai = new Label { Text = "Trạng thái: Sẵn sàng", AutoSize = true, MaximumSize = new Size(200, 0), Location = new Point(15, 270), Font = new Font("Segoe UI", 9.5f, FontStyle.Italic), ForeColor = Color.DarkBlue };

            pnlRight.Controls.AddRange(new Control[] { lblSize, _cboKichThuoc, lblAlgo, _cboThuatToan, _lblToaDo, _btnBatDau, _lblTrangThai });

            // --- BÀN CỜ BÊN TRÁI ---
            _boardView = new BoardView
            {
                Dock = DockStyle.Fill,
                BoardData = _board,
                CurrentKnightX = 0,
                CurrentKnightY = 0
            };

            _boardView.OnCellClicked += (row, col) =>
            {
                if (_timer.Enabled) return; // Đang chạy thì không cho đổi ô xuất phát
                _board.Reset();
                _knight.StartX = row;
                _knight.StartY = col;
                _lblToaDo.Text = $"Xuất phát: ({row}, {col})";
                _lblTrangThai.Text = "Trạng thái: Sẵn sàng";
            };

            this.Controls.Add(_boardView);
            this.Controls.Add(pnlRight);
        }

        private void DungVaResetBanCo()
        {
            _timer.Stop();
            int n = _cboKichThuoc.SelectedIndex + 5;
            _board = new Board(n);
            _knight = new Knight(0, 0);
            _boardView.BoardData = _board;
            _boardView.CurrentKnightX = 0;
            _boardView.CurrentKnightY = 0;
            _boardView.Invalidate();
            _lblToaDo.Text = "Xuất phát: (0, 0)";
            _lblTrangThai.Text = "Trạng thái: Sẵn sàng";
            _btnBatDau.Text = "▶ Bắt đầu chạy";
            _btnBatDau.BackColor = Color.SeaGreen;
        }

        private void BtnBatDau_Click(object sender, EventArgs e)
        {
            if (_timer.Enabled)
            {
                // Nếu đang chạy mà bấm lại thì Dừng & Reset
                _timer.Stop();
                _board.Reset();
                _boardView.CurrentKnightX = _knight.StartX;
                _boardView.CurrentKnightY = _knight.StartY;
                _boardView.Invalidate();
                _btnBatDau.Text = "▶ Bắt đầu chạy";
                _btnBatDau.BackColor = Color.SeaGreen;
                _lblTrangThai.Text = "Trạng thái: Đã dừng";
                return;
            }

            // Chọn thuật toán theo ComboBox (Tính đa hình OOP)
            _currentSolver = (_cboThuatToan.SelectedIndex == 0)
                ? new WarnsDorffSolver()
                : new BacktrackingSolver();

            // Gọi hàm Generator Steps() để lấy luồng từng bước đi
            _stepEnumerator = _currentSolver.Steps(_board, _knight).GetEnumerator();

            _btnBatDau.Text = "⏹ Dừng lại";
            _btnBatDau.BackColor = Color.IndianRed;
            _lblTrangThai.Text = $"Đang chạy {_currentSolver.TenThuatToan}...";
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_stepEnumerator != null && _stepEnumerator.MoveNext())
            {
                MoveStep step = _stepEnumerator.Current;

                // Tín hiệu hoàn thành (X == -1, Y == -1)
                if (step.X == -1 && step.Y == -1)
                {
                    _timer.Stop();
                    _btnBatDau.Text = "▶ Chạy lại";
                    _btnBatDau.BackColor = Color.SeaGreen;
                    _lblTrangThai.Text = $"Hoàn thành!\nQuay lui: {_currentSolver.SoLanQuayLui} lần";
                    return;
                }

                // Cập nhật vị trí quân Mã trên giao diện nếu là bước tiến tới
                if (!step.LaQuayLui)
                {
                    _boardView.CurrentKnightX = step.X;
                    _boardView.CurrentKnightY = step.Y;
                }

                _boardView.Invalidate(); // Vẽ lại bàn cờ nhịp này
            }
            else
            {
                _timer.Stop();
                _btnBatDau.Text = "▶ Bắt đầu chạy";
                _btnBatDau.BackColor = Color.SeaGreen;
                _lblTrangThai.Text = "Không tìm thấy đường đi!";
            }
        }
    }
}
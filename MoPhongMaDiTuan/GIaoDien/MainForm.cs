using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MoPhongMaDiTuan.MoHinh;

namespace MoPhongMaDiTuan.GiaoDien
{
    public partial class MainForm : Form
    {
        private Board _board = null!;
        private Knight _knight = null!;
        private BoardView _boardView = null!;
        private Label _lblToaDo = null!;
        private Label _lblTrangThai = null!;
        private Label _lblSoNuocThu = null!;
        private Label _lblSoLanQuayLui = null!;
        private Label _lblThoiGian = null!;
        private Label _lblTocDo = null!;
        private ComboBox _cboKichThuoc = null!;
        private ComboBox _cboThuatToan = null!;
        private Button _btnBatDau = null!;
        private Button _btnTamDung = null!;
        private Button _btnTungBuoc = null!;
        private Button _btnLamLai = null!;
        private TrackBar _trkTocDo = null!;

        // Bộ điều khiển chạy từng bước (Generator + Timer)
        private System.Windows.Forms.Timer _timer = null!;
        private IEnumerator<MoveStep>? _stepEnumerator;
        private Solver? _currentSolver;

        // Thời gian tính toán của thuật toán (không tính thời gian chờ giữa các nhịp vẽ)
        private readonly Stopwatch _dongHo = new Stopwatch();

        // Đường đi hiện tại: bước tiến thì thêm vào cuối, bước quay lui thì bỏ phần tử cuối
        private readonly List<(int x, int y)> _duongDi = new List<(int x, int y)>();
        private bool _daKetThuc = false;

        // Mức tốc độ 1..10: khoảng thời gian giữa 2 nhịp (ms) và số bước xử lý trong mỗi nhịp
        private static readonly int[] KhoangThoiGian = { 600, 400, 250, 120, 60, 30, 15, 15, 15, 15 };
        private static readonly int[] SoBuocMoiNhip = { 1, 1, 1, 1, 1, 1, 1, 20, 500, 20000 };

        public MainForm()
        {
            InitializeComponent();
            KhoiTaoGiaoDien();
        }

        // Đổi kích thước thiết kế (ở tỉ lệ 100%) sang điểm ảnh thật theo DPI của màn hình
        private int S(int giaTri) => this.LogicalToDeviceUnits(giaTri);

        private void KhoiTaoGiaoDien()
        {
            this.Text = "Mô phỏng Bài toán Mã đi tuần - C# OOP WinForms";
            // Kích thước tính theo DPI màn hình (máy đặt tỉ lệ 125%, 150%... vẫn hiển thị đủ chữ)
            this.Size = new Size(S(900), S(700));
            this.MinimumSize = new Size(S(760), S(660));
            this.StartPosition = FormStartPosition.CenterScreen;

            _board = new Board(8);
            _knight = new Knight(0, 0);

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += Timer_Tick;

            Font fontTieuDe = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Font fontThuong = new Font("Segoe UI", 9.5f);
            int beRongNoiDung = S(250);   // bề rộng vùng nội dung của bảng điều khiển

            // --- BẢNG ĐIỀU KHIỂN BÊN PHẢI: xếp dọc tự động, không dùng tọa độ cố định ---
            TableLayoutPanel pnlRight = new TableLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = beRongNoiDung + S(30),
                Padding = new Padding(S(12), S(8), S(12), S(8)),
                BackColor = Color.WhiteSmoke,
                ColumnCount = 1,
                AutoScroll = true,
                GrowStyle = TableLayoutPanelGrowStyle.AddRows
            };
            pnlRight.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Thêm một điều khiển vào hàng tiếp theo, cách điều khiển phía trên khoangCach điểm ảnh
            void Them(Control c, int khoangCach = 4, bool keoGian = true)
            {
                c.Margin = new Padding(0, S(khoangCach), 0, 0);
                c.Anchor = keoGian ? (AnchorStyles.Left | AnchorStyles.Right) : AnchorStyles.Left;
                pnlRight.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                pnlRight.Controls.Add(c);
            }
            Label TaoNhan(string text, Font font) =>
                new Label { Text = text, AutoSize = true, MaximumSize = new Size(beRongNoiDung, 0), Font = font };

            Label lblSize = TaoNhan("1. Kích thước bàn cờ (N):", fontTieuDe);
            _cboKichThuoc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = fontThuong };
            _cboKichThuoc.Items.AddRange(new object[] { "3 x 3", "4 x 4", "5 x 5", "6 x 6", "7 x 7", "8 x 8" });
            _cboKichThuoc.SelectedIndex = 5;
            _cboKichThuoc.SelectedIndexChanged += (s, e) => DungVaResetBanCo();

            Label lblAlgo = TaoNhan("2. Thuật toán giải:", fontTieuDe);
            _cboThuatToan = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = fontThuong };
            _cboThuatToan.Items.AddRange(new object[] { "Heuristic Warnsdorff", "Quay lui thuần (Backtracking)" });
            _cboThuatToan.SelectedIndex = 0;

            _lblToaDo = TaoNhan("Xuất phát: (0, 0)\n(Click vào ô trên bàn cờ để chọn)", fontThuong);

            _btnBatDau = new Button
            {
                Text = "▶ Bắt đầu",
                Height = S(42),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnBatDau.Click += BtnBatDau_Click;

            // Hai nút Tạm dừng và Từng bước nằm cùng một hàng, mỗi nút chiếm một nửa bề rộng
            TableLayoutPanel hangNut = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, Height = S(40), Padding = Padding.Empty };
            hangNut.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            hangNut.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _btnTamDung = new Button { Text = "⏸ Tạm dừng", Dock = DockStyle.Fill, Font = fontThuong, Cursor = Cursors.Hand, Margin = new Padding(0, 0, S(3), 0) };
            _btnTamDung.Click += BtnTamDung_Click;
            _btnTungBuoc = new Button { Text = "⏭ Từng bước", Dock = DockStyle.Fill, Font = fontThuong, Cursor = Cursors.Hand, Margin = new Padding(S(3), 0, 0, 0) };
            _btnTungBuoc.Click += BtnTungBuoc_Click;
            hangNut.Controls.Add(_btnTamDung, 0, 0);
            hangNut.Controls.Add(_btnTungBuoc, 1, 0);

            _btnLamLai = new Button { Text = "↺ Làm lại", Height = S(36), Font = fontThuong, Cursor = Cursors.Hand };
            _btnLamLai.Click += (s, e) => LamLai();

            Label lblTocDoTieuDe = TaoNhan("3. Tốc độ mô phỏng:", fontTieuDe);
            _trkTocDo = new TrackBar { Minimum = 1, Maximum = 10, Value = 4, TickFrequency = 1 };
            _trkTocDo.ValueChanged += (s, e) => CapNhatTocDo();
            _lblTocDo = TaoNhan("", fontThuong);

            Label lblThongKe = TaoNhan("4. Thống kê:", fontTieuDe);
            _lblSoNuocThu = TaoNhan("", fontThuong);
            _lblSoLanQuayLui = TaoNhan("", fontThuong);
            _lblThoiGian = TaoNhan("", fontThuong);

            Label lblChuThich = TaoNhan("Vàng: quân mã đang đứng\nXanh lá: ô đã đi qua\nĐỏ: ô vừa bị gỡ khi quay lui", new Font("Segoe UI", 8.5f));
            lblChuThich.ForeColor = Color.DimGray;

            _lblTrangThai = TaoNhan("Trạng thái: Sẵn sàng", new Font("Segoe UI", 9.5f, FontStyle.Italic));
            _lblTrangThai.ForeColor = Color.DarkBlue;

            pnlRight.SuspendLayout();
            Them(lblSize, 6, false);
            Them(_cboKichThuoc, 2);
            Them(lblAlgo, 12, false);
            Them(_cboThuatToan, 2);
            Them(_lblToaDo, 10, false);
            Them(_btnBatDau, 12);
            Them(hangNut, 6);
            Them(_btnLamLai, 6);
            Them(lblTocDoTieuDe, 16, false);
            Them(_trkTocDo, 2);
            Them(_lblTocDo, 0, false);
            Them(lblThongKe, 16, false);
            Them(_lblSoNuocThu, 4, false);
            Them(_lblSoLanQuayLui, 2, false);
            Them(_lblThoiGian, 2, false);
            Them(lblChuThich, 14, false);
            Them(_lblTrangThai, 12, false);
            pnlRight.ResumeLayout();

            // --- BÀN CỜ BÊN TRÁI ---
            _boardView = new BoardView
            {
                Dock = DockStyle.Fill,
                BoardData = _board,
                DuongDi = _duongDi,
                CurrentKnightX = 0,
                CurrentKnightY = 0
            };

            _boardView.OnCellClicked += (row, col) =>
            {
                _knight.StartX = row;
                _knight.StartY = col;
                _lblToaDo.Text = $"Xuất phát: ({row}, {col})";
                LamLai();
            };

            this.Controls.Add(_boardView);
            this.Controls.Add(pnlRight);

            CapNhatTocDo();
            CapNhatThongKe();
            CapNhatNut();
        }

        // ================== ĐIỀU KHIỂN MÔ PHỎNG ==================

        private void BtnBatDau_Click(object? sender, EventArgs e)
        {
            if (_timer.Enabled) return;
            if (_daKetThuc) LamLai();          // "Chạy lại" sau khi đã kết thúc
            if (!ChuanBiLanChay()) return;

            _timer.Start();
            _lblTrangThai.Text = $"Đang chạy: {_currentSolver!.TenThuatToan}...";
            CapNhatNut();
        }

        private void BtnTamDung_Click(object? sender, EventArgs e)
        {
            if (!_timer.Enabled) return;
            _timer.Stop();
            _lblTrangThai.Text = "Trạng thái: Tạm dừng";
            CapNhatNut();
        }

        private void BtnTungBuoc_Click(object? sender, EventArgs e)
        {
            if (_daKetThuc) return;
            _timer.Stop();
            if (!ChuanBiLanChay()) return;

            XuLyBuoc(1);
            if (!_daKetThuc) _lblTrangThai.Text = "Trạng thái: Chạy từng bước";
            CapNhatNut();
        }

        // Tạo bộ giải và dãy bước nếu chưa có (lần bấm Bắt đầu / Từng bước đầu tiên)
        private bool ChuanBiLanChay()
        {
            if (_stepEnumerator != null) return true;

            int n = _board.N;
            bool quayLuiThuan = _cboThuatToan.SelectedIndex == 1;
            if (quayLuiThuan && n >= 6)
            {
                DialogResult traLoi = MessageBox.Show(
                    $"Quay lui thuần túy trên bàn {n}x{n} có thể phải quay lui hàng triệu lần và chạy rất lâu.\n" +
                    "Nên chọn tốc độ mức 10 hoặc dùng Heuristic Warnsdorff.\n\nBạn vẫn muốn chạy?",
                    "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (traLoi != DialogResult.Yes) return false;
            }

            // Chọn thuật toán theo ComboBox (Tính đa hình OOP)
            _currentSolver = quayLuiThuan ? new BacktrackingSolver() : new WarnsDorffSolver();

            // Gọi hàm Generator Steps() để lấy luồng từng bước đi
            _stepEnumerator = _currentSolver.Steps(_board, _knight).GetEnumerator();

            _duongDi.Clear();
            _boardView.OQuayLui = null;
            _dongHo.Reset();
            _daKetThuc = false;
            KhoaDieuKhien(true);
            return true;
        }

        private void LamLai()
        {
            _timer.Stop();
            _stepEnumerator?.Dispose();
            _stepEnumerator = null;
            _currentSolver = null;
            _daKetThuc = false;
            _dongHo.Reset();
            _duongDi.Clear();
            _board.Reset();

            _boardView.OQuayLui = null;
            _boardView.CurrentKnightX = _knight.StartX;
            _boardView.CurrentKnightY = _knight.StartY;
            _boardView.Invalidate();

            KhoaDieuKhien(false);
            _lblTrangThai.Text = "Trạng thái: Sẵn sàng";
            CapNhatThongKe();
            CapNhatNut();
        }

        private void DungVaResetBanCo()
        {
            int n = _cboKichThuoc.SelectedIndex + 3;   // danh sách bắt đầu từ 3 x 3
            _board = new Board(n);
            _knight = new Knight(0, 0);
            _boardView.BoardData = _board;
            _lblToaDo.Text = "Xuất phát: (0, 0)";
            LamLai();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            XuLyBuoc(SoBuocMoiNhip[_trkTocDo.Value - 1]);
        }

        // Lấy tối đa soBuoc bước từ Generator rồi vẽ lại bàn cờ một lần
        private void XuLyBuoc(int soBuoc)
        {
            if (_stepEnumerator == null || _currentSolver == null) return;

            for (int i = 0; i < soBuoc; i++)
            {
                _dongHo.Start();
                bool conBuoc = _stepEnumerator.MoveNext();
                _dongHo.Stop();

                if (!conBuoc)
                {
                    KetThuc(false);   // Đã thử hết mọi khả năng mà không đi hết bàn cờ
                    break;
                }

                MoveStep step = _stepEnumerator.Current;

                // Tín hiệu hoàn thành (X == -1, Y == -1)
                if (step.X == -1 && step.Y == -1)
                {
                    KetThuc(true);
                    break;
                }

                if (step.LaQuayLui)
                {
                    // Gỡ ô cuối khỏi đường đi, quân mã lùi về ô trước đó
                    if (_duongDi.Count > 0) _duongDi.RemoveAt(_duongDi.Count - 1);
                    _boardView.OQuayLui = (step.X, step.Y);
                }
                else
                {
                    _duongDi.Add((step.X, step.Y));
                    _boardView.OQuayLui = null;
                }
            }

            if (_duongDi.Count > 0)
            {
                var (x, y) = _duongDi[_duongDi.Count - 1];
                _boardView.CurrentKnightX = x;
                _boardView.CurrentKnightY = y;
            }

            CapNhatThongKe();
            _boardView.Invalidate(); // Vẽ lại bàn cờ nhịp này
        }

        private void KetThuc(bool thanhCong)
        {
            _timer.Stop();
            _daKetThuc = true;
            _stepEnumerator?.Dispose();
            _stepEnumerator = null;
            _boardView.OQuayLui = null;

            int n = _board.N;
            if (thanhCong)
                _lblTrangThai.Text = $"Hoàn thành! Quân mã đã đi hết {n * n} ô.";
            else if (n < 5)
                _lblTrangThai.Text = $"Không có lời giải: bàn {n}x{n} (N < 5) không có hành trình đi hết bàn cờ.";
            else
                _lblTrangThai.Text = $"Không tìm thấy hành trình từ ô ({_knight.StartX}, {_knight.StartY}).";

            KhoaDieuKhien(false);
            CapNhatNut();
        }

        // ================== CẬP NHẬT GIAO DIỆN ==================

        private void KhoaDieuKhien(bool dangGiai)
        {
            _cboKichThuoc.Enabled = !dangGiai;
            _cboThuatToan.Enabled = !dangGiai;
            _boardView.ChoPhepChon = !dangGiai;
        }

        private void CapNhatNut()
        {
            bool dangChay = _timer.Enabled;
            _btnBatDau.Enabled = !dangChay;
            _btnBatDau.Text = _daKetThuc ? "▶ Chạy lại" : (_stepEnumerator != null ? "▶ Tiếp tục" : "▶ Bắt đầu");
            _btnTamDung.Enabled = dangChay;
            _btnTungBuoc.Enabled = !_daKetThuc;
        }

        private void CapNhatTocDo()
        {
            int muc = _trkTocDo.Value;
            _timer.Interval = KhoangThoiGian[muc - 1];
            int soBuoc = SoBuocMoiNhip[muc - 1];
            _lblTocDo.Text = soBuoc == 1
                ? $"Mức {muc}/10: {_timer.Interval} ms mỗi bước"
                : $"Mức {muc}/10: {soBuoc:N0} bước mỗi nhịp";
        }

        private void CapNhatThongKe()
        {
            long soNuocThu = _currentSolver?.SoNuocDaThu ?? 0;
            long soLanQuayLui = _currentSolver?.SoLanQuayLui ?? 0;
            _lblSoNuocThu.Text = $"Số nước đã thử: {soNuocThu:N0}";
            _lblSoLanQuayLui.Text = $"Số lần quay lui: {soLanQuayLui:N0}";
            _lblThoiGian.Text = $"Thời gian tính toán: {_dongHo.Elapsed.TotalMilliseconds:N1} ms";
        }
    }
}

# Đồ án Cơ sở ngành – Mô phỏng bài toán Mã đi tuần

Đề tài số 8: **Lập trình mô phỏng bài toán Mã đi tuần trong môi trường đồ họa**
Sinh viên: Đặng Ngọc Nhật Duy – Lớp 66.CNPM · GVHD: TS. Nguyễn Đình Hưng
Ngôn ngữ: C# (.NET 10, Windows Forms) · Môi trường: Visual Studio 2026

## Cấu trúc thư mục

| Thư mục | Nội dung |
| --- | --- |
| `BackTrack1/` | Tìm đường trong mê cung bằng quay lui (ví dụ dẫn dắt) |
| `BackTrack2/` | Mã đi tuần bằng quay lui thuần túy (dòng lệnh) |
| `WarnsDorff/` | Mã đi tuần bằng quay lui kết hợp heuristic Warnsdorff (dòng lệnh) |
| `KiemChungToanDien/` | Chạy Warnsdorff từ cả 64 ô của bàn 8×8 và tự kiểm tra kết quả |
| `MoPhongMaDiTuan/` | Ứng dụng mô phỏng đồ họa (Windows Forms) |

Ứng dụng `MoPhongMaDiTuan` gồm hai phần:

- `MoHinh/` – phần thuật toán, không phụ thuộc giao diện:
  - `Board`: bàn cờ N×N (ô chưa đi = -1, ô đã đi = số thứ tự bước).
  - `Knight`: ô xuất phát và 8 nước đi của quân mã.
  - `Solver` (lớp trừu tượng) với hai lớp con `BacktrackingSolver` và `WarnsDorffSolver`. Phương thức `Steps()` trả về dãy `MoveStep` bằng `yield return`.
  - `MoveStep`: một bước mô phỏng (ô, số thứ tự, bước tiến hay quay lui).
- `GIaoDien/` – phần giao diện:
  - `BoardView`: vẽ bàn cờ, quân mã, số thứ tự, đường đi và ô quay lui.
  - `MainForm`: nhận thao tác người dùng, điều khiển mô phỏng bằng `Timer`.

## Cách chạy

1. Mở `MoPhongMaDiTuan/MoPhongMaDiTuan.slnx` bằng Visual Studio 2026 (cần .NET 10 SDK).
2. Nhấn **F5** để chạy.

Các chương trình dòng lệnh mở bằng file `.slnx` trong từng thư mục tương ứng.

## Chức năng

- Chọn kích thước bàn cờ từ 3×3 đến 8×8. Bàn 3×3 và 4×4 dùng để minh họa trường hợp không có lời giải.
- Click chuột vào một ô để chọn ô xuất phát.
- Chọn thuật toán: Heuristic Warnsdorff hoặc quay lui thuần túy.
- Mô phỏng từng bước: đánh số thứ tự ô, vẽ đường đi. Ô vừa bị gỡ khi quay lui được tô đỏ.
- Điều khiển: **Bắt đầu / Tiếp tục**, **Tạm dừng**, **Từng bước**, **Làm lại**, thanh **tốc độ** 10 mức.
- Thống kê: số nước đã thử, số lần quay lui, thời gian tính toán.

## Lưu ý

Quay lui thuần túy trên bàn 6×6 trở lên có thể phải quay lui hàng triệu lần. Chương trình hiện cảnh báo trước khi chạy; nên chọn tốc độ mức 10 hoặc dùng Warnsdorff.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KiemChungToanDien
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int N = 8;
            int[] dx = { -2, 2, -2, 2, -1, 1, -1, 1 };
            int[] dy = { 1, 1, -1, -1, 2, 2, -2, -2 };

            bool HopLe(int x, int y, int[,] banco)
            {
                return x >= 0 && x < N && y >= 0 && y < N && banco[x, y] == -1;
            }

            int DemLoiThoat(int x, int y, int[,] banco)
            {
                int count = 0;
                for (int i = 0; i < 8; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (HopLe(nx, ny, banco))
                        count++;
                }
                return count;
            }

            bool GiaiMaDiTuan(int x, int y, int buoc, int[,] banco)
            {
                if (buoc == N * N)
                    return true;

                List<(int nx, int ny, int bac)> dsNuocDi = new List<(int, int, int)>();
                for (int i = 0; i < 8; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];
                    if (HopLe(nx, ny, banco))
                    {
                        int bac = DemLoiThoat(nx, ny, banco);
                        dsNuocDi.Add((nx, ny,bac));
                    }
                }

                dsNuocDi.Sort((a, b) => a.bac.CompareTo(b.bac));

                foreach (var nuocDi in dsNuocDi)
                {
                    banco[nuocDi.nx, nuocDi.ny] = buoc;
                    if (GiaiMaDiTuan(nuocDi.nx, nuocDi.ny, buoc + 1, banco))
                        return true;

                    banco[nuocDi.nx, nuocDi.ny] = -1;
                }

                return false;
            }

            
            bool KiemTraKetQua(int[,] banco, out bool laHanhTrinhDong)
            {
                laHanhTrinhDong = false;
                int[] posX = new int[N * N];
                int[] posY = new int[N * N];
                bool[] daCoSo = new bool[N * N];

                
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        int buoc = banco[i, j];
                        if (buoc < 0 || buoc >= N * N || daCoSo[buoc])
                            return false;

                        daCoSo[buoc] = true;
                        posX[buoc] = i;
                        posY[buoc] = j;
                    }
                }

               
                for (int k = 0; k < N * N - 1; k++)
                {
                    int lechX = Math.Abs(posX[k] - posX[k + 1]);
                    int lechY = Math.Abs(posY[k] - posY[k + 1]);
                    if (lechX * lechY != 2)
                        return false;
                }

              
                int cuoiX = Math.Abs(posX[N * N - 1] - posX[0]);
                int cuoiY = Math.Abs(posY[N * N - 1] - posY[0]);
                laHanhTrinhDong = (cuoiX * cuoiY == 2);

                return true;
            }

       
            int soThanhCong = 0;
            int soHanhTrinhDong = 0;
            int soHanhTrinhMo = 0;

            Stopwatch dongHo = Stopwatch.StartNew();

            for (int startX = 0; startX < N; startX++)
            {
                for (int startY = 0; startY < N; startY++)
                {
                    // Tạo bàn cờ mới cho mỗi lần thử
                    int[,] banco = new int[N, N];
                    for (int i = 0; i < N; i++)
                        for (int j = 0; j < N; j++)
                            banco[i, j] = -1;

                    banco[startX, startY] = 0;

                    if (GiaiMaDiTuan(startX, startY, 1, banco))
                    {
                        if (KiemTraKetQua(banco, out bool laDong))
                        {
                            soThanhCong++;
                            if (laDong) soHanhTrinhDong++;
                            else soHanhTrinhMo++;
                        }
                        else
                        {
                            Console.WriteLine($"[LOI LOGIC] Duong di tu ({startX}, {startY}) khong hop le!");
                        }
                    }
                }
            }

            dongHo.Stop();

            Console.WriteLine($"=== KET QUA KIEM THU TOAN DIEN BAN CO {N}x{N} ===");
            Console.WriteLine($"- Tong so o xuat phat thu nghiem : {N * N}");
            Console.WriteLine($"- So o giai thanh cong & hop le  : {soThanhCong} / {N * N}");
            Console.WriteLine($"  + Hanh trinh DONG (Closed Tour): {soHanhTrinhDong}");
            Console.WriteLine($"  + Hanh trinh MO (Open Tour)    : {soHanhTrinhMo}");
            Console.WriteLine($"- Tong thoi gian chay ca 64 van  : {dongHo.ElapsedMilliseconds} ms");
        }
    }
}
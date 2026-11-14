using System;
using System.Diagnostics;

namespace BackTrack2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            long dem = 0;
            int N = 5;
            int[,] banco = new int[N, N];

            // Sửa dy[4] từ 1 thành -1 để đủ 8 hướng khác nhau
            int[] dx = { 2, -2, 1, -1, 2, -2, 1, -1 };
            int[] dy = { 1, 1, 2, 2, -1, -1, -2, -2 };

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    banco[i, j] = -1;
                }
            }

            bool hople(int x, int y)
            {
                return x >= 0 && x < N && y >= 0 && y < N && banco[x, y] == -1;
            }

            bool GiaiMaDiTuan(int x, int y, int buoc)
            {
                // Sửa lại cú pháp nhân N * N
                if (buoc == N * N)
                {
                    return true;
                }

                for (int i = 0; i < 8; i++)
                {
                    int nextx = x + dx[i];
                    int nexty = y + dy[i];
                    if (hople(nextx, nexty))
                    {
                        banco[nextx, nexty] = buoc;
                        if (GiaiMaDiTuan(nextx, nexty, buoc + 1))
                        {
                            return true;
                        }
                        banco[nextx, nexty] = -1;
                        dem++; 
                        
                    }
                }
                return false;
            }

            banco[0, 0] = 0;

            if (GiaiMaDiTuan(0, 0, 1))
            {
                
                Console.WriteLine($"Tim thay hanh trinh tren ban co {N}x{N}:");
               
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        Console.Write($"{banco[i, j],3} ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("Khong tim thay duong di!");
            }
        }
    }
}
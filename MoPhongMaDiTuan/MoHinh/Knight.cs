using System;
using System.Collections.Generic;
using System.Text;

namespace MoPhongMaDiTuan.MoHinh
{
    public class Knight
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public static readonly int[] Dx = { -2, 2, -2, 2, -1, 1, -1, 1 };
        public static readonly int[] Dy = { 1, 1, -1, -1, 2, 2, -2, -2 };
        public Knight(int startX, int startY)
        {
            StartX = startX;
            StartY = startY;
        }
        public List<(int x, int y)>NuocDiHopLe(int x, int y, Board b)
        {
            var ds = new List<(int x, int y)>();
            for (int i = 0; i < 8; i++)
            {
                int nx = x + Dx[i];
                int ny = y + Dy[i];
                if(b.HopLe(nx, ny))
                {
                    ds.Add((nx, ny));
                }
            }
            return ds;
        }

        }
    }

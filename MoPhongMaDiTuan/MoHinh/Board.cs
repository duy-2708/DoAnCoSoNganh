using System;
using System.Collections.Generic;
using System.Text;

namespace MoPhongMaDiTuan.MoHinh
{
    public class Board
    {
        public int N { get; private set; }
        public int[,] MaTran { get; private set; }
        public Board(int n)
        {
            N = n;
            MaTran = new int[N, N];
            Reset();
        }
        public void Reset()
        {
            for(int i = 0; i< N; i++)
            {
                for(int j = 0; j < N; j++)
                {
                    MaTran[i, j] = -1;
                }
            }
        }
        public bool HopLe(int x, int y)
        {
            return x >= 0 && x < N && y >= 0 && y < N && MaTran[x, y] == -1;
        }
        public void DanhDau(int x, int y , int buoc)
        {
            MaTran[x, y] = buoc;
        }
        public void XoaDau(int x , int y)
        {
            MaTran[x, y] = -1;
        }

    }
}

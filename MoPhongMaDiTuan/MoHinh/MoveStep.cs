using System;
using System.Collections.Generic;
using System.Text;

namespace MoPhongMaDiTuan.MoHinh
{
       public class MoveStep
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Buoc { get; set; }
        public bool LaQuayLui { get; set; }

        public MoveStep(int x ,int y,int buoc ,bool laQuayLui = false)
        {
            X = x;
            Y = y;
            Buoc = buoc;
            LaQuayLui = laQuayLui;
        }
    }
}

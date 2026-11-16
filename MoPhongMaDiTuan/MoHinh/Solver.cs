using System;
using System.Collections.Generic;
using System.Text;

namespace MoPhongMaDiTuan.MoHinh
{
   public abstract class Solver
    {
        public abstract string TenThuatToan { get; }
        public long SoLanQuayLui { get; protected set; }

        public abstract IEnumerable<MoveStep> Steps(Board board, Knight knight);
    }
}

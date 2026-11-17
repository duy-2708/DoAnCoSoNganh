using System;
using System.Collections.Generic;
using System.Text;

namespace MoPhongMaDiTuan.MoHinh
{
    public abstract class Solver
    {
        public abstract string TenThuatToan { get; }

        // Thống kê phục vụ yêu cầu "số nước đã thử, số lần quay lui" (mục 3.1.1)
        public long SoLanQuayLui { get; protected set; }
        public long SoNuocDaThu { get; protected set; }

        public abstract IEnumerable<MoveStep> Steps(Board board, Knight knight);
    }
}

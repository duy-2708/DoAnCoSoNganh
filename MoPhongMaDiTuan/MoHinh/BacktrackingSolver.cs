using System;
using System.Collections.Generic;
using System.Text;

namespace MoPhongMaDiTuan.MoHinh
{
    public class BacktrackingSolver : Solver
    {
        public override string TenThuatToan => "Quay lui vét cạn ";
        public override IEnumerable<MoveStep> Steps(Board b,Knight kn)
        {
            SoLanQuayLui = 0;
            b.Reset();
            b.DanhDau(kn.StartX,kn.StartY,0);
            yield return new MoveStep(kn.StartX, kn.StartY, 0);
            foreach(var step in GiaiMaDiTuan(kn.StartX, kn.StartY, 1, b, kn))
            {
                yield return step;
            }
        }
        private IEnumerable<MoveStep>GiaiMaDiTuan(int x , int y , int buoc ,Board b, Knight kn) {
              if(buoc == b.N * b.N)
            {
                yield return new MoveStep(-1, -1, buoc);
                yield break;
            }
              foreach(var(nx,ny) in kn.NuocDiHopLe(x, y, b))
            {
                b.DanhDau(nx, ny, buoc);
                yield return new MoveStep(nx, ny, buoc, false);
                bool timThay = false;
                foreach(var stepTam in GiaiMaDiTuan(nx, ny, 1, b, kn))
                {
                    if(stepTam.X == -1 && stepTam.Y == -1)
                    {
                        timThay = true;
                        yield return stepTam;
                        yield break;
                    }
                    yield return stepTam;
                }
                if (timThay) yield break;
                b.XoaDau(nx, ny);
                SoLanQuayLui++;
                yield return new MoveStep(nx, ny, -1, true);
            }
        }
    }

}

namespace WarnsDorff
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int N = 8;
            int[,] banco = new int[N, N];
            int[] dx = { -2, 2, -2, 2, -1, 1, -1, 1 };
            int[] dy = { 1, 1, -1, -1, 2, 2, -2, -2 };
            long dem = 0;
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
            int DemLoiThoat(int x, int y)
            {
                int count = 0;
                for (int i = 0; i < 8; i++)
                {
                    int nextx = x + dx[i];
                    int nexty = y + dy[i];
                    if (hople(nextx, nexty))
                    {
                        count++;
                    }
                }
                return count;
            }
            bool GiaiMaDiTuan(int x, int y, int buoc)

            {
                if (buoc == N * N)
                {
                    return true;
                }
                List<(int nx, int ny, int bac)> danhSachNuocDi = new List<(int, int, int)>();
                for (int i = 0; i < 8; i++)
                {

                    int nextx = x + dx[i];
                    int nexty = y + dy[i];
                    if (hople(nextx, nexty))
                    {
                        int bac = DemLoiThoat(nextx, nexty);
                        danhSachNuocDi.Add((nextx, nexty, bac));

                    }
                }
                danhSachNuocDi.Sort((a, b) => a.bac.CompareTo(b.bac));
                foreach (var nuocdi in danhSachNuocDi)
                {
                    banco[nuocdi.nx, nuocdi.ny] = buoc;
                    if (GiaiMaDiTuan(nuocdi.nx, nuocdi.ny, buoc + 1))
                    {
                        return true;
                    }
                    banco[nuocdi.nx, nuocdi.ny] = -1;
                    dem++;



                }
                return false;
            }
            banco[0, 0] = 0;
            if (GiaiMaDiTuan(0, 0, 1))
            {
                Console.WriteLine($"Tim thay hanh trinh Warnsdorff tren ban {N}x{N}:\n");
                Console.WriteLine($"Tong so lan thuc hien {dem}");
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

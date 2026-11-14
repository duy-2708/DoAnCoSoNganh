namespace BackTrack1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int N = 4;
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };


            int[,] meCung = {
                { 0, 0, 0, 1 },
                { 0, 1, 1, 1 },
                { 0, 0, 1, 0 },
                { 1, 0, 0, 0 }
            };
            bool HopLe(int x , int y)
            {
                return x >= 0 && x < N && y >= 0 && y < N && meCung[x, y] == 0;
            }
            bool Timduong(int x , int y)
            {
                if( x == N-1 && y == N-1 && meCung[x,y] == 0)
                {
                    meCung[x, y] = 2;
                    return true;
                }
                if (HopLe(x, y))
                {
                    meCung[x, y] = 2;
                    for( int i = 0; i < 4; i++)
                    {
                        if (Timduong(x + dx[i], y + dy[i]))
                            return true;
                    }
                    meCung[x, y] = 1;
                }
                return false;
            }
            if(Timduong(0,0))
            {
                for(int i = 0; i< N; i++)
                {
                    for( int j =0; j<N; j++)
                    {
                        Console.Write(meCung[i, j] == 2 ? "* " : ". ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.Write("Khong tim thay");
            }
        }
    }
}

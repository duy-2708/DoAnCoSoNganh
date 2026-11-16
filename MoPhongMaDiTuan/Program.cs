using System;
using System.Windows.Forms;
using MoPhongMaDiTuan.GiaoDien;

namespace MoPhongMaDiTuan
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
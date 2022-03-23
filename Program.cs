using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace UR_点动控制器
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                Console.WriteLine("Application.Run(new Form1()) throw");
                Console.WriteLine(e.ToString());
            }
        }
    }
}

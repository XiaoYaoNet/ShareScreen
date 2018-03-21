using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TCPClient
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
            Form1 f = new Form1();
            f.Visible = true;
            f.Hide();
            Application.Run(f);
            
        }
    }
}

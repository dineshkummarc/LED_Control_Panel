using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LED_Control_Panel
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
         
        public static ledControlPanel window; 
        
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            window= new ledControlPanel();
            Application.Run(window);
        }
    }
}

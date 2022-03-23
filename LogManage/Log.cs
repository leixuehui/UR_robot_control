using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManage
{
    public class Log
    {
        private enum ErrorType
        {
            W = 0,      //警告信息
            E = 1,      //错误信息
            I = 2,      //普通信息
        }
        
        private static string LogOutputFile = "";
        public static void renameLog()
        {
            DateTime dt = DateTime.Now;
            string time = dt.ToString().Replace(" ", "");
            time = time.Replace(":", "");
            time = time.Replace("/", "");
            LogOutputFile = string.Format(".\\Log\\{0}log.txt",time);
        }
        private static ErrorType errType = Log.ErrorType.I;
        private static bool WriteLogToFile(string module, string strlog)
        {
            try
            {
                if (File.Exists(Log.LogOutputFile) == false)
                {
                    FileStream fsm = File.Create(Log.LogOutputFile);
                    if (fsm != null)
                        fsm.Close();
                }

                DateTime dt = File.GetCreationTime(Log.LogOutputFile);
                dt = dt.AddDays(3);

                //删除三天前log信息
                if (dt.CompareTo(DateTime.Now) < 0)
                {
                    File.Delete(Log.LogOutputFile);
                    //File.Create(Log.LogOutputFile);
                }
                //时间
                string inputstr = DateTime.Now.ToString();
                inputstr += "  ";

                //错误类型
                inputstr += errType.ToString();
                inputstr += "  ";

                //模块名,只保留20个字符，文本统一
                string modulename = "";
                if (module.Length >= 20)
                    modulename = module.Substring(0, 20);
                else
                {
                    modulename = module;
                    do
                    {
                        modulename = modulename.Insert(modulename.Length, "-");
                    }
                    while (modulename.Length < 20);
                }
                //modulename = module;
                inputstr += modulename;
                inputstr += "  ";

                //log信息
                inputstr += strlog;

                //FileStream ot = File.OpenWrite(LogOutputFile);

                //ot.w
                StreamWriter output = File.AppendText(Log.LogOutputFile);
                output.WriteLine(inputstr);
                Console.WriteLine(inputstr);
                
                output.Close();

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool I(string module, string strlog)
        {
            Log.errType = ErrorType.I;
            return WriteLogToFile(module, strlog);
        }


        public static bool W(string module, string strlog)
        {
            Log.errType = ErrorType.W;
            return WriteLogToFile(module, strlog);
        }

        public static bool E(string module, string strlog)
        {
            Log.errType = ErrorType.E;
            return WriteLogToFile(module, strlog);
        }
        public static string GetTime()
        {
            DateTime dt = DateTime.Now;
            string time = dt.ToString().Replace(" ", "");
            time = time.Replace(":", "");
            time = time.Replace("/", "");
            return time;
        }
        public static void Clear()
        {
            if (File.Exists(Log.LogOutputFile) == true)
                File.Delete(Log.LogOutputFile);
        }
    }
}

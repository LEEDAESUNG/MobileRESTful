using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace MobileRestAPI
{
    class CommonUtil
    {
        private readonly object fileLock = new object();
        string sIniPath = ".\\MobileRestAPI.ini";
        string sDocPath = ".\\Doc\\";

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            String section,
            String key,
            string def,
            StringBuilder retVal,
            int size,
            String filePath
        );
        //public String Get_Ini(String section, String key)
        //{
        //    StringBuilder temp = new StringBuilder(255);
        //    int i = GetPrivateProfileString(section, key, "", temp, 255, sIniPath);
        //    return temp.ToString();
        //}
        public String Get_Ini(String section, String key, String Default_Value)
        {
            StringBuilder RetVal = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, Default_Value, RetVal, 255, sIniPath); //ANSI 인코딩 파일 로드

            //byte[] buffer = Encoding.ASCII.GetBytes(RetVal.ToString());
            //string text = Encoding.Default.GetString(buffer);
            //StreamReader sr = new StreamReader(RetVal.ToString(), Encoding.Default);

            if (RetVal.Length == 0)
                return Default_Value;
            else
                return RetVal.ToString();
        }

        public void Delay_Time(int ms)
        {
            //DateTime nowTime = DateTime.Now;
            //TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            //DateTime afterTime = nowTime.Add(duration);
            //while(afterTime>=nowTime)
            //{
            //    System.Windows.Forms.Application.DoEvents();
            //    nowTime = DateTime.Now;
            //}
            ////return DateTime.Now;
        }

        public void FileLogger(string log)
        {
            string sDate;
            string sTime;
            string sDateTime;
            string sLog;

            sDate = DateTime.Now.ToString("yyyy-MM-dd");
            sTime = DateTime.Now.ToString("HH:mm:ss");
            sDateTime = sDate + " " + sTime;
            sLog = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + log;

            lock (fileLock)
            {
                try
                {
                    if (!File.Exists(sDocPath + "Mobile_" + sDate + ".txt"))
                    {
                        using (StreamWriter sw = File.CreateText(sDocPath + "Mobile_" + sDate + ".txt"))
                        {
                        }
                    }

                    using (StreamWriter sw = File.AppendText(sDocPath + "Mobile_" + sDate + ".txt"))
                    {
                        sw.WriteLine(sLog);
                    }
                }
                catch(Exception ex) {
                    Console.WriteLine(sDocPath + "Mobile_" + sDate + ".txt" + ex.Message);
                }
            }
        }

        //UTF8 문자열을 euc-kr문자열로 변환한다.
        public string UTF8_TO_EUCKR(string strUTF8)
        {

            return Encoding.GetEncoding("euc-kr").GetString(
                Encoding.Convert(
                Encoding.UTF8,
                Encoding.GetEncoding("euc-kr"),
                Encoding.UTF8.GetBytes(strUTF8)));
        }

        //euc-kr 문자열을 UTF8문자열로 변환한다.
        public string EUCKR_TO_UTF8(string strEUCKR)
        {
            return Encoding.UTF8.GetString(
                   Encoding.Convert(
                   Encoding.GetEncoding("euc-kr"),
                   Encoding.UTF8,
                   Encoding.GetEncoding("euc-kr").GetBytes(strEUCKR)));
        }

    }
}

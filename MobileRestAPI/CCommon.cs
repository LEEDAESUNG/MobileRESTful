using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MobileRestAPI
{
    public class CCommonUtil
    {
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
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

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
        //Public Sub Put_Ini(App_Name As String, Key_Name As String, Put_Data As String)
        //Dim tmp
        //tmp = WritePrivateProfileString(App_Name, Key_Name, Put_Data, IniFileName$)
        //End Sub

        public void Put_Ini(String section, String key, String Default_Value)
        {
            WritePrivateProfileString(section, key, Default_Value, sIniPath);
        }


        public void Delay_Time(int ms)
        {
            DateTime nowTime = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, ms);
            DateTime afterTime = nowTime.Add(duration);
            while (afterTime >= nowTime)
            {
                //System.Windows.Forms.Application.DoEvents();
                nowTime = DateTime.Now;
            }
            //return DateTime.Now;
        }

        public void FileLogger(string log)
        {
            string sDate;
            string sLog;

            sDate = DateTime.Now.ToString("yyyyMMdd");
            sLog = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + log;

            //string CurrentDirectory = Directory.GetCurrentDirectory() + "\\Doc";
            //DirectoryInfo di = new DirectoryInfo(CurrentDirectory);


            try
            {
                lock (Global.filelogLock)
                {
                    DirectoryInfo di = new DirectoryInfo(sDocPath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }

                    if (!File.Exists(sDocPath + sDate + ".txt"))
                    {
                        using (StreamWriter sw = File.CreateText(sDocPath + sDate + ".txt"))
                        {
                        }
                    }

                    using (StreamWriter sw = File.AppendText(sDocPath + sDate + ".txt"))
                    {
                        sw.WriteLine(sLog);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(sLog + " 로그파일생성 오류(FileLogger)!! 시스템점검 바랍니다. \n" + ex.Message, " \n 시스템오류 \n", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#if DEBUG
            Console.WriteLine(sLog);
#endif
        }
        public void ErrLogger(string log)
        {
            FileLogger(log);

            string sDate;
            string sLog;

            sDate = DateTime.Now.ToString("yyyyMMdd");
            sLog = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + log;

            try
            {
                lock (Global.errlogLock)
                {
                    //string CurrentDirectory = Directory.GetCurrentDirectory() + "\\Doc";
                    //DirectoryInfo di = new DirectoryInfo(CurrentDirectory);
                    DirectoryInfo di = new DirectoryInfo(sDocPath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }

                    if (!File.Exists(sDocPath + "Error_" + sDate + ".txt"))
                    {
                        using (StreamWriter sw = File.CreateText(sDocPath + "Error_" + sDate + ".txt"))
                        {
                        }
                    }
                    using (StreamWriter sw = File.AppendText(sDocPath + "Error_" + sDate + ".txt"))
                    {
                        sw.WriteLine(sLog);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("로그파일생성 오류(ErrLogger)!! 시스템점검 바랍니다. \n" + ex.Message, " \n 시스템오류 \n", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#if DEBUG
            Console.WriteLine(sLog);
#endif
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

        public string Left(string _str, int _length)
        {
            if (_str.Length < _length)
            {
                _length = _str.Length;
            }
            return _str.Substring(0, _length);
        }
        public string Right(string _str, int _length)
        {
            if (_str.Length < _length)
            {
                _length = _str.Length;
            }
            return _str.Substring(_str.Length - _length, _length);
        }
        public string Mid(string _str, int _start, int _end)
        {
            if (_start < _str.Length || _end < _str.Length)
                return _str.Substring(_start, _end);
            else
                return _str;
        }
        public int LenH(string _str)
        {
            int byteLength = Encoding.Default.GetByteCount(_str);
            return byteLength;
        }
        public string LeftH(string _str, int _length)
        {
            int retLength = 0;
            int byteLength = Encoding.Default.GetByteCount(_str);
            byte[] buf = System.Text.Encoding.Default.GetBytes(_str);

            if (_length < byteLength)
            {
                retLength = _length;
            }
            else
            {
                retLength = byteLength;
            }
            return System.Text.Encoding.Default.GetString(buf, 0, retLength);
        }
        public string RightH(string _str, int _length)
        {
            int byteLength = Encoding.Default.GetByteCount(_str);
            byte[] buf = System.Text.Encoding.Default.GetBytes(_str);

            return System.Text.Encoding.Default.GetString(buf, byteLength - _length, _length);
        }
        public string MidH(string _str, int _start, int _length)
        {
            int byteLength = Encoding.Default.GetByteCount(_str);
            byte[] buf = System.Text.Encoding.Default.GetBytes(_str);

            return System.Text.Encoding.Default.GetString(buf, _start, _length);
        }

        //정규식,문자열에서 숫자만 가져오기
        public string GetNumeric(string _str)
        {
            string val = Regex.Replace(_str, @"D", "");
            return val;
        }
        //정규식,문자열에서 알파벳문자만 가져오기
        public string GetAlphabetic(string _str)
        {
            string val = Regex.Replace(_str, @"[^a-zA-Z]", "");
            return val;
        }
        //정규식,문자열에서 한글만 가져오기
        public string GetKorean(string _str)
        {
            string val = Regex.Replace(_str, @"[^가-힣]", "");
            return val;
        }
        //정규식,문자열에서 특수문자제거(숫자,영문자,한글만 가져오기)
        public string GetStringExceptSpecial(string _str)
        {
            string val = Regex.Replace(_str, @"[^0-9a-zA-Z가-힣]", "");
            return val;
        }

        /// <summary>
        /// 외부 공인IP 가져오기
        /// </summary>
        /// <returns></returns>
        public string GetExternalIPAddress()
        {
            string externalip = new WebClient().DownloadString("http://ipinfo.io/ip").Trim(); //http://icanhazip.com
            if (String.IsNullOrWhiteSpace(externalip))
            {
                externalip = GetInternalIPAddress();//null경우 Get Internal IP를 가져오게 한다.
            }
            return externalip;
        }
        public string GetInternalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        /// <summary>
        /// 맥어드레스 가져오기
        /// </summary>
        /// <returns></returns>
        public string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString();
        }
        public string GetHashCode(string _key)
        {
            //return JwtEncoder.JwtEncode(_key, CDefind.PROJECT_KEY);
            return "";
        }

        /// <summary>
        /// Convert Byte to HexString
        /// </summary>
        /// <param name="convertArr"></param>
        /// <returns></returns>
        public static string ConvertByteToHexString(byte[] convertArr)
        {
            string convertArrString = string.Empty;
            convertArrString = string.Concat(Array.ConvertAll(convertArr, byt => byt.ToString("X2")));
            return convertArrString;
        }

        /// <summary>
        /// Convert HexString to Byte
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexToByte(string _hexString)
        {
            if (_hexString == null) return null;
            if (_hexString.Length % 2 == 1) return null;

            byte[] byteArr = new byte[_hexString.Length / 2];
            for (int i = 0; i < byteArr.Length; i++)
            {
                //byteArr[i] = Convert.ToByte(_hexString.Substring(i * 2), 16);
                byteArr[i] = Convert.ToByte(_hexString.Substring(i * 2, 2), 16);
            }
            return byteArr;
        }
    }
}

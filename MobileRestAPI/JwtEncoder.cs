
namespace MobileRestAPI
{
    using System;
    using System.Text;

    /// <summary>
    /// 
    /// </summary>
    public class JwtEncoder
    {
        //// 엔코딩 테이블 c     
        private static readonly string[] EncodeTable =
        {
            @"y.s1[*m!PR#;J8C6Io<n`w:zB$""D>Sq),?N0lGL@_WfT&794^jv%Ftr{3~kEuUQ]p|=i'K(5dcAVb\Z/a2}xhe+-MHOYXg",
            @"{;aev>3NqHT2^xMDFBP[#]o/9?EK,m<0ZU.iS\bsOL=6R4(I@G*_kWQ""jg~X5$8+'c17)A}r%Vu-!tdlnhwJCf&|:pyzY`",
            @"3pM-]V(;Lf$|%sAnl<2.8#U@>+QKy\obWq*FtXk'&dhBjx9rGTYRe=:D6}[NZcJ5,^vHO1IazwEm{_u7)g~S""C`P0i/4?!",
            @"-""+Fj9=]Crh<\2@JWG7yzw6eq5/ml&v3k,oHD#n}~p(?41Za:IV{R_U8;0td)%N.KPMb!`LOY*f|T'AguX>^$x[cSiEsBQ",
            @"*_7L~Z?'8F}!>P=-xc[Xs^l$pwG]&4C\h)Yidv1,Wbr`0Nn;RzoT.EHDM:@{k65AJO|m#uV/<f+Baq(Q%yKtjI9S3""e2Ug"
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string JwtEncode(string msg, string key)
        {
            string _msg = Reverse(msg);
            string _key = Reverse(key);
            //List<string> table = new List<string>();
            string result = string.Empty;

            //// 역문자열을 유니코드 바이트 배열로 변환
            byte[] bytesMsg = Encoding.Unicode.GetBytes(_msg);

            //// 2바이트 간격으로 자리를 바꾼다. VB코드로직에 맞춰주기위함.
            for(int i =0; i<bytesMsg.Length;i+=2)
            {
                byte temp = bytesMsg[i];
                bytesMsg[i] = bytesMsg[i + 1];
                bytesMsg[i + 1] = temp;
            }

            //// 역키와 바이트 배열간 XOR 연산
            for (int i = 0; i < bytesMsg.Length; i++)
            {
                byte[] sid = Encoding.ASCII.GetBytes(_key.Substring(i % _key.Length, 1)); ////역키의 0번 인덱스 문자부터 마지막 문자까지, 매 루틴마다 한글자씩 인출
                bytesMsg[i] = (byte)(bytesMsg[i] ^ sid[0]);
            }

            ////키의 각 값과 바이트 배열을 XOR 연산
            for(int i =0; i<bytesMsg.Length;i++)
            {
                for(int j =0; j<key.Length; j++)
                {
                    byte[] sid = Encoding.ASCII.GetBytes(key.Substring(j % key.Length, 1));
                    bytesMsg[i] = (byte)(bytesMsg[i] ^ sid[0]);
                }
            }

            StringBuilder resultSb = new StringBuilder();
            for (int i =0; i<bytesMsg.Length;i++)
            {
                int fir, sec, thr;
                fir = bytesMsg[i] % 5;
                sec = bytesMsg[i] / 94;
                thr = bytesMsg[i] % 94;
                //result = result + EncodeTable[fir].Substring(sec, 1) + EncodeTable[fir].Substring(thr, 1);
                resultSb.Append(EncodeTable[fir].Substring(sec, 1));
                resultSb.Append(EncodeTable[fir].Substring(thr, 1));
            }
            result = Reverse(resultSb.ToString());

            return result;
        }
        
        public static string JwtDecode(string msg, string key)
        {
            string _msg = Reverse(msg);
            string _key = Reverse(key);
            string result = string.Empty;

            //// 역문자열을 유니코드 바이트 배열로 변환
            byte[] bytesMsg = new byte[_msg.Length / 2];

            ////테이블과 대조
            for(int i =0; i<_msg.Length; i+=2)
            {
                char firstChar = _msg[i];
                char secondChar = _msg[i + 1];
                for(int j =0; j< EncodeTable.Length;j++)
                {
                    if (EncodeTable[j].Substring(0,3).IndexOf(firstChar) != -1)
                    {                    
                        int sec = EncodeTable[j].IndexOf(firstChar) * 94;
                        int thr = EncodeTable[j].IndexOf(secondChar);
                        bytesMsg[i/2] = (byte)(sec+thr);
                    }
                }
                
            }
            
            for (int i = 0; i < bytesMsg.Length; i++)
            {
                foreach (char keyChar in key)
                {
                    bytesMsg[i] = (byte)(bytesMsg[i] ^ keyChar);
                }
            }

            //// 역키와 바이트 배열간 XOR 연산
            for (int i = 0; i < bytesMsg.Length; i++)
            {
                byte[] sid = Encoding.ASCII.GetBytes(_key.Substring(i % _key.Length, 1)); ////역키의 0번 인덱스 문자부터 마지막 문자까지, 매 루틴마다 한글자씩 인출
                bytesMsg[i] = (byte)(bytesMsg[i] ^ sid[0]);
            }

            //// 2바이트 간격으로 자리를 바꾼다. VB코드로직에 맞춰주기위함.
            for (int i = 0; i < bytesMsg.Length; i += 2)
            {
                byte temp = bytesMsg[i];
                bytesMsg[i] = bytesMsg[i + 1];
                bytesMsg[i + 1] = temp;
            }

            result = Reverse(Encoding.Unicode.GetString(bytesMsg));

            return result;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strByte"></param>
        /// <returns></returns>
        private static string ByteToString(byte[] strByte)
        {
            string str = Encoding.Default.GetString(strByte);
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static byte[] StringToByte(string str)
        {
            byte[] StrByte = Encoding.Unicode.GetBytes(str);

            for (int k = 0; k < StrByte.Length; k += 2)
            {
                byte temp = StrByte[k];
                StrByte[k] = StrByte[k + 1];
                StrByte[k + 1] = temp;
            }

            return StrByte;
        }

    }
}

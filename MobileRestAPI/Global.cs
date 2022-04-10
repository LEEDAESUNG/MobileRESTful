
using MobileRestAPI.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using SimpleTCP;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Messaging;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace MobileRestAPI
{
    

    public class ReturnCode
    {
        static public int Success = 1;
        static public int UnAuthInvalidToken1 = 2;
        static public int UnAuthInvalidToken2 = 3;
        static public int InvalidParam = 4;
        static public int InvalidParam_Carno = 5;
        static public int InvalidParam_ExpireDate = 6;
        static public int NotFound = 7;
        static public int NotSupport = 8;
        static public int CarnoRecognitionError = 9;
        static public int InternalServerError = 99;
        
    }
    public class JsonReturn
    {
        public string httpcode = "";
        public string msg = "";
        public string detail_msg = "";
    }
    public class Global
    {
        public static readonly string connStr = "server=localhost;user=root;password=jawootek;database=jwt_sanps;Charset=utf8";
        public static CCommonUtil util = new CCommonUtil();
        public static string RootPath = System.IO.Directory.GetCurrentDirectory();
        public static string APIServerIP = "127.0.0.1";
        public static int APIServerPORT = 39157;




        static public string _rootPath;
        static public string _authFilePath;
        static public string result;

        public static readonly object errlogLock = new object();
        public static readonly object filelogLock = new object();
        public static readonly object saveImageLock = new object();

        public static long m_nRecogInitDLL = 0;

        static public string BearerTocken = "c5ZPrqf11rnhE9REQ5nzfDSzYBAzQgLQ";
        static public bool AuthCheck(string tocken)
        {
            bool result = false;
            try
            {
                if (tocken != null)
                {
                    string[] words = tocken.Split(' ');
                    if (words[0].ToLower() == "bearer" && words[1] == Global.BearerTocken)
                        result = true;
                }
            }
            catch(Exception e)
            {
                result = false;
            }
            return result;
        }
        static public int InsParamCheck(RegistryCarModel model)
        {
            if (model.carno == null || model.carno.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (model.dong == null || model.dong.Length <= 0) return ReturnCode.InvalidParam;
            if (model.ho == null || model.ho.Length <= 0) return ReturnCode.InvalidParam;
            if (model.effStart == null || model.effStart.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (model.effEnd == null || model.effEnd.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (model.name == null || model.name.Length <= 0) return ReturnCode.InvalidParam;
            if (model.contact == null || model.contact.Length <= 0) return ReturnCode.InvalidParam;

            //Date 타입 유효성검사
            DateTime dtRtn;
            var formats = new[] { "yyyy-MM-dd" };
            if (!DateTime.TryParseExact(model.effStart, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtRtn))
                return ReturnCode.InvalidParam;
            if (!DateTime.TryParseExact(model.effEnd, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtRtn))
                return ReturnCode.InvalidParam;
            if (string.Compare(model.effStart, model.effEnd) > 0) return ReturnCode.InvalidParam;

            //날짜차이 3일까지만 허용
            DateTime T1 = DateTime.Parse(model.effStart);
            DateTime T2 = DateTime.Parse(model.effEnd);
            TimeSpan TS = T2 - T1;
            int diffDay = TS.Days;  //날짜의 차이 구하기
            int diffHour = TS.Hours;  //날짜의 차이 구하기
            int diffMin = TS.Minutes;  //날짜의 차이 구하기
            int diffSec = TS.Seconds;  //날짜의 차이 구하기
            if (diffDay >= 0 && diffHour >= 0 && diffMin >= 0 && diffSec >= 0)
                return ReturnCode.Success;
            else
                return ReturnCode.InvalidParam;

            return ReturnCode.Success;
        }
        static public int DelParamCheck(RegistryCarModel model)
        {
            if (model.carno == null || model.carno.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (model.dong == null || model.dong.Length <= 0) return ReturnCode.InvalidParam;
            if (model.ho == null || model.ho.Length <= 0) return ReturnCode.InvalidParam;
            if (model.tkno == null || model.tkno.Length <= 0) return ReturnCode.InvalidParam;

            return ReturnCode.Success;
        }
        static public int ReserveDelParamCheck(RegistryCarModel model)
        {
            if (model.carno == null || model.carno.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (model.dong == null || model.dong.Length <= 0) return ReturnCode.InvalidParam;
            if (model.ho == null || model.ho.Length <= 0) return ReturnCode.InvalidParam;
            if (model.belong == null || model.belong.Length <= 0) return ReturnCode.InvalidParam;

            return ReturnCode.Success;
        }
        static public int getReserveParamCheck(string carno, string dong, string ho)
        {
            if (carno == null || carno.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (dong == null || dong.Length <= 0) return ReturnCode.InvalidParam;
            if (ho == null || ho.Length <= 0) return ReturnCode.InvalidParam;

            carno = carno.Trim();
            dong = dong.Trim();
            ho = ho.Trim();

            return ReturnCode.Success;
        }
        static public int IOSDataParamCheck(string carno, string sDateTime, string eDateTime, int maxSearchDay)
        {
            if (carno == null || carno.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (sDateTime == null || sDateTime.Length <= 0) return ReturnCode.InvalidParam;
            if (eDateTime == null || eDateTime.Length <= 0) return ReturnCode.InvalidParam;

            carno = carno.Trim();
            sDateTime = sDateTime.Trim();
            eDateTime = eDateTime.Trim();

            //DateTime 타입 유효성검사
            DateTime dtRtn;
            var formats = new[] { "yyyy-MM-dd HH:mm:ss" };
            if (!DateTime.TryParseExact(sDateTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtRtn))
                return ReturnCode.InvalidParam;
            if (!DateTime.TryParseExact(eDateTime, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtRtn))
                return ReturnCode.InvalidParam;

            //날짜차이 3일까지만 허용
            DateTime T1 = DateTime.Parse(sDateTime);
            DateTime T2 = DateTime.Parse(eDateTime);
            TimeSpan TS = T2 - T1;
            int diffDay = TS.Days;  //날짜의 차이 구하기
            int diffHour = TS.Hours;  //날짜의 차이 구하기
            int diffMin = TS.Minutes;  //날짜의 차이 구하기
            int diffSec = TS.Seconds;  //날짜의 차이 구하기
            if (diffDay >= 0 && diffDay <= maxSearchDay && diffHour>= 0 && diffMin >= 0 && diffSec >= 0)
                return ReturnCode.Success;
            else
                return ReturnCode.InvalidParam;
        }
        static public int ReserveParamCheck(RegistryCarModel model)
        {
            if (model.carno == null || model.carno.Length <= 0) return ReturnCode.InvalidParam_Carno;
            if (model.dong == null || model.dong.Length <= 0) return ReturnCode.InvalidParam;
            if (model.ho == null || model.ho.Length <= 0) return ReturnCode.InvalidParam;

            model.carno = model.carno.Trim();
            model.dong= model.dong.Trim();
            model.ho = model.ho.Trim();
            model.reservestart = model.reservestart.Trim();
            model.reserveend = model.reserveend.Trim();
            model.remark = model.remark.Trim();

            //DateTime 타입 유효성검사
            DateTime dtRtn;
            var formats = new[] { "yyyy-MM-dd" };
            if (!DateTime.TryParseExact(model.reservestart, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtRtn))
                return ReturnCode.InvalidParam;
            if (!DateTime.TryParseExact(model.reserveend, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtRtn))
                return ReturnCode.InvalidParam;

            //예약시작일<예약종료일 판단
            DateTime T1 = DateTime.Parse(model.reservestart);
            DateTime T2 = DateTime.Parse(model.reserveend);
            TimeSpan TS = T2 - T1;
            int diffDay = TS.Days;  //날짜의 차이 구하기
            int diffHour = TS.Hours;  //날짜의 차이 구하기
            int diffMin = TS.Minutes;  //날짜의 차이 구하기
            int diffSec = TS.Seconds;  //날짜의 차이 구하기
            if (diffDay >= 0 && diffHour >= 0 && diffMin >= 0 && diffSec >= 0)
                return ReturnCode.Success;
            else
                return ReturnCode.InvalidParam;
        }
        //정기권등록
        static public void RegistCar(RegistryCarModel model, ref int result, ref string respTime)
        {
            int _result = -1;

            if (model == null) return;
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "INSERT INTO tb_reg (CAR_GUBUN, CAR_NO,DRIVER_DEPT,DRIVER_CLASS,START_DATE,END_DATE,DRIVER_NAME,DRIVER_PHONE,ETC, REG_DATE,DAY_ROTATION_YN,LANE1,LANE2,LANE3,LANE4,LANE5,LANE6,WEEK1,WEEK2,WEEK3,WEEK4,WEEK5,WEEK6,WEEK7) VALUES ('정기권',@carno,@dong,@ho,@effStart,@effEnd,@name,@contact,@remark,@regTime,'적용','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y')";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", model.carno);
                mySqlCommand.Parameters.AddWithValue("@dong", model.dong);
                mySqlCommand.Parameters.AddWithValue("@ho", model.ho);
                mySqlCommand.Parameters.AddWithValue("@effStart", model.effStart + " 00:00:00");
                mySqlCommand.Parameters.AddWithValue("@effEnd", model.effEnd + " 23:59:59");
                mySqlCommand.Parameters.AddWithValue("@name", model.name);
                mySqlCommand.Parameters.AddWithValue("@contact", model.contact);
                mySqlCommand.Parameters.AddWithValue("@remark", model.remark);
                mySqlCommand.Parameters.AddWithValue("@regTime", nowTime);
                if(mySqlCommand.ExecuteNonQuery() == 1)
                {
                    _result = 1;//성공
                    respTime = nowTime;
                }
                else
                {
                    _result = 0;//실패
                    respTime = nowTime;
                    util.FileLogger("[ERROR][DataBase Insert] " + "입력실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.effStart + "," + model.effEnd + "," + model.name + "," + model.contact + "," + model.remark);
                }
            }
            catch (Exception ex)
            {
                _result = -1; //내부오류
                respTime = nowTime;
                util.FileLogger("[ERROR][DataBase Insert] " + ex.Message);
            }
            finally
            {
                result = _result;
                respTime = nowTime;
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        /// <summary>
        /// 방문예약 등록
        /// </summary>
        /// <param name="model"></param>
        /// <param name="result"></param>
        /// <param name="respTime"></param>
        static public void ReserveCar(RegistryCarModel model, ref int result, ref string respTime)
        {
            int _result = -1;

            if (model == null) return;
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "INSERT INTO tb_guestreg (CAR_GUBUN, CAR_NO,DRIVER_DEPT,DRIVER_CLASS,START_DATE,END_DATE,ETC,REG_DATE, DAY_ROTATION_YN,LANE1,LANE2,LANE3,LANE4,LANE5,LANE6,WEEK1,WEEK2,WEEK3,WEEK4,WEEK5,WEEK6,WEEK7,GUESTREG_ID) VALUES ('방문예약',@carno,@dong,@ho,@reserveStart,@reserveEnd,@remark,@regTime,'적용','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Mobile')";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", model.carno);
                mySqlCommand.Parameters.AddWithValue("@dong", model.dong);
                mySqlCommand.Parameters.AddWithValue("@ho", model.ho);
                mySqlCommand.Parameters.AddWithValue("@reserveStart", model.reservestart + " 00:00:00");
                mySqlCommand.Parameters.AddWithValue("@reserveEnd", model.reserveend + " 23:59:59");
                mySqlCommand.Parameters.AddWithValue("@remark", model.remark);
                mySqlCommand.Parameters.AddWithValue("@regTime", nowTime);
                if (mySqlCommand.ExecuteNonQuery() == 1)
                {
                    _result = 1;//성공
                    respTime = nowTime;
                }
                else
                {
                    _result = 0;//실패
                    respTime = nowTime;
                    util.FileLogger("[ERROR][DataBase ReserveCar] " + "검색실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.reservestart + "," + model.reserveend + "," + model.remark);
                }
            }
            catch (Exception ex)
            {
                _result = -1; //내부오류
                respTime = nowTime;
                util.FileLogger("[ERROR][DataBase ReserveCar] " + ex.Message);
            }
            finally
            {
                result = _result;
                respTime = nowTime;
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void ReserveInfo(string _carno, string _dong, string _ho, ref JObject respJson)
        {
            string tmpCarno = "";
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_guestreg WHERE CAR_NO=@carno AND DRIVER_DEPT=@dong AND DRIVER_CLASS=@ho ORDER BY REG_DATE";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", _carno);
                mySqlCommand.Parameters.AddWithValue("@dong", _dong);
                mySqlCommand.Parameters.AddWithValue("@ho", _ho);
                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                while (mySqlDataReader.Read())
                {
                    string dong = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("DRIVER_DEPT"));
                    string ho = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("DRIVER_CLASS"));
                    string carNo = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("CAR_NO")); tmpCarno = carNo;
                    string reserveStart = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("START_DATE"));
                    string reserveEnd = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("END_DATE"));
                    string remark = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("ETC"));
                    string tkNo = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("SEQ"));

                    var jElem = new JObject();
                    jElem.Add("dong", dong);
                    jElem.Add("ho", ho);
                    jElem.Add("carNo", carNo);
                    jElem.Add("reserveStart", reserveStart.Substring(0, 10));
                    jElem.Add("reserveEnd", reserveEnd.Substring(0, 10));
                    jElem.Add("remark", remark);
                    jElem.Add("belong", tkNo);
                    JBody.Add(jElem);
                }

                JHead.Add("values", JBody);
                respJson = JHead;

                mySqlDataReader.Close();
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase ReserveInfo] " + ex.Message + " " + tmpCarno);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        static public void gateOpen(string _id, string _password, string _cmd, string _gateno, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            bool isMember = false;
            string res = "FAILED";

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();


                var sql = "SELECT * FROM tb_id WHERE ID = @id AND (MENU10 = @password) ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", _password);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;


                    var client = new SimpleTcpClient().Connect(APIServerIP, APIServerPORT);
                    SimpleTCP.Message message = client.WriteLineAndGetReply(_cmd + "_" + _gateno, TimeSpan.FromSeconds(5000));

                    util.FileLogger("[Recv Host gateOpen Result] " + message.MessageString);
                    if (message.MessageString == "ACK")
                        res = "SUCCESS";
                    else
                        res = "FAILED";
                    client.Dispose();


                    
                    var jElem = new JObject();
                    jElem.Add("cmd", _cmd);
                    jElem.Add("gateno", _gateno);
                    jElem.Add("result", res); //FAIL or SUCCESS
                    JBody.Add(jElem);
                }

                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;

                mySqlDataReader.Close();
            }
            catch(SocketException se)
            {
                util.FileLogger("[ERROR][Recv Host gateOpen Socket Error] " + se.ErrorCode);
                util.FileLogger("[ERROR][Recv Host gateOpen Socket Error] " + se.Message);
                if( se.ErrorCode == 10061)
                {
                    var jElem = new JObject();
                    jElem.Add("cmd", _cmd);
                    jElem.Add("gateno", _gateno);
                    jElem.Add("result", res); //FAIL or SUCCESS
                    JBody.Add(jElem);

                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][Recv Host gateOpen Error] " + ex.Message);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void gateList(string _id, string _password, string _cmd, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                //var sql = "SELECT * FROM tb_id WHERE ID = @id AND (MENU10 = @password) ";
                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_cctv";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string seq = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("SEQ"));
                        string url = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("url"));
                        string comments = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("comments"));
                        string ex1 = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("ex1"));
                        string ex2 = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("ex2"));
                        string stream_yn = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("stream_yn"));
                        string gateno = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("gateno"));
                        string gate_yn = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("gate_yn"));
                        string gubun = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("gubun"));
                        var jElem2 = new JObject();
                        jElem2.Add("seq", seq);
                        jElem2.Add("url", url);
                        jElem2.Add("comments", comments);
                        jElem2.Add("ex1", ex1);
                        jElem2.Add("ex2", ex2);
                        jElem2.Add("stream_yn", stream_yn);
                        jElem2.Add("gateno", gateno);
                        jElem2.Add("gate_yn", gate_yn);
                        jElem2.Add("gubun", gubun);
                        JBody.Add(jElem2);
                        //jElem.RemoveAll();
                    }
                    mySqlDataReader2.Close();
                    //util.FileLogger(APIServerIP + ":" + APIServerPORT.ToString());
                    //var client = new SimpleTcpClient().Connect(APIServerIP, APIServerPORT);
                    //Message message = client.WriteLineAndGetReply("GATELIST_ALL", TimeSpan.FromSeconds(5000));
                    //util.FileLogger("[Recv Host gateList Result] " + Encoding.UTF8.GetString(message.Data));
                    //////res = message.MessageString;
                    //res = Encoding.UTF8.GetString(message.Data);
                    //client.Dispose();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("cmd", _cmd);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][Recv Host gateList Socket Error] " + se.ErrorCode);
                util.FileLogger("[ERROR][Recv Host gateList Socket Error] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("url", "error");
                    jElem.Add("comments", "error");
                    jElem.Add("ex1", "error");
                    jElem.Add("ex2", "error");
                    jElem.Add("hostStream_yn", "error");
                    jElem.Add("gateno", "error");
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("cmd", _cmd);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][Recv Host gateList Error] " + ex.Message);

                jElem.Add("url", "error");
                jElem.Add("comments", "error");
                jElem.Add("ex1", "error");
                jElem.Add("ex2", "error");
                jElem.Add("hostStream_yn", "error");
                jElem.Add("gateno", "error");
                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("cmd", _cmd);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void getCarnoInfo(string _id, string _password, string _token, string _carno, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_reg where CAR_NO = @carno ";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    mySqlCommand2.Parameters.AddWithValue("@carno", _carno);
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    //while (mySqlDataReader2.Read())

                    string seq = "-1";
                    string car_no = "";
                    string car_model = "";
                    string car_gubun = "";
                    string driver_name = "";
                    string driver_phone = "";
                    string driver_dept = "";
                    string driver_class = "";
                    string start_date = "";
                    string end_date = "";
                    string etc = "";
                    var jElem2 = new JObject();
                    if (mySqlDataReader2.Read())
                    {
                        seq = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("SEQ"));
                        car_no = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("CAR_NO"));
                        car_model = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("CAR_MODEL"));
                        car_gubun = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("CAR_GUBUN"));
                        driver_name = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_NAME"));
                        driver_phone = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_PHONE"));
                        driver_dept = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_DEPT"));
                        driver_class = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_CLASS"));
                        start_date = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("START_DATE"));
                        end_date = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("END_DATE"));
                        etc = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("ETC"));
                    } 
                    else
                    {
                        car_no = _carno;

                        if (car_no == "인식실패")
                            car_gubun = "";
                        else
                            car_gubun = "미등록차량";
                    }
                     
                    jElem2.Add("seq", seq);
                    jElem2.Add("car_no", car_no);
                    jElem2.Add("car_model", car_model);
                    jElem2.Add("car_gubun", car_gubun);
                    jElem2.Add("driver_name", driver_name);
                    jElem2.Add("driver_phone", driver_phone);
                    jElem2.Add("driver_dept", driver_dept);
                    jElem2.Add("driver_class", driver_class);
                    jElem2.Add("start_date", start_date);
                    jElem2.Add("end_date", end_date);
                    jElem2.Add("etc", etc);
                    JBody.Add(jElem2);

                    mySqlDataReader2.Close();
                    //util.FileLogger(APIServerIP + ":" + APIServerPORT.ToString());
                    //var client = new SimpleTcpClient().Connect(APIServerIP, APIServerPORT);
                    //Message message = client.WriteLineAndGetReply("GATELIST_ALL", TimeSpan.FromSeconds(5000));
                    //util.FileLogger("[Recv Host gateList Result] " + Encoding.UTF8.GetString(message.Data));
                    //////res = message.MessageString;
                    //res = Encoding.UTF8.GetString(message.Data);
                    //client.Dispose();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("cmd", "carnosearch");
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][Recv Host gateList Socket Error] " + se.ErrorCode);
                util.FileLogger("[ERROR][Recv Host gateList Socket Error] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("url", "error");
                    jElem.Add("comments", "error");
                    jElem.Add("ex1", "error");
                    jElem.Add("ex2", "error");
                    jElem.Add("hostStream_yn", "error");
                    jElem.Add("gateno", "error");
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("cmd", "carnosearch");
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][Recv Host gateList Error] " + ex.Message);

                jElem.Add("url", "error");
                jElem.Add("comments", "error");
                jElem.Add("ex1", "error");
                jElem.Add("ex2", "error");
                jElem.Add("hostStream_yn", "error");
                jElem.Add("gateno", "error");
                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("cmd", "carnosearch");
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void custInfo(string _id, string _password, string _token, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            bool isMember = false;

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                //var sql = "SELECT * FROM tb_id WHERE ID = @id AND (MENU10 = @password) ";
                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                while (mySqlDataReader.Read())
                {
                    isMember = true;
                    GetStringFieldValue(mySqlDataReader, "GUBUN"); //차량구분

                    string tkNo = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("SEQ"));
                    string id = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("ID"));
                    string gubun = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("GUBUN"));
                    string mobile1 = GetStringFieldValue(mySqlDataReader, "MOBILE1");
                    string mobile2 = GetStringFieldValue(mySqlDataReader, "MOBILE2");
                    string mobile3 = GetStringFieldValue(mySqlDataReader, "MOBILE3");
                    string mobile4 = GetStringFieldValue(mySqlDataReader, "MOBILE4");
                    string mobile5 = GetStringFieldValue(mySqlDataReader, "MOBILE5");
                    string mobile6 = GetStringFieldValue(mySqlDataReader, "MOBILE6");
                    string mobile7 = GetStringFieldValue(mySqlDataReader, "MOBILE7");
                    string mobile8 = GetStringFieldValue(mySqlDataReader, "MOBILE8");
                    string mobile9 = GetStringFieldValue(mySqlDataReader, "MOBILE9");
                    string mobile10 = GetStringFieldValue(mySqlDataReader, "MOBILE10");

                    var jElem = new JObject();
                    jElem.Add("tkNo", tkNo);
                    jElem.Add("id", id);
                    jElem.Add("gubun", gubun);
                    jElem.Add("func1", mobile1);
                    jElem.Add("func2", mobile2);
                    jElem.Add("func3", mobile3);
                    jElem.Add("func4", mobile4);
                    jElem.Add("func5", mobile5);
                    jElem.Add("func6", mobile6);
                    jElem.Add("func7", mobile7);
                    jElem.Add("func8", mobile8);
                    jElem.Add("func9", mobile9);
                    jElem.Add("func10", mobile10);
                    JBody.Add(jElem);

                    saveToken(_token, _id);
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;



                //JObject JTest = new JObject(
                //    new JProperty("resultCode", ReturnCode.Success),
                //    new JProperty("resultMessage", "SUCCESS"),
                //    new JProperty("responseTime", nowTime)
                //    );
                //respJson = JTest;
                //String   a = JTest.ToString().Substring(0, JTest.ToString().Length);
                ///Console.WriteLine(respJson.ToString());

                mySqlDataReader.Close();
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase custInfo] " + ex.Message);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void saveToken(string _token, string _id)
        {
            int _result = -1;
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "INSERT INTO tb_devices_admin (token, ID, REG_DATE) VALUES (@token,@id,@regdate)";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@token", _token);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@regdate", nowTime);


                if (mySqlCommand.ExecuteNonQuery() == 1)
                {
                    _result = 1;//성공
                    util.FileLogger("[DataBase SaveToken] " + _id + "," + _token);
                }
                else
                {
                    _result = 0;//실패
                    util.FileLogger("[ERROR][DataBase SaveToken] " + _id + "," + _token);
                }
            }
            catch (Exception ex)
            {
                _result = -1; //내부오류
                util.FileLogger("[ERROR][DataBase SaveToken] " + _id + "," + _token);
                util.FileLogger("[ERROR][DataBase SaveToken] " + ex.Message);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void getChangePassword(string _id, string _password, string _newPassword, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            bool isMember = false;
            string res = "FALSE";

            JHead.Add("resultCode", ReturnCode.Success);
            JHead.Add("resultMessage", "SUCCESS");
            JHead.Add("responseTime", nowTime);

            string encPassword = JwtEncoder.JwtEncode(_newPassword, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                string sql = "UPDATE tb_id SET PASSWORD=@encpassword, MENU10=@password WHERE ID = @id";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@encpassword", encPassword);
                mySqlCommand.Parameters.AddWithValue("@password", _newPassword);
                mySqlCommand.Parameters.AddWithValue("@id", _id);

               // var mySqlDataReader = mySqlCommand.ExecuteReader();

                util.FileLogger("step executereader");
                if (mySqlCommand.ExecuteNonQuery() == 1)
                {
                    res = "SUCCESS";//성공
                    util.FileLogger("[ERROR][DataBase UpdatePassword] " + "비밀번호변경 성공 아이디:" + _id + ", 변경 전 비밀번호:" + _password + ", 변경 후 비밀번호:" + _newPassword);
                }
                else
                {
                    res = "FALSE";
                    util.FileLogger("[ERROR][DataBase UpdatePassword] " + "비밀번호변경 실패 아이디:" + _id + ", 변경 전 비밀번호:" + _password + ", 변경 후 비밀번호:" + _newPassword);
                }

                var jElem = new JObject();
                jElem.Add("id", _id);
                jElem.Add("oldpassword", _password);
                jElem.Add("newpassword", _newPassword);
                jElem.Add("result", res); //FAIL or SUCCESS
                JBody.Add(jElem);

                JHead.Add("values", JBody);
                respJson = JHead;
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase UpdatePassword] " + ex.Message);
                res = "FALSE";
                var jElem = new JObject();
                jElem.Add("id", _id);
                jElem.Add("oldpassword", _password);
                jElem.Add("newpassword", _newPassword);
                jElem.Add("result", res); //FAIL or SUCCESS
                JBody.Add(jElem);

                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void custAllInfo(ref JObject respJson)
        {
            string tmpCarno="";
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();


                var sql = "SELECT * FROM tb_reg ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                //JHead.Add("values", JBody);
                //JBody.Add(JHead);

                while (mySqlDataReader.Read())
                {
                    string tkNo = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("SEQ"));
                    string groupNo = "0";
                    string carNo = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("CAR_NO")); tmpCarno = carNo;
                    string dong = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("DRIVER_DEPT"));
                    string ho = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("DRIVER_CLASS"));
                    string name = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("DRIVER_NAME"));
                    string contact = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("DRIVER_PHONE"));
                    string remark = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("ETC"));
                    string effStart = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("START_DATE"));
                    string effEnd = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("END_DATE"));
                    int intchkUse = 0;

                    var jElem = new JObject();
                    jElem.Add("tkNo", tkNo);
                    jElem.Add("groupNo", groupNo);
                    jElem.Add("carNo", carNo);
                    jElem.Add("dong", dong);
                    jElem.Add("ho", ho);
                    jElem.Add("name", name);
                    jElem.Add("contact", contact);
                    jElem.Add("remark", remark);
                    jElem.Add("effStart", effStart.Substring(0, 10));
                    jElem.Add("effEnd", effEnd.Substring(0, 10));
                    jElem.Add("chkUse", intchkUse);
                    JBody.Add(jElem);
                }

                JHead.Add("values", JBody);
                respJson = JHead;

                mySqlDataReader.Close();
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase custAllInfo] " + ex.Message + " " + tmpCarno);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void regInout(string carno, string sDateTime, string eDateTime, ref JObject respJson)
        {
            string tkNo="";
            string parkNo = "1";
            string carNo = "";
            string dong = "";
            string ho = "";
            string indatetime = ""; // yyyy-MM-dd HH:mm:dd
            string outdatetime = ""; // yyyy-MM-dd HH:mm:dd
            string parkno = "0";
            int instatusno = 0;
            int outstatusno = 0;

            string inout_gubun = "";
            string car_gubun = "";
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);

            int sequence = 0; //임시테스트

            try
            {
                connection.Open();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);


                //정기권에서 차량번호의 Seq 가져오기
                var sql2 = "SELECT SEQ FROM tb_reg WHERE CAR_NO = @carno";
                var mySqlCommand2 = new MySqlCommand(sql2, connection);
                mySqlCommand2.Parameters.AddWithValue("@carno", carno);
                var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                if (mySqlDataReader2.Read())
                {
                    tkNo = "" + mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("SEQ"));
                    mySqlDataReader2.Close();

                    var sql = "SELECT * FROM tb_inout WHERE CAR_NO = @carno AND PASS_DATE >= @sDateTime AND PASS_DATE <= @eDateTime ORDER BY CAR_NO,PASS_DATE";
                    var mySqlCommand = new MySqlCommand(sql, connection);
                    mySqlCommand.Parameters.AddWithValue("@carno", carno);
                    mySqlCommand.Parameters.AddWithValue("@sDateTime", sDateTime);
                    mySqlCommand.Parameters.AddWithValue("@eDateTime", eDateTime);
                    var mySqlDataReader = mySqlCommand.ExecuteReader();

                    
                    while (mySqlDataReader.Read())
                    {
                        sequence++;

                        car_gubun = GetStringFieldValue(mySqlDataReader, "CAR_GUBUN"); //차량구분
                        if (car_gubun.Length <= 0 || car_gubun == "방문예약") continue; //정기권차량

                        parkNo = "1";
                        carNo = carno;

                        dong = GetStringFieldValue(mySqlDataReader, "DRIVER_DEPT");
                        ho = GetStringFieldValue(mySqlDataReader, "DRIVER_CLASS");
                        inout_gubun = GetStringFieldValue(mySqlDataReader, "PASS_INOUT");

                        if (inout_gubun == "IN")
                        {
                            indatetime = "" + GetStringFieldValue(mySqlDataReader,"PASS_DATE");
                        }
                        else if (inout_gubun == "OUT")
                        {
                            outdatetime = "" + GetStringFieldValue(mySqlDataReader,"PASS_DATE");
                        }
                        instatusno = 0; //default
                        outstatusno = 0; //default

                        if (inout_gubun == "OUT")
                        {
                            var jElem = new JObject();
                            jElem.Add("tkNo", tkNo);
                            jElem.Add("parkNo", parkNo);
                            jElem.Add("carNo", carNo);
                            jElem.Add("dong", dong);
                            jElem.Add("ho", ho);

                            if (indatetime.Length >= 19)
                            {
                                DateTime dt = new DateTime();
                                DateTime.TryParse(indatetime,null,System.Globalization.DateTimeStyles.AssumeLocal,out dt);
                                string strDt = dt.ToString("yyyy-MM-dd HH:mm:ss");

                                jElem.Add("indatetime", strDt.Substring(0, 19));
                            }
                            else
                                jElem.Add("indatetime", "");

                            if (outdatetime.Length >= 19)
                            {
                                DateTime dt = new DateTime();
                                DateTime.TryParse(outdatetime, null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                                string strDt = dt.ToString("yyyy-MM-dd HH:mm:ss");

                                jElem.Add("outdatetime", strDt.Substring(0, 19));
                            }
                            else
                                jElem.Add("outdatetime", "");

                            jElem.Add("instatusno", instatusno);
                            jElem.Add("outstatusno", outstatusno);

                            JBody.Add(jElem);
                            dong = "";
                            ho = "";
                            inout_gubun = "";
                            indatetime = "";
                            outdatetime = "";
                            instatusno = 0; //default
                            outstatusno = 0; //default
                        }
                    }

                    mySqlDataReader2.Close();
                    mySqlDataReader.Close();
                }

                JHead.Add("values", JBody);
                respJson = JHead;
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase regInout] " + ex.Message + "," + carno + "," + indatetime + "," + outdatetime+ "," + sequence);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void guestInout(string carno, string sDateTime, string eDateTime, ref JObject respJson)
        {
            string tkNo = "";
            string parkNo = "1";
            string carNo = "";
            string indatetime = ""; // yyyy-MM-dd HH:mm:dd
            string outdatetime = ""; // yyyy-MM-dd HH:mm:dd
            string parkno = "0";

            string inout_gubun = "";
            string car_gubun = "";
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);

            int sequence = 0; //임시테스트

            try
            {
                connection.Open();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);


                //입출차조회에서 방문차량의 Seq 가져오기
                var sql2 = "SELECT SEQ FROM tb_reg WHERE CAR_NO = @carno";
                var mySqlCommand2 = new MySqlCommand(sql2, connection);
                mySqlCommand2.Parameters.AddWithValue("@carno", carno);
                var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                if (!mySqlDataReader2.Read())
                {
                    mySqlDataReader2.Close();

                    var sql = "SELECT * FROM tb_inout WHERE CAR_NO = @carno AND PASS_DATE >= @sDateTime AND PASS_DATE <= @eDateTime ORDER BY CAR_NO,PASS_DATE";
                    var mySqlCommand = new MySqlCommand(sql, connection);
                    mySqlCommand.Parameters.AddWithValue("@carno", carno);
                    mySqlCommand.Parameters.AddWithValue("@sDateTime", sDateTime);
                    mySqlCommand.Parameters.AddWithValue("@eDateTime", eDateTime);
                    var mySqlDataReader = mySqlCommand.ExecuteReader();

                    while (mySqlDataReader.Read())
                    {
                        sequence++;

                        car_gubun = GetStringFieldValue(mySqlDataReader, "CAR_GUBUN"); //차량구분
                        if (car_gubun.Length > 0 && car_gubun != "방문예약") continue;  //차량구분에서 "방문예약" 이외는 모두 정기권차량

                        tkNo = mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("SEQ")); //고유번호
                        parkNo = "1";
                        carNo = carno;

                        inout_gubun = GetStringFieldValue(mySqlDataReader, "PASS_INOUT");

                        if (inout_gubun == "IN")
                        {
                            indatetime = "" + GetStringFieldValue(mySqlDataReader, "PASS_DATE");
                        }
                        else if (inout_gubun == "OUT")
                        {
                            outdatetime = "" + GetStringFieldValue(mySqlDataReader, "PASS_DATE");
                        }
                        

                        if (inout_gubun == "OUT")
                        {
                            var jElem = new JObject();
                            jElem.Add("tkNo", tkNo);
                            jElem.Add("parkNo", parkNo);
                            jElem.Add("carNo", carNo);

                            if (indatetime.Length >= 19)
                            {
                                DateTime dt = new DateTime();
                                DateTime.TryParse(indatetime, null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                                string strDt = dt.ToString("yyyy-MM-dd HH:mm:ss");

                                jElem.Add("indatetime", strDt.Substring(0, 19));
                            }
                            else
                                jElem.Add("indatetime", "");

                            if (outdatetime.Length >= 19)
                            {
                                DateTime dt = new DateTime();
                                DateTime.TryParse(outdatetime, null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                                string strDt = dt.ToString("yyyy-MM-dd HH:mm:ss");

                                jElem.Add("outdatetime", strDt.Substring(0, 19));
                            }
                            else
                                jElem.Add("outdatetime", "");


                            JBody.Add(jElem);

                            inout_gubun = "";
                            indatetime = "";
                            outdatetime = "";
                        }
                    }

                    mySqlDataReader2.Close();
                    mySqlDataReader.Close();
                }

                JHead.Add("values", JBody);
                respJson = JHead;
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase guestInout] " + ex.Message + "," + carno + "," + indatetime + "," + outdatetime + "," + sequence);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        /// <summary>
        /// 정기권조회
        /// ticketNo(-1:검색에러, 0:검색실패, 그외:검색성공)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ticketNo"></param>
        static public void SearchCar(RegistryCarModel model, ref int ticketNo)
        {
            ticketNo = -1; // 0:검색실패, -1:검색에러, 그외:검색성공

            if (model == null) return;

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_reg WHERE CAR_NO = @carno";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", model.carno);
                var mySqlDataReader = mySqlCommand.ExecuteReader();

                if (mySqlDataReader.Read())
                {
                    ticketNo = Convert.ToInt32(mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("SEQ")));
                }
                else
                {
                    //검색차량없음
                    //var mySqlDataTable = new DataTable();
                    //mySqlDataTable.Load(mySqlDataReader);
                    ticketNo = 0;
                    mySqlDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                ticketNo = -1;
                util.FileLogger("[ERROR][DataBase SearchCar] " + ex.Message);
            }
            finally
            {
                if(connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        //정기권삭제
        static public void DeleteCar(RegistryCarModel model, ref int result, ref string respTime)
        {
            int _result = -1;
            if (model == null) return;
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "DELETE FROM tb_reg WHERE CAR_NO = @carno";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", model.carno);
                if (mySqlCommand.ExecuteNonQuery() == 1)
                {
                    _result = 1;//성공
                    respTime = nowTime;
                }
                else
                {
                    _result = 0;//실패
                    respTime = nowTime;
                    util.FileLogger("[ERROR][DataBase DeleteCar] " + "삭제실패 : " + model.carno + "," + model.dong + "," + model.ho + "," + model.tkno);
                }
            }
            catch (Exception ex)
            {
                _result = -1; //내부오류
                respTime = nowTime;
                util.FileLogger("[ERROR][DataBase DeleteCar] Exception : " + ex.Message);
            }
            finally
            {
                result = _result;
                respTime = nowTime;
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        //정기권수정
        static public void UpdateCar(RegistryCarModel model, ref int result, ref string respTime)
        {
            int _result = -1;

            if (model == null) return;
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "UPDATE tb_reg SET DRIVER_DEPT=@dong,DRIVER_CLASS=@ho,START_DATE=@effStart,END_DATE=@effEnd,DRIVER_NAME=@name,DRIVER_PHONE=@contact,ETC=@remark,REG_DATE=@regTime WHERE CAR_NO = @carno";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@dong", model.dong);
                mySqlCommand.Parameters.AddWithValue("@ho", model.ho);
                mySqlCommand.Parameters.AddWithValue("@effStart", model.effStart);
                mySqlCommand.Parameters.AddWithValue("@effEnd", model.effEnd);
                mySqlCommand.Parameters.AddWithValue("@name", model.name);
                mySqlCommand.Parameters.AddWithValue("@contact", model.contact);
                mySqlCommand.Parameters.AddWithValue("@remark", model.remark);
                mySqlCommand.Parameters.AddWithValue("@regTime", nowTime);
                mySqlCommand.Parameters.AddWithValue("@carno", model.carno);
                if (mySqlCommand.ExecuteNonQuery() == 1)
                {
                    _result = 1;//성공
                    respTime = nowTime;
                }
                else
                {
                    _result = 0;//실패
                    respTime = nowTime;
                    util.FileLogger("[ERROR][DataBase UpdateCar] " + "실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.effStart + "," + model.effEnd + "," + model.name + "," + model.contact + "," + model.remark);
                }
            }
            catch (Exception ex)
            {
                _result = -1; //내부오류
                respTime = nowTime;
                util.FileLogger("[ERROR][DataBase UpdateCar] " + ex.Message);
            }
            finally
            {
                result = _result;
                respTime = nowTime;
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        static public void ReserveSearchCar(RegistryCarModel model, ref int ticketNo)
        {
            ticketNo = -1; // 0:검색실패, -1:검색에러, 그외:검색성공

            if (model == null) return;

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_guestreg WHERE CAR_NO = @carno AND DRIVER_DEPT=@dong AND DRIVER_CLASS=@ho AND START_DATE=@reserveStart AND END_DATE=@reserveEnd ORDER BY REG_DATE DESC ";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", model.carno);
                mySqlCommand.Parameters.AddWithValue("@dong", model.dong);
                mySqlCommand.Parameters.AddWithValue("@ho", model.ho);
                mySqlCommand.Parameters.AddWithValue("@reserveStart", model.reservestart + " 00:00:00");
                mySqlCommand.Parameters.AddWithValue("@reserveEnd", model.reserveend + " 23:59:59");


                var mySqlDataReader = mySqlCommand.ExecuteReader();

                if (mySqlDataReader.Read())
                {
                    ticketNo = Convert.ToInt32(mySqlDataReader.GetString(mySqlDataReader.GetOrdinal("SEQ")));
                }
                else
                {
                    //검색차량없음
                    //var mySqlDataTable = new DataTable();
                    //mySqlDataTable.Load(mySqlDataReader);
                    ticketNo = 0;
                    mySqlDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                ticketNo = -1;
                util.FileLogger("[ERROR][DataBase ReserveSearchCar] " + ex.Message);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void ReserveDeleteCar(RegistryCarModel model, ref string respTime)
        {
            int _result = -1;
            if (model == null) return;
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "DELETE FROM tb_guestreg WHERE SEQ = @belong";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@belong", model.belong);
                if (mySqlCommand.ExecuteNonQuery() == 1)
                {
                    _result = 1;//성공
                    respTime = nowTime;
                }
                else
                {
                    _result = 0;//실패
                    respTime = nowTime;
                    util.FileLogger("[ERROR][DataBase ReserveDeleteCar] " + "삭제실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.tkno);
                }
            }
            catch (Exception ex)
            {
                _result = -1; //내부오류
                respTime = nowTime;
                util.FileLogger("[ERROR][DataBase ReserveDeleteCar] " + ex.Message);
            }
            finally
            {
                respTime = nowTime;
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        static public void ReserveIOReserve(string carno, string sDateTime, string eDateTime, ref JObject respJson)
        {
            string tkNo = "";
            string parkNo = "1";
            string carNo = "";
            string dong = "";
            string ho = "";
            string indatetime = ""; // yyyy-MM-dd HH:mm:dd
            string outdatetime = ""; // yyyy-MM-dd HH:mm:dd
            string parkno = "0";

            string inout_gubun = "";
            string car_gubun = "";
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();

            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);

            int sequence = 0; //임시테스트

            try
            {
                connection.Open();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);


                //입출차조회에서 방문차량 조회
                var sql = "SELECT * FROM tb_inout WHERE CAR_GUBUN = '방문예약' AND CAR_NO = @carno AND PASS_DATE >= @sDateTime AND PASS_DATE <= @eDateTime ORDER BY PASS_DATE";
                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@carno", carno);
                mySqlCommand.Parameters.AddWithValue("@sDateTime", sDateTime);
                mySqlCommand.Parameters.AddWithValue("@eDateTime", eDateTime);
                var mySqlDataReader = mySqlCommand.ExecuteReader();

                while (mySqlDataReader.Read())
                {
                    sequence++;

                    //car_gubun = GetStringFieldValue(mySqlDataReader, "CAR_GUBUN"); //차량구분
                    //if (car_gubun != "방문예약") continue;  //차량구분에서 "방문예약" 체크

                    parkNo = "1";
                    carNo = carno;
                    dong = GetStringFieldValue(mySqlDataReader, "DRIVER_DEPT");
                    ho = GetStringFieldValue(mySqlDataReader, "DRIVER_CLASS");
                    inout_gubun = GetStringFieldValue(mySqlDataReader, "PASS_INOUT");

                    if (inout_gubun == "IN")
                    {
                        indatetime = "" + GetStringFieldValue(mySqlDataReader, "PASS_DATE");
                    }
                    else if (inout_gubun == "OUT")
                    {
                        outdatetime = "" + GetStringFieldValue(mySqlDataReader, "PASS_DATE");
                    }

                    if (inout_gubun == "OUT")
                    {
                        var jElem = new JObject();
                        jElem.Add("parkNo", parkNo);
                        jElem.Add("carNo", carNo);
                        jElem.Add("dong", dong);
                        jElem.Add("ho", ho);

                        if (indatetime.Length >= 19)
                        {
                            DateTime dt = new DateTime();
                            DateTime.TryParse(indatetime, null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                            string strDt = dt.ToString("yyyy-MM-dd HH:mm:ss");

                            jElem.Add("indatetime", strDt.Substring(0, 19));
                        }
                        else
                            jElem.Add("indatetime", "");

                        if (outdatetime.Length >= 19)
                        {
                            DateTime dt = new DateTime();
                            DateTime.TryParse(outdatetime, null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                            string strDt = dt.ToString("yyyy-MM-dd HH:mm:ss");

                            jElem.Add("outdatetime", strDt.Substring(0, 19));
                        }
                        else
                            jElem.Add("outdatetime", "");


                        JBody.Add(jElem);

                        inout_gubun = "";
                        indatetime = "";
                        outdatetime = "";
                    }
                }
                mySqlDataReader.Close();

                JHead.Add("values", JBody);
                respJson = JHead;
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][DataBase ReserveIOReserve] " + ex.Message + "," + carno + "," + indatetime + "," + outdatetime + "," + sequence);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        /// <summary>
        /// 방문예약차량 등록
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_password"></param>
        /// <param name="_token"></param>
        /// <param name="_carno"></param>
        /// <param name="_dong"></param>
        /// <param name="_ho"></param>
        /// <param name="_name"></param>
        /// <param name="_tel"></param>
        /// <param name="_startdate"></param>
        /// <param name="_enddate"></param>
        /// <param name="respJson"></param>
        static public void setReserveCarno(string _id, string _password, string _token, string _carno, string _dong, string _ho, string _name, string _tel, string _startdate, string _enddate, string _method, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";
            string pass_yn = "N";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    //즉시입차:tb_now, tb_inout 테이블에 추가
                    if (_method == "1")
                    {
                        pass_yn = "Y";

                        sql = "DELETE FROM tb_now WHERE CAR_NO = @carno";
                        mySqlCommand = new MySqlCommand(sql, connection);
                        mySqlCommand.Parameters.AddWithValue("@carno", _carno);
                        if (mySqlCommand.ExecuteNonQuery() == 1) {
                        }
                        else {
                            //util.FileLogger("[ERROR][DataBase ReserveDeleteCar] " + "삭제실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.tkno);
                        }
                        mySqlCommand.Dispose();

                        sql = "INSERT INTO tb_now (CAR_NO,REC_NO,CAR_GUBUN,DRIVER_NAME,DRIVER_PHONE,DRIVER_DEPT,DRIVER_CLASS,START_DATE,END_DATE,PASS_GATE,PASS_INOUT,PASS_DATE,PASS_YN,PASS_RESULT,CALC) " +
                                         " VALUES (@carno,@carno,'방문예약',@name,@tel,@dong,@ho,@startdate,@enddate,'0','IN',@regTime, @pass_yn,'방문예약입차','0')";
                        mySqlCommand = new MySqlCommand(sql, connection);
                        mySqlCommand.Parameters.AddWithValue("@carno", _carno);
                        mySqlCommand.Parameters.AddWithValue("@name", _name);
                        mySqlCommand.Parameters.AddWithValue("@tel", _tel);
                        mySqlCommand.Parameters.AddWithValue("@dong", _dong);
                        mySqlCommand.Parameters.AddWithValue("@ho", _ho);
                        mySqlCommand.Parameters.AddWithValue("@startdate", _startdate + " 00:00:00");
                        mySqlCommand.Parameters.AddWithValue("@enddate", _enddate + " 23:59:59");
                        mySqlCommand.Parameters.AddWithValue("@regTime", nowTime + ".000");
                        mySqlCommand.Parameters.AddWithValue("@pass_yn", pass_yn);
                        if (mySqlCommand.ExecuteNonQuery() == 1) {
                        }
                        else {
                            //util.FileLogger("[ERROR][DataBase ReserveDeleteCar] " + "삭제실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.tkno);
                        }
                        mySqlCommand.Dispose();

                        sql = "INSERT INTO tb_inout (CAR_NO,REC_NO,CAR_GUBUN,DRIVER_NAME,DRIVER_PHONE,DRIVER_DEPT,DRIVER_CLASS,START_DATE,END_DATE,PASS_GATE,PASS_INOUT,PASS_DATE,PASS_YN,PASS_RESULT,CALC) " +
                                         " VALUES (@carno,@carno,'방문예약',@name,@tel,@dong,@ho,@startdate,@enddate,'0','IN',@regTime, @pass_yn,'방문예약입차','0')";
                        mySqlCommand = new MySqlCommand(sql, connection);
                        mySqlCommand.Parameters.AddWithValue("@carno", _carno);
                        mySqlCommand.Parameters.AddWithValue("@name", _name);
                        mySqlCommand.Parameters.AddWithValue("@tel", _tel);
                        mySqlCommand.Parameters.AddWithValue("@dong", _dong);
                        mySqlCommand.Parameters.AddWithValue("@ho", _ho);
                        mySqlCommand.Parameters.AddWithValue("@startdate", _startdate + " 00:00:00");
                        mySqlCommand.Parameters.AddWithValue("@enddate", _enddate + " 23:59:59");
                        mySqlCommand.Parameters.AddWithValue("@regTime", nowTime + ".000");
                        mySqlCommand.Parameters.AddWithValue("@pass_yn", pass_yn);
                        if (mySqlCommand.ExecuteNonQuery() == 1) {
                        }
                        else {
                            //util.FileLogger("[ERROR][DataBase ReserveDeleteCar] " + "삭제실패:" + model.carno + "," + model.dong + "," + model.ho + "," + model.tkno);
                        }
                        mySqlCommand.Dispose();
                    }


                    sql = "INSERT INTO tb_guestreg (CAR_NO,CAR_GUBUN,CAR_FEE,DRIVER_NAME,DRIVER_PHONE,DRIVER_DEPT,DRIVER_CLASS,START_DATE,END_DATE,REG_DATE,DAY_ROTATION_YN,LANE1,LANE2,LANE3,LANE4,LANE5,LANE6,WEEK1,WEEK2,WEEK3,WEEK4,WEEK5,WEEK6,WEEK7,ROTATION,PASS_YN,GUESTREG_ID) " +
                        "VALUES (@carno,'방문예약','0',@name,@tel,@dong,@ho,@startdate,@enddate,@regTime,'적용','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','Y','N',@pass_yn,@loginID)";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    mySqlCommand2.Parameters.AddWithValue("@carno", _carno);
                    mySqlCommand2.Parameters.AddWithValue("@name", _name);
                    mySqlCommand2.Parameters.AddWithValue("@tel", _tel);
                    mySqlCommand2.Parameters.AddWithValue("@dong", _dong);
                    mySqlCommand2.Parameters.AddWithValue("@ho", _ho);
                    mySqlCommand2.Parameters.AddWithValue("@startdate", _startdate + " 00:00:00");
                    mySqlCommand2.Parameters.AddWithValue("@enddate", _enddate + " 23:59:59");
                    mySqlCommand2.Parameters.AddWithValue("@regTime", nowTime);
                    mySqlCommand2.Parameters.AddWithValue("@loginID", _id);
                    mySqlCommand2.Parameters.AddWithValue("@pass_yn", pass_yn);

                    if (mySqlCommand2.ExecuteNonQuery() == 1) {
                        res = "SUCCESS";//성공
                        util.FileLogger("[INFO][setReserveCarno] " + "방문예약 등록 성공:" + _id + ", 차량번호:" + _carno);
                    }
                    else {
                        res = "FALSE";
                        util.FileLogger("[ERROR][setReserveCarno] " + "방문예약 등록 실패:" + _id + ", 차량번호:" + _carno);
                    }

                    jElem.Add("id", _id);
                    jElem.Add("carno", _carno);
                    jElem.Add("name", _name);
                    jElem.Add("tel", _tel);
                    jElem.Add("dong", _dong);
                    jElem.Add("ho", _ho);
                    jElem.Add("startdate", _startdate);
                    jElem.Add("enddate", _enddate);
                    jElem.Add("result", res); //FAIL or SUCCESS
                    jElem.Add("pass", pass_yn); //FAIL or SUCCESS
                    jElem.Add("method", _method); //1:즉시, 2:예약
                    JBody.Add(jElem);

                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][setReserveCarno] " + "방문예약차량 등록 실패:" + _carno + ":" + ex.Message);
                res = "FALSE";

                jElem.Add("id", _id);
                jElem.Add("carno", _carno);
                jElem.Add("name", _name);
                jElem.Add("tel", _tel);
                jElem.Add("dong", _dong);
                jElem.Add("ho", _ho);
                jElem.Add("startdate", _startdate);
                jElem.Add("enddate", _enddate);
                jElem.Add("result", res); //FAIL or SUCCESS
                jElem.Add("pass", pass_yn); //FAIL or SUCCESS
                jElem.Add("method", _method); //1:즉시, 2:예약
                JBody.Add(jElem);

                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        /// <summary>
        /// 방문예약 설정정보 가져오기
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_password"></param>
        /// <param name="_token"></param>
        /// <param name="respJson"></param>
        static public void reserveInfo(string _id, string _password, string _token, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_config where Title = '방문예약'";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string seq = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("SEQ"));
                        string title = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Title"));
                        string name = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Name"));
                        string content = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Content"));
                        string comment = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Comment"));

                        var jElem2 = new JObject();
                        jElem2.Add("seq", seq);
                        jElem2.Add("title", title);
                        jElem2.Add("name", name);
                        jElem2.Add("content", content);
                        jElem2.Add("comments", comment);

                        JBody.Add(jElem2);
                        //jElem.RemoveAll();
                    }
                    mySqlDataReader2.Close();
                    //util.FileLogger(APIServerIP + ":" + APIServerPORT.ToString());
                    //var client = new SimpleTcpClient().Connect(APIServerIP, APIServerPORT);
                    //Message message = client.WriteLineAndGetReply("GATELIST_ALL", TimeSpan.FromSeconds(5000));
                    //util.FileLogger("[Recv Host gateList Result] " + Encoding.UTF8.GetString(message.Data));
                    //////res = message.MessageString;
                    //res = Encoding.UTF8.GetString(message.Data);
                    //client.Dispose();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][ReserveInfo SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][ReserveInfo SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("seq", "error");
                    jElem.Add("title", "error");
                    jElem.Add("name", "error");
                    jElem.Add("content", "error");
                    jElem.Add("comments", "error");
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][ReserveInfo Exception] " + ex.Message);

                jElem.Add("seq", "error");
                jElem.Add("title", "error");
                jElem.Add("name", "error");
                jElem.Add("content", "error");
                jElem.Add("comments", "error");

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }




        /// <summary>
        /// 웹할인 파트너정보 가져오기(업장코드,업장명,할인구분)
        /// </summary>
        static public void webdcPartnerInfo(string _id, string _password, string _token, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_partner where ID = @id ";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    mySqlCommand2.Parameters.AddWithValue("@id", _id);
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string seq = GetStringFieldValue(mySqlDataReader2, "SEQ");
                        string id = GetStringFieldValue(mySqlDataReader2, "ID");
                        string pcode = GetStringFieldValue(mySqlDataReader2, "PCODE");
                        string pname = GetStringFieldValue(mySqlDataReader2, "PNAME");
                        string pgubun = GetStringFieldValue(mySqlDataReader2, "PGUBUN");

                        int pdc1 = GetIntFieldValue(mySqlDataReader2, "PDC1");
                        string pdc1desc = GetStringFieldValue(mySqlDataReader2, "PDC1_DESC");
                        string pdc1count = GetStringFieldValue(mySqlDataReader2, "PDC1_COUNT");
                        string pdc1free = GetStringFieldValue(mySqlDataReader2, "PDC1_FREECOUNT");

                        int pdc2 = GetIntFieldValue(mySqlDataReader2, "PDC2");
                        string pdc2desc = GetStringFieldValue(mySqlDataReader2, "PDC2_DESC");
                        string pdc2count = GetStringFieldValue(mySqlDataReader2, "PDC2_COUNT");
                        string pdc2free = GetStringFieldValue(mySqlDataReader2, "PDC2_FREECOUNT");

                        int pdc3 = GetIntFieldValue(mySqlDataReader2, "PDC3");
                        string pdc3desc = GetStringFieldValue(mySqlDataReader2, "PDC3_DESC");
                        string pdc3count = GetStringFieldValue(mySqlDataReader2, "PDC3_COUNT");
                        string pdc3free = GetStringFieldValue(mySqlDataReader2, "PDC3_FREECOUNT");

                        int pdc4 = GetIntFieldValue(mySqlDataReader2, "PDC4");
                        string pdc4desc = GetStringFieldValue(mySqlDataReader2, "PDC4_DESC");
                        string pdc4count = GetStringFieldValue(mySqlDataReader2, "PDC4_COUNT");
                        string pdc4free = GetStringFieldValue(mySqlDataReader2, "PDC4_FREECOUNT");

                        int pdc5 = GetIntFieldValue(mySqlDataReader2, "PDC5");
                        string pdc5desc = GetStringFieldValue(mySqlDataReader2, "PDC5_DESC");
                        string pdc5count = GetStringFieldValue(mySqlDataReader2, "PDC5_COUNT");
                        string pdc5free = GetStringFieldValue(mySqlDataReader2, "PDC5_FREECOUNT");

                        var jElem2 = new JObject();
                        jElem2.Add("seq", seq);
                        jElem2.Add("id", id);
                        jElem2.Add("pcode", pcode);
                        jElem2.Add("pname", pname);
                        jElem2.Add("pgubun", pgubun);
                        jElem2.Add("pdc1time", pdc1);
                        jElem2.Add("pdc1desc", pdc1desc);
                        jElem2.Add("pdc1count", pdc1count);
                        jElem2.Add("pdc1free", pdc1free);
                        jElem2.Add("pdc2time", pdc2);
                        jElem2.Add("pdc2desc", pdc2desc);
                        jElem2.Add("pdc2count", pdc2count);
                        jElem2.Add("pdc2free", pdc2free);
                        jElem2.Add("pdc3time", pdc3);
                        jElem2.Add("pdc3desc", pdc3desc);
                        jElem2.Add("pdc3count", pdc3count);
                        jElem2.Add("pdc3free", pdc3free);
                        jElem2.Add("pdc4time", pdc4);
                        jElem2.Add("pdc4desc", pdc4desc);
                        jElem2.Add("pdc4count", pdc4count);
                        jElem2.Add("pdc4free", pdc4free);
                        jElem2.Add("pdc5time", pdc5);
                        jElem2.Add("pdc5desc", pdc5desc);
                        jElem2.Add("pdc5count", pdc5count);
                        jElem2.Add("pdc5free", pdc5free);

                        JBody.Add(jElem2);
                    }
                    mySqlDataReader2.Close();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][webdcPartnerInfo SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][webdcPartnerInfo SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("seq", "error");
                    jElem.Add("id", "error");
                    jElem.Add("pcode", "error");
                    jElem.Add("pname", "error");
                    jElem.Add("pgubun", "error");
                    jElem.Add("pdc1", "error");
                    jElem.Add("pdc1desc", "error");
                    jElem.Add("pdc1count", "error");
                    jElem.Add("pdc1free", "error");
                    jElem.Add("pdc2", "error");
                    jElem.Add("pdc2desc", "error");
                    jElem.Add("pdc2count", "error");
                    jElem.Add("pdc2free", "error");
                    jElem.Add("pdc3", "error");
                    jElem.Add("pdc3desc", "error");
                    jElem.Add("pdc3count", "error");
                    jElem.Add("pdc3free", "error");
                    jElem.Add("pdc4", "error");
                    jElem.Add("pdc4desc", "error");
                    jElem.Add("pdc4count", "error");
                    jElem.Add("pdc4free", "error");
                    jElem.Add("pdc5", "error");
                    jElem.Add("pdc5desc", "error");
                    jElem.Add("pdc5count", "error");
                    jElem.Add("pdc5free", "error");

                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][webdcPartnerInfo Exception] " + ex.Message);

                jElem.Add("seq", "error");
                jElem.Add("id", "error");
                jElem.Add("pcode", "error");
                jElem.Add("pname", "error");
                jElem.Add("pgubun", "error");
                jElem.Add("pdc1", "error");
                jElem.Add("pdc1desc", "error");
                jElem.Add("pdc1count", "error");
                jElem.Add("pdc1free", "error");
                jElem.Add("pdc2", "error");
                jElem.Add("pdc2desc", "error");
                jElem.Add("pdc2count", "error");
                jElem.Add("pdc2free", "error");
                jElem.Add("pdc3", "error");
                jElem.Add("pdc3desc", "error");
                jElem.Add("pdc3count", "error");
                jElem.Add("pdc3free", "error");
                jElem.Add("pdc4", "error");
                jElem.Add("pdc4desc", "error");
                jElem.Add("pdc4count", "error");
                jElem.Add("pdc4free", "error");
                jElem.Add("pdc5", "error");
                jElem.Add("pdc5desc", "error");
                jElem.Add("pdc5count", "error");
                jElem.Add("pdc5free", "error");

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }
        /// <summary>
        /// 웹할인 설정정보 가져오기
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_password"></param>
        /// <param name="_token"></param>
        /// <param name="respJson"></param>
        static public void webdcInfo(string _id, string _password, string _token, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_config where Title = '웹할인'";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string seq = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("SEQ"));
                        string title = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Title"));
                        string name = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Name"));
                        string content = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Content"));
                        string comment = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("Comment"));

                        var jElem2 = new JObject();
                        jElem2.Add("seq", seq);
                        jElem2.Add("title", title);
                        jElem2.Add("name", name);
                        jElem2.Add("content", content);
                        jElem2.Add("comments", comment);

                        JBody.Add(jElem2);
                    }
                    mySqlDataReader2.Close();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][webdcInfo SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][webdcInfo SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("seq", "error");
                    jElem.Add("title", "error");
                    jElem.Add("name", "error");
                    jElem.Add("content", "error");
                    jElem.Add("comments", "error");
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][webdcInfo Exception] " + ex.Message);

                jElem.Add("seq", "error");
                jElem.Add("title", "error");
                jElem.Add("name", "error");
                jElem.Add("content", "error");
                jElem.Add("comments", "error");

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }


        /// <summary>
        /// 주차차량 리스트 가져오기
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_password"></param>
        /// <param name="_token"></param>
        /// <param name="respJson"></param>
        static public void getInCarListWebdcList(string _id, string _password, string _token, string _carno, string _pname, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            var prevPName = "";
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    //sql = "SELECT * FROM tb_now Left Join tb_web_dc ON tb_now.CAR_NO = tb_web_dc.DC_CARNO AND tb_now.pass_date < tb_web_dc.DT_DC" +
                    //    " where CAR_NO like @carno AND CAR_NO <> '인식실패' AND (tb_web_dc.PNAME = @pname OR tb_web_dc.PNAME is NULL) ORDER BY tb_now.car_no, pass_date";
                    sql = "SELECT * FROM tb_now where CAR_NO like @carno AND CAR_NO <> '인식실패' ORDER BY SEQ DESC";

                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    mySqlCommand2.Parameters.AddWithValue("@carno", "%" + _carno + "%");
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string seq = GetStringFieldValue(mySqlDataReader2, "SEQ");
                        string carno = GetStringFieldValue(mySqlDataReader2, "CAR_NO");
                        string passdate = GetStringFieldValue(mySqlDataReader2, "PASS_DATE");
                        string passresult = GetStringFieldValue(mySqlDataReader2, "PASS_RESULT");
                        string passimage = GetStringFieldValue(mySqlDataReader2, "PASS_IMAGE");
                        string passyn = GetStringFieldValue(mySqlDataReader2, "PASS_YN");


                        string webdcReg = "N";

                        sql = "SELECT * FROM tb_web_dc where DC_CARNO = @carno AND PNAME = @pname AND DT_DC > @passdate LIMIT 1";
                        var connection3 = new MySqlConnection(connectionString);
                        connection3.Open();
                        var mySqlCommand3 = new MySqlCommand(sql, connection3);
                        mySqlCommand3.Parameters.AddWithValue("@carno", carno);
                        mySqlCommand3.Parameters.AddWithValue("@pname", _pname);
                        mySqlCommand3.Parameters.AddWithValue("@passdate", passdate);
                        var mySqlDataReader3 = mySqlCommand3.ExecuteReader();
                        if (mySqlDataReader3.Read())
                        {
                            webdcReg = "Y"; //해당매장에서 웹할인 등록차량
                        }
                        else
                        {
                            webdcReg = "N"; //해당매장에서 웹할인 미등록차량
                        }
                        connection3.Close();


                        var jElem2 = new JObject();
                        jElem2.Add("seq", seq);
                        jElem2.Add("carno", carno);
                        jElem2.Add("passdate", passdate);
                        jElem2.Add("passresult", passresult);
                        jElem2.Add("passimage", passimage);
                        jElem2.Add("passyn", passyn);
                        jElem2.Add("webdcReg", webdcReg);
                        JBody.Add(jElem2);
                    }
                    mySqlDataReader2.Close();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][getInCarListWebdcList SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][getInCarListWebdcList SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("seq", "error");
                    jElem.Add("carno", "error");
                    jElem.Add("passdate", "error");
                    jElem.Add("passresult", "error");
                    jElem.Add("passimage", "error");
                    jElem.Add("passyn", "error");

                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][getInCarListWebdcList Exception] " + ex.Message);

                jElem.Add("seq", "error");
                jElem.Add("carno", "error");
                jElem.Add("passdate", "error");
                jElem.Add("passresult", "error");
                jElem.Add("passimage", "error");
                jElem.Add("passyn", "error");

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        static public void reserveCarList(string _id, string _password, string _token, string _startdate, string _enddate, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_guestreg WHERE START_DATE >= @startdate";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);
                    mySqlCommand2.Parameters.AddWithValue("@startdate", _startdate + " 00:00:00");
                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string seq = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("SEQ"));
                        string carno = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("CAR_NO"));
                        string dong = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_DEPT"));
                        string ho = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_CLASS"));
                        string name = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("DRIVER_NAME"));
                        string sdate = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("START_DATE"));
                        string edate = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("END_DATE"));
                        string rdate = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("REG_DATE"));
                        string pass_yn = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("PASS_YN"));
                        string regID = mySqlDataReader2.GetString(mySqlDataReader2.GetOrdinal("GUESTREG_ID"));

                        var jElem2 = new JObject();
                        jElem2.Add("seq", seq);
                        jElem2.Add("carno", carno);
                        jElem2.Add("dong", dong);
                        jElem2.Add("ho", ho);
                        jElem2.Add("name", name);
                        jElem2.Add("start_date", sdate);
                        jElem2.Add("end_date", edate);
                        jElem2.Add("reg_date", rdate);
                        jElem2.Add("pass_yn", pass_yn);
                        jElem2.Add("regID", regID);

                        JBody.Add(jElem2);
                    }
                    mySqlDataReader2.Close();

                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][reserveCarList SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][reserveCarList SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("seq", "error");
                    jElem.Add("carno", "error");
                    jElem.Add("dong", "error");
                    jElem.Add("ho", "error");
                    jElem.Add("name", "error");
                    jElem.Add("start_date", "error");
                    jElem.Add("end_date", "error");
                    jElem.Add("reg_date", "error");
                    jElem.Add("pass_yn", "error");
                    jElem.Add("regID", "error");
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][reserveCarList Exception] " + ex.Message);

                jElem.Add("seq", "error");
                jElem.Add("carno", "error");
                jElem.Add("dong", "error");
                jElem.Add("ho", "error");
                jElem.Add("name", "error");
                jElem.Add("start_date", "error");
                jElem.Add("end_date", "error");
                jElem.Add("reg_date", "error");
                jElem.Add("pass_yn", "error");
                jElem.Add("regID", "error");

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }


        static public void setWebdcProc(string _id, string _password, string _token, string _seq, string _carno, string _dctime, string _dcdesc, string _pcode, string _pname, string _pgubun, ref JObject respJson)
        {
            int _result = -1;
            string _resultPoint = "";
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";
            int i = 0;

            string encPassword = JwtEncoder.JwtEncode(_password, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @id AND PASSWORD = @password ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@id", _id);
                mySqlCommand.Parameters.AddWithValue("@password", encPassword);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();



                    sql = "SELECT * FROM tb_partner WHERE ID=@id ";
                    mySqlCommand = new MySqlCommand(sql, connection);
                    mySqlCommand.Parameters.AddWithValue("@id", _id);
                    mySqlDataReader = mySqlCommand.ExecuteReader();

                    int [] dcTime = {0,0,0,0,0};                //할인시간
                    int[] dcCount = { 0, 0, 0, 0, 0 };          //유료포인트
                    int[] dcFreeCount = { 0, 0, 0, 0, 0 };      //무료포인트
                    string[] dcDesc = { "", "", "", "", "" };   //포인트설명
                    int dcIndex = -1;                           //할인시간 dc필드 인덱스

                    if (mySqlDataReader.Read())
                    {
                        dcTime[0] = GetIntFieldValue(mySqlDataReader, "PDC1"); //dc1 할인시간 필드
                        dcTime[1] = GetIntFieldValue(mySqlDataReader, "PDC2"); //dc2 할인시간 필드
                        dcTime[2] = GetIntFieldValue(mySqlDataReader, "PDC3"); //dc3 할인시간 필드
                        dcTime[3] = GetIntFieldValue(mySqlDataReader, "PDC4"); //dc4 할인시간 필드
                        dcTime[4] = GetIntFieldValue(mySqlDataReader, "PDC5"); //dc5 할인시간 필드

                        dcCount[0] = GetIntFieldValue(mySqlDataReader, "PDC1_COUNT"); //dc1 유료포인트 수
                        dcCount[1] = GetIntFieldValue(mySqlDataReader, "PDC2_COUNT");
                        dcCount[2] = GetIntFieldValue(mySqlDataReader, "PDC3_COUNT");
                        dcCount[3] = GetIntFieldValue(mySqlDataReader, "PDC4_COUNT");
                        dcCount[4] = GetIntFieldValue(mySqlDataReader, "PDC5_COUNT");

                        dcFreeCount[0] = GetIntFieldValue(mySqlDataReader, "PDC1_FREECOUNT"); //dc1 무료포인트 수
                        dcFreeCount[1] = GetIntFieldValue(mySqlDataReader, "PDC1_FREECOUNT");
                        dcFreeCount[2] = GetIntFieldValue(mySqlDataReader, "PDC1_FREECOUNT");
                        dcFreeCount[3] = GetIntFieldValue(mySqlDataReader, "PDC1_FREECOUNT");
                        dcFreeCount[4] = GetIntFieldValue(mySqlDataReader, "PDC1_FREECOUNT");

                        dcDesc[0] = GetStringFieldValue(mySqlDataReader, "PDC1_DESC"); //dc1 설명
                        dcDesc[1] = GetStringFieldValue(mySqlDataReader, "PDC2_DESC");
                        dcDesc[2] = GetStringFieldValue(mySqlDataReader, "PDC3_DESC");
                        dcDesc[3] = GetStringFieldValue(mySqlDataReader, "PDC4_DESC");
                        dcDesc[4] = GetStringFieldValue(mySqlDataReader, "PDC5_DESC");

                        for(i=0; i<5; i++)
                        {
                            if (_dctime == dcTime[i].ToString()) //할인시간과 같은 테이블 필드찾기
                            {
                                dcIndex = i;

                                if (dcFreeCount[dcIndex] > 0) _resultPoint = "무료포인트사용";
                                else if (dcCount[dcIndex] > 0) _resultPoint = "유료포인트사용";
                                else _resultPoint = "포인트부족";

                                break;
                            }
                        }
                    }


                    if (dcIndex >= 0)
                    {
                        if (_resultPoint == "무료포인트사용" || _resultPoint == "유료포인트사용")
                        {
                            connection.Close();
                            connection.Open();

                            //웹할인 등록
                            sql = "INSERT INTO tb_web_dc (PCODE, PNAME,PGUBUN,DC_CARNO,DC_NAME,DC_CODE,DT_DC,USE_YN) VALUES (@pcode,@pname,@pgubun,@carno,@desc,@dcvalue,@regTime,'N')";
                            var mySqlCommand2 = new MySqlCommand(sql, connection);
                            //mySqlCommand2.Parameters.AddWithValue("@pcode", _pcode);
                            mySqlCommand2.Parameters.AddWithValue("@pcode", _seq); //로그인ID의 고유번호
                            mySqlCommand2.Parameters.AddWithValue("@pname", _pname);
                            mySqlCommand2.Parameters.AddWithValue("@pgubun", _pgubun);
                            mySqlCommand2.Parameters.AddWithValue("@carno", _carno);
                            mySqlCommand2.Parameters.AddWithValue("@desc", _dcdesc);
                            mySqlCommand2.Parameters.AddWithValue("@dcvalue", _dctime);
                            mySqlCommand2.Parameters.AddWithValue("@regTime", nowTime);

                            if (mySqlCommand2.ExecuteNonQuery() == 1)
                            {
                                _result = 1;//성공

                                if (_resultPoint == "무료포인트사용")
                                {
                                    if (dcIndex == 0) sql = "UPDATE tb_partner SET PDC1_FREECOUNT=@point WHERE ID = @id ";
                                    else if (dcIndex == 1) sql = "UPDATE tb_partner SET PDC2_FREECOUNT=@point WHERE WHERE ID = @id ";
                                    else if (dcIndex == 2) sql = "UPDATE tb_partner SET PDC3_FREECOUNT=@point WHERE WHERE ID = @id ";
                                    else if (dcIndex == 3) sql = "UPDATE tb_partner SET PDC4_FREECOUNT=@point WHERE WHERE ID = @id ";
                                    else if (dcIndex == 4) sql = "UPDATE tb_partner SET PDC5_FREECOUNT=@point WHERE WHERE ID = @id ";
                                }
                                else if (_resultPoint == "유료포인트사용")
                                {
                                    if (dcIndex == 0) sql = "UPDATE tb_partner SET PDC1_COUNT=@point WHERE ID = @id ";
                                    else if (dcIndex == 1) sql = "UPDATE tb_partner SET PDC2_COUNT=@point WHERE ID = @id ";
                                    else if (dcIndex == 2) sql = "UPDATE tb_partner SET PDC3_COUNT=@point WHERE ID = @id ";
                                    else if (dcIndex == 3) sql = "UPDATE tb_partner SET PDC4_COUNT=@point WHERE ID = @id ";
                                    else if (dcIndex == 4) sql = "UPDATE tb_partner SET PDC5_COUNT=@point WHERE ID = @id ";
                                }

                                //var connection3 = new MySqlConnection(connectionString);
                                var mySqlCommand3 = new MySqlCommand(sql, connection);
                                if (_resultPoint == "무료포인트사용")
                                {
                                    mySqlCommand3.Parameters.AddWithValue("@point", --dcFreeCount[dcIndex]);
                                    mySqlCommand3.Parameters.AddWithValue("@id", _id);
                                }
                                else if (_resultPoint == "유료포인트사용")
                                {
                                    mySqlCommand3.Parameters.AddWithValue("@point", --dcCount[dcIndex]);
                                    mySqlCommand3.Parameters.AddWithValue("@id", _id);
                                }
                                //웹할인 포인트 차감
                                mySqlCommand3.ExecuteNonQuery();


                                jElem.Add("seq", _seq);
                                jElem.Add("pcode", _pcode);
                                jElem.Add("pname", _pname);
                                jElem.Add("pgubun", _pgubun);
                                jElem.Add("carno", _carno);
                                //jElem.Add("dc1desc", dcDesc[0]);
                                //jElem.Add("dc1time", dcTime[0]);
                                //jElem.Add("dc1count", dcCount[0]);
                                //jElem.Add("dc1free", dcFreeCount[0]);
                                //jElem.Add("dc2desc", dcDesc[1]);
                                //jElem.Add("dc2time", dcTime[1]);
                                //jElem.Add("dc2count", dcCount[1]);
                                //jElem.Add("dc2free", dcFreeCount[1]);
                                //jElem.Add("dc3desc", dcDesc[2]);
                                //jElem.Add("dc3time", dcTime[2]);
                                //jElem.Add("dc3count", dcCount[2]);
                                //jElem.Add("dc3free", dcFreeCount[2]);
                                //jElem.Add("dc4desc", dcDesc[3]);
                                //jElem.Add("dc4time", dcTime[3]);
                                //jElem.Add("dc4count", dcCount[3]);
                                //jElem.Add("dc4free", dcFreeCount[3]);
                                //jElem.Add("dc5desc", dcDesc[4]);
                                //jElem.Add("dc5time", dcTime[4]);
                                //jElem.Add("dc5count", dcCount[4]);
                                //jElem.Add("dc5free", dcFreeCount[4]);
                                jElem.Add("result", "SUCCESS"); //FAIL or SUCCESS
                                JBody.Add(jElem);
                            }
                            else
                            {
                                _result = 0;//실패
                                            //respTime = nowTime;

                                jElem.Add("seq", _seq);
                                jElem.Add("pcode", _pcode);
                                jElem.Add("pname", _pname);
                                jElem.Add("pgubun", _pgubun);
                                jElem.Add("carno", _carno);
                                //jElem.Add("desc", _dcdesc);
                                //jElem.Add("dctime", _dctime);
                                jElem.Add("result", "웹할인 등록실패(Insert Query Failed)"); //FAIL or SUCCESS
                                JBody.Add(jElem);
                                util.FileLogger("[ERROR][setWebdcProc DataBase Insert Fail] " + "setWebdcProc:" + _pname + "," + _dcdesc + "," + _carno + ":웹할인 등록 실패(Insert Query Failed)");
                            }
                        }
                        else if (_resultPoint == "포인트부족")
                        {
                            _result = 0;//실패

                            jElem.Add("seq", _seq);
                            jElem.Add("pcode", _pcode);
                            jElem.Add("pname", _pname);
                            jElem.Add("pgubun", _pgubun);
                            jElem.Add("carno", _carno);
                            //jElem.Add("desc", _dcdesc);
                            //jElem.Add("dctime", _dctime);
                            jElem.Add("result", "웹할인 등록실패(포인트부족)"); //FAIL or SUCCESS
                            JBody.Add(jElem);
                            util.FileLogger("[ERROR][setWebdcProc DataBase Insert Fail] " + "setWebdcProc:" + _pname + "," + _dcdesc + "," + _carno + ":웹할인 등록 실패(포인트부족)");
                        }
                    }
                    else
                    {
                        _result = 0;//실패

                        jElem.Add("seq", _seq);
                        jElem.Add("pcode", _pcode);
                        jElem.Add("pname", _pname);
                        jElem.Add("pgubun", _pgubun);
                        jElem.Add("carno", _carno);
                        jElem.Add("result", "웹할인 등록실패(코드에러)"); //FAIL or SUCCESS
                        JBody.Add(jElem);
                        util.FileLogger("[ERROR][setWebdcProc DataBase Insert Fail] " + "setWebdcProc:" + _pname + "," + _dcdesc + "," + _carno + ":웹할인 등록 실패(할인시간에러)");
                    }
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][setWebdcProc SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][setWebdcProc SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("seq", "error");
                    jElem.Add("pcode", "error");
                    jElem.Add("pname", "error");
                    jElem.Add("pgubun", "error");
                    jElem.Add("carno", "error");
                    //jElem.Add("desc", "error");
                    //jElem.Add("dctime", "error");
                    jElem.Add("result", se.Message); //FAIL or SUCCESS
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                    util.FileLogger("[ERROR][setWebdcProc Fail] " + "setWebdcProc:" + _pname + "," + _dcdesc + "," + _carno + ":" + se.Message);
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][setWebdcProc Exception] " + ex.Message);

                jElem.Add("seq", "error");
                jElem.Add("pcode", "error");
                jElem.Add("pname", "error");
                jElem.Add("pgubun", "error");
                jElem.Add("carno", "error");
                //jElem.Add("desc", "error");
                //jElem.Add("dctime", "error");
                jElem.Add("result", ex.Message); //FAIL or SUCCESS

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                util.FileLogger("[ERROR][setWebdcProc Fail] " + "setWebdcProc:" + _pname + "," + _dcdesc + "," + _carno + ":" + ex.Message);
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        /// <summary>
        /// Host 에서 수신
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_password"></param>
        /// <param name="_token"></param>
        /// <param name="respJson"></param>
        static public void pushAlarm(string _seq, string _sender, string _recver, string _gubun, string _gateno, string _gatename, string _title, string _message, string _datetime, string _rtsp, ref JObject respJson)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var JHead = new JObject();
            var JBody = new JArray();
            var jElem = new JObject();
            bool isMember = false;
            string res = "FAILED";

            //string decID = JwtEncoder.JwtDecode(_recver, "www.jawootek.com");
            var connectionString = connStr;
            var connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                var sql = "SELECT * FROM tb_id WHERE ID = @recver ";

                var mySqlCommand = new MySqlCommand(sql, connection);
                mySqlCommand.Parameters.AddWithValue("@recver", _recver);

                var mySqlDataReader = mySqlCommand.ExecuteReader();

                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                if (mySqlDataReader.Read())
                {
                    isMember = true;

                    connection.Close();
                    connection.Open();

                    sql = "SELECT * FROM tb_devices_admin ";
                    var mySqlCommand2 = new MySqlCommand(sql, connection);

                    var mySqlDataReader2 = mySqlCommand2.ExecuteReader();
                    while (mySqlDataReader2.Read())
                    {
                        string id = GetStringFieldValue(mySqlDataReader2, "ID");
                        string deviceId = GetStringFieldValue(mySqlDataReader2, "token");

                        SendToFirebaseMessagingServerAsync(_seq, _sender, deviceId, _gubun, _gateno, _gatename, _title, _message, _datetime, _rtsp);

                        var jElem2 = new JObject();
                        jElem2.Add("id", id); //수신ID
                        JBody.Add(jElem2);
                    }
                    mySqlDataReader2.Close();
                }
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
                mySqlDataReader.Close();
            }
            catch (SocketException se)
            {
                util.FileLogger("[ERROR][pushAlarm SocketException] " + se.ErrorCode);
                util.FileLogger("[ERROR][pushAlarm SocketException] " + se.Message);
                if (se.ErrorCode == 10061)
                {
                    jElem.Add("id", "error");
                    JBody.Add(jElem);
                    JHead.Add("isMember", isMember);
                    JHead.Add("values", JBody);
                    respJson = JHead;
                }
            }
            catch (Exception ex)
            {
                util.FileLogger("[ERROR][pushAlarm Exception] " + ex.Message);

                jElem.Add("id", "error");

                JBody.Add(jElem);
                JHead.Add("isMember", isMember);
                JHead.Add("values", JBody);
                respJson = JHead;
            }
            finally
            {
                if (connection.State.ToString() == "Open")
                    connection.Close();
            }
        }

        static public async Task SendToFirebaseMessagingServerAsync(String _seq, String _sender, String deviceId, string _gubun, string _gateno, string _gatename, string _title, string _message, string _datetime, string _rtsp)
        {
            //await OnGetAsync(_token, _message);
            SendNotification(_seq, _sender, deviceId, _gubun, _gateno, _gatename, _title, _message, _datetime, _rtsp);
        }
        static public string SendNotification(String _seq, String _sender, string deviceId, String _gubun, string _gateno, string _gatename, String _title, string _message, string _datetime, string _rtsp)
        {
            string SERVER_API_KEY = "AAAAWpOlMiM:APA91bGK8ZGVd78d5015lLY8QVfTFnMsEeF6HuVgNsgUl9IU7X4wpZLdKFEs6SEOWdHNBBChCo53ZF6kAfhI5FN78qzry9W0YdUFUr_Lc42LlYk_6Qsy38CtxY62rHF63onpfzQtsQ9q";
            var result = "";
            var image = Directory.GetCurrentDirectory() + "\\images\\Parking_Red.ico";
            //string image1 = Directory.GetCurrentDirectory() + "\\images\\login.png";

            var notificationInputDto = new
            {
                to = deviceId,
                notification = new
                {
                    body = _message,
                    title = _title,
                    icon = "",
                    type = ""
                },
                data = new
                {
                    seq = _seq,
                    gubun = _gubun,
                    gateno = _gateno,
                    gatename = _gatename,
                    date = _datetime,
                    rtsp = _rtsp,
                    key1 = "value1",
                    key2 = "value2",
                },
                android = new //안드로이드
                {
                    notification = new
                    {
                        //image = "https://firebasestorage.googleapis.com/v0/b/parkingmanager-11242.appspot.com/o/profile%2Fgate.png?alt=media&token=5cdebe38-ca3e-4909-97db-b84794d67a7f"
                        imageUrl = "https://img6.yna.co.kr/photo/yna/YH/2010/02/01/PYH2010020102450000400_P4.jpg"
                    },
                    priority = "max"
                },
                apns = new //IOS
                {
                    headers = new
                    {
                        //apns-priority = 5
                        priority = 5
                    },
                    payload = new
                    {
                        aps = new
                        {
                            multable_content = 1
                        }
                    },
                    fcm_options = new
                    {
                        image = "https://firebasestorage.googleapis.com/v0/b/parkingmanager-11242.appspot.com/o/profile%2Fgate.png?alt=media&token=5cdebe38-ca3e-4909-97db-b84794d67a7f"
                    }
                },
                webpush = new //WEB
                {
                  headers = new {
                    Urgency = "high",
                      image = "https://firebasestorage.googleapis.com/v0/b/parkingmanager-11242.appspot.com/o/profile%2Fgate.png?alt=media&token=5cdebe38-ca3e-4909-97db-b84794d67a7f"
                  }
                }
            };
            try
            {
                
                var webAddr = "https://fcm.googleapis.com/fcm/send";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization:key=" + SERVER_API_KEY);
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(notificationInputDto));
                    streamWriter.Flush();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //throw;
                result = ex.Message;
            }

            Global.util.FileLogger("[PUSH ALARM] " + result);
            return result;
        }

        static public async Task OnGetAsync(String _token, String _data)
        {
            //_authFilePath = _rootPath + ".\\Auth_parking.json";
            //_authFilePath = _rootPath + ".\\google-services.json";
            _authFilePath = "\\MobileRestAPI\\MobileRestAPI\\" + "google-services.json";
            FirebaseApp app = null;
            try
            {
                try
                {
                    app = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(_authFilePath)
                    }, "myApp");
                }
                catch (Exception ex)
                {
                    app = FirebaseApp.GetInstance("myApp");
                }


                var fcm = FirebaseAdmin.Messaging.FirebaseMessaging.GetMessaging(app);

                FirebaseAdmin.Messaging.Message message = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new Notification
                    {
                        Title = "My push notification title",
                        Body = "Content for this push notification"
                    },
                    Data = new Dictionary<string, string>()
                    {
                        { "AdditionalData1", "data 1" },
                        { "AdditionalData2", "data 2" },
                        { "AdditionalData3", "data 3" },
                    },

                    Topic = "fcm_test"
                };

                try
                {
                    result = await fcm.SendAsync(message);
                    Console.WriteLine("sent fcm message");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("sent fcm message error : " + ex.Message);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("fcm message error : " + e.Message);
            }
        }

        static public void GetMessage(int code, ref string http, ref string msg, ref string detailmsg)
        {
            switch (code)
            {
                case 1:
                    //http = "200";
                    //msg = "Success";
                    //detailmsg = "";
                    break;
                case 2:
                    http = "401";
                    msg = "Unauthorized, Invalid token";
                    detailmsg = "권한이 없습니다. 토큰을 확인 바랍니다";
                    break;
                case 3:
                    http = "401";
                    msg = "Unauthorized, Invalid token";
                    detailmsg = "권한이 없습니다. 시크릿을 확인 바랍니다";
                    break;
                case 4:
                    http = "400";
                    msg = "Invalid parameters";
                    detailmsg = "올바르지 않은 파라미터입니다";
                    break;
                case 5:
                    http = "400";
                    msg = "Invalid parameters";
                    detailmsg = "차량번호 및 날짜타입이 맞지 않습니다";
                    break;
                case 6:
                    http = "400";
                    msg = "Invalid parameters";
                    detailmsg = "방문기간이 초과 되었습니다";
                    break;
                case 7:
                    http = "404";
                    msg = "Not Found";
                    detailmsg = "페이지를 찾을 수 없습니다";
                    break;
                case 8:
                    http = "405";
                    msg = "Requested method is not supported";
                    detailmsg = "지원하지 않는 http 메소드 입니다";
                    break;
                case 9:
                    http = "500";
                    msg = "Internal Server Error";
                    //detailmsg = "차량번호 인식오류입니다";
                    detailmsg = "인식시스템 준비중..";
                    break;
                case 99:
                    http = "500";
                    msg = "Internal Server Error";
                    detailmsg = "서버에서 에러가 발생했습니다";
                    break;
            }
        }

        static public string GetStringFieldValue(MySqlDataReader rd, string fieldName)
        {
            string ret = "";
            try
            {
                if (rd.IsDBNull(rd.GetOrdinal(fieldName)) == true)
                {
                    ret = "";
                }
                else
                {
                    ret = rd[rd.GetOrdinal(fieldName)].ToString();
                }
            }
            catch (Exception e)
            {
                util.FileLogger("[DataBase GetStringFieldValue] " + e.Message);
            }
            return ret;
        }
        static public int GetIntFieldValue(MySqlDataReader rd, string fieldName)
        {
            int ret = 0;
            try
            {
                if (rd.IsDBNull(rd.GetOrdinal(fieldName)) == true)
                {
                    ret = 0;
                }
                else
                {
                    if (rd[rd.GetOrdinal(fieldName)].GetType().Name == "Int32")
                    {
                        ret = (int)rd[rd.GetOrdinal(fieldName)];
                    }
                    else if (rd[rd.GetOrdinal(fieldName)].GetType().Name == "Int64")
                    {
                        ret = (int)(Int64)rd[rd.GetOrdinal(fieldName)];
                    }
                }
            }
            catch (Exception e)
            {
                util.FileLogger("[DataBase GetIntFieldValue] " + e.Message);
            }
            return ret;
        }
    }
}

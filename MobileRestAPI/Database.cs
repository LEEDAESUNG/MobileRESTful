using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace MobileRestAPI.Model
{
    class CDatabase
    {
        //public OdbcConnection oConnection;
        //public OdbcCommand oCommand;
        //public string strConn = "DRIVER={MySQL ODBC 3.51 Driver}; Server=localhost; Database=jwt_sanps; Uid=admin; Pwd=jawootek; CharSet=utf8;";

        public OdbcConnection myConnection;
        public OdbcCommand myCommand;
        public string sConn;

        int ErrQueryExecCount;
        private CCommonUtil MyUtil;

        //public static string sMainDBConn = "DRIVER={MySQL ODBC 3.51 Driver}; SERVER=localhost;DATABASE=jwt_sanps;UID=admin;PWD=jawootek;OPTION=1+2+8+32+2048+16384;STMT=SET NAMES EUCKR";



        public CDatabase(string _sConn)
        {
            MyUtil = new CCommonUtil();
            myConnection = null;
            myCommand = null;

            sConn = _sConn;

        }
        //~CDatabase() => Console.WriteLine("~CDatabase");

        public void DatabaseOpen()
        {
            try
            {
                myConnection = new OdbcConnection(sConn);
                myConnection.Open();
            }
            catch (Exception ex)
            {
                MyUtil.ErrLogger("[DatabaseOpen Error] " + "데이터베이스 오픈 실패:" + ex.Message);
                //MessageBox.Show("Database Open 에러!! \n" + ex.Message, " \n Database Open \n", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Create/Insert/Update/Delete
        public int QueryExecute(string _sQry)
        {
            // 매번 커넥션 만드는 문제.
            //int nRecord = -1;
            //using (OdbcConnection connection = new OdbcConnection(_sConn))
            //{
            //    //OdbcTransaction transaction = null;
            //    OdbcCommand command = new OdbcCommand(_sQry, connection);
            //    try
            //    {
            //        //transaction = connection.BeginTransaction();
            //        //command.Transaction = transaction;

            //        connection.Open();
            //        nRecord = command.ExecuteNonQuery();

            //        //transaction.Commit();
            //    }
            //    catch (Exception ex)
            //    {
            //        //try
            //        //{
            //        //    transaction.Rollback();
            //        //}
            //        //catch (Exception e)
            //        //{
            //        //    Console.WriteLine("transaction err : " + ex.Message);
            //        //}
            //        Console.WriteLine(ex.Message);
            //        return -2;
            //    }
            //}
            ErrQueryExecCount = 0;
        RETRY_JOB:
            int nRecord = -1;
            try
            {
                ////if (myConnection == null) {
                //    myConnection = new OdbcConnection(sConn);
                ////}

                ////if (myCommand == null)
                ////{
                //    myCommand = new OdbcCommand(_sQry, myConnection);
                //    myConnection.Open();
                ////}

                //myCommand.CommandText = _sQry;
                //nRecord = myCommand.ExecuteNonQuery();


                //using (myConnection = new OdbcConnection(sConn))
                //{
                //    myConnection.Open();
                //    myCommand = new OdbcCommand(_sQry, myConnection);
                //    myCommand.CommandText = _sQry;
                //    nRecord = myCommand.ExecuteNonQuery();
                //}

                using (myCommand = new OdbcCommand(_sQry, myConnection))
                {
                    myCommand.CommandText = _sQry;
                    nRecord = myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("10013")) // {[MySQL][ODBC 3.51 Driver]Can't connect to MySQL server on 'jawootek.iptime.org' (10013)}
                {
                    //Console.WriteLine("[QueryExcute Error] " + "방화벽이나 백신 프로그램에서 차단됐습니다.");
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Connect failed:" + ex.Message);
                }
                else if (ex.Message.Contains("2006")) // MySQL server has gone away
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database has gone away:" + ex.Message);
                }
                else if (ex.Message.Contains("2013")) // Lost connection to MySQL server during query
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Lost connection:" + ex.Message);
                }
                else if (ex.Message.Contains("1194")) // Table '%s' is marked as crashed and should be repaired
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Crashed:" + ex.Message);
                    //myCommand = new OdbcCommand(_sQry, myConnection);
                    //myConnection.Open();
                }
                else if (ex.Message.Contains("1195")) // Table '%s' is marked as crashed and last (automatic?) repair failed
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Repair failed:" + ex.Message);
                    //myCommand = new OdbcCommand(_sQry, myConnection);
                    //myConnection.Open();
                }
                else if (ex.Message.Contains("Duplicate entry")) // Duplicate entry
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Duplicate entry:" + _sQry);
                    //MyUtil.ErrLogger("[QueryExcute Error] " + "Database Duplicate entry:" + ex.Message);
                }
                else
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Other failed:" + ex.Message);
                }

                if (ErrQueryExecCount < 3)
                {
                    try
                    {
                        ErrQueryExecCount = ErrQueryExecCount + 1;
                        MyUtil.ErrLogger("[QueryExcute Error] " + "Database Retry and Repair(" + ErrQueryExecCount + ")");

                        myConnection = null;
                        myConnection = new OdbcConnection(sConn);
                        myConnection.Open();
                    }
                    catch (Exception ex2)
                    {
                        MyUtil.ErrLogger("[QueryExcute Error] " + "Database Open failed. Database and System Check." + ex2.Message);
                    }

                    goto RETRY_JOB;
                }
                else
                {
                    MyUtil.ErrLogger("[QueryExcute Error] " + "Database Retry and Repair failed. Database and System Check.");
                    return -1;
                }
            }



            return nRecord;
        }

        public string GetStringFieldValue(OdbcDataReader rd, string fieldName)
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
                MyUtil.FileLogger("[DataBase GetStringFieldValue] " + e.Message);
            }
            return ret;
        }

        public int GetIntFieldValue(OdbcDataReader rd, string fieldName)
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
                MyUtil.FileLogger("[DataBase GetIntFieldValue] " + e.Message);
            }
            return ret;
        }
        public OdbcDataReader QuerySelect(string _sQry)
        {
            try
            {
                using (OdbcCommand MyCommand = new OdbcCommand(_sQry, myConnection))
                {
                    OdbcDataReader MyDataReader = MyCommand.ExecuteReader();
                    //while (MyDataReader.Read())
                    //{
                    //    if (string.Compare(myConnection.Driver, "myodbc3.dll") == 0)
                    //    {
                    //        //Supported only by Connector/ODBC 3.51
                    //        //Console.WriteLine("Data:" + MyDataReader.GetInt32(0) + " " +
                    //        //                  MyDataReader.GetString(1) + " " +
                    //        //                  MyDataReader.GetInt64(2));
                    //    }
                    //    else
                    //    {
                    //        //BIGINTs not supported by Connector/ODBC
                    //        //Console.WriteLine("Data:" + MyDataReader.GetInt32(0) + " " +
                    //        //                  MyDataReader.GetString(1) + " " +
                    //        //                  MyDataReader.GetInt32(2));
                    //    }
                    //}

                    ////MyDataReader.Close();
                    return MyDataReader;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Database QuerySelect 에러!! \n" + ex.Message, " \n Database QuerySelect \n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}

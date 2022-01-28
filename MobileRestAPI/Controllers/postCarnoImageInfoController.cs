using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using MobileRestAPI.Models;
using System.Runtime.InteropServices;

namespace MobileRestAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]


    public class ThreeModule32
    {
        [DllImport("StRecThreeModule32.dll")]
        public static extern int SPT_EngineOpen32T();

        [DllImport("StRecThreeModule32.dll")]
        public static extern int SPT_EngineClose32T();

        [DllImport("StRecThreeModule32.dll")]
        public static extern int SPT_SetTimeLimit(int nMillisecond);

        [DllImport("StRecThreeModule32.dll")]
        unsafe public static extern int SPT_Engine32T_CS(
                          char[] psFilePath
                        , int nVLeft, int nVTop, int nVRight, int nVBottom
                        , int nRatioX, int nRatioY, int nDegreeX, int nDegreeY
                        , [In, Out] char[] result
                        , int* nLeft, int* nTop, int* nRight, int* nBottom);

        [DllImport("StRecThreeModule32.dll")]
        unsafe public static extern int SPT_Engine32T_Buffer_Revise(
                          IntPtr pImage, int nBPL, int nBPP, int nWidth, int nHeight
                        , int nVLeft, int nVTop, int nVRight, int nVBottom
                        , int nRatioX, int nRatioY, int nDegreeX, int nDegreeY
                        , [In, Out] char[] result
                        , int* nLeft, int* nTop, int* nRight, int* nBottom);


        public static string GetResultString(int nRes)
        {
            switch (nRes)
            {
                case 0: return "0.오류인식";
                case 1: return "1.성공!";     //(서울12 가1234)
                case 2: return "2.성공.OLD";  //(서울3 가1234)
                case 3: return "3.성공.2004"; //(12가 1234)
                case 4: return "4.성공.2005"; //(가로 1줄짜리 번호판, 현재 경찰차)
                case 5: return "5.성공.특장";   //(주황색 특장차 번호판)
                case 6: return "6.성공.2006"; //(가로 1줄짜리 번호판, 신번호판);
                case 7: return "7.성공.3자리";
                case 13: return "13.성공.외교관";
                case 14: return "14.성공.임시번호";
                case 15: return "15.성공.국기번호";
                case 16: return "16.성공.7자리";
                case 17: return "17.성공.제외";
                case 18: return "18.성공.오토바이";
                case 19: return "19.성공.특장차";
            }

            return "인식실패";
        }
    }

    public class ImageModel
    {
        public string Id { get; set; }
        public string Pw { get; set; }
        public string HostName { get; set; }
        public IFormFile ImageFile { get; set; }
    }

    public class PostCarnoImageInfoController : Controller
    {
        
        [HttpPost]
        public async Task<ActionResult<IEnumerable<string>>> ImageRecognition(ImageModel imageModel)
        {
            Global.util.FileLogger("[INFO][HttpPost PostCarnoImageInfo] ");

            JsonReturn JReturn = new JsonReturn();
            JObject respJson = new JObject();

            
            var request = HttpContext.Request;
            string authHeaderValue = request.Headers["Authorization"];
            bool auth = Global.AuthCheck(authHeaderValue);
            if (auth == true)
            {
                string id = "" + imageModel.Id;
                string password = "" + imageModel.Pw;
                string hostName = "" + imageModel.HostName + "\\"; hostName= hostName.Replace(":", "_"); //포트번호 구분자는 언더바로 대체.
                string fileName = "" + imageModel.ImageFile.FileName;
                string recogCarno = "";
                string recogResult = "";
                string token = "";

                Global.util.FileLogger("[INFO][HttpPost PostCarnoImageInfo] : id:" + id + ", password:" + password + ", hostName:" + hostName + ", fileName:" + fileName);

                //string sRootPath = System.IO.Directory.GetCurrentDirectory();
                string sDocPath = Global.RootPath + "\\Image\\";
                string sDate = DateTime.Now.ToString("yyyyMMdd") + "\\";
                try
                {
                    lock (Global.saveImageLock)
                    {
                        DirectoryInfo di = new DirectoryInfo(sDocPath);
                        if (di.Exists == false)
                        {
                            di.Create();
                        }
                        DirectoryInfo di2 = new DirectoryInfo(sDocPath + hostName);
                        if (di2.Exists == false)
                        {
                            di2.Create();
                        }
                        DirectoryInfo di3 = new DirectoryInfo(sDocPath + hostName + sDate);
                        if (di3.Exists == false)
                        {
                            di3.Create();
                        }
                    }

                    //파일저장
                    string savePath = sDocPath + hostName + sDate + imageModel.ImageFile.FileName;

                    using (Stream fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        await imageModel.ImageFile.CopyToAsync(fileStream);
                    }


                    if (Global.m_nRecogInitDLL != 1)
                    {
                        try
                        {
                            int nOpenRes = ThreeModule32.SPT_EngineOpen32T();
                            if (nOpenRes <= 0)
                            {
                                if (nOpenRes == 0)
                                {
                                    //MessageBox.Show("번호판인식모듈 초기화 실패!\r\n\r\n동일한 증상이 반복될 경우, 모듈 공급자에게 문의하세요!!");
                                }
                                Global.GetMessage(ReturnCode.CarnoRecognitionError, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
                                Global.util.FileLogger("[ERROR][HttpPost PostCarnoImageInfo] : id:" + id + ", password:" + password + ", hostName:" + hostName + ", fileName:" + fileName + ":" + JReturn.detail_msg + ":" + nOpenRes);
                                return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
                            }
                            else
                            {
                                RecogProcessFile(savePath, ref recogCarno, ref recogResult);
                                Global.getCarnoInfo(id, password, token, recogCarno, ref respJson);
                                //Global.util.FileLogger("[INFO][HttpPost PostCarnoImageInfo] : id:" + id + ", password:" + password + ", hostName:" + hostName + ", fileName:" + fileName + ":" + recogCarno);
                                return Json(respJson);
                            }

                        }
                        catch (Exception e)
                        {
                            Global.GetMessage(ReturnCode.CarnoRecognitionError, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
                            Global.util.FileLogger("[ERROR][HttpPost PostCarnoImageInfo] : id:" + id + ", password:" + password + ", hostName:" + hostName + ", fileName:" + fileName + ":" + JReturn.detail_msg);
                            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Global.util.FileLogger("[INFO][HttpPost PostCarnoImageInfo] Error : " + ex.Message);
                    Global.GetMessage(ReturnCode.InternalServerError, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
                }

                return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
            }
            else
            {
                Global.GetMessage(ReturnCode.UnAuthInvalidToken1, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
                return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
            }
        }
        unsafe public void RecogProcessFile(String filePath, ref String recogCarno, ref String recogResult)
        {
            long nSTicks = DateTime.UtcNow.Ticks;
            //////////////////////////////////////////////////
            char[] pPath = filePath.ToCharArray();
            char[] pNums = new char[500];

            int nL, nT, nR, nB;

            int nRes = ThreeModule32.SPT_Engine32T_CS(pPath, 0, 0, 0, 0, 0, 0, 0, 0, pNums, &nL, &nT, &nR, &nB);

            int m_nLeft = nL;
            int m_nTop = nT;
            int m_nRight = nR;
            int m_nBottom = nB;


            double dSpent = (DateTime.UtcNow.Ticks - nSTicks) / 1000.0 / TimeSpan.TicksPerMillisecond;

            recogResult = ThreeModule32.GetResultString(nRes);
            if (nRes == 0) {
                recogCarno = "인식실패";
            }
            else
            {
                recogCarno = new string(pNums);
                recogCarno = recogCarno.Replace("\0", "");
            }
            String edtTime = dSpent.ToString("N3");
        }
    }

        //// POST: postCarnoImageInfoController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: postCarnoImageInfoController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: postCarnoImageInfoController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: postCarnoImageInfoController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: postCarnoImageInfoController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
        //}
    }

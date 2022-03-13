using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileRestAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class setWebdcProcController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet setWebdcProcController] "); //웹할인 등록

            JsonReturn JReturn = new JsonReturn();
            JObject respJson = new JObject();
            var request = HttpContext.Request;
            string authHeaderValue = request.Headers["Authorization"];
            bool auth = Global.AuthCheck(authHeaderValue);
            if (auth == true)
            {
                string id = "" + request.Query["Id"];
                string password = "" + request.Query["Pw"];
                string token = "" + request.Query["Token"];
                string seq = request.Query["aseq"];         //id 일련번호(seq)
                string carno = request.Query["carno"];      //차량번호
                string dctime = request.Query["dctime"];    //할인시간
                string dcdesc = request.Query["dcdesc"];    //할인내용
                string pcode = request.Query["pcode"];      //파트너코드
                string pname = request.Query["pname"];      //파트너명
                string pgubun = request.Query["pgubun"];    //할인구분(T:시간)


                Global.util.FileLogger("[INFO][HttpGet setWebdcProcController] : id:" + id + ", password:" + password + ", token:" + token + ", seq:" + seq + ", carno:" + carno + ", dctime:" + dctime + ", dcdesc:" + dcdesc + ", pcode:" + pcode + ", pname:" + pname + ", pgubun:" + pgubun);

                Global.setWebdcProc(id, password, token, seq, carno, dctime, dcdesc, pcode, pname, pgubun, ref respJson);

                return Json(respJson);
            }
            else
            {
                //return new string[] { "Failed Auth" };
                Global.GetMessage(ReturnCode.UnAuthInvalidToken1, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
            }
            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }
    }
}

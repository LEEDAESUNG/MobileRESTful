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
    public class setReserveCarnoController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet setReserveCarnoController] ");

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
                string carno = "" + request.Query["Carno"];
                string dong = "" + request.Query["Dong"];
                string ho = "" + request.Query["Ho"];
                string name = "" + request.Query["Name"];
                string tel = "" + request.Query["Tel"];
                string startdate = "" + request.Query["StartDate"];
                string enddate = "" + request.Query["EndDate"];
                string incarMethod = "" + request.Query["Method"]; //1:즉시, 2:예약

                Global.util.FileLogger("[INFO][HttpGet setReserveCarnoController] : id:" + id + ", password:" + password + ", token:" + token + ", carno:" + carno);

                Global.setReserveCarno(id, password, token, carno, dong, ho, name, tel, startdate, enddate, incarMethod, ref respJson);

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

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
    public class getPushAlarmController : Controller
    {
        // GET: getPushAlarmController
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet getPushAlarmController] ");

            JsonReturn JReturn = new JsonReturn();
            JObject respJson = new JObject();
            var request = HttpContext.Request;

            string id = "" + request.Query["Id"];
            string gubun = "" + request.Query["Gubun"];
            string gateno = "" + request.Query["Gateno"];
            string gatename = "" + request.Query["Gatename"];
            string title = "" + request.Query["Title"];
            string message = "" + request.Query["Message"];
            Global.util.FileLogger("[INFO][HttpGet getPushAlarmController] : id:" + id + ", gubun:" + gubun + ", gateno:" + gateno + ", gatename:" + gatename + ", title:" + title + ", message:" + message);
            Global.pushAlarm(id, gubun, gateno, gatename, title, message, ref respJson); //token:FCM token
            return Json(respJson);

            //return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }
    }
}

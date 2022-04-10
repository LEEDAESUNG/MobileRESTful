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
            //seq,sender,recver,gubun,gateno,gatename,title,message,occurDateTime,rtsp
            string seq = "" + request.Query["Seq"];
            string sender = "" + request.Query["Sender"];
            string recver = "" + request.Query["Recver"];
            string gubun = "" + request.Query["Gubun"];
            string gateno = "" + request.Query["Gateno"];
            string gatename = "" + request.Query["Gatename"];
            string title = "" + request.Query["Title"];
            string message = "" + request.Query["Message"];
            string datetime = "" + request.Query["occurDateTime"];
            string rtsp = "" + request.Query["Rtsp"];
            Global.util.FileLogger("[INFO][HttpGet getPushAlarmController] : sender:" + sender + ", gubun:" + gubun + ", gateno:" + gateno + ", gatename:" + gatename + ", title:" + title + ", message:" + message);
            Global.pushAlarm(seq, sender, recver, gubun, gateno, gatename, title, message, datetime, rtsp, ref respJson); //token:FCM token
            return Json(respJson);

            //return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }
    }
}

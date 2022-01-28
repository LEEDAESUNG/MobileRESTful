using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MobileRestAPI.Model;
using Newtonsoft.Json.Linq;

namespace MobileRestAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("[controller]")]
    [ApiController]
    public class getGateStatusController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet getGateStatusController] ");

            JsonReturn JReturn = new JsonReturn();
            JObject respJson = new JObject();
            var request = HttpContext.Request;
            string authHeaderValue = request.Headers["Authorization"];
            bool auth = Global.AuthCheck(authHeaderValue);
            if (auth == true)
            {
                string nowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var JHead = new JObject();
                JHead.Add("resultCode", ReturnCode.Success);
                JHead.Add("resultMessage", "SUCCESS");
                JHead.Add("responseTime", nowTime);

                return Json(JHead);
            }
            else
            {
                Global.GetMessage(ReturnCode.UnAuthInvalidToken1, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
            }
            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }

        [HttpPost]
        public async Task<ActionResult<RegistryCarModel>> PostFunction([FromBody] RegistryCarModel regcar)
        {
            Global.util.FileLogger("[INFO][HttpPost getGateStatusController] ");

            JsonReturn JReturn = new JsonReturn();
            Global.GetMessage(ReturnCode.NotFound, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }
    }
}
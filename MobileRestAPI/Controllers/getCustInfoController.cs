using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileRestAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("[controller]")]
    [ApiController]
    public class getCustInfoController : Controller
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet getCustInfoController] ");

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
                Global.util.FileLogger("[INFO][HttpGet getCustInfoController] : id:" + id + ", password:" + password + ", token:" + token);

                Global.custInfo(id, password, token, ref respJson); //token:FCM token
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

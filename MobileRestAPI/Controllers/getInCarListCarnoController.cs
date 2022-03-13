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
    public class getInCarListWebdcListController : Controller //입차내역 + 입차이후 웹할인리스트 처리여부
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet getInCarListWebdcListController] ");

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
                string pname = "" + request.Query["Pname"];
                Global.util.FileLogger("[INFO][HttpGet getInCarListWebdcListController] : id:" + id + ", password:" + password + ", carno:" + carno + ", token:" + token);

                Global.getInCarListWebdcList(id, password, token, carno, pname, ref respJson); //token:FCM token
                return Json(respJson);
            }
            else
            {
                //return new string[] { "Failed Auth" };
                Global.GetMessage(ReturnCode.UnAuthInvalidToken1, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
            }
            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}

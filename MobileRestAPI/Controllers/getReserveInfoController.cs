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
    public class getReserveInfoController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet getReserveInfoController] ");

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
                Global.util.FileLogger("[INFO][HttpGet getReserveInfoController] : id:" + id + ", password:" + password + ", token:" + token);

                Global.reserveInfo(id, password, token, ref respJson); //token:FCM token
                return Json(respJson);
            }
            else
            {
                //return new string[] { "Failed Auth" };
                Global.GetMessage(ReturnCode.UnAuthInvalidToken1, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
            }
            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }

        // GET: getReserveInfoController
        public ActionResult Index()
        {
            return View();
        }

        // GET: getReserveInfoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: getReserveInfoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: getReserveInfoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: getReserveInfoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: getReserveInfoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: getReserveInfoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: getReserveInfoController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

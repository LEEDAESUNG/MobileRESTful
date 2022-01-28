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
    public class getReservationCarListController : Controller
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Global.util.FileLogger("[INFO][HttpGet getReservationCarListController] ");

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
                string startdate = "" + request.Query["Startdate"];
                string enddate = "" + request.Query["Enddate"];
                Global.util.FileLogger("[INFO][HttpGet getReservationCarListController] : id:" + id + ", password:" + password + ", token:" + token);

                Global.reserveCarList(id, password, token, startdate, enddate, ref respJson); //token:FCM token
                return Json(respJson);
            }
            else
            {
                //return new string[] { "Failed Auth" };
                Global.GetMessage(ReturnCode.UnAuthInvalidToken1, ref JReturn.httpcode, ref JReturn.msg, ref JReturn.detail_msg);
            }
            return Json(new { code = JReturn.httpcode, message = JReturn.msg, detail_message = JReturn.detail_msg });
        }

        // GET: getReservationCarInfoController
        public ActionResult Index()
        {
            return View();
        }

        // GET: getReservationCarInfoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: getReservationCarInfoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: getReservationCarInfoController/Create
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

        // GET: getReservationCarInfoController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: getReservationCarInfoController/Edit/5
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

        // GET: getReservationCarInfoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: getReservationCarInfoController/Delete/5
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

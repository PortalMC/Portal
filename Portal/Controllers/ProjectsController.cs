using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel;
using Portal.Models;

namespace Portal.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        public IActionResult Index(string uuid)
        {
            if (uuid == null)
            {
                return View();
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            ViewData["Uuid"] = uuid;
            return View("ProjectIndex");
        }

        public IActionResult Editor(string uuid)
        {
            if (uuid == null)
            {
                return RedirectToAction("Index");
            }
            if (!Utils.IsCorrectUuid(uuid))
            {
                // wrong uuid format
                return BadRequest();
            }
            ViewData["Uuid"] = uuid;
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

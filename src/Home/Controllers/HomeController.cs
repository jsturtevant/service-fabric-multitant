using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Fabric;

namespace Home.Controllers
{
    public class HomeController : Controller
    {
        private string appPath;

        public HomeController(StatelessServiceContext context)
        {
            ConfigurationPackage configPackage =
                       context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            appPath = configPackage.Settings.Sections["Web"].Parameters["AppPath"].Value;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = appPath;

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

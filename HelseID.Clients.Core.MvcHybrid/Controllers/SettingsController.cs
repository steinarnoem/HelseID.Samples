using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace HelseID.Clients.Core.MvcHybrid.Controllers
{
    public class SettingsController : Controller
    {
        private IApplicationLifetime ApplicationLifetime { get; set; }
        public SettingsController(IApplicationLifetime appLifetime)
        {
            ApplicationLifetime = appLifetime;
        }
        // GET: Settings
        public ActionResult Index()
        {
            var config = Config.Setting();
            return View(config);
        }

        // POST: Settings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Settings settings)
        {
            try
            {
                Config.Save(settings);

                ApplicationLifetime.StopApplication();

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return View();
            }
        }
    }
}
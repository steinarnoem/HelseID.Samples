using System.Web;
using System.Web.Mvc;

namespace HelseID.Clients.Owin.MvcHybrid.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public ActionResult Index()
        {
            var config = Config.Setting();
            return View(config);
        }

        [HttpPost]
        public ActionResult Index(Settings settings)
        {
            try
            {
                Config.Save(settings);

                HttpRuntime.UnloadAppDomain();

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return View();
            }
        }
    }
}

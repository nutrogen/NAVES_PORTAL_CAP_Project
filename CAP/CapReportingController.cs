using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    [Authorize]
    [CheckSession]
    public class CapReportingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

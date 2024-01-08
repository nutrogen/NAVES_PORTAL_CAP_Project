using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.Data;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    [Authorize]
    [CheckSession]
    public class CapProjectMonitoringController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;

        public CapProjectMonitoringController(BM_NAVES_PortalContext db)
        {
            _repository = db;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}

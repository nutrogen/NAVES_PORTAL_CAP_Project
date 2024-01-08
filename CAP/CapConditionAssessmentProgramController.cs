using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.Data;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    [Authorize]
    [CheckSession]
    public class CapConditionAssessmentProgramController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;

        public CapConditionAssessmentProgramController(BM_NAVES_PortalContext db)
        {
            _repository = db;
        }

        public IActionResult Index()
        {
            ViewBag.DataSource = _repository.VNAV_SELECT_CAP_MAIN_LISTs.ToList().OrderByDescending(M => M.NO);
            return View();
        }
    }
}

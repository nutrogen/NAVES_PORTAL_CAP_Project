using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.ViewModels;
using NuGet.Protocol.Core.Types;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using NavesPortalforWebWithCoreMvc.Common;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.RfSystemModels;
using Microsoft.EntityFrameworkCore;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    [Authorize]
    [CheckSession]
    public class CapDefaultDbController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        private readonly INavesPortalCommonService _commonService;

#if NET6_0
        private IWebHostEnvironment hostingEnv;
        //public UploaderController(IWebHostEnvironment env)
        public CapDefaultDbController(IWebHostEnvironment env, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository, INavesPortalCommonService commonService, IBM_NAVES_PortalContextProcedures procedures)
#else
        private IWebHostEnvironment hostingEnv;
        //public UploaderController(IHostingEnvironment env)
        public CapDefaultDbController(IHostingEnvironment env, BM_NAVES_PortalContext repository, RfSystemContext rfSystemRepository)
#endif
        {
            this.hostingEnv = env;

            _repository = repository;
            _rfSystemRepository = rfSystemRepository;
            _commonService = commonService;
            _procedures = procedures;
        }

        public IActionResult Index()
        {
            ViewBag.dataSource = _repository.VNAV_SELECT_CAP_DEFAULT_DB_LISTs.ToList().OrderByDescending(M => M.NO);

            return View();
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.UserList = _rfSystemRepository.RFV_USER_DEPTs.ToList();

            List<PNAV_SIP_GET_NSN_LISTResult> nsnList = await _procedures.PNAV_SIP_GET_NSN_LISTAsync();
            ViewBag.NsnDataSource = nsnList.AsEnumerable();

            List<QuantityTypeViewModel> _QuantityType = new List<QuantityTypeViewModel>();
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });

            ViewBag.QuantityType = _QuantityType;

            // Work Category Code List
            ViewBag.ProjectCategory = _commonService.getWorkCategoryCodeListAsync()
                .Where(m => m.Category.Equals("CAP"));

            ViewBag.CurrentIdx = Guid.NewGuid();

            // User Name
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AfterCreate(CapDefaultDbViewModel? capDefaultViewModel)
        {
            string result = String.Empty;

            try
            {
                if (capDefaultViewModel is not null)
                {
                    Guid _DefaultdbIdx = Guid.NewGuid();
                    DateTime _regDate = DateTime.Now;
                    List<TNAV_CAP_DEFAULT_DB_PIC> tNAV_CAP_DEFAULT_DB_PIC = new List<TNAV_CAP_DEFAULT_DB_PIC>();

                    TNAV_CAP_DEFAULT_DB tNAV_CAP_DEFAULT_DB = new TNAV_CAP_DEFAULT_DB()
                    {
                        CAP_DEFAULT_DB_IDX = _DefaultdbIdx,
                        CATEGORY_ID = capDefaultViewModel.CATEGORY_ID,
                        JOB_ID = capDefaultViewModel.JOB_ID,
                        NSN_ID = capDefaultViewModel.NSN_ID,
                        DESCRIPTION = capDefaultViewModel.DESCRIPTION,
                        QUANTITY_AMOUNT = capDefaultViewModel.QUANTITY_AMOUNT,
                        QUANTITY_TYPE = capDefaultViewModel.QUANTITY_TYPE,
                        LATEST_INSPECTION_DATE = capDefaultViewModel.LATEST_INSPECTION_DATE,
                        SI_OBJECT_DELIVERY_DATE = capDefaultViewModel.SI_OBJECT_DELIVERY_DATE,
                        INSPECTION_PLACE = capDefaultViewModel.INSPECTION_PLACE,
                        INSPECTION_INTERVAL = capDefaultViewModel.INSPECTION_INTERVAL,
                        INSPECTION_PERIOD_START_DATE = capDefaultViewModel.INSPECTION_PERIOD_START_DATE,
                        INSPECTION_PERIOD_END_DATE = capDefaultViewModel.INSPECTION_PERIOD_END_DATE,
                        REG_DATE = _regDate,
                        IS_DELETED = false
                    };

                    // 할당된 PIC 목록
                    if (capDefaultViewModel.DEFAULT_PIC is not null)
                    {
                        foreach (string pic in capDefaultViewModel.DEFAULT_PIC)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PicUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pic).First();

                            TNAV_CAP_DEFAULT_DB_PIC PIC = new TNAV_CAP_DEFAULT_DB_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                CAP_DEFAULT_DB_IDX = _DefaultdbIdx,
                                USER_ID = PicUser.USER_ID,
                                USER_NAME_KR = PicUser.USER_NAME,
                                USRE_NAME_EN = PicUser.USER_NAME_E,
                                DEPT_ID = PicUser.DEPT_ID,
                                DEPT_NAME_KR = PicUser.DEPT_NAME,
                                DEPT_NAME_EN = PicUser.DEPT_NAME_E,
                                DEGREE_KR = PicUser.DEGREE,
                                DEGREE_EN = PicUser.DEGREE_E,
                                EMP_ID = PicUser.EMP_ID,
                                SUR_NO = PicUser.SUR_NO,
                                POSITION_KR = PicUser.POSITION_K,
                                POSITION_EN = PicUser.POSITION_E,
                                REG_DATE = _regDate
                            };

                            tNAV_CAP_DEFAULT_DB_PIC.Add(PIC);
                            result = "ok";
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        _repository.Add(tNAV_CAP_DEFAULT_DB);
                        _repository.AddRange(tNAV_CAP_DEFAULT_DB_PIC);
                        _repository.SaveChanges();
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction("Index", "CapDefaultDb");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveDefaultDb(CapDefaultDbViewModel? capDefaultViewModel)
        {
            string result = string.Empty;
            string message = string.Empty;

            try
            {
                if (capDefaultViewModel is not null)
                {
                    Guid capDefaultDbIdx = capDefaultViewModel.CAP_DEFAULT_DB_IDX;
                    DateTime regDate = DateTime.Now;

                    List<TNAV_CAP_DEFAULT_DB_PIC> tNAV_CAP_DEFAULT_DB_PIC = new List<TNAV_CAP_DEFAULT_DB_PIC>();

                    TNAV_CAP_DEFAULT_DB tNAV_DEFAULT_DB = new TNAV_CAP_DEFAULT_DB()
                    {
                        CAP_DEFAULT_DB_IDX = capDefaultDbIdx,
                        CATEGORY_ID = capDefaultViewModel.CATEGORY_ID,
                        JOB_ID = capDefaultViewModel.JOB_ID,
                        NSN_ID = capDefaultViewModel.NSN_ID,
                        DESCRIPTION = capDefaultViewModel.DESCRIPTION,
                        QUANTITY_AMOUNT = capDefaultViewModel.QUANTITY_AMOUNT,
                        QUANTITY_TYPE = capDefaultViewModel.QUANTITY_TYPE,
                        LATEST_INSPECTION_DATE = capDefaultViewModel.LATEST_INSPECTION_DATE,
                        SERVICE_LIFE = capDefaultViewModel.SERVICE_LIFE,
                        INSPECTION_PLACE = capDefaultViewModel.INSPECTION_PLACE,
                        INSPECTION_INTERVAL = capDefaultViewModel.INSPECTION_INTERVAL,
                        INSPECTION_PERIOD_START_DATE = capDefaultViewModel.INSPECTION_PERIOD_START_DATE,
                        INSPECTION_PERIOD_END_DATE = capDefaultViewModel.INSPECTION_PERIOD_END_DATE,
                        SI_OBJECT_DELIVERY_DATE = capDefaultViewModel.SI_OBJECT_DELIVERY_DATE,
                        REG_DATE = regDate
                    };

                    // 할당된 PIC 목록
                    if (capDefaultViewModel.DEFAULT_PIC is not null)
                    {
                        foreach (string pic in capDefaultViewModel.DEFAULT_PIC)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PicUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pic).First();

                            TNAV_CAP_DEFAULT_DB_PIC PIC = new TNAV_CAP_DEFAULT_DB_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                CAP_DEFAULT_DB_IDX = capDefaultDbIdx,
                                USER_ID = PicUser.USER_ID,
                                USER_NAME_KR = PicUser.USER_NAME,
                                USRE_NAME_EN = PicUser.USER_NAME_E,
                                DEPT_ID = PicUser.DEPT_ID,
                                DEPT_NAME_KR = PicUser.DEPT_NAME,
                                DEPT_NAME_EN = PicUser.DEPT_NAME_E,
                                DEGREE_KR = PicUser.DEGREE,
                                DEGREE_EN = PicUser.DEGREE_E,
                                EMP_ID = PicUser.EMP_ID,
                                SUR_NO = PicUser.SUR_NO,
                                POSITION_KR = PicUser.POSITION_K,
                                POSITION_EN = PicUser.POSITION_E,
                                REG_DATE = regDate
                            };

                            tNAV_CAP_DEFAULT_DB_PIC.Add(PIC);
                            result = "ok";
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        _repository.Add(tNAV_DEFAULT_DB);
                        _repository.AddRange(tNAV_CAP_DEFAULT_DB_PIC);
                        await _repository.SaveChangesAsync();

                        result = "OK";
                    }
                    else
                    {
                        result = "INVALID";
                        message = "ModelState InValid";
                    }
                }
                else
                {
                    result = "NONE";
                    message = "Parameter Empty";
                }
            }
            catch (Exception ex)
            {
                result = "ERROR";
                message = ex.Message;
            }

            return Json(new { result = result, message = message });
        }

        public async Task<IActionResult> Detail(Guid? id)
        {
            var tNAV_CAP_DEFAULT_DB = await _repository.TNAV_CAP_DEFAULT_DBs.FirstOrDefaultAsync(m => m.CAP_DEFAULT_DB_IDX == id);

            if (tNAV_CAP_DEFAULT_DB == null)
            {
                return NotFound();
            }

            ViewBag.UserList = _rfSystemRepository.RFV_USER_DEPTs.ToList();

            // Work Category Code List
            ViewBag.ProjectCategory = _commonService.getWorkCategoryCodeListAsync()
                .Where(m => m.Category.Equals("CAP"));

            List<PNAV_SIP_GET_NSN_LISTResult> resultNsnList = await _procedures.PNAV_SIP_GET_NSN_LISTAsync();
            ViewBag.NsnDataSource = resultNsnList.AsEnumerable();

            List<QuantityTypeViewModel> _QuantityType = new List<QuantityTypeViewModel>();
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });

            ViewBag.QuantityType = _QuantityType;

            List<TNAV_CAP_DEFAULT_DB_PIC> tNAV_CAP_DEFAULT_DB_PIC = await _repository.TNAV_CAP_DEFAULT_DB_PICs.Where(m => m.CAP_DEFAULT_DB_IDX == id).ToListAsync();
            ViewBag.Pic = tNAV_CAP_DEFAULT_DB_PIC;

            // 파일 업로드 목록 바인딩
            // 첨부파일 목록
            ViewBag.FileListFinalTechnicalReport = await _repository.TNAV_COM_FILEs.Where(m => m.DOCUMENT_IDX == id && m.KIND_OF_FILES == "FinalTechnicalReport").ToListAsync();
            ViewBag.FileListPhotoMovie = await _repository.TNAV_COM_FILEs.Where(m => m.DOCUMENT_IDX == id && m.KIND_OF_FILES == "PhotoMovie").ToListAsync();

            // User Name
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            if (tNAV_CAP_DEFAULT_DB.SI_OBJECT_DELIVERY_DATE != null && tNAV_CAP_DEFAULT_DB.LATEST_INSPECTION_DATE != null && tNAV_CAP_DEFAULT_DB.SERVICE_LIFE != null)
            {
                //TimeSpan timeSpan = tNAV_DEFAULT_DB.SI_OBJECT_DELIVERY_DATE.Value.AddYears((int)tNAV_DEFAULT_DB.SERVICE_LIFE) - tNAV_DEFAULT_DB.LATEST_INSPECTION_DATE.Value;

                DateTime serviceLifeDate = tNAV_CAP_DEFAULT_DB.SI_OBJECT_DELIVERY_DATE.Value.AddYears((int)tNAV_CAP_DEFAULT_DB.SERVICE_LIFE);
                DateTime latestInspectionDate = tNAV_CAP_DEFAULT_DB.LATEST_INSPECTION_DATE.Value;

                int diffMonthCount = 12 * (latestInspectionDate.Year - serviceLifeDate.Year) + (latestInspectionDate.Month - serviceLifeDate.Month);

                int diffYear = diffMonthCount / 12;
                int diffMonth = diffMonthCount % 12;

                ViewBag.ServiceLifeText = $"검사일 기준 내구연한 대비 {diffYear} 년 {diffMonth} 개월 경과 중";
            }
            else
            {
                ViewBag.ServiceLifeText = "관련 필드를 모두 입력하세요.";
                //ViewBag.ServiceLifeText = "경과 값을 계산할 수 없습니다.";
            }

            if (tNAV_CAP_DEFAULT_DB.NSN_ID != null)
            {
                PNAV_SIP_GET_NSN_LISTResult resultNsn = resultNsnList.FirstOrDefault(m => m.NSN_ID == tNAV_CAP_DEFAULT_DB.NSN_ID);

                if (resultNsn != null)
                {
                    ViewBag.NsnText = $"{resultNsn.VESSEL_NAME_KR}  {resultNsn.VESSEL_CATEGORY_NAME}";
                }
                else
                {
                    ViewBag.NsnText = String.Empty;
                }
            }
            else
            {
                ViewBag.NsnText = String.Empty;
            }

            return View(tNAV_CAP_DEFAULT_DB);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateCapDefaultDb(CapDefaultDbViewModel? capDefaultViewModel)
        {
            string result = string.Empty;
            string message = string.Empty;

            List<TNAV_CAP_DEFAULT_DB_PIC> tNAV_CAP_DEFAULT_DB_PIC_Add = new List<TNAV_CAP_DEFAULT_DB_PIC>();
            List<TNAV_CAP_DEFAULT_DB_PIC> tNAV_CAP_DEFAULT_DB_PIC_Remove = _repository.TNAV_CAP_DEFAULT_DB_PICs.Where(m => m.CAP_DEFAULT_DB_IDX == capDefaultViewModel.CAP_DEFAULT_DB_IDX).ToList();

            try
            {
                if (capDefaultViewModel is not null)
                {
                    Guid defaultDbIdx = capDefaultViewModel.CAP_DEFAULT_DB_IDX;

                    TNAV_DEFAULT_DB tNAV_DEFAULT_DB = _repository.TNAV_DEFAULT_DBs.FirstOrDefault(m => m.DEFAULT_DB_IDX == defaultDbIdx && m.IS_DELETED == false);

                    tNAV_DEFAULT_DB.CATEGORY_ID = capDefaultViewModel.CATEGORY_ID;
                    tNAV_DEFAULT_DB.JOB_ID = capDefaultViewModel.JOB_ID;
                    tNAV_DEFAULT_DB.NSN_ID = capDefaultViewModel.NSN_ID;
                    tNAV_DEFAULT_DB.DESCRIPTION = capDefaultViewModel.DESCRIPTION;
                    tNAV_DEFAULT_DB.QUANTITY_AMOUNT = capDefaultViewModel.QUANTITY_AMOUNT;
                    tNAV_DEFAULT_DB.QUANTITY_TYPE = capDefaultViewModel.QUANTITY_TYPE;
                    tNAV_DEFAULT_DB.LATEST_INSPECTION_DATE = capDefaultViewModel.LATEST_INSPECTION_DATE;
                    tNAV_DEFAULT_DB.SERVICE_LIFE = capDefaultViewModel.SERVICE_LIFE;
                    tNAV_DEFAULT_DB.INSPECTION_PLACE = capDefaultViewModel.INSPECTION_PLACE;
                    tNAV_DEFAULT_DB.INSPECTION_INTERVAL = capDefaultViewModel.INSPECTION_INTERVAL;
                    tNAV_DEFAULT_DB.INSPECTION_PERIOD_START_DATE = capDefaultViewModel.INSPECTION_PERIOD_START_DATE;
                    tNAV_DEFAULT_DB.INSPECTION_PERIOD_END_DATE = capDefaultViewModel.INSPECTION_PERIOD_END_DATE;
                    tNAV_DEFAULT_DB.SI_OBJECT_DELIVERY_DATE = capDefaultViewModel.SI_OBJECT_DELIVERY_DATE;

                    // PIC 수정
                    if (capDefaultViewModel.DEFAULT_PIC is not null)
                    {
                        foreach (string pic in capDefaultViewModel.DEFAULT_PIC)
                        {
                            // 연계 데이터베이스에서 사용자 정보를 조회한다.
                            RFV_USER_DEPT PicUser = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.EMP_ID == pic).First();

                            TNAV_CAP_DEFAULT_DB_PIC PIC = new TNAV_CAP_DEFAULT_DB_PIC()
                            {
                                USER_IDX = Guid.NewGuid(),
                                CAP_DEFAULT_DB_IDX = defaultDbIdx,
                                USER_ID = PicUser.USER_ID,
                                USER_NAME_KR = PicUser.USER_NAME,
                                USRE_NAME_EN = PicUser.USER_NAME_E,
                                DEPT_ID = PicUser.DEPT_ID,
                                DEPT_NAME_KR = PicUser.DEPT_NAME,
                                DEPT_NAME_EN = PicUser.DEPT_NAME_E,
                                DEGREE_KR = PicUser.DEGREE,
                                DEGREE_EN = PicUser.DEGREE_E,
                                EMP_ID = PicUser.EMP_ID,
                                SUR_NO = PicUser.SUR_NO,
                                POSITION_KR = PicUser.POSITION_K,
                                POSITION_EN = PicUser.POSITION_E,
                                //REG_DATE = _regDate
                            };

                            tNAV_CAP_DEFAULT_DB_PIC_Add.Add(PIC);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        _repository.Update(tNAV_DEFAULT_DB);
                        _repository.RemoveRange(tNAV_CAP_DEFAULT_DB_PIC_Remove);
                        _repository.AddRange(tNAV_CAP_DEFAULT_DB_PIC_Add);
                        await _repository.SaveChangesAsync();

                        result = "OK";
                    }
                    else
                    {
                        result = "INVALID";
                        message = "ModelState InValid";
                    }
                }
                else
                {
                    result = "NONE";
                    message = "Parameter Empty";
                }
            }
            catch (Exception ex)
            {
                result = "ERROR";
                message = ex.Message;
            }

            return Json(new { result = result, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteCapDefaultDb(CapDefaultDbViewModel? capDefaultViewModel)
        {
            string result = string.Empty;
            string message = string.Empty;

            List<TNAV_CAP_DEFAULT_DB_PIC> tNAV_CAP_DEFAULT_DB_PIC_Remove = _repository.TNAV_CAP_DEFAULT_DB_PICs.Where(m => m.CAP_DEFAULT_DB_IDX == capDefaultViewModel.CAP_DEFAULT_DB_IDX).ToList();

            try
            {
                if (capDefaultViewModel is not null)
                {
                    TNAV_DEFAULT_DB tNAV_DEFAULT_DB = _repository.TNAV_DEFAULT_DBs.FirstOrDefault(m => m.DEFAULT_DB_IDX == capDefaultViewModel.CAP_DEFAULT_DB_IDX && m.IS_DELETED == false);

                    if (tNAV_DEFAULT_DB != null)
                    {
                        tNAV_DEFAULT_DB.IS_DELETED = true;
                        tNAV_DEFAULT_DB.DELETE_DATE = DateTime.Now;
                        tNAV_DEFAULT_DB.DELETE_USER = HttpContext.Session.GetString("UserName");

                        if (ModelState.IsValid)
                        {
                            _repository.Update(tNAV_DEFAULT_DB);
                            _repository.RemoveRange(tNAV_CAP_DEFAULT_DB_PIC_Remove);
                            await _repository.SaveChangesAsync();

                            result = "OK";
                        }
                        else
                        {
                            result = "INVALID";
                            message = "ModelState InValid";
                        }
                    }
                    else
                    {
                        result = "NONE";
                        message = "tNAV_DEFAULT_DB is null";
                    }
                }
                else
                {
                    result = "NONE";
                    message = "Parameter Empty";
                }
            }
            catch (Exception ex)
            {
                result = "ERROR";
                message = ex.Message;
            }

            return Json(new { result = result, message = message });
        }

        [HttpPost]
        public async Task<JsonResult> SelectUser(string USER_NAME_E)
        {
            try
            {
                if (!string.IsNullOrEmpty(USER_NAME_E))
                {
                    var result = await _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.USER_NAME_E == USER_NAME_E).FirstOrDefaultAsync();
                    return Json(result);
                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception)
            {
                throw;
                //return Json(string.Empty);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetSelected(string USER_NAME_E)
        {
            try
            {
                if (!string.IsNullOrEmpty(USER_NAME_E))
                {
                    var result = await _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.USER_NAME_E == USER_NAME_E).FirstOrDefaultAsync();
                    return Json(result);
                }
                else
                {
                    return Json(null);
                }
            }
            catch (Exception)
            {
                throw;
                //return Json(string.Empty);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSetTerm()
        {
            try
            {
                var tNAV_CAP_CATEGORies = await _repository.TNAV_CAP_CATEGORies.OrderBy(m => m.ORDER).ToListAsync();

                return Json(new { result = "OK", categoryData = tNAV_CAP_CATEGORies });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSetTerm(int gcaServiceLife, int ncaServiceLife, int pcaServiceLife, int scaServiceLife)
        {
            string result = string.Empty;
            string message = string.Empty;

            try
            {
                var tNAV_CAP_CATEGORies = await _repository.TNAV_CAP_CATEGORies.OrderBy(m => m.ORDER).ToListAsync();

                if (tNAV_CAP_CATEGORies != null)
                {
                    foreach (var tNAV_CAP_CATEGORY in tNAV_CAP_CATEGORies)
                    {
                        switch (tNAV_CAP_CATEGORY.CATEGORY_ID)
                        {
                            case "GCA":
                                tNAV_CAP_CATEGORY.INSPECTION_INTERVAL = gcaServiceLife;
                                break;
                            case "NCA":
                                tNAV_CAP_CATEGORY.INSPECTION_INTERVAL = ncaServiceLife;
                                break;
                            case "PCA":
                                tNAV_CAP_CATEGORY.INSPECTION_INTERVAL = pcaServiceLife;
                                break;
                            case "SCA":
                                tNAV_CAP_CATEGORY.INSPECTION_INTERVAL = scaServiceLife;
                                break;
                            default:
                                break;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        _repository.UpdateRange(tNAV_CAP_CATEGORies);
                        await _repository.SaveChangesAsync();

                        result = "OK";
                    }
                    else
                    {
                        result = "ERROR";
                        message = "tNAV_CAP_CATEGORies is null";
                    }
                }
                else
                {
                    result = "ERROR";
                    message = "ModelState is invalid";
                }
            }
            catch (Exception ex)
            {
                result = "ERROR";
                message = ex.Message;
            }

            return Json(new { result = result, message = message });
        }
    }
}

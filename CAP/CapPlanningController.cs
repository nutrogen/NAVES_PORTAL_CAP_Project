using Microsoft.AspNetCore.Mvc;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using NavesPortalforWebWithCoreMvc.ViewModels;
using NavesPortalforWebWithCoreMvc.Models;
using Microsoft.EntityFrameworkCore;
using NavesPortalforWebWithCoreMvc.Common;
using Syncfusion.EJ2.Base;
using System.Collections;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    //[Authorize]
    //[CheckSession]
    public class CapPlanningController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly RfSystemContext _rfSystemRepository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CapPlanningController(BM_NAVES_PortalContext db, RfSystemContext rfSystemRepository, IBM_NAVES_PortalContextProcedures procedures)
        {
            _repository = db;
            _rfSystemRepository = rfSystemRepository;
            _procedures = procedures;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null || SearchString == String.Empty)
                {
                    SearchString = "";
                }

                List<PNAV_CAP_GET_PLANNING_LISTResult> resultList = await _procedures.PNAV_CAP_GET_PLANNING_LISTAsync(SearchString, StartDate, EndDate);

                IEnumerable DataSource = resultList.AsEnumerable();
                DataOperations operation = new DataOperations();

                //Search
                if (dm.Search != null && dm.Search.Count > 0)
                {
                    DataSource = operation.PerformSearching(DataSource, dm.Search);
                }

                if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
                {
                    DataSource = operation.PerformSorting(DataSource, dm.Sorted);
                }

                //Filtering
                if (dm.Where != null && dm.Where.Count > 0)
                {
                    DataSource = operation.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
                }

                int count = DataSource.Cast<PNAV_CAP_GET_PLANNING_LISTResult>().Count();

                //Paging
                if (dm.Skip != 0)
                {

                    DataSource = operation.PerformSkip(DataSource, dm.Skip);
                }

                if (dm.Take != 0)
                {
                    DataSource = operation.PerformTake(DataSource, dm.Take);
                }

                return dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(new { result = DataSource });
            }
            catch (Exception e)
            {
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "CapPlanning", returnView = "Index" });
            }
        }

        public async Task<IActionResult> Create(Guid? id)
        {
            Guid? PLANNING_IDX;

            var result = await _repository.VNAV_SELECT_CAP_WORK_DETAIL_LISTs.Where(m => m.WORK_IDX == id).ToListAsync();

            try
            {
                PLANNING_IDX = result[0].PLANNING_IDX;
            } catch(Exception)
            {
                PLANNING_IDX = Guid.Parse("00000000-0000-0000-0000-000000000000");
            }

            var resultCapPlanning = await _repository.TNAV_CAP_PLANNINGs.Where(m => m.PLANNING_IDX == PLANNING_IDX && m.IS_DELETE == false).FirstOrDefaultAsync();
            var resultCapPlanningPicGroup = await _repository.TNAV_CAP_PLANNING_PIC_GROUPs.Where(m => m.PLANNING_IDX == PLANNING_IDX).ToListAsync();
            var resultInspectionSchedule = await _repository.TNAV_CAP_PLANNING_PIC_SCHEDULEs.Where(m => m.PLANNING_IDX == PLANNING_IDX).ToListAsync();
            var resultPic = (
                    from PIC in _repository.TNAV_CAP_PLANNING_PICs
                    join GROUP in _repository.TNAV_CAP_PLANNING_PIC_GROUPs on new { PIC.PLANNING_IDX, PIC.PIC_TYPE, PIC.USER_ID } equals new { GROUP.PLANNING_IDX, GROUP.PIC_TYPE, GROUP.USER_ID }
                    where PIC.PLANNING_IDX == PLANNING_IDX
                    select new { PIC, GROUP }
            ).ToList();

            ViewBag.Result = result.First();
            ViewBag.resultCapPlanning = resultCapPlanning;
            ViewBag.resultCapPlanningPicGroup = resultCapPlanningPicGroup;
            ViewBag.resultInspectionSchedule = resultInspectionSchedule;
            ViewBag.resultPic = resultPic;

            //ViewBag.resultCapPlanning_json = System.Text.Json.JsonSerializer.Serialize(resultCapPlanning);
            //ViewBag.resultCapPlanningPicGroup_json = System.Text.Json.JsonSerializer.Serialize(resultCapPlanningPicGroup);
            //ViewBag.resultInspectionSchedule_json = System.Text.Json.JsonSerializer.Serialize(resultInspectionSchedule);
            //ViewBag.resultPic_json = System.Text.Json.JsonSerializer.Serialize(resultPic);

            ViewBag.dataSource = result;
            ViewBag.PROJECT = _repository.TNAV_PROJECTs.Where(m => m.PROJECT_IDX == result[0].PROJECT_IDX).FirstOrDefault();

            ViewBag.PicFatigue = new List<dropdownViewModel>();
            ViewBag.PicHull = new List<dropdownViewModel>();
            ViewBag.PicMachinery = new List<dropdownViewModel>();
            ViewBag.PicElectric = new List<dropdownViewModel>();
            ViewBag.PicSafety = new List<dropdownViewModel>();

            return View();
        }

        public List<QuantityTypeViewModel> GetDataQuantityType()
        {
            List<QuantityTypeViewModel> _QuantityType = new List<QuantityTypeViewModel>();
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Set", Value = "Set" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Unit", Value = "Unit" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "Pcs", Value = "Pcs" });
            _QuantityType.Add(new QuantityTypeViewModel { Text = "EA", Value = "EA" });

            return _QuantityType;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetWorkDetailList(string workId)
        {
            try
            {
                var result = _repository.VNAV_SELECT_CAP_WORK_DETAIL_LISTs.Where(m => m.WORK_ID == workId).ToList();
                var tNAV_PROJECT = _repository.TNAV_PROJECTs.Where(m => m.PROJECT_IDX == result[0].PROJECT_IDX).FirstOrDefault();

                ////workid 입력시 데이터 바인딩
                //string superVisorName = _repository.TNAV_PROJECTs.Where(m => m. == workId).Select(m => m.INSPECTION_PLACE).FirstOrDefault();
                ////STATUS = _repository.TNAV_PROJECTs.Where(m => m.WORK_ID == workId).Select(m => m.STATUS).FirstOrDefault();
                //LATEST_INSPECTION_DATE = _repository.TNAV_PLANNING_WORK_LASTs.Where(m => m.WORK_ID == workId).Select(m => m.LATEST_INSPECTION_DATE).FirstOrDefault();
                //SI_OBJECTS_DELEVERY_DATE = _repository.TNAV_PLANNING_WORK_LASTs.Where(m => m.WORK_ID == workId).Select(m => m.SI_OBJECTS_DELEVERY_DATE).FirstOrDefault();
                //SERVICE_LIFE = _repository.TNAV_PLANNING_WORK_LASTs.Where(m => m.WORK_ID == workId).Select(m => m.SERVICE_LIFE).FirstOrDefault();
                //EQUIPMENT_SPECIFICATION_A = _repository.TNAV_PLANNING_WORK_LASTs.Where(m => m.WORK_ID == workId).Select(m => m.EQUIPMENT_SPECIFICATION_A).FirstOrDefault();
                //EQUIPMENT_SPECIFICATION_B = _repository.TNAV_PLANNING_WORK_LASTs.Where(m => m.WORK_ID == workId).Select(m => m.EQUIPMENT_SPECIFICATION_B).FirstOrDefault();

                //return Json(result);

                return Json(new { list = result, project = tNAV_PROJECT });
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public JsonResult SavePlanning(CapPlanningViewModel capPlanningViewModel)
        public JsonResult SavePlanning([FromForm] CapPlanningViewModel CAP_PLAN)
        {
            string result = String.Empty;

            try
            {
                if (CAP_PLAN == null)
                {
                    result = "NONE";
                }
                else
                {
                    if(CAP_PLAN.tNAV_CAP_PLANNING_PIC_GROUP_DELETE == null)
                    {
                        CAP_PLAN.tNAV_CAP_PLANNING_PIC_GROUP_DELETE = new List<TNAV_CAP_PLANNING_PIC_GROUP_DELETE>();
                    }

                    _repository.Database.BeginTransaction();

                    if (CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                    {
                        CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX = Guid.NewGuid();
                        CAP_PLAN.tNAV_CAP_PLANNING.REG_DATE = DateTime.Now;
                        CAP_PLAN.tNAV_CAP_PLANNING.IS_DELETE = false;

                        _repository.Add(CAP_PLAN.tNAV_CAP_PLANNING);
                    }
                    else
                    {
                        var item = _repository.TNAV_CAP_PLANNINGs.Where(m => m.PLANNING_IDX == CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX).FirstOrDefault();

                        CAP_PLAN.tNAV_CAP_PLANNING.REG_DATE = item.REG_DATE;
                        CAP_PLAN.tNAV_CAP_PLANNING.IS_DELETE = false;

                        _repository.Entry(item).State = EntityState.Detached;
                        _repository.Entry(CAP_PLAN.tNAV_CAP_PLANNING).State = EntityState.Modified;

                        _repository.Update(CAP_PLAN.tNAV_CAP_PLANNING);
                    }

                    var removePicGroupItem = _repository.TNAV_CAP_PLANNING_PIC_GROUPs.Where(m => m.PLANNING_IDX == CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX);
                    _repository.RemoveRange(removePicGroupItem);

                    foreach (TNAV_CAP_PLANNING_PIC_GROUP obj in CAP_PLAN.tNAV_CAP_PLANNING_PIC_GROUP)
                    {
                        var userInfo = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.SUR_NO == obj.SUR_NO).FirstOrDefault();
                        obj.USER_NAME_KR = userInfo.USER_NAME;
                        obj.USER_NAME_EN = userInfo.USER_NAME_E;
                        obj.DEPT_ID = userInfo.DEPT_ID;
                        obj.DEPT_NAME_KR = userInfo.DEPT_NAME;
                        obj.DEPT_NAME_EN = userInfo.DEPT_NAME_E;
                        obj.DEGREE_KR = userInfo.DEGREE;
                        obj.DEGREE_EN = userInfo.DEGREE_E;
                        obj.EMP_ID = userInfo.EMP_ID;
                        obj.POSITION_KR = userInfo.POSITION_K;
                        obj.POSITION_EN = userInfo.POSITION_E;
                        obj.USER_ID = userInfo.USER_ID;
                        obj.REG_DATE = DateTime.Now;
                        obj.PLANNING_IDX = CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX;

                        _repository.Add(obj);
                    }

                    foreach (TNAV_CAP_PLANNING_PIC_GROUP_DELETE obj in CAP_PLAN.tNAV_CAP_PLANNING_PIC_GROUP_DELETE)
                    {
                        var item = new TNAV_CAP_PLANNING_PIC_SCHEDULE();
                        item.PIC_SCHEDULE_IDX = obj.PIC_SCHEDULE_IDX;

                        _repository.Remove(item);
                    }

                    foreach (TNAV_CAP_PLANNING_PIC_SCHEDULE obj in CAP_PLAN.tNAV_CAP_PLANNING_PIC_SCHEDULE)
                    {
                        var item = _repository.TNAV_CAP_PLANNING_PIC_SCHEDULEs.Where(m => m.PIC_SCHEDULE_IDX == obj.PIC_SCHEDULE_IDX).FirstOrDefault();

                        if(item == null)
                        {
                            obj.PLANNING_IDX = CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX;
                            obj.REG_DATE = DateTime.Now;
                            _repository.Add(obj);
                        }
                        else
                        {
                            obj.PLANNING_IDX = CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX;
                            obj.REG_DATE = item.REG_DATE;

                            _repository.Entry(item).State = EntityState.Detached;
                            _repository.Entry(obj).State = EntityState.Modified;

                            _repository.Update(obj);
                        }
                    }

                    foreach (PIC_LIST obj in CAP_PLAN.tNAV_PIC_LIST)
                    {
                        var userInfo = _rfSystemRepository.RFV_USER_DEPTs.Where(m => m.SUR_NO == obj.SUR_NO).FirstOrDefault();
                        obj.PLANNING_IDX = CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX;

                        TNAV_CAP_PLANNING_PIC pic = new TNAV_CAP_PLANNING_PIC();
                        pic.PIC_SCHEDULE_IDX = obj.PIC_SCHEDULE_IDX;
                        pic.PLANNING_IDX = CAP_PLAN.tNAV_CAP_PLANNING.PLANNING_IDX;
                        pic.PIC_TYPE = obj.PIC_TYPE;
                        pic.USER_ID = userInfo.USER_ID;

                        var item = _repository.TNAV_CAP_PLANNING_PICs.Where(m => m.PIC_SCHEDULE_IDX == obj.PIC_SCHEDULE_IDX
                                                                              && m.PLANNING_IDX == obj.PLANNING_IDX
                                                                              && m.PIC_TYPE == obj.PIC_TYPE 
                                                                              && m.USER_ID == userInfo.USER_ID).FirstOrDefault();

                        pic.REG_DATE = DateTime.Now;

                        if (item != null)
                        {
                            pic.REG_DATE = item.REG_DATE;
                            _repository.Entry(item).State = EntityState.Detached;
                            _repository.Entry(pic).State = EntityState.Modified;
                            _repository.Update(pic);
                        }
                        else
                        {
                            pic.REG_DATE = DateTime.Now;
                            _repository.Add(pic);
                        }
                    }

                    /*
                    if (ModelState.IsValid)
                    {
                        //_repository.Add(tNAV_PLANNING_GROUP);
                        //_repository.SaveChanges();
                        result = "OK";
                    }
                    else
                    {
                        result = "ModelState InValid";
                    }
                    */

                    _repository.SaveChanges();
                    result = "OK";

                    _repository.Database.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                _repository.Database.RollbackTransaction();
                result = ex.Message;
            }

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetProjectPicList(Guid projectIdx, string __RequestVerificationToken)
        {
            // Project에서 할당된 PIC 목록
            List<TNAV_PROJECT_PIC> Pic = _repository.TNAV_PROJECT_PICs.ToList().Where(m => m.PROJECT_IDX == projectIdx && m.PROJECT_POSTION == "PIC").ToList();
            List<dropdownViewModel> ddlPic = new List<dropdownViewModel>();
            foreach (TNAV_PROJECT_PIC item in Pic)
            {
                ddlPic.Add(new dropdownViewModel
                {
                    Name = item.USRE_NAME_EN + " (" + item.SUR_NO + ")",
                    Value = item.SUR_NO
                });
            }

            return Json(ddlPic);
        }

        /// <summary>
        /// 선택된 PIC 정보 가져오기
        /// </summary>
        /// <param name="SUR_NO"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetSelectedPicInfo(string SUR_NO, string __RequestVerificationToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(SUR_NO))
                {
                    var result = await _rfSystemRepository.RFV_USER_DEPTs
                        .Where(m => m.SUR_NO == SUR_NO)
                        .FirstOrDefaultAsync();
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
            }
        }

        public IActionResult GetGUID()
        {
            return Json(new { Value = Guid.NewGuid() });
        }
    }
}

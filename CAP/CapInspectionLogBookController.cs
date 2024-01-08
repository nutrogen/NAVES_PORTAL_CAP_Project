using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.Models;
using Syncfusion.EJ2.Base;
using System.Collections;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.RfSystemData;
using NuGet.Protocol.Core.Types;
using NavesPortalforWebWithCoreMvc.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    [Authorize]
    [CheckSession]
    public class CapInspectionLogBookController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;
        private readonly IBM_NAVES_PortalContextProcedures _procedures;

        public CapInspectionLogBookController(BM_NAVES_PortalContext db, IBM_NAVES_PortalContextProcedures procedures)
        {
            _repository = db;
            _procedures = procedures;
        }

        /// <summary>
        /// Cap Inspection Log List View
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Cap Inspection Log List 조회
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> UrlDataSource(string SearchString, DateTime? StartDate, DateTime? EndDate, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                if (SearchString is null || SearchString == String.Empty)
                {
                    SearchString = "";
                }

                List<PNAV_CAP_GET_INSPECTION_LISTResult> resultList = await _procedures.PNAV_CAP_GET_INSPECTION_LISTAsync(SearchString, StartDate, EndDate);

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

                int count = DataSource.Cast<PNAV_CAP_GET_INSPECTION_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "CapInspectionLogBook", returnView = "Index" });
            }
        }

        public async Task<IActionResult> UrlDataSourcePic(Guid planningIdx, [FromBody] DataManagerRequest? dm)
        {
            try
            {
                List<PNAV_CAP_GET_INSPECTION_PIC_LISTResult> resultList = await _procedures.PNAV_CAP_GET_INSPECTION_PIC_LISTAsync(planningIdx);

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

                int count = DataSource.Cast<PNAV_CAP_GET_INSPECTION_PIC_LISTResult>().Count();

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
                return RedirectToAction("SaveException", "Error", new { ex = e.InnerException.Message, returnController = "CapInspectionLogBook", returnView = "Index" });
            }
        }

        /// <summary>
        /// Cap Inspection Log per Designated PIC View
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> DetailPic(Guid id)
        {
            // id : TNAV_CAP_PLANNING_PIC_SCHEDULE.PIC_SCHEDULE_IDX

            List<PNAV_CAP_GET_INSPECTION_LOG_PIC_LISTResult> resultList = await _procedures.PNAV_CAP_GET_INSPECTION_LOG_PIC_LISTAsync(id);

            ViewBag.dataSource = resultList;

            return View(resultList.FirstOrDefault());
        }

        /// <summary>
        /// Cap Inspection Log per Designated PIC 저장
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SetCapInspectionLogPic(TNAV_CAP_INSPECTION_LOG_PIC model)
        {
            string result = String.Empty;

            try
            {
                if (model == null)
                {
                    result = "NULL";
                }
                else
                {
                    var tNAV_CAP_INSPECTION_LOG_PIC = _repository.TNAV_CAP_INSPECTION_LOG_PICs
                        .FirstOrDefault(m => m.PIC_SCHEDULE_IDX == model.PIC_SCHEDULE_IDX 
                                          && m.CAP_INSPECTION_LOG_PIC_IDX == model.CAP_INSPECTION_LOG_PIC_IDX
                                          && m.IS_DELETE == false);

                    if (tNAV_CAP_INSPECTION_LOG_PIC == null)
                    {
                        model.CAP_INSPECTION_LOG_PIC_IDX = Guid.NewGuid();
                        model.REG_DATE = DateTime.Now;
                        model.IS_DELETE = false;

                        _repository.Add(model);
                    }
                    else
                    {
                        model.IS_DELETE = false;
                        model.REG_DATE = tNAV_CAP_INSPECTION_LOG_PIC.REG_DATE;

                        _repository.Entry(tNAV_CAP_INSPECTION_LOG_PIC).State = EntityState.Detached;
                        _repository.Entry(model).State = EntityState.Modified;

                        _repository.Update(model);
                    }

                    await _repository.SaveChangesAsync();
                    result = "OK";
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result);
        }

        /// <summary>
        /// Cap Inspection Log per Designated PIC 삭제
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteInspectionLogPic(TNAV_CAP_INSPECTION_LOG_PIC model)
        {
            string result = String.Empty;

            try
            {
                if (model == null)
                {
                    result = "NULL";
                }
                else
                {
                    var tNAV_CAP_INSPECTION_LOG_PIC = _repository.TNAV_CAP_INSPECTION_LOG_PICs
                        .FirstOrDefault(m => m.PIC_SCHEDULE_IDX == model.PIC_SCHEDULE_IDX
                                          && m.CAP_INSPECTION_LOG_PIC_IDX == model.CAP_INSPECTION_LOG_PIC_IDX
                                          && m.IS_DELETE == false);

                    if (tNAV_CAP_INSPECTION_LOG_PIC != null)
                    {
                        tNAV_CAP_INSPECTION_LOG_PIC.IS_DELETE = true;
                        tNAV_CAP_INSPECTION_LOG_PIC.DELETE_DATE = DateTime.Now;
                        tNAV_CAP_INSPECTION_LOG_PIC.DELETE_USER = HttpContext.Session.GetString("UserId");

                        if (ModelState.IsValid)
                        {
                            _repository.Update(tNAV_CAP_INSPECTION_LOG_PIC);
                            _repository.SaveChanges();
                            result = "OK";
                        }
                        else
                        {
                            result = "ModelState InValid";
                        }
                    }
                    else
                    {
                        result = "NONE";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Json(result);
        }

        public async Task<IActionResult> DetailWork(Guid id)
        {
            List<PNAV_CAP_GET_INSPECTION_LOG_WORK_LISTResult> resultList = await _procedures.PNAV_CAP_GET_INSPECTION_LOG_WORK_LISTAsync(id);

            ViewBag.dataSource = resultList;

            return View(resultList.FirstOrDefault());
        }
    }
}

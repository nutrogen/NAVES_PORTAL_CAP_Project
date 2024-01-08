using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NavesPortalforWebWithCoreMvc.Controllers.AuthFromIntranetController;
using NavesPortalforWebWithCoreMvc.Data;
using NavesPortalforWebWithCoreMvc.Models;
using NavesPortalforWebWithCoreMvc.ViewModels;
using NuGet.Protocol.Core.Types;
using Syncfusion.EJ2.Inputs;
using System.Net.Http.Headers;

namespace NavesPortalforWebWithCoreMvc.Controllers.CAP
{
    [Authorize]
    [CheckSession]
    public class CapDefaultDbFileUploadController : Controller
    {
        private readonly BM_NAVES_PortalContext _repository;

        private string _uploadFolderName = "UploadFiles";
        private IWebHostEnvironment hostingEnv;

        public CapDefaultDbFileUploadController(IWebHostEnvironment env, BM_NAVES_PortalContext repository)
        {
            this.hostingEnv = env;
            _repository = repository;
        }


        /// <summary>
        /// Final Technical Report 파일 수정 저장 및 업로드
        /// </summary>
        /// <param name="UploadFilesFinalTechnicalReport"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="ReleatedInfo"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        //[RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        //[RequestSizeLimit(100_000_000)]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        //[HttpPost("upload"), DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = Int32.MaxValue, ValueLengthLimit = Int32.MaxValue)]
        public IActionResult SaveUploadFilesFinalTechnicalReport(IList<IFormFile> UploadFilesFinalTechnicalReport,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId, 
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {
            Save(UploadFilesFinalTechnicalReport, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);

            return Content("");
        }

        /// <summary>
        /// Final Technical Report Uploader에서 파일 삭제
        /// </summary>
        /// <param name="UploadFilesFinalTechnicalReport"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="ReleatedInfo"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult RemoveUploadFilesFinalTechnicalReport(IList<IFormFile> UploadFilesFinalTechnicalReport,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {

            Remove(UploadFilesFinalTechnicalReport, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);
            return Content("");
        }

        /// <summary>
        /// Photo, Movie 파일 수정 저장 및 업로드
        /// </summary>
        /// <param name="UploadFilesPhotoMovie"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="ReleatedInfo"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult SaveUploadFilesPhotoMovie(IList<IFormFile> UploadFilesPhotoMovie,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {
            Save(UploadFilesPhotoMovie, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);

            return Content("");
        }

        /// <summary>
        /// Photo, Movie Uploader에서 파일 삭제
        /// </summary>
        /// <param name="UploadFilesPhotoMovie"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="ReleatedInfo"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        /// <returns></returns>
        [AcceptVerbs("Post")]
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public IActionResult RemoveUploadFilesPhotoMovie(IList<IFormFile> UploadFilesPhotoMovie,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {

            Remove(UploadFilesPhotoMovie, PlatformName, CurrentDocIdx, Type, ReleatedInfo, OriginalFileName, SavedFileName, ProjectIdx, ProjectId, WorkIdx, WorkId, CreateUserName);
            return Content("");
        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="UploadFiles"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="ReleatedInfo"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        private void Save(IList<IFormFile> UploadFiles,
                string PlatformName,
                Guid CurrentDocIdx,
                string Type,
                Guid ReleatedInfo,
                string OriginalFileName,
                string SavedFileName,
                Guid? ProjectIdx,
                string? ProjectId,
                Guid? WorkIdx,
                string? WorkId,
                string CreateUserName)
        {
            try
            {
                foreach (var file in UploadFiles)
                {
                    if (UploadFiles != null)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var targetFolder = hostingEnv.WebRootPath + $@"\{_uploadFolderName}\{PlatformName}";


                        // 플랫폼별 대상 경로 확인
                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        var targetProjectFolder = $@"{targetFolder}";
                        if (!string.IsNullOrEmpty(ProjectId) || ProjectId != null)
                        {
                            targetProjectFolder = $@"{targetProjectFolder}\{ProjectId}";

                            // Project 별 대상 경로 확인
                            if (!Directory.Exists(targetProjectFolder))
                            {
                                Directory.CreateDirectory(targetProjectFolder);
                            }
                        }

                        var targetWorkFolder = $@"{targetProjectFolder}";
                        if (!string.IsNullOrEmpty(WorkId) || WorkId != null)
                        {
                            targetWorkFolder = $@"{targetWorkFolder}\{WorkId}";

                            // Work ID 별 대상 경로 확인
                            if (!Directory.Exists(targetWorkFolder))
                            {
                                Directory.CreateDirectory(targetWorkFolder);
                            }
                        }

                        fileName = $@"{targetWorkFolder}\{fileName}";

                        // 파일 유무 확인 후 업로드
                        if (!System.IO.File.Exists(fileName))
                        {
                            using (FileStream fs = System.IO.File.Create(fileName))
                            {
                                file.CopyTo(fs);
                                fs.Flush();
                            }

                            //업로드 파일 정보 저장
                            TNAV_COM_FILE _COM_FILE = new TNAV_COM_FILE()
                            {
                                IDX = Guid.NewGuid(),
                                PLATFORM = PlatformName,
                                DOCUMENT_IDX = CurrentDocIdx,
                                KIND_OF_FILES = Type,
                                RELATED_INFO = ReleatedInfo.ToString().ToUpper(),
                                PROJECT_IDX = ProjectIdx,
                                PROJECT_ID = ProjectId,
                                WORK_IDX = WorkIdx,
                                WORK_ID = WorkId,
                                SAVED_FULL_PATH = fileName,
                                ORIGINAL_FILENAME = OriginalFileName,
                                EXTENSION = Path.GetExtension(fileName).ToLower(),
                                SAVED_FILENAME = SavedFileName,
                                REG_DATE = DateTime.Now,
                                CREATE_USER_NAME = CreateUserName
                            };

                            _repository.Add(_COM_FILE);
                            _repository.SaveChanges();
                        }
                        else
                        {
                            Response.Clear();
                            Response.StatusCode = 204;
                            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File already exists.";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "No Content";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="UploadFilesContractDoc"></param>
        /// <param name="PlatformName"></param>
        /// <param name="CurrentDocIdx"></param>
        /// <param name="Type"></param>
        /// <param name="ReleatedInfo"></param>
        /// <param name="OriginalFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ProjectIdx"></param>
        /// <param name="ProjectId"></param>
        /// <param name="WorkIdx"></param>
        /// <param name="WorkId"></param>
        /// <param name="CreateUserName"></param>
        public void Remove(IList<IFormFile> UploadFilesContractDoc,
            string PlatformName,
            Guid CurrentDocIdx,
            string Type,
            Guid ReleatedInfo,
            string OriginalFileName,
            string SavedFileName,
            Guid? ProjectIdx,
            string? ProjectId,
            Guid? WorkIdx,
            string? WorkId,
            string CreateUserName)
        {
            try
            {
                foreach (var file in UploadFilesContractDoc)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var targetFolder = hostingEnv.WebRootPath + $@"\{_uploadFolderName}\{PlatformName}";
                    fileName = $@"{targetFolder}\{fileName}";

                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);

                        //업로드된 파일 삭제
                        if (_repository.TNAV_COM_FILEs.Where(m => m.SAVED_FILENAME == SavedFileName).Any())
                        {
                            var _COM_FILE = _repository.TNAV_COM_FILEs.Where(m => m.SAVED_FILENAME == SavedFileName).First();
                            _repository.Remove(_COM_FILE);
                            _repository.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.StatusCode = 200;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File removed successfully";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }
    }
}

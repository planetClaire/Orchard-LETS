using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using LETSBootstrap.Services;
using LETSBootstrap.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.Media.Services;
using Orchard.UI.Notify;

namespace LETSBootstrap.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly IMediaService _mediaService;
        const string MediaFolder = "Logos";

        public AdminController(
            IOrchardServices services,
            ISettingsService settingsService, IMediaService mediaService)
        {
            _settingsService = settingsService;
            _mediaService = mediaService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index()
        {
            var settings = _settingsService.GetSettings();

            var viewModel = new SettingsViewModel
            {
                BackgroundColor = settings.BackgroundColor,
                Logo = settings.Logo
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(SettingsViewModel viewModel)
        {
            var settings = _settingsService.GetSettings();

            settings.BackgroundColor= viewModel.BackgroundColor;

            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
            {
                var fileName = Path.GetFileName(Request.Files[0].FileName);
                var uniqueFileName = fileName;
                try
                {
                    // try to create the folder before uploading a file into it
                    _mediaService.CreateFolder(null, MediaFolder);
                }
                catch
                {
                    // the folder can't be created because it already exists, continue
                }
                var filesInFolder = _mediaService.GetMediaFiles(MediaFolder).ToList();
                var found =
                    filesInFolder.Any(
                        f => 0 == String.Compare(fileName, f.Name, StringComparison.OrdinalIgnoreCase));
                var index = 0;
                while (found)
                {
                    index++;
                    uniqueFileName = String.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(fileName),
                                                   index,
                                                   Path.GetExtension(fileName));
                    found =
                        filesInFolder.Any(
                            f => 0 == String.Compare(uniqueFileName, f.Name, StringComparison.OrdinalIgnoreCase));
                }

                _mediaService.UploadMediaFile(MediaFolder, uniqueFileName, Request.Files[0].InputStream, false);
                settings.Logo = uniqueFileName;
            }

            Services.Notifier.Information(T("Your settings have been saved."));

            return View(viewModel);
        }

        public ActionResult ClearLogo()
        {
            var settings = _settingsService.GetSettings();
            settings.Logo = string.Empty;
            Services.Notifier.Information(T("Done"));
           
            return RedirectToAction("Index");
        }

    }
}
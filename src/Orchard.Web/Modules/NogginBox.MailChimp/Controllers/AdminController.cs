using System.Linq;
using System.Web.Mvc;
using NogginBox.MailChimp.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using NogginBox.MailChimp.Services;
using Orchard.DisplayManagement;

namespace NogginBox.MailChimp.Controllers
{
	public class AdminController : Controller, IUpdateModel
	{
		private readonly IContentManager _contentManager;
		private readonly IOrchardServices _services;
		private readonly IMailChimpService _mailChimpService;

		dynamic Shape { get; set; }
		public Localizer T { get; set; }

		public AdminController(IContentManager contentManager, IOrchardServices orchardServices, IMailChimpService mailChimpService, IShapeFactory shapeFactory)
		{
			_contentManager = contentManager;
			_services = orchardServices;
			_mailChimpService = mailChimpService;
			Shape = shapeFactory;
		}

		public ActionResult Index()
		{
			var settings = _mailChimpService.MailChimpSettings;

			var editor = createSettingsEditor(settings);
			var model = _services.ContentManager.BuildEditor(settings);
			model.Content.Add(editor);

			return View((object)model);
		}

		[HttpPost]
		public ActionResult Index(SettingsRecord mailChimpSettings)
		{
			var settings = _mailChimpService.MailChimpSettings;
			if (settings.Id == 0)
			{
				_services.ContentManager.Create(settings);
			}

			var editor = createSettingsEditor(settings);
			var model = _contentManager.UpdateEditor(settings, this);
			model.Content.Add(editor);

			if (!ModelState.IsValid)
				return View((object)model);

			_services.Notifier.Information(T("MailChimp settings updated."));

			return View((object)model);
		}

		private dynamic createSettingsEditor(MailChimpSettingsPart settings)
		{
			var editor = Shape.EditorTemplate(TemplateName: "Parts/MailChimpSettings", Model: settings, Prefix: null);
			editor.Metadata.Position = "2";
			return editor;
		}

		#region IUpdateModel members
		
		bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
		{
			return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
		}

		void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
		{
			ModelState.AddModelError(key, errorMessage.ToString());
		}
		
		#endregion
	}
}
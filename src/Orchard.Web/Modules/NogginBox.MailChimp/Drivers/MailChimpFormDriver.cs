using System;
using JetBrains.Annotations;
using NogginBox.MailChimp.Models;
using NogginBox.MailChimp.Services;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.ContentManagement;
using NogginBox.MailChimp.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace NogginBox.MailChimp.Drivers
{
	[UsedImplicitly]
	public class MailChimpFormDriver : ContentPartDriver<MailChimpFormPart>
	{
		private readonly IMailChimpService _mailChimpService;
		private readonly IOrchardServices _services;

		public MailChimpFormDriver(IMailChimpService mailChimpService, IOrchardServices orchardServices)
		{
			_mailChimpService = mailChimpService;
			_services = orchardServices;
		}

		public Localizer T { get; set; }

		protected override DriverResult Display(MailChimpFormPart part, String displayType, dynamic shapeHelper)
		{
			List<MergeVariableModel> mergeVariableModelsForDisplay = new List<MergeVariableModel>();
			List<InterestGroupsModel> interestGroupsModelsForDisplay = new List<InterestGroupsModel>();
			bool sent = false;
			String emailType = "html";
			if (_services.WorkContext.HttpContext.Request.Form["a"] != null)
			{
				emailType = _services.WorkContext.HttpContext.Request.Form["emailType"];
				sent = trySendForm(_services.WorkContext.HttpContext.Request.Form, part, mergeVariableModelsForDisplay, interestGroupsModelsForDisplay, emailType);
			}
			else
			{
				// Create Merge Variable view models
				foreach(var mvar in part.MergeVariables.OrderBy(t => t.DisplayOrder))
				{
					var mergeModel = new MergeVariableModel { Variable = mvar };
					if (mvar.Type == (int)MergeVariableType.Address) mergeModel.Countries = new SelectList(_mailChimpService.CountryCodes, "Key", "Value");
					mergeVariableModelsForDisplay.Add(mergeModel);
				}

				// Create Interest Group view models
				foreach (var interestGroup in part.InterestGroups)
				{
					var interestModel = new InterestGroupsModel { Name = interestGroup.Name, GroupId = interestGroup.GroupId };
					interestModel.AddAllGroupItems(interestGroup.getGroups());
					interestGroupsModelsForDisplay.Add(interestModel);
				}
			}

			return ContentShape("Parts_MailChimpForm",
							() => shapeHelper.Parts_MailChimpForm(
								ContentPart: part,
								Sent: sent,
								Message: new MvcHtmlString(part.Message),
								ThankyouMessage: new MvcHtmlString(part.ThankyouMessage),
								InterestGroups: interestGroupsModelsForDisplay,
								MergeVariables: mergeVariableModelsForDisplay,
								EmailType: emailType
							));
		}

		//GET
		protected override DriverResult Editor(MailChimpFormPart part, dynamic shapeHelper)
		{
			var model = buildEditor(part);
			return showEditorForModel(model, shapeHelper);
		}

		//POST
		protected override DriverResult Editor(MailChimpFormPart part, IUpdateModel updater, dynamic shapeHelper)
		{
			// Update list id and then build editor (updated list id needed to query stuff from API)
			updater.TryUpdateModel(part, Prefix, new String[] { "ListId" }, null);
			var model = buildEditor(part);

			List<InterestGroupingsRecord> interestGroupFromApi = null;
			if (model.InterestGroups != null)
			{
				// Copy the API retrived values from buildEditor that TryUpdateModel overwrites
				var interestGroupFromApiArray = new InterestGroupingsRecord[model.InterestGroups.Count];
				model.InterestGroups.CopyTo(interestGroupFromApiArray);
				interestGroupFromApi = interestGroupFromApiArray.ToList();
			}

			updater.TryUpdateModel(model, Prefix, null, null);

			// Update remaining part properties
			part.Message = model.Message;
			part.ThankyouMessage = model.ThankyouMessage;

			if (model.AvailableMergeVariables != null)
			{
				_mailChimpService.UpdateMergeVariablesForContentItem(part, model.MergeVariables, model.AvailableMergeVariables);
			}

			if (interestGroupFromApi != null)
			{
				var shownInterestGroupIds = model.InterestGroups.Where(t => t.Show).Select(t => t.GroupId);
				_mailChimpService.UpdateInterestGroupsForContentItem(part, shownInterestGroupIds, interestGroupFromApi);
			}

			return showEditorForModel(model, shapeHelper);
		}

		private DriverResult showEditorForModel(EditFormModel model, dynamic shapeHelper)
		{
			return ContentShape("Parts_MailChimpForm_Edit",
								() => shapeHelper.EditorTemplate(TemplateName: "Parts/MailChimpForm", Model: model, Prefix: Prefix));
		}

		private EditFormModel buildEditor(MailChimpFormPart part)
		{
			var settings = _mailChimpService.MailChimpSettings;

			var editorModel = new EditFormModel()
			{
				ListId = part.ListId,
				Message = part.Message,
				ThankyouMessage = part.ThankyouMessage,
				MergeVariables = createMergeVarList(part.MergeVariables)
			};

			editorModel.HasApiKey = !String.IsNullOrEmpty(settings.ApiKey);
			if (!editorModel.HasApiKey)
			{
				_services.Notifier.Warning(T("You don't have an API Key set up. You must set one up in Settings/MailChimp API Key."));
				return editorModel;
			}

			editorModel.PossibleLists = _mailChimpService.getListsFromApi();

			if (String.IsNullOrEmpty(part.ListId))
			{
				return editorModel;
			}
			
			editorModel.AvailableMergeVariables = _mailChimpService.getMergeVariablesFromApi(part.ListId);
			editorModel.InterestGroups = createInterestGroupList(_mailChimpService.getInterestGroupsFromApi(part.ListId), part.InterestGroups);

			return editorModel;
		}

		private List<InterestGroupingsRecord> createInterestGroupList(List<InterestGroupingsRecord> interestGroupsFromApi, IEnumerable<InterestGroupingsRecord> interestGroupsFromDB)
		{
			if (interestGroupsFromDB != null)
			{
				foreach (var interestGroupFromApi in interestGroupsFromApi)
				{
					var interestGroupFromDb = interestGroupsFromDB.FirstOrDefault(t => t.GroupId == interestGroupFromApi.GroupId);
					if (interestGroupFromDb != null)
					{
						interestGroupFromApi.Show = interestGroupFromDb.Show;
					}
				}
			}

			return interestGroupsFromApi;
		}

		private IList<MergeVariableEntry> createMergeVarList(IEnumerable<MergeVariableRecord> mergeRecords)
		{
			var list = new List<MergeVariableEntry>();
			if (mergeRecords != null)
			{
				foreach (var record in mergeRecords)
				{
					list.Add(new MergeVariableEntry { Tag = record.Tag });
				}
			}
			return list;
		}

		private bool trySendForm(NameValueCollection formFields, MailChimpFormPart part, List<MergeVariableModel> mergeVarsForDisplay, List<InterestGroupsModel> interestGroupsForDisplay, String emailType)
		{
			var subscriberEmail = formFields["email"];
			var allOk = true;

			var formFieldList = formFields.ToPairs();
			var mergeVarsForMailChimp = new Dictionary<String, object>();

			// Get merge vars
			foreach (var mergeVarRecord in part.MergeVariables.OrderBy(t => t.DisplayOrder))
			{
				var displayMergeVar = new MergeVariableModel { Variable = mergeVarRecord };

				if (((MergeVariableType)mergeVarRecord.Type) != MergeVariableType.Address)
				{
					var userValue = formFieldList.FirstOrDefault(t => t.Key == mergeVarRecord.Tag).Value;

					if (!String.IsNullOrEmpty(userValue))
					{
						displayMergeVar.Value = userValue;

						// Check field is valid
						try
						{
							var apiValue = tryParseMailChimpValue(userValue, (MergeVariableType)mergeVarRecord.Type);
							if (mergeVarRecord.Tag != "email") mergeVarsForMailChimp.Add(mergeVarRecord.Tag, apiValue);
						}
						catch (Exception e)
						{
							displayMergeVar.Message = e.Message;
							allOk = false;
						}
						
					}
					else if (mergeVarRecord.Required)
					{
						// Missing required mergeVariable
						allOk = false;

						// Add display message
						displayMergeVar.Message = String.Format(T("{0} is required").Text, mergeVarRecord.Label);
					}
				}
				else
				{
					allOk = addAddressMergeVars(mergeVarsForMailChimp, displayMergeVar, mergeVarRecord, formFieldList) && allOk;
					displayMergeVar.Countries = new SelectList(_mailChimpService.CountryCodes, "Key", "Value", displayMergeVar.getField("country"));
				}
				
				mergeVarsForDisplay.Add(displayMergeVar);
			}

			// Get interest groups
			var interestGroupsForMailChimp = new Dictionary<int, List<String>>();
			foreach (var grouping in part.InterestGroups)
			{
				var interestFormStart = String.Format("Interest[{0}][", grouping.GroupId);
				var interestGroups = new List<String>();
				var interestGroupForDisplay = new InterestGroupsModel
				{
					Name = grouping.Name,
					GroupId = grouping.GroupId
				};
				interestGroupsForDisplay.Add(interestGroupForDisplay);

				foreach (String group in grouping.getGroups())
				{
					var groupMatch = formFieldList.Count(t => t.Value == group && t.Key.StartsWith(interestFormStart));
					var groupDisplayItem = new InterestGroupItemModel { Name = group };
					interestGroupForDisplay.Groups.Add(groupDisplayItem);
					if (groupMatch > 0)
					{
						interestGroups.Add(group);
						groupDisplayItem.Checked = true;
					}
				}
				if(interestGroups.Count > 0)interestGroupsForMailChimp.Add(grouping.GroupId, interestGroups);
			}

			var sent = false;
			if (allOk)
			{
				try
				{
					sent = _mailChimpService.Subscribe(part.ListId, subscriberEmail, mergeVarsForMailChimp, emailType, interestGroupsForMailChimp);
					if (sent) _services.Notifier.Information(T("You have subscribed. Please check your email for a confirmation email."));
				}
				catch (Exception e)
				{
					_services.Notifier.Error(T(e.Message));
				}
			}
			else
			{
				_services.Notifier.Error(T("Please check your form."));
			}

			return sent;
		}

		/// <summary>
		/// Try to parse the date value using globisation settings of host
		/// </summary>
		/// <param name="userValue"></param>
		/// <param name="displayMergeVar"></param>
		/// <returns></returns>
		private string tryParseMailChimpValue(String value, MergeVariableType mergeType)
		{
			switch(mergeType)
			{
				case MergeVariableType.Email:
					var re = new Regex(@"^[a-zA-Z0-9_\+-]+(\.[a-zA-Z0-9_\+-]+)*@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.([a-zA-Z]{2,4})$");
					if (re.IsMatch(value))
					{
						return value;
					}
					else
					{
						throw new FormatException("This should be an email address.");
					}
				case MergeVariableType.Date:
					try
					{
						var date = DateTime.Parse(value);
						return date.ToString("yyyy-MM-dd");
					}
					catch (FormatException e)
					{
						throw new FormatException("This should be a date.", e);
					}
				case MergeVariableType.Number:
					try
					{
						Double.Parse(value);
						return value;
					}
					catch (FormatException e)
					{
						throw new FormatException("This should be a number.", e);
					}

				default:
					// Todo check the other fields are valid
					return value;
			}			
		}

		private bool addAddressMergeVars(Dictionary<string, object> mergeVarsForMailChimp, MergeVariableModel mergeVarForDisplay, MergeVariableRecord mergeVarRecord, IEnumerable<KeyValuePair<string, string>> formFieldList)
		{
			var addressKeys = new List<String> { "addr1", "addr2", "city", "state", "zip", "country" };
			var requiredAddressKeys = new List<String> { "addr1", "city", "state", "zip", };

			// Go through seperate address fields
			// Ref: http://perceptivemcapi.codeplex.com/discussions/248909
			var address = new Dictionary<String, object>();
			var anyProvided = false;
			var anyRequiredMissing = false;
			foreach (var key in addressKeys)
			{
				var userValue = formFieldList.FirstOrDefault(t => t.Key == String.Format("{0}.{1}", mergeVarRecord.Tag, key)).Value;
				if (String.IsNullOrEmpty(userValue))
				{
					if (requiredAddressKeys.Contains(key))
					{
						anyRequiredMissing = true;
					}
				}
				else
				{
					address.Add(key, userValue);
					mergeVarForDisplay.Fields.Add(key, userValue);
					anyProvided = true;
				}
			}
			mergeVarsForMailChimp.Add(mergeVarRecord.Tag, address);

			// Are there any missing address fields that we should complain about
			if ((anyProvided || mergeVarRecord.Required) && anyRequiredMissing)
			{
				mergeVarForDisplay.Message = "Please complete all address fields.";
				return false;
			}

			return true;
		}
	}
}
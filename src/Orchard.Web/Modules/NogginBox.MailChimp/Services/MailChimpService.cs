using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.ContentManagement;
using NogginBox.MailChimp.Models;
using Orchard.Logging;
using PerceptiveMCAPI;
using PerceptiveMCAPI.Methods;
using PerceptiveMCAPI.Types;
using Orchard.Data;
using NogginBox.MailChimp.ViewModels;

namespace NogginBox.MailChimp.Services
{
	public class MailChimpService : IMailChimpService
	{
		private readonly IContentManager _contentManager;
		private readonly IRepository<MergeVariableRecord> _mergeVarsRepository;
		private readonly IRepository<InterestGroupingsRecord> _interestGroupingsRepository;

		public MailChimpService(IContentManager contentManager, IRepository<MergeVariableRecord> mergeVarsRepository, IRepository<InterestGroupingsRecord> interestGroupingsRepository)
		{
			_contentManager = contentManager;
			_mergeVarsRepository = mergeVarsRepository;
			_interestGroupingsRepository = interestGroupingsRepository;
		}

		private MailChimpSettingsPart _settings;
		public MailChimpSettingsPart MailChimpSettings
		{
			get
			{
				if (_settings == null)
				{
					_settings = _contentManager.Query<MailChimpSettingsPart, SettingsRecord>()
									.List().FirstOrDefault();

					if (_settings == null)
						_settings = _contentManager.New<MailChimpSettingsPart>("MailChimpSettings");
				}
				return _settings;
			}
		}

		public bool IsApiKeyValid(String apiKey)
		{
			if (String.IsNullOrEmpty(apiKey)) return false;

			var pingInput = new pingInput(apiKey);
			var pingCmd = new ping(pingInput);

			var answer = pingCmd.Execute();
			
			if(answer.api_ErrorMessages.Count == 0)
			{
				return true;
			}

			var errorCodes = answer.api_ErrorMessages.Select(t => t.code);
			if (errorCodes.Contains("104")) {
				return false;
			}

			// Something went wrong
			// Todo: log this
			return false;
			//throw new Exception("MailChimp API problem: " + String.Join(", ", answer.api_ErrorMessages));
		}

		private Dictionary<String, String> _countryCodes;
		public Dictionary<String, String> CountryCodes
		{
			get
			{
				if (_countryCodes == null)
				{
					// http://www.iso.org/iso/english_country_names_and_code_elements
					_countryCodes = new Dictionary<string, string>()
					{
						{"GB", "United Kingdom"},
						{"US", "United States"},
						{"AF", "Afghanistan"},
						{"AX", "Aland Islands"},
						{"AL", "Albania"},
						{"DZ", "Algeria"},
						{"AS", "American Samoa"},
						{"AD", "Andorra"},
						{"AO", "Angola"},
						{"AI", "Anguilla"},
						{"AQ", "Antarctica"},
						{"AG", "Antigua and Barbuda"},
						{"AR", "Argentina"},
						{"AM", "Armenia"},
						{"AW", "Aruba"},
						{"AU", "Australia"},
						{"AT", "Austria"},
						{"AZ", "Azerbaijan"},
						{"BS", "Bahamas"},
						{"BH", "Bahrain"},
						{"BD", "Bangladesh"},
						{"BB", "Barbados"},
						{"BY", "Belarus"},
						{"BE", "Belgium"},
						{"BZz", "Belize"},
						{"BJj", "Benin"},
						{"BM", "Bermuda"},
						{"BT", "Bhutan"},
						{"BO", "Bolivia, Plurinational State Of"},
						{"BQ", "Bonaire, Saint Eustatius and Saba"},
						{"BA", "Bosnia and Herzegovina"},
						{"BW", "Botswana"},
						{"BV", "Bouvet Island"},
						{"BR", "Brazil"},
						{"IO", "British Indian Ocean Territory"},
						{"BN", "Brunei Darussalam"},
						{"BG", "Bulgaria"},
						{"BF", "Burkina Faso"},
						{"BI", "Burundi"},
						{"KH", "Cambodia"},
						{"CM", "Cameroon"},
						{"CA", "Canada"},
						{"CV", "Cape Verde"},
						{"KY", "Cayman Islands"},
						{"CF", "Central African Republic"},
						{"TD", "Chad"},
						{"CL", "Chile"},
						{"CN", "China"},
						{"CX", "Christmas Island"},
						{"CC", "Cocos (Keeling) Islands"},
						{"CO", "Colombia"},
						{"KM", "Comoros"},
						{"CG", "Congo"},
						{"CD", "Congo, The Democratic Republic Of The"},
						{"CK", "Cook Islands"},
						{"CR", "Costa Rica"},
						{"CI", "Cote D'Ivoire"},
						{"HR", "Croatia"},
						{"CU", "Cuba"},
						{"CW", "Curacao"},
						{"CY", "Cyprus"},
						{"CZ", "Czech Republic"},
						{"DK", "Denmark"},
						{"DJ", "Djibouti"},
						{"DM", "Dominica"},
						{"DO", "Dominican Republic"},
						{"EC", "Ecuador"},
						{"EG", "Egypt"},
						{"SV", "El Salvador"},
						{"GQ", "Equatorial Guinea"},
						{"ER", "Eritrea"},
						{"EE", "Estonia"},
						{"ET", "Ethiopia"},
						{"FK", "Falkland Islands (Malvinas)"},
						{"FO", "Faroe Islands"},
						{"FJ", "Fiji"},
						{"FI", "Finland"},
						{"FR", "France"},
						{"GF", "French Guiana"},
						{"PF", "French Polynesia"},
						{"TF", "French Southern Territories"},
						{"GA", "Gabon"},
						{"GM", "Gambia"},
						{"GE", "Georgia"},
						{"DE", "Germany"},
						{"GH", "Ghana"},
						{"GI", "Gibraltar"},
						{"GR", "Greece"},
						{"GL", "Greenland"},
						{"GD", "Grenada"},
						{"GP", "Guadeloupe"},
						{"GU", "Guam"},
						{"GT", "Guatemala"},
						{"GG", "Guernsey"},
						{"GN", "Guinea"},
						{"GW", "Guinea-Bissau"},
						{"GY", "Guyana"},
						{"HT", "Haiti"},
						{"HM", "Heard Island and Mcdonald Islands"},
						{"HN", "Honduras"},
						{"HK", "Hong Kong"},
						{"HU", "Hungary"},
						{"IS", "Iceland"},
						{"IN", "India"},
						{"ID", "Indonesia"},
						{"IR", "Iran, Islamic Republic Of"},
						{"IQ", "Iraq"},
						{"IE", "Ireland"},
						{"IM", "Isle Of Man"},
						{"IL", "Israel"},
						{"IT", "Italy"},
						{"JM", "Jamaica"},
						{"JP", "Japan"},
						{"JE", "Jersey"},
						{"JO", "Jordan"},
						{"KZ", "Kazakhstan"},
						{"KE", "Kenya"},
						{"KI", "Kiribati"},
						{"KP", "Korea, Democratic People'S Republic Of"},
						{"KR", "Korea, Republic Of"},
						{"KW", "Kuwait"},
						{"KG", "Kyrgyzstan"},
						{"LA", "Lao People'S Democratic Republic"},
						{"LV", "Latvia"},
						{"LB", "Lebanon"},
						{"LS", "Lesotho"},
						{"LR", "Liberia"},
						{"LY", "Libyan Arab Jamahiriya"},
						{"LI", "Liechtenstein"},
						{"LT", "Lithuania"},
						{"LU", "Luxembourg"},
						{"MO", "Macao"},
						{"MK", "Macedonia, The Former Yugoslav Republic Of"},
						{"MG", "Madagascar"},
						{"MW", "Malawi"},
						{"MY", "Malaysia"},
						{"MV", "Maldives"},
						{"ML", "Mali"},
						{"MT", "Malta"},
						{"MH", "Marshall Islands"},
						{"MQ", "Martinique"},
						{"MR", "Mauritania"},
						{"MU", "Mauritius"},
						{"YT", "Mayotte"},
						{"MX", "Mexico"},
						{"FM", "Micronesia, Federated States Of"},
						{"MD", "Moldova, Republic Of"},
						{"MC", "Monaco"},
						{"MN", "Mongolia"},
						{"ME", "Montenegro"},
						{"MS", "Montserrat"},
						{"MA", "Morocco"},
						{"MZ", "Mozambique"},
						{"MM", "Myanmar"},
						{"NA", "Namibia"},
						{"NR", "Nauru"},
						{"NP", "Nepal"},
						{"NL", "Netherlands"},
						{"NC", "New Caledonia"},
						{"NZ", "New Zealand"},
						{"NI", "Nicaragua"},
						{"NE", "Niger"},
						{"NG", "Nigeria"},
						{"NU", "Niue"},
						{"NF", "Norfolk Island"},
						{"MP", "Northern Mariana Islands"},
						{"NO", "Norway"},
						{"OM", "Oman"},
						{"PK", "Pakistan"},
						{"PW", "Palau"},
						{"PS", "Palestinian Territory, Occupied"},
						{"PA", "Panama"},
						{"PG", "Papua New Guinea"},
						{"PY", "Paraguay"},
						{"PE", "Peru"},
						{"PH", "Philippines"},
						{"PN", "Pitcairn"},
						{"PL", "Poland"},
						{"PT", "Portugal"},
						{"PR", "Puerto Rico"},
						{"QA", "Qatar"},
						{"RE", "Reunion"},
						{"RO", "Romania"},
						{"RU", "Russian Federation"},
						{"RW", "Rwanda"},
						{"BL", "Saint Barthelemy"},
						{"SH", "Saint Helena, Ascension and Tristan Da Cunha"},
						{"KN", "Saint Kitts and Nevis"},
						{"LC", "Saint Lucia"},
						{"MF", "Saint Martin (French Part)"},
						{"PM", "Saint Pierre and Miquelon"},
						{"VC", "Saint Vincent and The Grenadines"},
						{"WS", "Samoa"},
						{"SM", "San Marino"},
						{"ST", "Sao Tome and Principe"},
						{"SA", "Saudi Arabia"},
						{"SN", "Senegal"},
						{"RS", "Serbia"},
						{"SC", "Seychelles"},
						{"SL", "Sierra Leone"},
						{"SG", "Singapore"},
						{"SX", "Sint Maarten (Dutch Part)"},
						{"SK", "Slovakia"},
						{"SI", "Slovenia"},
						{"SB", "Solomon Islands"},
						{"SO", "Somalia"},
						{"ZA", "South Africa"},
						{"GS", "South Georgia and The South Sandwich Islands"},
						{"ES", "Spain"},
						{"LK", "Sri Lanka"},
						{"SD", "Sudan"},
						{"SR", "Suriname"},
						{"SJ", "Svalbard and Jan Mayen"},
						{"SZ", "Swaziland"},
						{"SE", "Sweden"},
						{"CH", "Switzerland"},
						{"SY", "Syrian Arab Republic"},
						{"TW", "Taiwan, Province Of China"},
						{"TJ", "Tajikistan"},
						{"TZ", "Tanzania, United Republic Of"},
						{"TH", "Thailand"},
						{"TL", "Timor-Leste"},
						{"TG", "Togo"},
						{"TK", "Tokelau"},
						{"TO", "Tonga"},
						{"TT", "Trinidad and Tobago"},
						{"TN", "Tunisia"},
						{"TR", "Turkey"},
						{"TM", "Turkmenistan"},
						{"TC", "Turks and Caicos Islands"},
						{"TV", "Tuvalu"},
						{"UG", "Uganda"},
						{"UA", "Ukraine"},
						{"AE", "United Arab Emirates"},
						{"UM", "United States Minor Outlying Islands"},
						{"UY", "Uruguay"},
						{"UZ", "Uzbekistan"},
						{"VU", "Vanuatu"},
						{"VA", "Vatican City State"},
						{"VE", "Venezuela, Bolivarian Republic Of"},
						{"VN", "Viet Nam"},
						{"VG", "Virgin Islands, British"},
						{"VI", "Virgin Islands, U.S."},
						{"WF", "Wallis and Futuna"},
						{"EH", "Western Sahara"},
						{"YE", "Yemen"},
						{"ZM", "Zambia"},
						{"ZW", "Zimbabwe"}
					};
				}
				return _countryCodes;
			}
		}

		public Dictionary<String, String> getListsFromApi()
		{
			var listsInput = new listsInput(MailChimpSettings.ApiKey);
			var listsCmd = new lists(listsInput);

			var answer = listsCmd.Execute();

			var lists = new Dictionary<String, String>();
			foreach (var result in answer.result)
			{
				lists.Add(result.id, result.name);
			}

			return lists;
		}


		public List<InterestGroupingsRecord> getInterestGroupsFromApi(String listId)
		{
			var interestInput = new listInterestGroupingsInput(MailChimpSettings.ApiKey, listId);
			var interestCommand = new listInterestGroupings(interestInput);

			var answer = interestCommand.Execute();

			var interestGroupsRecords = new List<InterestGroupingsRecord>();
			foreach (var group in answer.result)
			{
				var interestGroup = new InterestGroupingsRecord
				{
					GroupId = group.grouping_id,
					Name = group.name,
					Type = group.form_field.ToString()
				};
				var groupNames = group.groups.OrderBy(t => t.display_order).Select(t => t.name).ToList();
				interestGroup.setGroups(groupNames);

				interestGroupsRecords.Add(interestGroup);
			}

			return interestGroupsRecords;
		}


		public IList<MergeVariableRecord> getMergeVariablesFromApi(String listId)
		{
			var mergeInput = new listMergeVarsInput(MailChimpSettings.ApiKey, listId);
			var mergeCommand = new listMergeVars(mergeInput);

			var answer = mergeCommand.Execute();

			var mergeVars = new List<MergeVariableRecord>();
			foreach (var result in answer.result)
			{
				var mergeVar = new MergeVariableRecord()
				{
					Tag = result.tag,
					Label = result.name,
					Type = getTypeInt(result.field_type),
					Required = result.req,
					DisplayOrder = result.order,
					Choices = String.Join(",", result.choices)
				};
				mergeVars.Add(mergeVar);
			}

			return mergeVars;
		}

        public bool Subscribe(String listId, String subscribeEmail, Dictionary<String, object> mergeVars, String emailType, Dictionary<int, List<String>> interestGroups = null)
        {
            return Subscribe(listId, subscribeEmail, mergeVars, emailType, true, false, true, false, interestGroups);
        }

        public bool Subscribe(String listId, String subscribeEmail, Dictionary<String, object> mergeVars, String emailType, bool doubleOptin, bool updateExisting, bool replaceInterests, bool sendWelcome, Dictionary<int, List<String>> interestGroups = null)
		{
			subscribeAddIntererestGroupsToMergeVars(interestGroups, mergeVars);
			
			EnumValues.emailType emailTypeEnum;
			try
			{
				emailTypeEnum = (EnumValues.emailType) Enum.Parse(typeof(EnumValues.emailType), emailType);
			}
			catch
			{
				emailTypeEnum = EnumValues.emailType.html;
			}

			// Send data to API
            var subscribeInput = new listSubscribeInput(MailChimpSettings.ApiKey, listId, subscribeEmail, mergeVars, emailTypeEnum, doubleOptin, updateExisting, replaceInterests, sendWelcome);
			var subscribeCommand = new listSubscribe(subscribeInput);
			var result = subscribeCommand.Execute();

			// Check for errors
			if (result.api_ErrorMessages.Count > 0)
			{
				var a = result.api_ErrorMessages.Select(t => t.error);
				throw new Exception("API errors: " + String.Join(",", a));
			}
			if (result.api_ValidatorMessages.Count > 0)
			{
				var a = result.api_ValidatorMessages.Select(t => t.MessageText);
				throw new Exception(String.Join(",", a));
			}

			return result.result;
		}

		private void subscribeAddIntererestGroupsToMergeVars(Dictionary<int, List<String>> interestGroups, Dictionary<String, object> mergeVars)
		{
			if (interestGroups != null && interestGroups.Count > 0)
			{
				var listOfGroups = new List<interestGroupings>();
				foreach (var a in interestGroups)
				{
					var group = new interestGroupings
					{
						id = a.Key,
						name = "",
						groups = a.Value

					};
					listOfGroups.Add(group);
				}

				mergeVars.Add("groupings", listOfGroups);
			}
		}

		public void UpdateInterestGroupsForContentItem(MailChimpFormPart part, IEnumerable<int> showInterestGroupIds, IEnumerable<InterestGroupingsRecord> interestGroups)
		{
			var oldInterestGroups = _interestGroupingsRepository.Fetch(
				r => r.FormRecord == part.Record);


			// Delete the rewards that are no longer there or shown
			foreach (var oldInterestGroup in oldInterestGroups)
			{
				if (!showInterestGroupIds.Contains(oldInterestGroup.Id) || !interestGroups.Select(t => t.Id).Contains(oldInterestGroup.Id))
				{
					_interestGroupingsRepository.Delete(oldInterestGroup);
				}
			}

			// Work out which ones are new and shown
			var newInterestGroupings = new List<InterestGroupingsRecord>();
			foreach (var interestGroup in interestGroups)
			{
				interestGroup.Show = showInterestGroupIds.Contains(interestGroup.GroupId);

				if (!oldInterestGroups.Select(t => t.Id).Contains(interestGroup.Id) && interestGroup.Show)
				{
					interestGroup.FormRecord = part.Record;
					newInterestGroupings.Add(interestGroup);
				}
			}

			// Add the new rewards
			foreach (var interestGroup in newInterestGroupings)
			{
				_interestGroupingsRepository.Create(interestGroup);
			}
		}

		public void UpdateMergeVariablesForContentItem(MailChimpFormPart part, IEnumerable<MergeVariableEntry> mergeVarInputs, IEnumerable<MergeVariableRecord> possibleMergeVars)
		{
			var oldMergeVars = _mergeVarsRepository.Fetch(
				r => r.FormRecord == part.Record);
			
			// Untick non available merge vars to oldVar (happens if someone changes list)
			foreach (var mergeVar in mergeVarInputs.Where(t => t.Checked))
			{
				if (!possibleMergeVars.Select(t => t.Tag).Contains(mergeVar.Tag))
				{
					mergeVar.Checked = false;
				}
			}

			var checkedMergeVars = mergeVarInputs.Where(t => t.Checked).ToList();

			// Add all required vars that aren't arlready in checked collection
			var requiredMergeVars = possibleMergeVars.Where(t => t.Required);
			foreach (var requiredMergeVar in requiredMergeVars)
			{
				if(!checkedMergeVars.Select(t => t.Tag).Contains(requiredMergeVar.Tag))
				{
					checkedMergeVars.Add(
						new MergeVariableEntry { Tag = requiredMergeVar.Tag, Checked = true }
					);
				}
			}

			// Delete the rewards that are no longer there
			
			foreach (var oldMergeVar in oldMergeVars)
			{
				if (!checkedMergeVars.Select(t => t.Tag).Contains(oldMergeVar.Tag))
				{
					_mergeVarsRepository.Delete(oldMergeVar);
				}
			}

			// Work out which ones are new
			var newMergeVars = new List<MergeVariableRecord>();
			foreach (var mergeVar in checkedMergeVars)
			{
				var oldMergeVar = oldMergeVars.FirstOrDefault(t => t.Tag == mergeVar.Tag);
				var newMergevar = possibleMergeVars.First(t => t.Tag == mergeVar.Tag);

				if (oldMergeVar == null)
				{
					// Add new value	
					newMergevar.FormRecord = part.Record;
					newMergeVars.Add(newMergevar);
				}
				else
				{
					// Old one so update some properties
					oldMergeVar.DisplayOrder = newMergevar.DisplayOrder;
					oldMergeVar.Label = newMergevar.Label;
					oldMergeVar.Required = newMergevar.Required;
					oldMergeVar.Type = newMergevar.Type;
					oldMergeVar.Choices = newMergevar.Choices;
				}
			}

			// Add the new rewards
			foreach (var mergeVar in newMergeVars)
			{
				_mergeVarsRepository.Create(mergeVar);
			}
		}

        public bool Unsubscribe(string listId, string email, bool deleteMember, bool sendGoodbye, bool sendNotify)
        {
            var listUnsubscribeInput = new listUnsubscribeInput(MailChimpSettings.ApiKey, listId, email, deleteMember,
                                                                sendGoodbye, sendNotify);
            return new listUnsubscribe(listUnsubscribeInput).Execute().result;
        }

        public IEnumerable<string> getListSubscribers(string listId, string status)
        {
            var listStatus = (EnumValues.listMembers_status)Enum.Parse(typeof(EnumValues.listMembers_status), status, true);
            var listMembersInput = new listMembersInput(MailChimpSettings.ApiKey, listId, listStatus);
            var listMembers = new listMembers(listMembersInput);
            var listSubscribers = listMembers.Execute().result;
            return listSubscribers.Select(listSubscriber => listSubscriber.email).ToList();
        }

		private int? getTypeInt(String typeName)
		{
			switch(typeName)
			{
				case "text":
					return (int)MergeVariableType.Text;
				case "number":
					return (int)MergeVariableType.Number;
				case "email":
					return (int)MergeVariableType.Email;
				case "phone":
					return (int)MergeVariableType.Phone;
				case "url":
					return (int)MergeVariableType.Website;
				case "address":
					return (int)MergeVariableType.Address;
				case "date":
					return (int)MergeVariableType.Date;
				case "dropdown":
					return (int)MergeVariableType.Drop_down;
				case "radio"://case "multi-choice":
					return (int)MergeVariableType.Multi_choice;
				case "imageurl":
					return (int)MergeVariableType.Image_url;
				default:
					return null;
			}
		}
	}
}
using System;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using JetBrains.Annotations;
using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Title.Models;
using Orchard.Data.Migration;
using Orchard.Indexing;
using Orchard.Security;
using Orchard.Settings;


namespace LETS {
    [UsedImplicitly]
    public class Migrations : DataMigrationImpl
    {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;
        private readonly IMembershipService _membershipService;

        public Migrations(ITaxonomyService taxonomyService, IOrchardServices orchardServices, ISiteService siteService, IMembershipService membershipService)
        {
            _taxonomyService = taxonomyService;
            _orchardServices = orchardServices;
            _siteService = siteService;
            _membershipService = membershipService;
        }

        [UsedImplicitly]
        public int Create()
        {
            // LETS settings
            SchemaBuilder.CreateTable("LETSSettingsPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("IdRoleMember")
                .Column<int>("IdTaxonomyNotices")
                .Column<string>("CurrencyUnit")
                .Column<int>("MaximumNoticeAgeDays")
                .Column<int>(
                    "OldestRecordableTransactionDays")
                .Column<int>("DefaultTurnoverDays")
                .Column<bool>("UseDemurrage")
                .Column<int>("DemurrageTimeIntervalDays")
                .Column<string>("DemurrageSteps")
                .Column<int>("IdDemurrageRecipient")
                .Column<string>("IdMailChimpList")
                .Column<string>("MemberLinksZone")
                .Column<string>("MemberLinksPosition")
                .Column<string>("MemberNoticesZone")
                .Column<string>("MemberNoticesPosition")
                .Column<DateTime>("DemurrageStartDate"));

            // Notice type
            SchemaBuilder.CreateTable("NoticeTypePartRecord", table => table
                .Column<int>("RequiredCount")
                .Column<int>("SortOrder")
                .ContentPartRecord());
            ContentDefinitionManager.AlterPartDefinition("NoticeTypePart", part => part
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Notice Type", type => type
                .Named("NoticeType")
                .WithPart("NoticeTypePart")
                .WithPart("TitlePart")
                .WithPart("CommonPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'slug'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .Creatable()
                .Indexed());

            // Member
            SchemaBuilder.CreateTable("MemberPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("FirstName", c => c.WithLength(50))
                .Column<string>("LastName", c => c.WithLength(50))
                .Column<string>("Telephone")
                .Column<int>("OpeningBalance"));
            ContentDefinitionManager.AlterPartDefinition("MemberPart", part => part
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("User", type => type
                .WithPart("MemberPart")
                .Indexed());

            // Locality
            SchemaBuilder.CreateTable("LocalityPartRecord", table => table
                .Column<string>("Postcode")
                .ContentPartRecord());
            ContentDefinitionManager.AlterPartDefinition("LocalityPart", part => part
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Locality", type => type
                .WithPart("LocalityPart")
                .WithPart("TitlePart")
                .WithPart("CommonPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'slug'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .Creatable()
                .Indexed());

            // Address
            SchemaBuilder.CreateTable("AddressPartRecord", table => table
                .ContentPartRecord().Column<int>("LocalityPartRecord_Id")
                .Column<string>("StreetAddress")
                .Column<string>("LatLong"));
            ContentDefinitionManager.AlterPartDefinition("AddressPart", part => part
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("User", type => type
                .WithPart("AddressPart"));

            // Category
            var categoryTaxonomy = _taxonomyService.GetTaxonomyByName("Category");
            if (categoryTaxonomy == null)
            {
                categoryTaxonomy = _orchardServices.ContentManager.Create<TaxonomyPart>("Taxonomy", VersionOptions.Draft);
                categoryTaxonomy.Name = "Category";
                categoryTaxonomy.As<TitlePart>().Title = "Category";
                categoryTaxonomy.As<CommonPart>().Owner = _membershipService.GetUser(_siteService.GetSiteSettings().SuperUser);
                _orchardServices.ContentManager.Publish(categoryTaxonomy.ContentItem);
                //_orchardServices.ContentManager.Flush();
                _siteService.GetSiteSettings().As<LETSSettingsPart>().IdTaxonomyNotices = categoryTaxonomy.Id;
            }

            // Notice
            SchemaBuilder.CreateTable("NoticePartRecord", table => table
                .ContentPartRecord()
                .Column<int>("NoticeTypePartRecord_Id")
                .Column<int>("Price"));
            ContentDefinitionManager.AlterPartDefinition("NoticePart", part => part
                .WithField("PaymentTerms", field => field
                    .OfType("EnumerationField")
                    .WithSetting(
                        "EnumerationFieldSettings.Options",
                        string.Join(Environment.NewLine,
                            new[]
                                {
                                    "free", "negotiable",
                                    "plus $costs"
                                }))
                    .WithSetting(
                        "EnumerationFieldSettings.ListMode",
                        "Checkbox"))
                .WithField("Per", field => field
                    .OfType("EnumerationField")
                    .WithSetting(
                        "EnumerationFieldSettings.Options",
                        string.Join(Environment.NewLine,
                            new[]
                                {
                                    "each", "per hour",
                                    "per head"
                                }))
                    .WithSetting(
                        "EnumerationFieldSettings.ListMode",
                        "Radiobutton"))
                .WithField("Description", field => field
                    .OfType("TextField")
                    .WithSetting(
                        "TextFieldSettings.Flavor", "Textarea")
                    .WithSetting("TextFieldSettings.Hint", "Don't add your contact details here.  They will be added automatically.")
                )
                .WithField("Photos", field => field
                    .OfType("AgileUploaderField")
                    .WithSetting(
                        "AgileUploaderFieldSettings.MaxWidth", "870")
                    .WithSetting(
                        "AgileUploaderFieldSettings.MaxHeight", "600")
                    .WithSetting(
                        "AgileUploaderFieldSettings.Hint", "Upload your photos here, they will be resized automatically before upload")
                    .WithSetting(
                        "AgileUploaderFieldSettings.MediaFolder", "{user-id}/{content-type}")
                )
                .WithField("Category", field => field
                    .OfType("TaxonomyField")
                    .WithSetting("TaxonomyFieldSettings.Taxonomy", "Category")
                    .WithSetting("TaxonomyFieldSettings.LeavesOnly", "true")
                    .WithSetting("TaxonomyFieldSettings.SingleChoice", "true")
                    .WithSetting("FieldIndexing.Included", "true"))
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Notice", type => type
                .WithPart("NoticePart")
                .WithPart("TitlePart")
                .WithPart("CommonPart")
                .WithPart("CommentsPart")
                .WithPart("ArchiveLaterPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Category then Slug', Pattern: '{Content.NoticeCategory}/{Content.Slug}', Description: 'Category slug then notice slug'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .Creatable()
                .Indexed());

            SchemaBuilder.CreateTable("TransactionPartRecord", table => table
                .ContentPartRecord()
                .Column<DateTime>("TransactionDate")
                .Column<string>("Description")
                .Column<int>("NoticePartRecord_Id")
                .Column<int>("SellerMemberPartRecord_Id")
                .Column<int>("BuyerMemberPartRecord_Id")
                .Column<int>("Value")
                .Column<int>("CreditValue")
                .Column<string>("TransactionType"));
            ContentDefinitionManager.AlterPartDefinition("TransactionPart", part => part
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("Transaction", type => type
                .WithPart("TransactionPart")
                .WithPart("CommonPart")
                .WithPart("IdentityPart")
                .Indexed());

            SchemaBuilder.CreateTable("CreditUsageRecord", table => table
                    .Column<int>("Id", c => c.PrimaryKey().Identity())
                    .Column<int>("IdTransactionEarnt", c => c.NotNull())
                    .Column<string>("TransactionType", c => c.NotNull())
                    .Column<int>("Value", c => c.NotNull())
                    .Column<DateTime>("RecordedDate")
                    .Column<int>("IdTransactionSpent", c => c.NotNull()));

            SchemaBuilder.CreateForeignKey("CreditUsage_TransactionEarnt", "CreditUsageRecord", new[] { "IdTransactionEarnt" },
                                           "TransactionPartRecord", new[] { "Id" });
            //SchemaBuilder.CreateForeignKey("CreditUsage_TransactionSpent", "CreditUsageRecord", new[] { "IdTransactionSpent" },
            //                               "TransactionPartRecord", new[] { "Id" });

            SchemaBuilder.CreateTable("MemberAdminPartRecord", table => table
                .ContentPartRecord()
                .Column<int>("OpeningBalance")
                .Column<DateTime>("JoinDate")
                .Column<string>("MemberType"));
            ContentDefinitionManager.AlterPartDefinition("MemberAdminPart", part => part
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("User", type => type
                .WithPart("MemberAdminPart")
                .Indexed());

            SchemaBuilder.CreateTable("MemberLinksPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("Website")
                .Column<string>("Facebook")
                .Column<string>("Twitter")
                .Column<string>("LinkedIn")
                .Column<string>("Tumblr")
                .Column<string>("Flickr")
                .Column<string>("Pinterest")
                .Column<string>("GooglePlus")
                .Column<string>("Goodreads")
                .Column<string>("Skype"));
            ContentDefinitionManager.AlterPartDefinition("MemberLinksPart", part => part
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("User", type => type
                .WithPart("MemberLinksPart")
                .Indexed());

            ContentDefinitionManager.AlterTypeDefinition("CommentsWidget", cfg => cfg
                .WithPart("CommentsWidgetPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterTypeDefinition("AuthNavigationWidget", type => type
                .WithPart("AuthNavigationWidgetPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            SchemaBuilder.CreateTable("MemberAccessOnlyPartRecord", table => table
                .ContentPartRecord());

            ContentDefinitionManager.AlterTypeDefinition("Member Only Page", type => type
                .Named("MemberOnlyPage")
                .WithPart("MemberAccessOnlyPart")
                .WithPart("TitlePart")
                .WithPart("PublishLaterPart")
                .WithPart("BodyPart")
                .WithPart("TagsPart")
                .WithPart("LocalizationPart")
                .WithPart("CommonPart")
                .WithPart("AutoroutePart", builder => builder
                    .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-page'}]")
                    .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
                .Creatable()
                .Draftable()
                .Indexed());

            SchemaBuilder.CreateTable("MembersMapPartRecord", table => table
                .Column<double>("Latitude")
                .Column<double>("Longitude")
                .Column<string>("ApiKey")
                .Column<int>("MapWidth")
                .Column<int>("MapHeight")
                .Column<int>("ZoomLevel")
                .ContentPartRecord());

            ContentDefinitionManager.AlterPartDefinition("MembersMapPart", part => part
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("MembersMapWidget", cfg => cfg
                  .WithPart("MembersMapPart")
                  .WithPart("WidgetPart")
                  .WithPart("CommonPart")
                  .WithSetting("Stereotype", "Widget"));

            ContentDefinitionManager.AlterTypeDefinition("StatsWidget", type => type
               .WithPart("StatsWidgetPart")
               .WithPart("WidgetPart")
               .WithPart("CommonPart")
               .WithSetting("Stereotype", "Widget"));

            SchemaBuilder.CreateTable("TransactionRecordSimulation", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<int>("IdTransaction")
                .Column<DateTime>("TransactionDate")
                .Column<string>("Description")
                .Column<int>("SellerMemberPartRecord_Id")
                .Column<int>("BuyerMemberPartRecord_Id")
                .Column<int>("Value")
                .Column<int>("CreditValue")
                .Column<string>("TransactionType"));

            SchemaBuilder.CreateTable("CreditUsageRecordSimulation", table => table
                .Column<int>("Id", c => c.PrimaryKey().Identity())
                .Column<int>("IdTransactionEarnt", c => c.NotNull())
                .Column<string>("TransactionType", c => c.NotNull())
                .Column<int>("Value", c => c.NotNull())
                .Column<DateTime>("RecordedDate")
                .Column<int>("IdTransactionSpent")
                .Column<int>("IdCreditUsage"));

            SchemaBuilder.ExecuteSql("CREATE FUNCTION LETS_CalculateMemberBalance (@Id int) RETURNS int AS BEGIN "
                + "DECLARE @Balance int DECLARE @Credits int DECLARE @Debits int "
                + "SET @Credits = (SELECT COALESCE(SUM(Value), 0) FROM LETS_TransactionPartRecord t "
                + "join Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id WHERE SellerMemberPartRecord_Id = @Id and v.Published = 1) "
                + "SET @Debits = (SELECT COALESCE(SUM(Value), 0) FROM LETS_TransactionPartRecord t join Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id "
                + "WHERE BuyerMemberPartRecord_Id = @Id and v.Published = 1) "
                + "SET @Balance = @Credits - @Debits RETURN @Balance END");

            SchemaBuilder.ExecuteSql("CREATE FUNCTION LETS_CalculateMemberTurnover (@Id int, @Days int) RETURNS int AS BEGIN "
                + "DECLARE @Turnover int "
                + "SET @Turnover = (SELECT COALESCE(SUM(Value), 0) FROM LETS_TransactionPartRecord t join Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id "
                + "WHERE (SellerMemberPartRecord_Id = @Id or BuyerMemberPartRecord_Id = @Id) and v.Published = 1 AND t.TransactionType = 'Trade' AND DATEDIFF(DAY, t.TransactionDate, GETDATE()) <= @Days) "
                + "RETURN @Turnover END");

            SchemaBuilder.ExecuteSql("CREATE FUNCTION LETS_CalculateTotalTurnover (@Days int) RETURNS int AS BEGIN "
                + "DECLARE @Turnover int "
                + "SET @Turnover = (SELECT COALESCE(SUM(Value), 0) FROM LETS_TransactionPartRecord t join Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id "
                + "where v.Published = 1 AND t.TransactionType = 'Trade' AND DATEDIFF(DAY, t.TransactionDate, GETDATE()) <= @Days) "
                + "RETURN @Turnover END");

            SchemaBuilder.ExecuteSql("CREATE FUNCTION LETS_GetOldestCreditValueTransaction (@idMember int) RETURNS int AS BEGIN RETURN "
                + "(SELECT top 1 t.Id FROM LETS_TransactionPartRecord t join LETS_MemberPartRecord m on m.Id = t.SellerMemberPartRecord_Id "
                + "join Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id where v.Published = '1' and t.CreditValue > 0 and m.Id = @idMember "
                + "order by t.TransactionDate) END ");

            SchemaBuilder.ExecuteSql("CREATE FUNCTION LETS_CustomMaxDateFunction (@v1 DATETIME, @v2 DATETIME) RETURNS TABLE AS "
                + "RETURN SELECT MAX(v) MaxDate FROM (VALUES(@v1),(@v2))dates(v);");

            SchemaBuilder.ExecuteSql("CREATE PROCEDURE LETS_GetMemberTransactions @idMember int, @pageSize int, @pageNumber int AS BEGIN "
                + "CREATE TABLE #memberTransactions (Id int, TransactionDate datetime, Value int, CreditValue int, IdTradingPartner int, Description nvarchar(255), TransactionType nvarchar(255))"
                + "INSERT INTO #memberTransactions"
                + "SELECT t.Id, TransactionDate, Value, CreditValue, BuyerMemberPartRecord_Id as 'IdTradingPartner', Description, TransactionType "
                + "FROM LETS_TransactionPartRecord t JOIN Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id "
                + "WHERE SellerMemberPartRecord_Id = @idMember AND v.Published = 1 "
                + "UNION ALL "
                + "SELECT t.Id, TransactionDate, -Value, CreditValue, SellerMemberPartRecord_Id as 'IdTradingPartner', Description, TransactionType "
                + "FROM LETS_TransactionPartRecord t JOIN Orchard_Framework_ContentItemVersionRecord v on v.ContentItemRecord_id = t.Id "
                + "WHERE BuyerMemberPartRecord_Id = @idMember AND v.Published = 1 "
                + "CREATE TABLE #sortedMemberTransactions (SequenceId int, Id int, TransactionDate datetime, IdTradingPartner int, Description nvarchar(255), Value int, CreditValue int, TransactionType nvarchar(255))"
                + "INSERT INTO #sortedMemberTransactions "
                + "SELECT ROW_NUMBER() OVER (ORDER BY Transactiondate, mt.Id) AS SequenceId, mt.Id, TransactionDate, IdTradingPartner, Description, Value, CreditValue, TransactionType "
                + "FROM #memberTransactions mt DROP TABLE #memberTransactions "
                + "SELECT ma.Id, TransactionDate, IdTradingPartner, m.FirstName + ' ' + m.LastName as 'TradingPartner', UserName , Description, Value, CreditValue, TransactionType , "
                + "Value + COALESCE ((SELECT SUM(Value) FROM #sortedMemberTransactions mt WHERE mt.SequenceId < ma.SequenceId),0) AS RunningBalance "
                + "FROM #sortedMemberTransactions ma JOIN LETS_MemberPartRecord m ON ma.IdTradingPartner = m.Id "
                + "JOIN Orchard_Users_UserPartRecord u on m.Id = u.Id ORDER BY SequenceId DESC "
                + "OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY DROP TABLE #sortedMemberTransactions "
                + "END");

            return 1;
        }

    }
}
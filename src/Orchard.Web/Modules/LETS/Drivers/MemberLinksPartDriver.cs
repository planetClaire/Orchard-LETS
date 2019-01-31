using LETS.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Settings;

namespace LETS.Drivers
{
    public class MemberLinksPartDriver : ContentPartDriver<MemberLinksPart>
    {
        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public MemberLinksPartDriver(ISiteService siteService, IWorkContextAccessor workContextAccessor)
        {
            _siteService = siteService;
            _workContextAccessor = workContextAccessor;
        }

        protected override DriverResult Display(MemberLinksPart part, string displayType, dynamic shapeHelper)
        {
            var shape = shapeHelper.Parts_MemberLinks(MemberLinks: part, Member: part.As<MemberPart>());
            var letsSettings = _siteService.GetSiteSettings().As<LETSSettingsPart>();
            if (string.IsNullOrWhiteSpace(letsSettings.MemberLinksZone))
            {
                return ContentShape("Parts_MemberLinks", () => shape);
            }
            if (displayType == "Detail" && !part.NoLinks)
            {
                _workContextAccessor.GetContext()
                    .Layout.Zones[letsSettings.MemberLinksZone]
                    .Add(shape, letsSettings.MemberLinksPosition);
            }
            return new DriverResult();
        }

        protected override DriverResult Editor(MemberLinksPart part, dynamic shapeHelper)
        {
            var result = ContentShape("Parts_MemberLinks_Edit",
                                        () => shapeHelper.EditorTemplate(
                                            TemplateName: "Parts/MemberLinks",
                                            Model: part,
                                            Prefix: Prefix));
            return result;
        }

        protected override DriverResult Editor(MemberLinksPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Importing(MemberLinksPart part, Orchard.ContentManagement.Handlers.ImportContentContext context)
        {
            var website = context.Attribute(part.PartDefinition.Name, "Website");
            if (website != null)
            {
                part.Website = website;
            }
            var facebook = context.Attribute(part.PartDefinition.Name, "Facebook");
            if (facebook != null)
            {
                part.Facebook = facebook;
            }
            var instagram = context.Attribute(part.PartDefinition.Name, "Instagram");
            if (instagram != null)
            {
                part.Instagram = instagram;
            }
            var twitter = context.Attribute(part.PartDefinition.Name, "Twitter");
            if (twitter != null)
            {
                part.Twitter = twitter;
            }
            var linkedIn = context.Attribute(part.PartDefinition.Name, "LinkedIn");
            if (linkedIn != null)
            {
                part.LinkedIn = linkedIn;
            }
            var tumblr = context.Attribute(part.PartDefinition.Name, "Tumblr");
            if (tumblr != null)
            {
                part.Tumblr = tumblr;
            }
            var flickr = context.Attribute(part.PartDefinition.Name, "Flickr");
            if (flickr != null)
            {
                part.Flickr = flickr;
            }
            var pinterest = context.Attribute(part.PartDefinition.Name, "Pinterest");
            if (pinterest != null)
            {
                part.Pinterest = pinterest;
            }
            var googlePlus = context.Attribute(part.PartDefinition.Name, "GooglePlus");
            if (googlePlus != null)
            {
                part.GooglePlus = googlePlus;
            }
            var goodreads = context.Attribute(part.PartDefinition.Name, "Goodreads");
            if (goodreads != null)
            {
                part.Goodreads = goodreads;
            }
            var skype = context.Attribute(part.PartDefinition.Name, "Skype");
            if (skype != null)
            {
                part.Skype = skype;
            }
        }

        protected override void Exporting(MemberLinksPart part, Orchard.ContentManagement.Handlers.ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Website", part.Website);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Facebook", part.Facebook);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Instagram", part.Instagram);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Twitter", part.Twitter);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LinkedIn", part.LinkedIn);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Tumblr", part.Tumblr);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Flickr", part.Flickr);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Pinterest", part.Pinterest);
            context.Element(part.PartDefinition.Name).SetAttributeValue("GooglePlus", part.GooglePlus);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Goodreads", part.Goodreads);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Skype", part.Skype);
        }
    }
}
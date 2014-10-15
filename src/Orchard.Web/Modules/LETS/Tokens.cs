using Orchard.Taxonomies.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Tokens;

namespace LETS
{
    public class Tokens : ITokenProvider
    {
        private readonly ITaxonomyService _taxonomyService;
        private readonly IWorkContextAccessor _workContextAccessor;
        public Localizer T { get; set; }

        public Tokens(ITaxonomyService taxonomyService, IWorkContextAccessor workContextAccessor)
        {
            _taxonomyService = taxonomyService;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context)
        {
            context.For("Content", T("Content items"), T("Content items"))
                .Token("NoticeCategory", T("Notice category"), T("The slug of the first taxonomy term"))
                ;
        }

        public void Evaluate(EvaluateContext context)
        {
            context.For<IContent>("Content")
                .Token("NoticeCategory", arg =>
                                        {
                                            var singleTermId =
                                                _workContextAccessor.GetContext().HttpContext.Request.Form[
                                                    "NoticePart.Category.SingleTermId"];
                                            int idTerm;
                                            return int.TryParse(singleTermId, out idTerm) ? _taxonomyService.GetTerm(idTerm).Slug : null;
                                        });
        }
    }
}
using System.Web.Mvc;
using LETS.Services;
using LETS.ViewModels;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace LETS.Controllers
{
    [Admin]
    public class TransactionsAdminController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IMemberService _memberService;
        private readonly IOrchardServices _orchardServices;
        dynamic Shape { get; set; }

        public TransactionsAdminController(ITransactionService transactionService, IMemberService memberService, IOrchardServices orchardServices, IShapeFactory shapeFactory)
        {
            _transactionService = transactionService;
            _memberService = memberService;
            _orchardServices = orchardServices;
            Shape = shapeFactory;
        }

        public ActionResult List(int id, PagerParameters pagerParameters)
        {
            var pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(_transactionService.GetTransactionCount(id));
            var memberTransactionsViewModel = new MemberTransactionsViewModel
                {
                    AdminIsViewing = true,
                    Transactions = _transactionService.GetTransactions(id, pager.PageSize, pager.Page),
                    Member = _memberService.GetMember(id),
                    Pager = pagerShape
                };
            return View(memberTransactionsViewModel);
        }

        public ActionResult CorrectCreditValues(int id)
        {
            _transactionService.CorrectMemberCreditValues(id);
            return RedirectToAction("List", new {id});
        }

        public ActionResult Demurrage()
        {
            return View(_transactionService.GetDemurrages());
        }

        public ActionResult DemurrageForecast()
        {
            return View(_transactionService.ForecastDemurrage());
        }

        public ActionResult ProcessDemurrage()
        {
            _transactionService.ProcessDemurrage();
            return RedirectToAction("Demurrage");
        }
    }
}

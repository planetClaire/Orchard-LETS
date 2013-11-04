using System.Collections.Generic;

namespace LETS.ViewModels
{
    public class DemurrageForecastViewModel
    {
        public int MemberBalance { get; set; }
        public IList<DemurrageTransactionsViewModel> DemurrageEvents { get; set; }
    }
}
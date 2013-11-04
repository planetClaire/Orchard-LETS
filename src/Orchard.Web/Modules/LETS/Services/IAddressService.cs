using System.Collections.Generic;
using LETS.Models;
using LETS.ViewModels;
using Orchard;
using Orchard.ContentManagement;

namespace LETS.Services
{
    public interface IAddressService : IDependency
    {
        LocalityPart GetLocality(int idLocality);
        IEnumerable<LocalityViewModel> GetLocalityViews();
        void UpdateAddressForContentItem(ContentItem contentItem, EditAddressViewModel model);
    }
}
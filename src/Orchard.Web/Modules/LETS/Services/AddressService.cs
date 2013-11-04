using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LETS.Models;
using LETS.ViewModels;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;

namespace LETS.Services
{
    [UsedImplicitly]
    public class AddressService : IAddressService
    {
        private readonly IContentManager _contentManager;
        private readonly IRepository<LocalityPartRecord> _localityRepository;
        private readonly ISignals _signals;

        public AddressService(IContentManager contentManager, IRepository<LocalityPartRecord> localityRepository, ISignals signals)
        {
            _contentManager = contentManager;
            _localityRepository = localityRepository;
            _signals = signals;
        }

        public LocalityPart GetLocality(int idLocality)
        {
            var localityContentItem = _contentManager.Get<LocalityPart>(idLocality);
            return localityContentItem;
        }

        public IEnumerable<LocalityViewModel> GetLocalityViews()
        {
            var localityRecords =
                _contentManager.Query<LocalityPart, LocalityPartRecord>(VersionOptions.Published).List();
            return localityRecords
                .Select(localityRecord => new LocalityViewModel
                {
                    Id = localityRecord.Id,
                    PostCode = localityRecord.Postcode,
                    Name = _contentManager.GetItemMetadata(_contentManager.Get(localityRecord.Id)).DisplayText
                }).OrderBy(l => l.Name).ToList();
        }

        public void UpdateAddressForContentItem(ContentItem contentItem, EditAddressViewModel model)
        {
            var addressPart = contentItem.As<AddressPart>();
            addressPart.StreetAddress = model.StreetAddress;
            addressPart.Locality = _localityRepository.Get(l => l.Id == model.IdLocality);
            addressPart.LatLong = GoogleGeoCode(string.Format("{0} {1}", addressPart.StreetAddress, GetLocality(model.IdLocality).Title));
            _signals.Trigger("letsMemberListChanged");
        }

        public string GoogleGeoCode(string address)
        {
            const string url = "http://maps.googleapis.com/maps/api/geocode/json?sensor=true&address=";

            dynamic googleResults = new Uri(url + address).GetDynamicJsonObject();
            return googleResults.results[0] != null ? string.Format("{0},{1}", googleResults.results[0].geometry.location.lat, googleResults.results[0].geometry.location.lng) : null;
        }


    }
}
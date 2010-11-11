﻿using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Themes;

namespace Orchard.Core.Routable.Controllers {
    [ValidateInput(false)]
    public class ItemController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IRoutablePathConstraint _routablePathConstraint;

        public ItemController(
            IContentManager contentManager, 
            ITransactionManager transactionManager, 
            IRoutablePathConstraint routablePathConstraint, 
            IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _routablePathConstraint = routablePathConstraint;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        [Themed]
        public ActionResult Display(string path) {
            var matchedPath = _routablePathConstraint.FindPath(path);
            if (string.IsNullOrEmpty(matchedPath)) {
                throw new ApplicationException("404 - should not have passed path constraint");
            }

            var hits = _contentManager
                .Query<RoutePart, RoutePartRecord>(VersionOptions.Published)
                .Where(r => r.Path == matchedPath)
                .Slice(0, 2);
            if (hits.Count() == 0) {
                throw new ApplicationException("404 - should not have passed path constraint");
            }
            if (hits.Count() != 1) {
                throw new ApplicationException("Ambiguous content");
            }

            var model = _contentManager.BuildDisplay(hits.Single());
            return View(model);
        }

        public ActionResult Slugify(string contentType, int? id, int? containerId) {
            const string slug = "";
            ContentItem contentItem = null;

            if (string.IsNullOrEmpty(contentType))
                return Json(slug);

            if (id != null)
                contentItem = _contentManager.Get((int)id, VersionOptions.Latest);

            if (contentItem == null) {
                contentItem = _contentManager.New(contentType);

                if (containerId != null) {
                    var containerItem = _contentManager.Get((int)containerId);
                    contentItem.As<ICommonPart>().Container = containerItem;
                }
            }

            _contentManager.UpdateEditor(contentItem, this);
            _transactionManager.Cancel();

            return Json(contentItem.As<IRoutableAspect>().Slug ?? slug);
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
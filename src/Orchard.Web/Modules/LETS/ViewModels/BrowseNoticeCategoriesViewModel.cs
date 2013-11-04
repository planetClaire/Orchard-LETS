using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Taxonomies.Models;
using LETS.Models;

namespace LETS.ViewModels
{
    public class BrowseNoticeCategoriesViewModel
    {
        public List<BrowseNoticeCategoryViewModel> Categories { get; set; }

        public BrowseNoticeCategoriesViewModel()
        {
            Categories = new List<BrowseNoticeCategoryViewModel>();
        }
    }

    public class BrowseNoticeCategoryViewModel
    {
        public TermPart Term { get; set; }
        public List<BrowseNoticeSubCategoryViewModel> SubCategories { get; set; }

        public BrowseNoticeCategoryViewModel()
        {
            SubCategories = new List<BrowseNoticeSubCategoryViewModel>();
        }
    }

    public class BrowseNoticeSubCategoryViewModel
    {
        public TermPart Term { get; set; }
        public int NoticeCount { get; set; }
    }
}
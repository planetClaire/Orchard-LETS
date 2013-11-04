using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LETS.Helpers
{
public class GroupedSelectListItem : SelectListItem
{
    public string GroupKey { get; set; }
    public string GroupName { get; set; }
}

}
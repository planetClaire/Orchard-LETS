using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Mvc.Routes;

namespace LETS
{
    [UsedImplicitly]
    public class Routes : IRouteProvider
    {
        //private readonly ITermPathConstraint _termPathConstraint;

        //public Routes(ITermPathConstraint termPathConstraint)
        //{
        //    _termPathConstraint = termPathConstraint;
        //}

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                //new RouteDescriptor {
                //    Priority = 100,
                //    Route = new Route(
                //        "{*termPath}",
                //        new RouteValueDictionary {
                //            {"area", "LETS"},
                //            {"controller", "Notice"},
                //            {"action", "TaxonomyItem"},
                //            {"termPath", ""}
                //        },
                //        new RouteValueDictionary {
                //                                    {"termPath", _termPathConstraint}
                //                                },
                //        new RouteValueDictionary {
                //            {"area", "LETS"}
                //        },
                //        new MvcRouteHandler())
                //    },
                new RouteDescriptor {
                    Priority = 100,
                    Route = new Route(
                        "Users/Account/ChallengeEmailSuccess",
                        new RouteValueDictionary {
                            {"area", "LETS"},
                            {"controller", "Account"},
                            {"action", "ChallengeEmailSuccess"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "LETS"}
                        },
                        new MvcRouteHandler())
                    },            
                new RouteDescriptor {
                    Priority = 100,
                    Route = new Route(
                        "Users/Account/LogOnMember",
                        new RouteValueDictionary {
                            {"area", "LETS"},
                            {"controller", "Account"},
                            {"action", "LogOnMember"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "LETS"}
                        },
                        new MvcRouteHandler())
                    },            
                new RouteDescriptor {
                    Priority = 100,
                    Route = new Route(
                        "Users/Account/RegisterMember",
                        new RouteValueDictionary {
                            {"area", "LETS"},
                            {"controller", "Account"},
                            {"action", "RegisterMember"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "LETS"}
                        },
                        new MvcRouteHandler())
                    },            
                new RouteDescriptor {
                    Priority = 100,
                    Route = new Route(
                        "Users/Account/RequestLostPasswordMember",
                        new RouteValueDictionary {
                            {"area", "LETS"},
                            {"controller", "Account"},
                            {"action", "RequestLostPasswordMember"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "LETS"}
                        },
                        new MvcRouteHandler())
                    },            
                new RouteDescriptor {
                    Priority = 100,
                    Route = new Route(
                        "Users/Account/LostPasswordMember",
                        new RouteValueDictionary {
                            {"area", "LETS"},
                            {"controller", "Account"},
                            {"action", "LostPasswordMember"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "LETS"}
                        },
                        new MvcRouteHandler())
                    },            
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }
    }
}
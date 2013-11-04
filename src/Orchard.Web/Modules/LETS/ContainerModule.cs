using System.Web.Mvc;
using Autofac;
using JetBrains.Annotations;

namespace LETS
{
    [UsedImplicitly]
    public class ContainerModule : Module
    {
        /// <summary>
        /// fix client validation
        /// http://orchard.codeplex.com/workitem/18269
        /// </summary>
        /// <param name="builder"></param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            ModelValidatorProviders.Providers.Clear();
            ModelValidatorProviders.Providers.Add(new DataAnnotationsModelValidatorProvider());
            ModelValidatorProviders.Providers.Add(new DataErrorInfoModelValidatorProvider());
            ModelValidatorProviders.Providers.Add(new ClientDataTypeModelValidatorProvider());
        }
    }
}
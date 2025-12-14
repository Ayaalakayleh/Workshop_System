using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Workshop.Resources
{
    public static class ResourcesServiceExtension
    {
        public static IServiceCollection AddResourceServices(this IServiceCollection services)
        {
            services.AddLocalization(options =>
            {
                options.ResourcesPath = string.Empty; // Resources are at the root of the assembly
            });

            return services;
        }
    }
}

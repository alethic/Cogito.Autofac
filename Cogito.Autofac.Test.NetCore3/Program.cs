using System.Threading.Tasks;

using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.Hosting;

namespace Cogito.Autofac.Test.NetCore3
{

    public static class Program
    {

        /// <summary>
        /// Main application entry point.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args) =>
            await Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b.RegisterAllAssemblyModules()))
                .RunConsoleAsync();

    }

}

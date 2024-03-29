﻿using System.Threading.Tasks;

using Cogito.Autofac.DependencyInjection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Cogito.Autofac.Test.Net6
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
                .ConfigureWebHostDefaults(b => b.UseStartup<Startup>())
                .RunConsoleAsync();

    }

}

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iControl.Agent.Interfaces;
using iControl.Agent.Services;
using iControl.Server.API;
using iControl.Server.Data;
using iControl.Server.Services;
using iControl.Shared.Models;
using iControl.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace iControl.Tests
{
    [TestClass]
    public class IoCActivator
    {
        public static IServiceProvider ServiceProvider { get; set; }
        private static IWebHostBuilder builder;

        public static void Activate()
        {
            if (builder is null)
            {
                builder = WebHost.CreateDefaultBuilder()
                   .UseStartup<Startup>()
                   .CaptureStartupErrors(true)
                   .ConfigureAppConfiguration(config =>
                   {
                       config.AddInMemoryCollection(new Dictionary<string, string>()
                       {
                           ["ApplicationOptions:DBProvider"] = "InMemory"
                       });
                   });

                builder.Build();
            }
        }


        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Activate();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDb, TestingDbContext>();

            services.AddIdentity<iControlUser, IdentityRole>(options => options.Stores.MaxLengthForKeys = 128)
             .AddEntityFrameworkStores<AppDb>()
             .AddDefaultUI()
             .AddDefaultTokenProviders();

            services.AddTransient<IAppDbFactory, AppDbFactory>();
            services.AddTransient<IDataService, DataService>();
            services.AddTransient<IApplicationConfig, ApplicationConfig>();
            services.AddTransient<IEmailSenderEx, EmailSenderEx>();

            if (EnvironmentHelper.IsWindows)
            {
                services.AddTransient<IDeviceInformationService, DeviceInformationServiceWin>();
            }
            else if (EnvironmentHelper.IsLinux)
            {
                services.AddTransient<IDeviceInformationService, DeviceInformationServiceLinux>();
            }

            IoCActivator.ServiceProvider = services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }

}

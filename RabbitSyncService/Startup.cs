using System.Linq;
using Rabbit.Bridge.Messages;
using RabbitSyncService.Extensions;
using RabbitSyncService.Handlers;
using RabbitSyncService.Hubs;
using RabbitSyncService.Infrastructure;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Persistence.FileSystem;
using Rebus.Retry.Simple;
using Rebus.ServiceProvider;
using Constants = RabbitSyncService.Helpers.Constants;

namespace RabbitSyncService
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Только один хендлер прописывается, остальные сами регистрируются
            services.AutoRegisterHandlersFromAssemblyOf<JobHandler>();
//            services.AutoRegisterHandlersFromAssemblyOf<RetryHandler>();

            services.AddRebus(configure => configure
                //                .Logging(l => l.Use(new MSLoggerFactoryAdapter(_loggerFactory)))
                .Logging(l => l.Serilog())
                .Transport(t => t.UseRabbitMq("amqp://localhost", "consumer"))
//                .Transport(t => t.UseRabbitMqAsOneWayClient("amqp://localhost"))
//                .Timeouts(t => t.UseExternalTimeoutManager("timeouts"))
                .Timeouts(t => t.UseFileSystem("\\timeouts"))
                .Options(o =>
                {
                    o.SetNumberOfWorkers(2);
                    o.SetMaxParallelism(30);
                    o.HandleMessagesInsideTransactionScope();
                    o.SimpleRetryStrategy(errorTrackingMaxAgeMinutes: 24 * 60,
                        secondLevelRetriesEnabled: true
                    );
                }));


            services.AddSignalR();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRebus(async bus =>
            {
                await bus.Subscribe<Job>();
                await bus.Subscribe<RetryExample>();
            });
//            app.UseCookiePolicy();
            app.UseMvc();

            app.UseSignalR(routes => { routes.MapHub<TestHub>(Constants.Hub.Test.Path); });
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you. If you
        // need a reference to the container, you need to use the
        // "Without ConfigureContainer" mechanism shown later.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            RegisterDependency(builder);
        }

        private static void RegisterDependency(ContainerBuilder builder)
        {
            var assembly = typeof(Program).Assembly;

            builder.RegisterAssemblyModules(assembly);

            var registrableTypes = assembly.GetExportedTypes()
                .Where(type => type.IsClass
                               && !type.IsAbstract
                               && typeof(IDependency).IsAssignableFrom(type))
                .ToList();

            foreach (var type in registrableTypes)
            {
                var registerType = builder.RegisterType(type).AsImplementedInterfaces();

                if (type.Is<ISingletonDependency>())
                {
                    registerType.InstancePerLifetimeScope();
                }
                else if (type.Is<IUnitOfWorkDependency>())
                {
                    registerType.InstancePerMatchingLifetimeScope("work");
                }
                else if (type.Is<ITransientDependency>())
                {
                    registerType.InstancePerDependency();
                }
            }
        }
    }
}
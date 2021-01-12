using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication.Helpers;
using WebApplication.Models;
using WebApplication.Models.Options;
using WebApplication.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApplication.Framework;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            System.Data.Common.DbProviderFactories.RegisterFactory("System.Data.SqlClient", Microsoft.Data.SqlClient.SqlClientFactory.Instance);
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

            Helpers.AzureSDK azureSDK = new Helpers.AzureSDK(System.Convert.ToBoolean(Configuration["UseMSI"]), Configuration["AZURE_CLIENT_ID"], Configuration["AZURE_CLIENT_SECRET"], Configuration["AZURE_TENANT_ID"]);

            SqlConnectionStringBuilder _scsb = new SqlConnectionStringBuilder
            {
                DataSource = Configuration["AdsGoFastTaskMetaDataDatabaseServer"],
                InitialCatalog = Configuration["AdsGoFastTaskMetaDataDatabaseName"]
            };

            services.AddDbContext<AdsGoFastContext>(options =>
            {
                SqlConnection _con = new SqlConnection(_scsb.ConnectionString);
                string _token = azureSDK.GetAzureRestApiToken("https://database.windows.net/");
                _con.AccessToken = _token;
                options.UseSqlServer(_con);
            });

            services.Configure<SecurityModelOptions>(Configuration.GetSection("SecurityModelOptions"));

            services.AddSingleton<AppInsightsContext>(new AppInsightsContext(azureSDK, Configuration["AppInsightsWorkspaceId"]));
            services.AddSingleton<AdsGoFastDapperContext>(new AdsGoFastDapperContext(azureSDK, Configuration["AdsGoFastTaskMetaDataDatabaseServer"], Configuration["AdsGoFastTaskMetaDataDatabaseName"]));
            services.AddSingleton<SecurityAccessProvider>();

            //todo: remove the concrete reference from the scaffolding upstream and register the singleton witht he interface.
            services.AddTransient<ISecurityAccessProvider>(services => services.GetService<SecurityAccessProvider>());
            services.AddTransient<IEntityRoleProvider, EntityRoleProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionAssignedViaRoleHandler>();

            services.AddControllersWithViews(opt =>
            {
                opt.Filters.Add(new Helpers.DefaultHelpLinkActionFilter());
            })
                    .AddMvcOptions(m => m.ModelMetadataDetailsProviders.Add(new HumanizerMetadataProvider()));




            services.AddRazorPages();

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration);
            services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options => options.AccessDeniedPath = "/Home/AccessDenied");

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .AddRequirements(new PermissionAssignedViaRole())
                            .Build();

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Dashboard}/{action=Index}/{id?}").RequireAuthorization();
                endpoints.MapRazorPages();
            });
        }
    }
}

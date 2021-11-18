using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;



namespace ReservationRestaurant
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAutoMapper(cfg =>
            //{
            //    cfg.CreateMap<Models.Sitting.Create, Data.Sitting>();
            //    cfg.CreateMap<Models.Sitting.Update, Data.Sitting>().ReverseMap();
            //});

            //globalisation for Azure Date dd/mm/yyyy problem
            services.AddMvc()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);



            //object p = services.AddPortableObjectLocalization();



            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                new CultureInfo("en-AU"),
                new CultureInfo("en-GB"),
                };
                options.DefaultRequestCulture = new RequestCulture("en-GB");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });



            services.AddScoped<PersonService>();
            services.AddTransient<iEmailService, SendGridEmailService>();// for Email Services

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                 .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();


            // Huda check this website:https://gavilan.blog/2021/05/19/fixing-the-error-a-possible-object-cycle-was-detected-in-different-versions-of-asp-net-core/

        //    services.AddControllers().AddJsonOptions(x =>
          //                              x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

            //From Api class 16-09-2021 Video we add this---> step one
            services.AddCors();


            services.AddControllersWithViews()
                 .AddNewtonsoftJson(options =>
                 options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)//, IServiceProvider serviceProvider
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
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


            //From Api class 16-09-2021 Video we add this---> step two
            app.UseCors(x =>
            {
                x.AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials()
                 .SetIsOriginAllowed(o => true);
            });



            app.UseRequestLocalization();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                       name: "areas",
                       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Reservation}/{action=PreCreate}/{id?}");
                endpoints.MapRazorPages();
            });
            createRoles(serviceProvider);
        }

        private void createRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Manager", "Employee", "Member" };
            foreach (string roleName in roleNames)
            {
                Task<bool> roleExists = roleManager.RoleExistsAsync(roleName);
                roleExists.Wait();
                if (!roleExists.Result)
                {
                    Task<IdentityResult> result = roleManager.CreateAsync(new IdentityRole(roleName));
                    result.Wait();
                }
            }
        }
    }
}

namespace OnlineLibrary
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using OnlineLibrary.Data;
    using OnlineLibrary.Data.Configuration;
    using OnlineLibrary.Data.Models;
    using OnlineLibrary.Data.Repository.Contracts;
    using OnlineLibrary.Services.Core.Interfaces;
    using OnlineLibrary.Web.Infrastructure.Extensions;
    using OnlineLibrary.Web.Infrastructure.Utilities;
    using OnlineLibrary.Web.Infrastructure.Utilities.Contracts;

    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            /* Get database connection string */
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            /* Register DbContext in DI */
            builder.Services.AddDbContext<OnlineLibraryDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            /* Register custom Services and DI*/

            builder.Services.RegisterUserServices(typeof(IAuthorService));
            builder.Services.RegisterRepositories(typeof(IAuthorRepository));

            builder.Services.AddSingleton<ISlugGenerator, SlugGenerator>();

            builder.Services
                  .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                  {
                      ConfigureIdentityOptions(options, builder.Configuration);
                  })
                  .AddUserManager<UserManager<ApplicationUser>>()
                  .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
                  .AddRoles<IdentityRole<Guid>>()
                  .AddSignInManager<SignInManager<ApplicationUser>>()
                  .AddEntityFrameworkStores<OnlineLibraryDbContext>()
                  .AddDefaultTokenProviders();

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            WebApplication app = builder.Build();

            using (IServiceScope scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                DatabaseSeeder.SeedRoles(services);
                DatabaseSeeder.AssignAdminRole(services);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
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


            app.Use((context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true && context.Request.Path == "/")
                {
                    if (context.User.IsInRole("Admin"))
                    {
                        context.Response.Redirect("/Admin/Home/Index");
                    }
                }
                return next();
            });


            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
               name: "areas",
               pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "slugRoute",
                pattern: "Books/Details/{slug:required}/{id:guid}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
               // .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");
            app.Run();
        }

        private static void ConfigureIdentityOptions(IdentityOptions options, ConfigurationManager configuration)
        {
            /* Bind the whole IdentityOptions section from configuration in appsettings.json or appsettings.Development.json, which will override the defaults. Default environment is Development, so appsettings.Development.json will be used.*/
            configuration.GetSection("IdentityOptions").Bind(options);

            /* Support legacy minute-based lockout key (DefaultLockoutTimeSpanMin) if present. */
            var minutes = configuration.GetValue<int?>("IdentityOptions:Lockout:DefaultLockoutTimeSpanMin");
            if (minutes.HasValue)
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(minutes.Value);
            }
            else
            {
                var timespanString = configuration.GetValue<string>("IdentityOptions:Lockout:DefaultLockoutTimeSpan");
                if (!string.IsNullOrEmpty(timespanString) && TimeSpan.TryParse(timespanString, out var ts))
                {
                    options.Lockout.DefaultLockoutTimeSpan = ts;
                }
            }
        }
    }
}

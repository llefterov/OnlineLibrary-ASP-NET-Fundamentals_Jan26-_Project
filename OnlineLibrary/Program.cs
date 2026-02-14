using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Services.Core;
using OnlineLibrary.Services.Core.Interfaces;

namespace OnlineLibrary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


            /* Register DbContext in DI */
            builder.Services.AddDbContext<OnlineLibraryDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            /* Register custom Services and DI*/
            builder.Services.AddScoped<IBooksService, BooksService>();
            builder.Services.AddScoped<IAuthorService, AuthorService>();
            builder.Services.AddScoped<IPublisherService, PublisherService>();

            /* Register Identity in DI */
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            ConfigureIdentity(options,builder.Configuration))
            .AddEntityFrameworkStores<OnlineLibraryDbContext>();
            builder.Services.AddControllersWithViews();


            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }

        private static void ConfigureIdentity(IdentityOptions options, ConfigurationManager configuration)
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

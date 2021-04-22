using DemoRoles.Server.Data;
using DemoRoles.Server.Models;
using DemoRoles.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace DemoRoles.Server {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			// using SQLite because it's very lightweight, SQL Server is overkill for most authentication tasks IMHO.
			// even if I was going to use SQLite for other database tasks I would keep the authentication stuff in
			// a different database.

			// I am not a fan of the entity framework, but I have left it in for the authentication system as it is
			// what the default projects come with, personally SQLite-net-PCL package is the easiest to use, models
			// are simple, they can be decorated to solve a number of tasks.
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlite(
					Configuration.GetConnectionString("DefaultConnection")));

			services.AddDatabaseDeveloperPageExceptionFilter();

			services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddRoles<IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddIdentityServer()
					.AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options => {
						options.IdentityResources["openid"].UserClaims.Add("role");
						options.ApiResources.Single().UserClaims.Add("role");
					});
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

			services.AddAuthentication()
				.AddIdentityServerJwt();

			services.AddAuthorization(options => options.AddAppPolicies());

			services.AddControllersWithViews();
			services.AddRazorPages();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
				InitializeId(serviceScope.ServiceProvider);
			}

			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
				app.UseWebAssemblyDebugging();
			} else {
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseIdentityServer();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapFallbackToFile("index.html");
			});
		}

		public void InitializeId(IServiceProvider serviceProvider) {
			using (var scope = serviceProvider.CreateScope()) {
				var provider = scope.ServiceProvider;
				var context = provider.GetRequiredService<ApplicationDbContext>();
				var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
				var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
				context.Database.Migrate();
				InstallUsers(userManager, roleManager);
			}
		}

		private void InstallUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
			const string ADMINROLE = "Admin";
			const string ADMINUSERNAME = "admin@test";
			const string ADMINPASSWORD = "Password1%";

			const string USERROLE = "User";
			const string USERUSERNAME = "user@test";
			const string USERPASSWORD = "Password1%";

			var roleExist = roleManager.RoleExistsAsync(ADMINROLE).Result;
			if (!roleExist) {
				//create the roles and seed them to the database
				roleManager.CreateAsync(new ApplicationRole(ADMINROLE)).GetAwaiter().GetResult();
			}

			var admin = userManager.FindByNameAsync(ADMINUSERNAME).Result;

			if (admin == null) {
				var serviceUser = new ApplicationUser {
					UserName = ADMINUSERNAME,
					Email = ADMINUSERNAME
					//					AccountEnabled = true
				};

				var createPowerUser = userManager.CreateAsync(serviceUser, ADMINPASSWORD).Result;
				if (createPowerUser.Succeeded) {
					var confirmationToken = userManager.GenerateEmailConfirmationTokenAsync(serviceUser).Result;
					var result = userManager.ConfirmEmailAsync(serviceUser, confirmationToken).Result;
					userManager.AddToRoleAsync(serviceUser, ADMINROLE).GetAwaiter().GetResult();
				}
			}

			roleExist = roleManager.RoleExistsAsync(USERROLE).Result;
			if (!roleExist) {
				//create the roles and seed them to the database
				roleManager.CreateAsync(new ApplicationRole(USERROLE)).GetAwaiter().GetResult();
			}

			var user = userManager.FindByNameAsync(USERUSERNAME).Result;
			//var admin = userManager.FindByNameAsync(USERUSERNAME).Result;

			if (user == null) {
				var serviceUser1 = new ApplicationUser {
					UserName = USERUSERNAME,
					Email = USERUSERNAME
				};

				var createPowerUser1 = userManager.CreateAsync(serviceUser1, USERPASSWORD).Result;
				if (createPowerUser1.Succeeded) {
					var confirmationToken = userManager.GenerateEmailConfirmationTokenAsync(serviceUser1).Result;
					var result = userManager.ConfirmEmailAsync(serviceUser1, confirmationToken).Result;
					userManager.AddToRoleAsync(serviceUser1, USERROLE).GetAwaiter().GetResult();
				}
			}

			// this tests the more than one role per user, this adds the admin account to the user role
			try {
				userManager.AddToRoleAsync(admin, USERROLE).GetAwaiter().GetResult();
			} catch { }
		}
	}
}
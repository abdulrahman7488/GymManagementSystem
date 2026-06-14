using GymManagementSystem.BLL.Mapping;
using GymManagementSystem.BLL.Services.Classes;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.DAL.Data.DataSeed;
using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models.Identity;
using GymManagementSystem.DAL.Repositories.Classes;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── MVC ──────────────────────────────────────────────
            builder.Services.AddControllersWithViews();

            // ── Database ─────────────────────────────────────────
            builder.Services.AddDbContext<GymDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── Identity ─────────────────────────────────────────
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<GymDbContext>()
            .AddDefaultTokenProviders();

            // ── Cookie settings ───────────────────────────────────
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            // ── Repositories ─────────────────────────────────────
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── AutoMapper ───────────────────────────────────────
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());

            // ── Services (existing) ──────────────────────────────
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<ITrainerService, TrainerService>();

            // ── Services (Task) ──────────────────────────────────
            builder.Services.AddScoped<IMemberPlanService, MemberPlanService>();
            builder.Services.AddScoped<IMemberSessionService, MemberSessionService>();

            // ── Attachment Service ────────────────────────────────
            builder.Services.AddScoped<IAttachmentService, AttachmentService>();

            var app = builder.Build();

            // ── Seed Data ────────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var seedPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "Files");

                await dbContext.Database.MigrateAsync();
                await GymDataSeeding.SeedAsync(dbContext, seedPath, logger);
            }

            // ── Middleware ───────────────────────────────────────
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication(); // ← لازم قبل UseAuthorization
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

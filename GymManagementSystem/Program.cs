using GymManagementSystem.BLL.Mapping;
using GymManagementSystem.BLL.Services.Classes;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.DAL.Data.DataSeed;
using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Repositories.Classes;
using GymManagementSystem.DAL.Repositories.Interfaces;
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

            // ── Repositories ─────────────────────────────────────
            // ── Repositories ─────────────────────────────────────────────
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<IMemberRepository, MemberRepository>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── AutoMapper ───────────────────────────────────────  (Fix: use AddProfile)
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());

            // ── Services (existing) ──────────────────────────────
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<ITrainerService, TrainerService>();

            // ── Services (Task) ──────────────────────────────────
            builder.Services.AddScoped<IMemberPlanService, MemberPlanService>();
            builder.Services.AddScoped<IMemberSessionService, MemberSessionService>();

            var app = builder.Build();

            // ── Seed Data ────────────────────────────────────────  (Fix: pass required args)
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

using HomeLibrary.ApplicationContext;
using HomeLibrary.Models.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace HomeLibrary
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      builder.Services.AddControllersWithViews();

      var connection = builder.Configuration.GetConnectionString("MSSQL");
      var conStringBuilder = new SqlConnectionStringBuilder
      {
        DataSource = "localhost",
        InitialCatalog = "libraryDb",
        IntegratedSecurity = true,
        TrustServerCertificate = true
      };
      builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conStringBuilder.ConnectionString));
      builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseRouting();
      app.UseStaticFiles();

      app.UseAuthorization();

      app.UseRouting();
      app.UseAuthorization();

      // Редирект корня на /books
      app.MapGet("/", () => Results.Redirect("/books"));

      app.MapControllers();

      app.Run();
    }
  }
}

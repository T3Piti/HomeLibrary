using HomeLibrary.Data;
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

      builder.Services.AddControllersWithViews();

      var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

      if (string.IsNullOrEmpty(connectionString))
        throw new Exception("Строка подключения 'MSSQL' не найдена в appsettings.json! Проверьте файл.");

      builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

      builder.Services.AddScoped<ILibraryRepository, LibraryRepository>();

      var app = builder.Build();

      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseRouting();
      app.UseAuthorization();

      app.MapGet("/", () => Results.Redirect("/books"));
      app.MapControllers();

      app.Run();
    }
  }
}

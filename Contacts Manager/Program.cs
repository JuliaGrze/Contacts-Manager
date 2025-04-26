using Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using Services;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

//Services
builder.Services.AddControllersWithViews();

//add services into IoC container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsService, PersonsService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

//PDF
Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

//Excel
ExcelPackage.License.SetNonCommercialPersonal("Julia");


app.UseStaticFiles();
app.UseRouting(); //W³¹cza system trasowania (routing middleware)
app.MapControllers();

app.Run();

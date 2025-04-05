var builder = WebApplication.CreateBuilder(args);

//Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting(); //W³¹cza system trasowania (routing middleware)
app.MapControllers();

app.Run();

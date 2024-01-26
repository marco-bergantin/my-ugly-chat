using MyUglyChat.Hubs;
using MyUglyChat.Startup;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.InstallSignalR(config);
builder.Services.InstallAuth0(config);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.InstallDependencies();

builder.Services.InstallMongoDb(config);

var endpoint = builder.Services.InstallServiceBus(config);

var app = builder.Build();

_ = await endpoint.Start(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();

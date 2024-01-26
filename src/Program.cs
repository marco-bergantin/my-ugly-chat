using Auth0.AspNetCore.Authentication;
using MyUglyChat.Hubs;
using MyUglyChat.Services;
using MyUglyChat.Startup;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddSignalR()
    .AddStackExchangeRedis(config["ConnectionStrings:Redis"],
        o => o.Configuration.AbortOnConnectFail = false);

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = config["Auth0:Domain"];
        options.ClientId = config["Auth0:ClientId"];
        options.Scope = "openid profile email";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<UserConnectionService>();
builder.Services.AddSingleton<ContactsService>();
builder.Services.AddSingleton<ChatArchiveService>();
builder.Services.AddSingleton<RecentChatsService>();

var mongoClient = new MongoClient(builder.Configuration["ConnectionStrings:MongoDB"]);
var database = mongoClient.GetDatabase("marco-chat");
builder.Services.AddSingleton(database);

var endpoint = builder.Services.RegisterServiceBus(config["ConnectionStrings:RabbitMq"]);

var app = builder.Build();

var endpointInstance = await endpoint.Start(app.Services);

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

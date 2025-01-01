using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Bintainer.WebApp;
using Bintainer.WebApp.Data;
using Bintainer.Model;
using Bintainer.Service.Helper;
using Bintainer.Model.DTO;
using Bintainer.Service;
using Bintainer.Repository.Interface;
using Bintainer.Repository.Service;
using Bintainer.Service.Interface;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Bintainer.SharedResources.Interface;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Console;
using Bintainer.Repository;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddLocalization();

var supportedCultures = new[]
{
    new CultureInfo("en-US")
};

builder.Services.Configure<RequestLocalizationOptions>(options => {
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Logging.SetMinimumLevel(LogLevel.Trace);
//builder.Logging.AddFile("Logs/Bintainer-{Date}.txt");

builder.Services.AddSingleton<IStringLocalizerFactory,ResourceManagerStringLocalizerFactory>();
builder.Services.AddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

builder.Services.AddControllersWithViews()
        .AddDataAnnotationsLocalization()
        .AddViewLocalization();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<BintainerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

if (builder.Environment.IsDevelopment())
{
	builder.Configuration.AddUserSecrets<Program>();
}
// Use AWS logging in Production
if (builder.Environment.IsProduction())
{
    //TODO: check this out
    //builder.Logging.AddAWSProvider(builder.Configuration.GetSection("Logging:AWS.Logging"));
}


builder.Services.Configure<DigikeySettings>(builder.Configuration.GetSection("Digikey"));



builder.Services.AddScoped<DigikeyService>();

builder.Services.AddRazorPages();

builder.Services.AddScoped<IBinRepository,BinRepository>();
builder.Services.AddScoped<IInventoryRepository,InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IBinService, BinService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

builder.Services.AddScoped<IAppLogger, AppLogger>();


builder.Services.AddTransient<IEmailSender, SESEmailSender>();
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");



var app = builder.Build();

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();	
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

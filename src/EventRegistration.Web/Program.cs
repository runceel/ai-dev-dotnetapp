using EventRegistration.Analytics.Application.Navigation;
using EventRegistration.Analytics.Infrastructure;
using EventRegistration.Events.Application.Navigation;
using EventRegistration.Events.Infrastructure;
using EventRegistration.Notifications.Infrastructure;
using EventRegistration.Registrations.Application.Navigation;
using EventRegistration.Registrations.Application.Services;
using EventRegistration.Registrations.Infrastructure;
using EventRegistration.SharedKernel.Application.DemoData;
using EventRegistration.Web.Adapters;
using EventRegistration.Web.Components;
using EventRegistration.Web.DemoData;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// MudBlazor の各種サービスを登録
builder.Services.AddMudServices();

// 各モジュールが提供するナビゲーション項目を登録
builder.Services.AddEventsModuleNavigation();
builder.Services.AddRegistrationsModuleNavigation();
builder.Services.AddAnalyticsModuleNavigation();

// 各モジュールの Infrastructure サービスを登録
builder.Services.AddEventsModuleInfrastructure();
builder.Services.AddRegistrationsModuleInfrastructure();
builder.Services.AddNotificationsModule();
builder.Services.AddAnalyticsModule();

// モジュール間アダプターを登録
builder.Services.AddScoped<IEventCapacityChecker, EventCapacityCheckerAdapter>();

// デモ用シードデータ投入機能を登録
// 設定セクションが省略された場合は Development 環境のみ既定で ON とする。
builder.Services
    .AddOptions<DemoDataOptions>()
    .Bind(builder.Configuration.GetSection(DemoDataOptions.SectionName))
    .PostConfigure(o =>
    {
        if (!builder.Configuration.GetSection(DemoDataOptions.SectionName).Exists())
        {
            o.Enabled = builder.Environment.IsDevelopment();
        }
    });
builder.Services.AddScoped<IDemoDataSeeder, EventsDemoDataSeeder>();
builder.Services.AddScoped<IDemoDataSeeder, RegistrationsDemoDataSeeder>();
builder.Services.AddHostedService<DemoDataHostedService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

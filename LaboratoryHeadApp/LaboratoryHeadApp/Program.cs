using LaboratoryHeadApp.Services;
using MOLServiceWebClient;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// MVC
builder.Services.AddControllersWithViews();

// Schedule API
builder.Services.AddHttpClient<IScheduleApiClient, ScheduleApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:ScheduleServiceUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("Ќе настроен ApiSettings:ScheduleServiceUrl");
    }

    client.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");

    // —инхронизаци€ расписани€ может идти дольше 100 секунд,
    // потому что обрабатываютс€ все группы университета
    client.Timeout = TimeSpan.FromMinutes(10);
});

// MOL API
builder.Services.AddHttpClient<IMolApiClient, MolApiClient>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:MolServiceUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("Ќе настроен ApiSettings:MolServiceUrl");
    }

    client.BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");

    // Ќа вс€кий случай, если список аудиторий/оборудовани€ будет грузитьс€ дольше обычного
    client.Timeout = TimeSpan.FromMinutes(3);
});

builder.Services.AddScoped<IInventoryReportPdfService, InventoryReportPdfService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
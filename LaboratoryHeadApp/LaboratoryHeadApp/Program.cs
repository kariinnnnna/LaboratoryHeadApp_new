using MOLServiceWebClient;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();


// Schedule API
builder.Services.AddHttpClient<IScheduleApiClient, ScheduleApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:ScheduleServiceUrl"]);
});

// MOL API
builder.Services.AddHttpClient<IMolApiClient, MolApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:MolServiceUrl"]);
});

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
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
using BlazorApp1;
using BlazorApp1.Data;
using BlazorApp1.Helpers;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient("GDriveApi", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/drive/v3/");
}).AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
            .ConfigureHandler(new [] { "https://www.googleapis.com/drive/v3/" }, new [] { "https://www.googleapis.com/auth/drive" })

) ;

//builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GDriveApi"));


builder.Services.AddHttpClient("GDriveApiUpload", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/upload/drive/v3/files");
    
}).AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
            .ConfigureHandler(new[] { "https://www.googleapis.com/upload/drive/v3/files" }, new[] { "https://www.googleapis.com/auth/drive" })

);
//builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("GDriveApiUpload"));

// Disable logging for Entity Framework Core
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None));



builder.Services.AddScoped<Project>();
builder.Services.AddSingleton<CounterService, CounterService>();

builder.Services.AddSingleton<SharedDataService, SharedDataService>();
builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddMasaBlazor();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazorContextMenu();



//builder.Services.AddDbContext<DataContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddSqlite<SNotesDBContext>("Data Source=snotesonline.db");
//builder.Services.AddDbContextFactory<DataContext>();
builder.Services.AddDbContextFactory<SNotesDBContext>(x => x.UseSqlite("Data Source=snotesonline.db"));






builder.Services.AddOidcAuthentication(options => {
    builder.Configuration.Bind("Local", options.ProviderOptions);
    options.ProviderOptions.DefaultScopes.Add("https://www.googleapis.com/auth/drive");
});

//builder.Services.AddScoped<DatabaseService>();
SeedData.Initialize();
await builder.Build().RunAsync();


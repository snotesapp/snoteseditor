using BlazorApp1;
using BlazorApp1.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<Project>();

//builder.Services.AddDbContext<DataContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddSqlite<DataContext>("Data Source=snotesonline.db");
//builder.Services.AddDbContextFactory<DataContext>();

//builder.Services.AddDbContextFactory<DataContext>(x => x.UseSqlite("Data Source=snotesonline.db"));

//builder.Services.AddScoped<DatabaseService>();
SeedData.Initialize();
await builder.Build().RunAsync();


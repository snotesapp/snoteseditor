using BlazorApp1;
using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Repositories;
using BlazorApp1.Services;
using BlazorApp1.ViewModels;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");





// Disable logging for Entity Framework Core
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None));



builder.Services.AddScoped<Project>();

builder.Services.AddSingleton<NotesCollectionViewModel, NotesCollectionViewModel>();
builder.Services.AddSingleton<NotesCollectionService, NotesCollectionService>();
builder.Services.AddSingleton<NotesCollectionRepository, NotesCollectionRepository>();

builder.Services.AddSingleton<PacketService,PacketService>();
builder.Services.AddSingleton<PacketViewModel,PacketViewModel>();


builder.Services.AddSingleton<SharedDataService, SharedDataService>();
builder.Services.AddSingleton<SkiaServices, SkiaServices>();
builder.Services.AddSingleton<GetProjectFileJS, GetProjectFileJS>();

builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddMasaBlazor();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();


//builder.Services.AddDbContext<DataContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddSqlite<SNotesDBContext>("Data Source=snotesonline.db");
//builder.Services.AddDbContextFactory<DataContext>();
builder.Services.AddDbContextFactory<SNotesDBContext>(x => x.UseSqlite("Data Source=snotesonline.db"));


//builder.Services.AddScoped<DatabaseService>();
SeedData.Initialize();
await builder.Build().RunAsync();


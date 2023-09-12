using BlazorApp1;
using BlazorApp1.Data;
using BlazorApp1.Helpers;
using BlazorApp1.Repositories;
using BlazorApp1.Services;
using BlazorApp1.ViewModels;
using BlazorBootstrap;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System.Runtime.Versioning;
using SqliteWasmHelper;

[assembly: SupportedOSPlatform("browser")]

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Disable logging for Entity Framework Core
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None));




builder.Services.AddScoped<Project>();
builder.Services.AddSingleton<CacheStorageAccessor, CacheStorageAccessor>();

builder.Services.AddSingleton<SharedDataService, SharedDataService>();

builder.Services.AddSingleton<NotesCollectionViewModel, NotesCollectionViewModel>();
builder.Services.AddSingleton<NotesCollectionService, NotesCollectionService>();
builder.Services.AddSingleton<NotesCollectionRepository, NotesCollectionRepository>();

builder.Services.AddSingleton<PacketService,PacketService>();
builder.Services.AddSingleton<PacketViewModel,PacketViewModel>();

builder.Services.AddSingleton<NoteService, NoteService>();
builder.Services.AddSingleton<NoteViewModel, NoteViewModel>();

builder.Services.AddSingleton<ProjectService, ProjectService>();
builder.Services.AddSingleton<ProjectViewModel, ProjectViewModel>();


builder.Services.AddSingleton<SkiaServices, SkiaServices>();
builder.Services.AddSingleton<GetProjectFileJS, GetProjectFileJS>();



builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddMasaBlazor();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddBlazorBootstrap();

//builder.Services.AddDbContext<DataContext>(x => x.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddSqlite<SNotesDBContext>("Data Source=snotesonline.db");
//builder.Services.AddDbContextFactory<DataContext>();

//builder.Services.AddDbContextFactory<SNotesDBContext>(x => x.UseSqlite("Data Source=snotesonline.db"));

builder.Services.AddSqliteWasmDbContextFactory<SNotesDBContext>(
  opts => opts.UseSqlite("Data Source=snotesonline.sqlite3"));


//builder.Services.AddSingleton<DbContextService>();


//builder.Services.AddScoped<DatabaseService>();
SeedData.Initialize();
await builder.Build().RunAsync();


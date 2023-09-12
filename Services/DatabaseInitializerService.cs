using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace BlazorApp1.Services
{
    public class DatabaseInitializerService
    {
        private readonly ISqliteWasmDbContextFactory<SNotesDBContext> _contextFactory;
        private readonly string _connectionString;

        public DatabaseInitializerService(ISqliteWasmDbContextFactory<SNotesDBContext> contextFactory, string connectionString)
        {
            _contextFactory = contextFactory;
            _connectionString = connectionString;
        }

        public void CreateDatabaseIfNotExists()
        {
            var options = new DbContextOptionsBuilder<SNotesDBContext>()
                .UseSqlite(_connectionString)
                .Options;

            using (var context = new SNotesDBContext(options))
            {
                context.Database.Migrate();
            }
        }
    }
}

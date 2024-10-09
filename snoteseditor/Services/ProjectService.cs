using BlazorApp1.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using SqliteWasmHelper;
using BlazorApp1.Helpers;

namespace BlazorApp1.Services
{
    public class ProjectService
    {
        /*
          private readonly IDbContextFactory<SNotesDBContext> _dbContextFactory;
          private readonly DbContextService _dbContextService;

          public ProjectService(DbContextService dbContextService)_dbContextFactory
          {
              _dbContextService = dbContextService;
          }
        */

        private ISqliteWasmDbContextFactory<SNotesDBContext> _dbContextFactory;
        private CacheStorageAccessor _cacheStorageAccessor;
        
        public ProjectService(ISqliteWasmDbContextFactory<SNotesDBContext> dbContextFactory, CacheStorageAccessor cacheStorageAccessor)
        {
            _dbContextFactory = dbContextFactory;
            _cacheStorageAccessor = cacheStorageAccessor;
           

        }

      
        public async Task<Project> GetProject()
        {
            using (var projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {
                
                Project mProject = await projectsContext.Projects.Include(nc => nc.NotesCollection).Include(cr => cr.Packets.Where(s => s.Selected == true)).ThenInclude(pc => pc.Parent).FirstOrDefaultAsync();
                return mProject;


            }

        }

       
        public async Task<Project> GetFullProject()
        {
           
            Project? fProject ;
            
            using (SNotesDBContext projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {
                
                fProject = await projectsContext.Projects.Include(nc => nc.NotesCollection).ThenInclude(ncn => ncn.Note).ThenInclude(nci => nci.Images)
                .Include(nc => nc.NotesCollection).ThenInclude(ncn => ncn.Note).ThenInclude(pth => pth.NotePaths)
                .Include(cr => cr.Packets).FirstOrDefaultAsync();
                
            }
            
            return fProject;

        }

    

        public async Task InsertProject(Project project)
        {
            using (var projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {
                // projectsContext.Add(project);
                await projectsContext.AddAsync(project);
                await projectsContext.SaveChangesAsync();             
            }            
        }


        public async Task UpdateProject(Project project)
        {

            using (var projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {
                 projectsContext.Update(project);
                await projectsContext.SaveChangesAsync();             
            }
        }

        public HttpRequestMessage CreateMessage() => new HttpRequestMessage(HttpMethod.Get, "/data/cache/snotesonline.sqlite3");

       
        public async Task RemoveSqliteCacheAsync()
        {
           
            await _cacheStorageAccessor.RemoveAsync(CreateMessage());
        }

        public async Task<byte[]> GetSqliteCacheValueAsync()
        {
            return await _cacheStorageAccessor.RetrieveFileFromCache(CreateMessage());
        }

        public async Task SqliteEnsureCreatedAsync( )
        {
            using (var projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {


                await projectsContext.Database.EnsureCreatedAsync();
                await projectsContext.SaveChangesAsync();
            }

        }

        public async Task SqliteEnsureDeletedAsync()
        {
            using (var projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {


                await projectsContext.Database.EnsureDeletedAsync();
               
            }

        }


        public async Task StoreSqliteCacheValueAsync(byte[] sqliteBytes)
        {
            using (var projectsContext = await _dbContextFactory.CreateDbContextAsync())
            {


                await projectsContext.Database.EnsureCreatedAsync();
                await projectsContext.SaveChangesAsync();
            }

            await _cacheStorageAccessor.StoreSqliteAsync(CreateMessage(), sqliteBytes);
                           
        }
    }

}

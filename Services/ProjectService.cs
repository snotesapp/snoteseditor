using BlazorApp1.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services
{
    public class ProjectService
    {
      
        private readonly IDbContextFactory<SNotesDBContext> _dbContextFactory;
        public ProjectService(IDbContextFactory<SNotesDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            using var db = _dbContextFactory.CreateDbContext();
            db.Database.EnsureCreatedAsync();
           
        }

        public async Task<Project> GetProject()
        {
            using (var projectsContext = _dbContextFactory.CreateDbContext())
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
            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {
                // projectsContext.Add(project);
                await projectsContext.AddAsync(project);
                await projectsContext.SaveChangesAsync();
               
            }

            
        }



        public async Task UpdateProject(Project project)
        {

            using (var projectsContext = _dbContextFactory.CreateDbContext())
            {
                 projectsContext.Update(project);
                await projectsContext.SaveChangesAsync();

              
            }
        }


    }
}

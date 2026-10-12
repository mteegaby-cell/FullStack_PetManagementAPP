using Microsoft.EntityFrameworkCore;

namespace PETMSCRUD.Data
{
    public class PETMSDBContext : DbContext
    {
        public PETMSDBContext(DbContextOptions<PETMSDBContext> options) : base(options)
        {
            
        }
        
        //EF pt. 1 Class DbSets go here (Slide 44)
    }
}

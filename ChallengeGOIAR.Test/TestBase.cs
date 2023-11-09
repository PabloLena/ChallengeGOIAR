using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChallengeGOIAR.Test
{
    public class TestBase
    {
        protected ApplicationDbContext BuildContext(string DbName) 
        { 
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(DbName).Options;
            var dbContext= new ApplicationDbContext(options);
            return dbContext;
        }
   
    }
}

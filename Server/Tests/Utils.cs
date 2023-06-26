using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grip.DAL;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Server.Tests
{
    public class Utils
    {
        public static ApplicationDbContext CreateTestContext(SqliteConnection connection)
        {
            return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options, null);
        }
    }
}
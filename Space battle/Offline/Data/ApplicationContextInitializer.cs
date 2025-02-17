using SQLite.CodeFirst;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_battle.Offline.Data
{
    internal class ApplicationContextInitializer : SqliteDropCreateDatabaseAlways<ApplicationContext>
    {
        public ApplicationContextInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder) { }

        protected override void Seed(ApplicationContext context)
        {
        }
    }
}

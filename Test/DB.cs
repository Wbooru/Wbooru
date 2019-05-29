using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class DB : DbContext
    {
        public DbSet<Data> Datas { get; set; }
    }
}

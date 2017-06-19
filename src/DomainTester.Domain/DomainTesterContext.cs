using DomainTester.Domain.Tables;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainTester.Domain
{
    public class DomainTesterContext : DbContext
    {
		public DomainTesterContext() : base("DomainTester")
		{

		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
		}


		public virtual DbSet<TestObject> TestObjects { get; set; }
	}
}

using DomainTester.Domain.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainTester.Service.Dtos
{
    public class TestObjectDto
    {
		public int Id { get; set; }
		public int A { get; set; }
		public bool B { get; set; }
		public string C { get; set; }

		public TestObjectDto(TestObject dbo)
		{
			this.Id = dbo.Id;
			this.A = dbo.A;
			this.B = dbo.B;
			this.C = dbo.C;
		}
	}
}

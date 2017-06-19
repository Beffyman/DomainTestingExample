using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DomainTester.Domain.Tables
{
    public class TestObject
    {
		[Key]
		public int Id { get; set; }
		public int A { get; set; }
		public bool B { get; set; }
		public string C { get; set; }
	}
}

using DomainTester.Service.Controllers;
using DomainTester.Tests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainTester.Integration
{
	public class TestFeatureModel
	{
		public MockDomainTester<TestController> Controller { get; set; }


		public int GetId { get; set; }
		public IActionResult GetResult { get; set; }
	}
}

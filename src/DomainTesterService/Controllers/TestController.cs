using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DomainTester.Service.Services;

namespace DomainTester.Service.Controllers
{
    [Route("api/[controller]")]
	public class TestController : Controller
	{
		protected readonly ITestService _testService;
		public TestController(ITestService testService)
		{
			_testService = testService;
		}


		[HttpGet("{id}")]
		public IActionResult Get(int id)
		{
			var dto = _testService.Get(id);
			if (dto != null)
			{
				return Ok(dto);
			}
			return NotFound();
		}
	}
}

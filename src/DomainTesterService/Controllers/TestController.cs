using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DomainTester.Service.Services;
using System.Net;
using DomainTester.Service.Dtos;
using DomainTester.Service.Commands.Test;
using System.Reflection;

namespace DomainTester.Service.Controllers
{
    [Route("api/test")]
	public class TestController : Controller
	{
		protected readonly ITestService _testService;
		public TestController(ITestService testService)
		{
			_testService = testService;
		}


		[HttpGet("{id}")]
		[ProducesResponseType(typeof(TestObjectDto), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(void), (int)HttpStatusCode.NotFound)]
		public IActionResult Get(int id)
		{
			var dto = _testService.Get(id);
			if (dto != null)
			{
				return Ok(dto);
			}
			return NotFound();
		}

		[HttpPost]
		[ProducesResponseType(typeof(TestObjectDto), (int)HttpStatusCode.OK)]
		public IActionResult Create(CreateTestObjectCommand command)
		{
			var dto = _testService.Create(command);

			var resultUri = this.GetUri(MethodBase.GetCurrentMethod(), new Dictionary<string, object>
			{
				{"id" , dto.Id}
			});

			return Created(resultUri, dto);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using DomainTester.Tests;
using Microsoft.AspNetCore.Mvc;
using DomainTester.Service.Dtos;
using NUnit.Framework;
using DomainTester.Domain.Tables;
using DomainTester.Service.Controllers;

namespace DomainTester.Integration
{
	[Binding]
	[Scope(Feature = "Test")]
	public class GenericTest
	{
		private readonly ScenarioContext _context;
		public GenericTest()
		{
			_context = ScenarioContext.Current;
			_context.Set(new TestFeatureModel(), nameof(TestFeatureModel));
		}


		[Given(@"I have a controller")]
		public void Given_Controller()
		{
			var model = _context.Get<TestFeatureModel>(nameof(TestFeatureModel));

			var controller = new MockDomainTester<TestController>();
			model.Controller = controller;
			model.Controller.SetupContext(x => x.TestObjects, new List<TestObject>
			{
				new TestObject
				{
					Id = 1,
					A = 12345,
					B = true,
					C = "HELLO"
				}
			});

			_context.Set(model, nameof(TestFeatureModel));
		}

		[Given(@"my Id is '(.*)'")]
		public void Given_Id(int Id)
		{
			var model = _context.Get<TestFeatureModel>(nameof(TestFeatureModel));

			model.GetId = Id;

			_context.Set(model, nameof(TestFeatureModel));
		}

		[Then(@"get the object")]
		public void Then_Get()
		{
			var model = _context.Get<TestFeatureModel>(nameof(TestFeatureModel));

			model.GetResult = model.Controller.Object.Get(model.GetId);

			_context.Set(model, nameof(TestFeatureModel));
		}

		[Then(@"it is valid")]
		public void Then_Valid()
		{
			var model = _context.Get<TestFeatureModel>(nameof(TestFeatureModel));

			if (model.GetResult is ObjectResult or)
			{
				if (or.Value is TestObjectDto dto)
				{
					Assert.AreEqual(dto.Id, model.GetId);
				}
			}
			else
			{
				Assert.Fail("Get failed");
			}

			_context.Set(model, nameof(TestFeatureModel));
		}
	}
}

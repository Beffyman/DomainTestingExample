using Microsoft.AspNetCore.Mvc;
using System;
using Moq;
using DomainTester.Domain;
using System.Data.Entity;
using DomainTester.Domain.Tables;
using Microsoft.Extensions.DependencyInjection;
using DomainTester.Service.Services;
using DomainTester.Service.Controllers;
using DomainTester.Service.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DomainTester.Service.Commands.Test;

namespace DomainTester.Tests
{
	[TestClass]
	public class TestServiceTests
	{
		protected MockDomainTester<TestController, DomainTesterContext> _mockController;

		[TestInitialize]

		public void Setup()
		{
			_mockController = new MockDomainTester<TestController, DomainTesterContext>();
		}

		[TestCleanup]

		public void Cleanup()
		{
			_mockController.Dispose();
		}




		[TestMethod]
		public void Get()
		{
			TestObject mockObject = new TestObject
			{
				Id = 1,
				A = 12345,
				B = true,
				C = "HELLO"
			};

			var testObjectsQueryable = new List<TestObject>
			{
				mockObject
			};

			_mockController.SetupContext(db => db.TestObjects, testObjectsQueryable);


			var result = _mockController.Object.Get(mockObject.Id);

			if (result is ObjectResult or)
			{
				if (or.Value is TestObjectDto dto)
				{
					Assert.AreEqual(dto.Id, mockObject.Id);
					return;
				}
			}

			Assert.Fail("(result) did not return a ObjectResult");
		}

		[TestMethod]
		public void Create()
		{
			CreateTestObjectCommand command = new CreateTestObjectCommand
			{
				A = 12345,
				B = true,
				C = "HELLO"
			};

			var testObjectsQueryable = new List<TestObject>
			{
			};

			_mockController.SetupContext(db => db.TestObjects, testObjectsQueryable);


			var result = _mockController.Object.Create(command);

			if (result is ObjectResult or)
			{
				if (or.Value is TestObjectDto dto)
				{
					Assert.AreNotEqual(dto.Id, 0);
					return;
				}
			}

			Assert.Fail("(result) did not return a ObjectResult");
		}
	}
}

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

namespace DomainTester.Tests
{
	[TestClass]
	public class TestServiceTests
	{
		protected Mock<DomainTesterContext> _mockContext;
		protected TestController _controller;

		protected IServiceProvider _serviceProvider;

		[TestInitialize]

		public void Setup()
		{
			_mockContext = new Mock<DomainTesterContext>();

			ServiceCollection services = new ServiceCollection();
			services.AddSingleton(_mockContext.Object);
			services.AddTransient<ITestService, TestService>();
			services.AddTransient<TestController>();

			_serviceProvider = services.BuildServiceProvider();

			_controller = _serviceProvider.GetService<TestController>();
		}

		[TestCleanup]

		public void Cleanup()
		{
			_serviceProvider = null;
			_controller = null;
			_mockContext = null;
		}


		private void MockDBSetWithQueryable<T>(Expression<Func<DomainTesterContext, DbSet<T>>> expression, IQueryable<T> queryable) where T : class
		{
			Mock<DbSet<T>> mockTestObjects = new Mock<DbSet<T>>();

			mockTestObjects.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
			mockTestObjects.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
			mockTestObjects.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
			mockTestObjects.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

			_mockContext.Setup(expression).Returns(mockTestObjects.Object);
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

			}.AsQueryable();

			MockDBSetWithQueryable(db => db.TestObjects, testObjectsQueryable);


			var result = _controller.Get(1);

			if (result is ObjectResult or)
			{
				if (or.Value is TestObjectDto dto)
				{
					Assert.AreEqual(dto.Id, mockObject.Id);
				}
				else
				{
					Assert.Fail("(result) did not return a TestObjectDto");
				}
			}
			else
			{
				Assert.Fail("(result) did not return a ObjectResult");
			}
		}
	}
}

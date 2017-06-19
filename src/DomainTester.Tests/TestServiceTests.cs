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
		protected Mock<DomainTesterContext> _mockContext;
		protected TestController _controller;

		protected IServiceProvider _serviceProvider;

		private void SetPrimaryKey<T>(DbSet<T> dbSet) where T : class, IEntity
		{
			int max = 0;
			if (dbSet.Any())
			{
				max = dbSet.Max(x => x.Id);
			}

			foreach (var entity in dbSet.Where(x => x.Id == 0).ToList())
			{
				entity.Id = ++max;
			}
		}

		[TestInitialize]

		public void Setup()
		{


			_mockContext = new Mock<DomainTesterContext>();
			_mockContext.Setup(x => x.SaveChanges()).Returns(() =>
			{
				var type = _mockContext.Object.GetType();
				var properties = type.GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)).ToList();
				foreach (var dbSetProperty in properties)
				{
					var dbSet = dbSetProperty.GetValue(_mockContext.Object);
					var dbSetType = typeof(DbSet<>).MakeGenericType(dbSetProperty.PropertyType.GetGenericArguments());

					var iType = this.GetType();
					var method = iType.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SingleOrDefault(x => x.Name == nameof(SetPrimaryKey));
					var genericMethod = method.MakeGenericMethod(dbSetProperty.PropertyType.GetGenericArguments());
					genericMethod.Invoke(this, new object[] { dbSet });
				}

				return 1;
			});





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


		private void MockDBSetWithList<T>(Expression<Func<DomainTesterContext, DbSet<T>>> expression, IList<T> enumerable) where T : class
		{
			var queryable = enumerable.AsQueryable();

			Mock<DbSet<T>> mockTestObjects = new Mock<DbSet<T>>();

			mockTestObjects.Setup(x => x.Create()).Returns(Activator.CreateInstance<T>());
			mockTestObjects.Setup(x => x.Add(It.IsAny<T>())).Callback<T>((item) => enumerable.Add(item));
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
			};

			MockDBSetWithList(db => db.TestObjects, testObjectsQueryable);


			var result = _controller.Get(mockObject.Id);

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

			MockDBSetWithList(db => db.TestObjects, testObjectsQueryable);


			var result = _controller.Create(command);

			if (result is ObjectResult or)
			{
				if (or.Value is TestObjectDto dto)
				{
					Assert.AreNotEqual(dto.Id, 0);
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

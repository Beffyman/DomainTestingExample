using DomainTester.Domain;
using DomainTester.Service.Commands.Test;
using DomainTester.Service.Controllers;
using DomainTester.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DomainTester.Tests
{
	public class MockDomainTester : IDisposable
	{
		protected Mock<DomainTesterContext> _mockContext;
		protected TestController _controller;

		protected IServiceProvider _serviceProvider;

		public MockDomainTester()
		{
			_mockContext = new Mock<DomainTesterContext>();
			_mockContext.Setup(x => x.SaveChanges()).Returns(MockSaveChanges);

			SetupDbSets();

			ServiceCollection services = new ServiceCollection();
			services.AddSingleton(_mockContext.Object);
			services.AddTransient<ITestService, TestService>();
			services.AddTransient<TestController>();

			_serviceProvider = services.BuildServiceProvider();

			_controller = _serviceProvider.GetService<TestController>();
		}

		private void SetupDbSets()
		{
			var type = _mockContext.Object.GetType();
			var properties = type.GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)).ToList();

			foreach (var dbSetProperty in properties)
			{
				var dbSet = dbSetProperty.GetValue(_mockContext.Object);//DbSet<T>

				var entityType = dbSetProperty.PropertyType.GetGenericArguments().SingleOrDefault();
				var genericList = typeof(List<>).MakeGenericType(entityType);
				var mockList = Activator.CreateInstance(genericList);


				ParameterExpression parameter = Expression.Parameter(typeof(DomainTesterContext), "i");
				MemberExpression property = Expression.Property(parameter, dbSetProperty.Name);
				var queryableType = typeof(DbSet<>).MakeGenericType(entityType);
				var delegateType = typeof(Func<,>).MakeGenericType(typeof(DomainTesterContext), queryableType);

				var expression = Expression.Lambda(delegateType, property, parameter);

				//SetupContext(,mockList);

				var iType = this.GetType();
				var method = iType.GetTypeInfo().DeclaredMethods.SingleOrDefault(x => x.Name == nameof(SetupContext));
				var genericMethod = method.MakeGenericMethod(entityType);
				genericMethod.Invoke(this, new object[] { expression, mockList });
			}

		}

		private int MockSaveChanges()
		{
			var type = _mockContext.Object.GetType();
			var properties = type.GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)).ToList();
			foreach (var dbSetProperty in properties)
			{
				var dbSet = dbSetProperty.GetValue(_mockContext.Object);
				var dbSetType = typeof(DbSet<>).MakeGenericType(dbSetProperty.PropertyType.GetGenericArguments());

				var iType = this.GetType();
				var method = iType.GetTypeInfo().DeclaredMethods.SingleOrDefault(x => x.Name == nameof(SetPrimaryKey));
				var genericMethod = method.MakeGenericMethod(dbSetProperty.PropertyType.GetGenericArguments());
				genericMethod.Invoke(this, new object[] { dbSet });
			}

			return 1;
		}


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

		public void SetupContext<T>(Expression<Func<DomainTesterContext, DbSet<T>>> expression, IList<T> enumerable) where T : class
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

		public IActionResult Get(int id)
		{
			return _controller.Get(id);
		}

		public IActionResult Create(CreateTestObjectCommand command)
		{
			return _controller.Create(command);
		}



		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					_serviceProvider = null;
					_controller = null;
					_mockContext = null;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~MockDomainTester() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion

	}
}

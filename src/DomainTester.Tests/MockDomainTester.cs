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
	public class MockDomainTester<TController,TContext> : IDisposable
		where TController : Controller
		where TContext : DbContext
	{
		protected Mock<TContext> _mockContext;
		protected TController _controller;

		public TController Object
		{
			get
			{
				return _controller;
			}
		}

		/// <summary>
		/// Container for all registered services, could replace w/ a castle.core implementation
		/// </summary>
		protected IServiceProvider _serviceProvider;

		public MockDomainTester()
		{
			_mockContext = new Mock<TContext>();
			_mockContext.Setup(x => x.SaveChanges()).Returns(MockSaveChanges);

			SetupDbSets();

			ServiceCollection services = new ServiceCollection();

			//Setup your DI
			var servicesStartupConfig = new Startup();
			servicesStartupConfig.ConfigureServices(services);

			var existingContext = services.SingleOrDefault(x => x.ServiceType == typeof(TContext));
			services.Remove(existingContext);//Remove REAL context
			services.AddSingleton(_mockContext.Object);//Insert mock context


			services.AddTransient<TController>();

			_serviceProvider = services.BuildServiceProvider();

			_controller = _serviceProvider.GetService<TController>();
		}

		/// <summary>
		/// Uses reflection to loop through all DbSets inside the TContext and provides them with a default value.
		/// </summary>
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


				ParameterExpression parameter = Expression.Parameter(typeof(TContext), "i");
				MemberExpression property = Expression.Property(parameter, dbSetProperty.Name);
				var queryableType = typeof(DbSet<>).MakeGenericType(entityType);
				var delegateType = typeof(Func<,>).MakeGenericType(typeof(TContext), queryableType);

				var expression = Expression.Lambda(delegateType, property, parameter);

				var iType = this.GetType();
				var method = iType.GetTypeInfo().DeclaredMethods.SingleOrDefault(x => x.Name == nameof(SetupContext));
				var genericMethod = method.MakeGenericMethod(entityType);
				genericMethod.Invoke(this, new object[] { expression, mockList });
			}

		}

		/// <summary>
		/// Mocks the save changes function
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Finds entities with no primary key, and sets them to max of dbSet +1
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <param name="dbSet"></param>
		private void SetPrimaryKey<K>(DbSet<K> dbSet) where K : class, IEntity
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

		/// <summary>
		/// Set up a DbSet with the list provided.
		/// </summary>
		/// <typeparam name="K"></typeparam>
		/// <param name="expression"></param>
		/// <param name="enumerable"></param>
		public void SetupContext<K>(Expression<Func<TContext, DbSet<K>>> expression, IList<K> enumerable) where K : class
		{
			var queryable = enumerable.AsQueryable();

			Mock<DbSet<K>> mockTestObjects = new Mock<DbSet<K>>();

			mockTestObjects.Setup(x => x.Create()).Returns(Activator.CreateInstance<K>());
			mockTestObjects.Setup(x => x.Add(It.IsAny<K>())).Callback<K>((item) => enumerable.Add(item));
			mockTestObjects.As<IQueryable<K>>().Setup(m => m.Provider).Returns(queryable.Provider);
			mockTestObjects.As<IQueryable<K>>().Setup(m => m.Expression).Returns(queryable.Expression);
			mockTestObjects.As<IQueryable<K>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
			mockTestObjects.As<IQueryable<K>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());


			_mockContext.Setup(expression).Returns(mockTestObjects.Object);
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

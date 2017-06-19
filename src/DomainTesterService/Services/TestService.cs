using DomainTester.Domain;
using DomainTester.Domain.Tables;
using DomainTester.Service.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainTester.Service.Services
{
	public interface ITestService
	{
		TestObjectDto Get(int id);
	}

	public class TestService : ITestService
	{
		protected readonly DomainTesterContext _dbContext;

		public TestService(DomainTesterContext dbContext)
		{
			_dbContext = dbContext;
		}

		public TestObjectDto Get(int id)
		{
			var entity = _dbContext.TestObjects.SingleOrDefault(x => x.Id == id);
			if(entity != null)
			{
				return new TestObjectDto(entity);
			}
			return null;
		}
	}
}

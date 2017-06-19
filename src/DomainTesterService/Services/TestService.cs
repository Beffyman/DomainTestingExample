using DomainTester.Domain;
using DomainTester.Domain.Tables;
using DomainTester.Service.Commands.Test;
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
		TestObjectDto Create(CreateTestObjectCommand command);
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

		public TestObjectDto Create(CreateTestObjectCommand command)
		{
			var entity = _dbContext.TestObjects.Create();
			entity.A = command.A;
			entity.B = command.B;
			entity.C = command.C;

			_dbContext.TestObjects.Add(entity);
			_dbContext.SaveChanges();

			return new TestObjectDto(entity);
		}
	}
}

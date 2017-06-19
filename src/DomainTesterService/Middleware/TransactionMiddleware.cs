using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DomainTester.Service.Middleware
{
    public class TransactionMiddleware
    {

		private readonly RequestDelegate _next;

		public TransactionMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task Invoke(HttpContext context)
		{
			using(var scope = CreateTransaction())
			{
				//Execute the method
				return this._next(context);
			}
		}

		private static TransactionScope CreateTransaction(Transaction parent = null)
		{
			if (parent != null)
			{
				return new TransactionScope(parent);
			}
			else
			{
				return new TransactionScope
				(
					TransactionScopeOption.Required,
					new TransactionOptions
					{
						IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
						Timeout = TransactionManager.MaximumTimeout
					}
				);
			}
		}
		private static Transaction GetTransaction()
		{
			Transaction transaction = null;
			try
			{
				transaction = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
			}
			catch { }
			return transaction;
		}
	}
}

using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace BookingVilla.Infrastructure.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _DbContext;
		internal DbSet<T> _dbSet;

		public Repository(ApplicationDbContext dbContext)
		{
			_DbContext = dbContext;
			_dbSet = _DbContext.Set<T>();
		}

		public void Add(T entity)
		{
			_dbSet.Add(entity);
		}

		public T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
		{
			IQueryable<T> query;

			if (tracked)
			{
				query = _dbSet;
			}
			else
			{
				query = _dbSet.AsNoTracking();
			}
			if (filter != null)
			{
				query = query.Where(filter);
			}
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var property in includeProperties
					.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(property.Trim());
				}
			}
			return query.FirstOrDefault();
		}

		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, bool tracked = false)
		{
			IQueryable<T> query;
			if (tracked)
			{
				query = _dbSet;
			}
			else
			{
				query = _dbSet.AsNoTracking();
			}

			if (filter != null)
			{
				query = query.Where(filter);
			}
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var property in includeProperties
					.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(property.Trim());
				}
			}
			return query.ToList();
		}

		public void Remove(T entity)
		{
			_dbSet.Remove(entity);
		}

		bool IRepository<T>.Any(Expression<Func<T, bool>> filter)
		{
			return _dbSet.Any(filter);
		}
	}
}

using GymBelepteto.Data;
using SQLite;

namespace GymBelepteto.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, new()
    {
        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Task.Run(() =>
                _context.Connection.Table<T>().ToList());
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await Task.Run(() =>
                _context.Connection.Find<T>(id));
        }

        public async Task AddAsync(T entity)
        {
            await Task.Run(() =>
                _context.Connection.Insert(entity));
        }

        public void Update(T entity)
        {
            _context.Connection.Update(entity);
        }

        public void Delete(T entity)
        {
            _context.Connection.Delete(entity);
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }
}
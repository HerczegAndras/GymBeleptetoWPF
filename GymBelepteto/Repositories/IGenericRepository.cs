using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

    namespace GymBelepteto.Repositories
    {
   
        public interface IGenericRepository<T>
            where T : class, new()
        {
            Task<IEnumerable<T>> GetAllAsync();
            Task<T?> GetByIdAsync(int id);
            Task AddAsync(T entity);
            void Update(T entity);
            void Delete(T entity);
            Task SaveAsync();
        }
    }



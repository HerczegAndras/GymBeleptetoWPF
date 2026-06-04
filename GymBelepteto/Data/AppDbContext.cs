using SQLite;
using GymBelepteto.Models;

namespace GymBelepteto.Data
{
    public class AppDbContext : IDisposable
    {
        public SQLiteConnection Connection { get; }

        public AppDbContext()
        {
            Connection = new SQLiteConnection("gym_belepteto.db");

            Connection.CreateTable<Customer>();
            Connection.CreateTable<User>();
            Connection.CreateTable<Product>();
        }

        public void Dispose()
        {
            Connection?.Close();
        }
    }
}
using SQLite;
using GymBelepteto.Models;

namespace GymBelepteto.Data
{
    public class AppDbContext
    {
        public SQLiteConnection Connection { get; }

        public AppDbContext()
        {
            Connection = new SQLiteConnection("gym_belepteto.db");

            Connection.CreateTable<Customer>();
            Connection.CreateTable<User>();
            Connection.CreateTable<Product>();
        }
    }
}
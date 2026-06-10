using GymBelepteto.Data;
using GymBelepteto.Models;
using GymBelepteto.ViewModels;
using System.Windows;

namespace GymBelepteto
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Tesztadatok generálása az új sqlite-net-pcl szerint
            InitializeDatabaseData();

            // Alapértelmezett kezdőoldal
            MainFrame.Navigate(new CustomerPage());
        }

        private void NavProducts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductPage());
        }

        private void NavCustomers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CustomerPage());
        }

        private void InitializeDatabaseData()
        {
            using var context = new AppDbContext();

            // TABLE CREATE
            // TABLES CREATE
            context.Connection.CreateTable<Customer>();
            context.Connection.CreateTable<Product>();

            // Lekérdezés
            var customers = context.Connection.Table<Customer>().ToList();

            if (customers.Count == 0)
            {
                context.Connection.Insert(new Customer
                {
                    FirstName = "András",
                    LastName = "Herczeg",
                    RemainingOccasions = 10,
                    PhoneNumber = "06301234567",
                    LoyaltyPoints = 0
                });

                context.Connection.Insert(new Customer
                {
                    FirstName = "Gábor",
                    LastName = "Szőcs",
                    RemainingOccasions = 0,
                    PhoneNumber = "06209876543",
                    LoyaltyPoints = 0
                });
            }
        }
    }
}
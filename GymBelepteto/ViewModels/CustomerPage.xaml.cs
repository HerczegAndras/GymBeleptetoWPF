using GymBelepteto.Data;
using GymBelepteto.Models;
using GymBelepteto.Repositories;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GymBelepteto.ViewModels
{
    public partial class CustomerPage : Page
    {
        private readonly GenericRepository<Customer> _customerRepository;
        private readonly AppDbContext _context;

        public CustomerPage()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _customerRepository = new GenericRepository<Customer>(_context);

            Loaded += CustomerPage_Loaded;
            Unloaded += CustomerPage_Unloaded;
        }

        private async void CustomerPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAsync();
        }

        private void CustomerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _context?.Dispose();
        }

        private async System.Threading.Tasks.Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();

                Dispatcher.Invoke(() =>
                {
                    CustomersDataGrid.ItemsSource = null;
                    CustomersDataGrid.ItemsSource = customers;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt az ügyfelek betöltése során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BeleptetesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is not Customer kijeloltVendeg)
            {
                MessageBox.Show("Kérlek, válassz ki egy vendéget a táblázatból!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (kijeloltVendeg.RemainingOccasions <= 0)
            {
                MessageBox.Show("Lejárt bérlet! Kérlek, adj el neki új bérletet!", "Figyelem!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                kijeloltVendeg.RemainingOccasions--;
                kijeloltVendeg.LoyaltyPoints += 10;

                _customerRepository.Update(kijeloltVendeg);
                await _customerRepository.SaveAsync();

                await LoadCustomersAsync();

                MessageBox.Show($"Sikeres belépés! {kijeloltVendeg.LastName} {kijeloltVendeg.FirstName} vendégnek {kijeloltVendeg.RemainingOccasions} alkalma maradt.", "Gym Beléptető", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a beléptetés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                await LoadCustomersAsync();
            }
        }
    }
}


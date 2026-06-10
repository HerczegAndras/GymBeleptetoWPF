using GymBelepteto.Data;
using GymBelepteto.Models;
using GymBelepteto.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GymBelepteto.ViewModels
{
    public partial class CustomerPage : Page
    {
        private readonly GenericRepository<Customer> _customerRepository;
        private readonly GenericRepository<Product> _productRepository;
        private readonly AppDbContext _context;

        public CustomerPage()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _customerRepository = new GenericRepository<Customer>(_context);
            _productRepository = new GenericRepository<Product>(_context);

            Loaded += CustomerPage_Loaded;
            Unloaded += CustomerPage_Unloaded;
        }

        private async void CustomerPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await SeedProductsIfNeeded();
                await LoadCustomers();
                await LoadTicketsToComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt az adatok betöltésekor: {ex.Message}", "Rendszerhiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Javított eseménykezelő név és belső hivatkozás
        private async void TxtCustomerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = TxtCustomerSearch.Text.Trim().ToLower();

            try
            {
                var allCustomers = await _customerRepository.GetAllAsync();

                if (string.IsNullOrEmpty(keyword))
                {
                    CustomersDataGrid.ItemsSource = allCustomers.ToList();
                }
                else
                {
                    var filtered = allCustomers.Where(c =>
                        (c.LastName != null && c.LastName.ToLower().Contains(keyword)) ||
                        (c.FirstName != null && c.FirstName.ToLower().Contains(keyword)) ||
                        (c.PhoneNumber != null && c.PhoneNumber.Contains(keyword))
                    ).ToList();

                    CustomersDataGrid.ItemsSource = filtered;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Keresési hiba: {ex.Message}");
            }
        }

        private async void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            string lastName = TxtLastName.Text.Trim();
            string firstName = TxtFirstName.Text.Trim();
            string phone = TxtPhone.Text.Trim();

            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
            {
                MessageBox.Show("A vezeték- és keresztnév megadása kötelező!", "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtInitialOccasions.Text, out int occasions))
            {
                occasions = 0;
            }

            try
            {
                Customer newCustomer = new Customer
                {
                    LastName = lastName,
                    FirstName = firstName,
                    PhoneNumber = phone,
                    RemainingOccasions = occasions,
                    LoyaltyPoints = 0
                };

                await _customerRepository.AddAsync(newCustomer);
                await _customerRepository.SaveAsync();

                TxtLastName.Clear();
                TxtFirstName.Clear();
                TxtPhone.Clear();
                TxtInitialOccasions.Text = "0";

                await LoadCustomers();
                MessageBox.Show("Vendég sikeresen regisztrálva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a mentés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CustomerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _context?.Dispose();
        }

        private async Task LoadCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            CustomersDataGrid.ItemsSource = customers.ToList();
        }

        private async Task LoadTicketsToComboBox()
        {
            var products = await _productRepository.GetAllAsync();
            var tickets = products.Where(p => p.Category == "Bérlet" && p.IsAvailable).ToList();
            TicketTypeComboBox.ItemsSource = tickets;
        }

        private async Task SeedProductsIfNeeded()
        {
            var products = await _productRepository.GetAllAsync();
            if (!products.Any())
            {
                await _productRepository.AddAsync(new Product { Name = "1 alkalmas jegy", Price = 2000, Category = "Bérlet", StockQuantity = 999, IsAvailable = true });
                await _productRepository.AddAsync(new Product { Name = "10 alkalmas bérlet", Price = 18000, Category = "Bérlet", StockQuantity = 999, IsAvailable = true });
                await _productRepository.AddAsync(new Product { Name = "Havi korlátlan bérlet", Price = 25000, Category = "Bérlet", StockQuantity = 999, IsAvailable = true });

                await _productRepository.SaveAsync();
            }
        }

        private async void VasarlasButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CustomersDataGrid.SelectedItem is Customer kijeloltVendeg))
            {
                MessageBox.Show("Kérlek, válassz ki egy vendéget a listából a vásárláshoz!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(TicketTypeComboBox.SelectedItem is Product kivalasztottBerlet))
            {
                MessageBox.Show("Kérlek, válassz ki egy bérlet típust a legördülő listából!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int pluszAlkalom = GetOccasionsFromProduct(kivalasztottBerlet);

                kijeloltVendeg.RemainingOccasions += pluszAlkalom;
                kijeloltVendeg.LoyaltyPoints += (int)(kivalasztottBerlet.Price / 1000);

                _customerRepository.Update(kijeloltVendeg);
                await _customerRepository.SaveAsync();

                await LoadCustomers();

                MessageBox.Show($"Sikeres vásárlás!\n{kijeloltVendeg.LastName} {kijeloltVendeg.FirstName} megvásárolta a következőt: {kivalasztottBerlet.Name}.\nÚj egyenleg: {kijeloltVendeg.RemainingOccasions} alkalom.", "Sikeres Tranzakció", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a mentés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BeleptetesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(CustomersDataGrid.SelectedItem is Customer kijeloltVendeg))
            {
                MessageBox.Show("Kérlek, válassz ki egy vendéget a listából a beléptetéshez!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (kijeloltVendeg.RemainingOccasions <= 0)
            {
                MessageBox.Show("Lejárt a vendég bérlete! Nincs több alkalma.", "Figyelem!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                kijeloltVendeg.RemainingOccasions--;
                kijeloltVendeg.LoyaltyPoints += 10;

                _customerRepository.Update(kijeloltVendeg);
                await _customerRepository.SaveAsync();

                await LoadCustomers();

                MessageBox.Show($"Sikeres belépés!\n{kijeloltVendeg.LastName} {kijeloltVendeg.FirstName}-nek {kijeloltVendeg.RemainingOccasions} alkalma maradt.", "Gym Manager", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a beléptetés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetOccasionsFromProduct(Product product)
        {
            if (product == null) return 0;
            string nameLower = product.Name.ToLower();

            if (nameLower.Contains("10 alkalmas")) return 10;
            if (nameLower.Contains("1 alkalmas") || nameLower.Contains("jegy")) return 1;
            if (nameLower.Contains("havi") || nameLower.Contains("korlátlan")) return 30;

            return 10;
        }
    }
}
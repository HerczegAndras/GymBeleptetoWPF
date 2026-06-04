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
        private readonly GenericRepository<Product> _productRepository; // ÚJ: Termék/Bérlet repository
        private readonly AppDbContext _context;

        public CustomerPage()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _customerRepository = new GenericRepository<Customer>(_context);
            _productRepository = new GenericRepository<Product>(_context); // ÚJ

            Loaded += CustomerPage_Loaded;
            Unloaded += CustomerPage_Unloaded;
        }

        private async void CustomerPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Teszt adatok generálása bérletekre, ha üres az adatbázis
            await SeedProductsIfNeeded();

            // Adatok betöltése
            await LoadCustomers();
            await LoadTicketsToComboBox();
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

        // ÚJ: Bérletek betöltése a legördülő listába
        private async Task LoadTicketsToComboBox()
        {
            var products = await _productRepository.GetAllAsync();
            // Csak a "Bérlet" kategóriájú dolgokat jelenítjük meg a legördülőben
            var tickets = products.Where(p => p.Category == "Bérlet" && p.IsAvailable).ToList();
            TicketTypeComboBox.ItemsSource = tickets;
        }

        // ÚJ: Teszt bérlet típusok, ha még nincs semmi az adatbázisban
        private async Task SeedProductsIfNeeded()
        {
            var products = await _productRepository.GetAllAsync();
            if (!products.Any())
            {
                // A te repository-d szerint: await ... AddAsync(...)
                await _productRepository.AddAsync(new Product { Name = "1 alkalmas jegy", Price = 2000, Category = "Bérlet", StockQuantity = 999, IsAvailable = true });
                await _productRepository.AddAsync(new Product { Name = "10 alkalmas bérlet", Price = 18000, Category = "Bérlet", StockQuantity = 999, IsAvailable = true });
                await _productRepository.AddAsync(new Product { Name = "Havi korlátlan bérlet", Price = 25000, Category = "Bérlet", StockQuantity = 999, IsAvailable = true });

                // Mivel az SQLite-net-pcl azonnal végrehajtja az Insert-et, 
                // a SaveAsync nálad üres (Task.CompletedTask), de a rend kedvéért bent hagyhatjuk:
                await _productRepository.SaveAsync();
            }
        }

        // ÚJ: BÉRLET VÁSÁRLÁS LOGIKA
        private async void VasarlasButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Ellenőrizzük, ki van-e jelölve vendég
            if (!(CustomersDataGrid.SelectedItem is Customer kijeloltVendeg))
            {
                MessageBox.Show("Kérlek, válassz ki egy vendéget a listából a vásárláshoz!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Ellenőrizzük, ki van-e jelölve bérlet a ComboBox-ban
            if (!(TicketTypeComboBox.SelectedItem is Product kivalasztottBerlet))
            {
                MessageBox.Show("Kérlek, válassz ki egy bérlet típust a legördülő listából!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3. Alkalmak meghatározása a bérlet neve alapján
            int pluszAlkalom = 0;
            if (kivalasztottBerlet.Name.Contains("10 alkalmas")) pluszAlkalom = 10;
            else if (kivalasztottBerlet.Name.Contains("1 alkalmas")) pluszAlkalom = 1;
            else if (kivalasztottBerlet.Name.Contains("Havi korlátlan")) pluszAlkalom = 30; // Korlátlannál pl. fix 30 alkalmat adunk neki egyszerűség kedvéért
            else pluszAlkalom = 10; // Alapértelmezett, ha más lenne

            // 4. Módosítások elvégzése
            kijeloltVendeg.RemainingOccasions += pluszAlkalom;
            // Minden vásárlás után adunk hűségpontot a bérlet ára alapján (minden 1000 Ft után 1 pont)
            kijeloltVendeg.LoyaltyPoints += (int)(kivalasztottBerlet.Price / 1000);

            // 5. Mentés az adatbázisba a Generic Repository-val
            _customerRepository.Update(kijeloltVendeg);
            await _customerRepository.SaveAsync();

            // 6. UI Frissítése
            CustomersDataGrid.ItemsSource = null;
            await LoadCustomers();

            MessageBox.Show($"Sikeres vásárlás!\n{kijeloltVendeg.FirstName} megvásárolta a következőt: {kivalasztottBerlet.Name}.\nÚj egyenleg: {kijeloltVendeg.RemainingOccasions} alkalom.", "Sikeres Tranzakció", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // A régi beléptető gomb kódja változatlanul megmarad
        private async void BeleptetesButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer kijeloltVendeg)
            {
                if (kijeloltVendeg.RemainingOccasions > 0)
                {
                    kijeloltVendeg.RemainingOccasions--;
                    kijeloltVendeg.LoyaltyPoints += 10;

                    _customerRepository.Update(kijeloltVendeg);
                    await _customerRepository.SaveAsync();

                    CustomersDataGrid.ItemsSource = null;
                    await LoadCustomers();

                    MessageBox.Show($"Sikeres belépés!\n{kijeloltVendeg.FirstName}-nek {kijeloltVendeg.RemainingOccasions} alkalma maradt.", "Gym Manager", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Lejárt a vendég bérlete! Nincs több alkalma.", "Figyelem!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Kérlek, válassz ki egy vendéget a listából a beléptetéshez!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
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
    public partial class ProductPage : Page
    {
        private readonly GenericRepository<Product> _productRepository;
        private readonly AppDbContext _context;

        public ProductPage()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _productRepository = new GenericRepository<Product>(_context);

            Loaded += ProductPage_Loaded;
            Unloaded += ProductPage_Unloaded;
        }

        private async void ProductPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private void ProductPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _context?.Dispose();
        }

        private async Task LoadProducts()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                ProductsDataGrid.ItemsSource = products.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a termékek betöltésekor: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 1. ÚJ TERMÉK HOZZÁADÁSA
        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            string name = TxtProductName.Text.Trim();
            string category = (CmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Egyéb";

            if (string.IsNullOrEmpty(name) || !decimal.TryParse(TxtPrice.Text, out decimal price) || !int.TryParse(TxtStock.Text, out int stock))
            {
                MessageBox.Show("Kérlek, töltsd ki megfelelően a mezőket! Az árnak és a készletnek számnak kell lennie.", "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Product newProduct = new Product
                {
                    Name = name,
                    Category = category,
                    Price = price,
                    StockQuantity = stock,
                    IsAvailable = ChkIsAvailable.IsChecked ?? true
                };

                await _productRepository.AddAsync(newProduct);
                await _productRepository.SaveAsync();

                // Mezők ürítése a sikeres felvétel után
                TxtProductName.Clear();
                TxtPrice.Clear();
                TxtStock.Text = "100";

                await LoadProducts();
                MessageBox.Show("Termék sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a mentés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 2. GYORS KÉSZLET NÖVELÉS A KIJELÖLT TERMÉKNÉL
        private async void IncreaseStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is Product selectedProduct)
            {
                try
                {
                    selectedProduct.StockQuantity++;

                    _productRepository.Update(selectedProduct);
                    await _productRepository.SaveAsync();

                    await LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba a frissítés során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Kérlek, válassz ki egy terméket a táblázatból!", "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
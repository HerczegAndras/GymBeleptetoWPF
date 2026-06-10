using GymBelepteto.Data;
using GymBelepteto.Models;
using GymBelepteto.Repositories;
using System;
using System.Linq;
using System.Security.Cryptography; // EZ KELL A HASH-ELÉSHEZ
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GymBelepteto.ViewModels
{
    public partial class LoginWindow : Window
    {
        private readonly GenericRepository<User> _userRepository;
        private readonly AppDbContext _context;

        public LoginWindow()
        {
            InitializeComponent();

            _context = new AppDbContext();
            _userRepository = new GenericRepository<User>(_context);

            Loaded += LoginWindow_Loaded;
        }

        private async void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await SeedAdminUserIfNeeded();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba az adatbázis inicializálásakor: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SeedAdminUserIfNeeded()
        {
            // Létrehozza a User táblát az új modelled alapján, ha még nem létezne
            _context.Connection.CreateTable<User>();

            var users = await _userRepository.GetAllAsync();
            if (!users.Any())
            {
                // Az "admin123" jelszót hash-elve mentjük el az adatbázisba!
                string hashedAdminPassword = HashPassword("admin123");

                await _userRepository.AddAsync(new User
                {
                    Username = "admin",
                    Email = "admin@gymsystem.hu",
                    PasswordHash = hashedAdminPassword, // Biztonságos tárolás
                    FirstName = "Központi",
                    LastName = "Admin",
                    Role = "Admin",
                    IsActive = true
                });
                await _userRepository.SaveAsync();
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Kérlek, töltsd ki mindkét mezőt!", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. A beírt jelszót is le-hash-eljük, mert az adatbázisban is így van megnyerve
                string inputPasswordHash = HashPassword(password);

                // 2. Felhasználó megkeresése (felhasználónév ÉS hash-elt jelszó egyezés)
                var allUsers = await _userRepository.GetAllAsync();
                var user = allUsers.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.PasswordHash == inputPasswordHash &&
                    u.IsActive); // Csak az aktív felhasználók léphetnek be

                if (user != null)
                {
                    // Sikeres belépés -> Főablak megnyitása
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Hibás felhasználónév, jelszó vagy inaktív fiók!", "Sikertelen bejelentkezés", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a bejelentkezés során: {ex.Message}", "Rendszerhiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // SEGÉDMETÓDUS: Szöveges jelszó átalakítása SHA256 HASH formátummá
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // Hexadecimális stringé alakítás
                }
                return builder.ToString();
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
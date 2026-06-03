using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using SQLite;



    namespace GymBelepteto.Models
    {
        public class Customer
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public int UserId { get; set; }
            public string FirstName { get; set; } = string.Empty; // EZ KELL
            public string LastName { get; set; } = string.Empty;  // EZ IS KELL
            public string PhoneNumber { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Country { get; set; } = string.Empty;
            public DateTime BirthDate { get; set; }
            public int LoyaltyPoints { get; set; } = 0;
            public int RemainingOccasions { get; set; } = 0;
        }
    }


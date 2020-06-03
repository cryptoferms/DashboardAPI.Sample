using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Threading.Tasks;

namespace DashboardAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; }
    }
}

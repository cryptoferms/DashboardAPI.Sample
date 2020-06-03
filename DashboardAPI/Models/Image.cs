using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardAPI.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int VehicleId { get; set; }
        [NotMapped]
        public byte[] ImageArray { get; set; }
        public ICollection<Image> Images { get; set; }
    }
}

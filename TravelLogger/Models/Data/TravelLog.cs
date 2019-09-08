using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TravelLogger.Models.Data
{
    public class TravelLog
    {
        public int Id { get; set; }

        [Required]
        public IdentityUser User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime TimeOfDeparture { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? TimeOfArrival { get; set; }

        public string Comment { get; set; }
    }
}

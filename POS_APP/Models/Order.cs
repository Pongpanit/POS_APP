using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace POS_APP.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

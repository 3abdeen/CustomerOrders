using CustomerOrder.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomerOrder.Infrastructure.Models
{
    public class Order : ModelBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderNo { get; set; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Missing Customer id")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public double TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string CustomerId { get; set; }
        [JsonIgnore]
        public virtual Customer Customer { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
    }
}

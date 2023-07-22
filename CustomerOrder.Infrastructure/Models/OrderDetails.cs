using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomerOrder.Infrastructure.Models
{
    public class OrderDetails:ModelBase
    {
        public OrderDetails() { }

        [Required(AllowEmptyStrings =false,ErrorMessage ="Missing Order Id")]
        public Guid OrderId { get; set; }
        [JsonIgnore]
        public virtual Order Order { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Missing Product Id")]
        public Guid ProductId { get; set; }
        [JsonIgnore]
        public virtual Product Product { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Missing Product name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
    }
}

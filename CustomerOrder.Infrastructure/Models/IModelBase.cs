using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerOrder.Infrastructure.Models
{
    public interface IModelBase
    {
        public Guid Id { get; set; }
    }
}

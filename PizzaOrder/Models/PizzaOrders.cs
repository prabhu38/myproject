using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaOrder.Models
{
    public class PizzaOrders
    {
        public string UserName { get; set; }
        public string OrderCode { get; set; }
        public IEnumerable<Pizza> OrderItems { get; set; }
    }
}

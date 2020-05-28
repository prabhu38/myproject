using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PizzaOrder.Services;
using PizzaOrder.Models;
using Microsoft.Extensions.Configuration;

namespace PizzaOrder.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PizzaOrderController : ControllerBase
    {
        private IConfiguration Configuration { get; set; }
        public PizzaOrderController(IConfiguration config)
        {
            Configuration = config;
        }

        [HttpGet]
        public IEnumerable<Pizza> GetAllPizza()
        {
            PizzaService service = new PizzaService(Configuration);
            return service.GetPizza();
        }

        [HttpGet("{username}")]
        public IEnumerable<Pizza> GetOrderItems(string username)
        {
            PizzaService service = new PizzaService(Configuration);
            return service.GetOrders(username);
        }

        [HttpPost]
        public void CreateOrder([FromBody] PizzaOrders orders)
        {
            PizzaService service = new PizzaService(Configuration);
            service.CreateOrder(orders);
        }

        [HttpPut]
        public void UpdateOrder([FromBody] PizzaOrders orders)
        {
            PizzaService service = new PizzaService(Configuration);
            service.UpdateOrder(orders);
        }

        [HttpDelete("{orderCode}")]
        public void DeleteOrder(string orderCode)
        {
            PizzaService service = new PizzaService(Configuration);
            service.DeleteOrder(orderCode);
        }
    }
}
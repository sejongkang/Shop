using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyShop.WebUI.Controllers
{
    [Authorize (Roles = "Admin")]
    public class OrderManagerController : Controller
    {
        IOrderService orderService;
        // GET: OrderManager
        public OrderManagerController(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        public ActionResult Index()
        {
            List<Order> orders = orderService.GetOrders();
            return View(orders);
        }
        public ActionResult UpdateOrder(string Id)
        {
            ViewBag.StatusList = new List<String>()
            {
                "Order Created",
                "Payment Processed",
                "Order Shipped",
                "Order Complete"
            };
            Order order = orderService.GetOrder(Id);
            return View(order);
        }
        [HttpPost]
        public ActionResult UpdateOrder(Order updatedOrder, string Id)
        {
            Order order = orderService.GetOrder(Id);
            order.OrderStatus = updatedOrder.OrderStatus;
            orderService.UpdateOrder(order);
            return RedirectToAction("Index");
        }

    }
}
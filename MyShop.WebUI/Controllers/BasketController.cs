using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyShop.WebUI.Controllers
{
    public class BasketController : Controller
    {
        IBasketService basketService;
        IOrderService orderService;
        // 생성자에서 받는 IBasketService는 Unitiy Service에서 IBasketService와 BasketService 결합해서 제공
        public BasketController(IBasketService BasketService, IOrderService OrderService)
        {
            this.basketService = BasketService;
            this.orderService = OrderService;
        }
        public ActionResult Index()
        {
            // BasketService에서 BasketItem 받아와서 리스트 뷰 출력
            var model = basketService.GetBasketItem(this.HttpContext);
            return View(model);
        }
        // Id에 해당하는 Product를 Basket에 추가
        public ActionResult AddToBasket(string Id)
        {
            // Product ID로 조회해서 BasketService에 추가하는 메소드 실행
            // this.HttpContext는 싱글톤으로서 Get가능
            basketService.AddToBasket(this.HttpContext, Id);
            return RedirectToAction("Index");
        }
        public ActionResult RemoveFromBasket(string Id)
        {
            basketService.RemoveFromBasket(this.HttpContext, Id);
            return RedirectToAction("Index");
        }
        public PartialViewResult BasketSummary()
        {
            var basketSummary = basketService.GetBasketSummary(this.HttpContext);
            return PartialView(basketSummary);
        }
        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Checkout(Order order)
        {
            var basketItems = basketService.GetBasketItem(this.HttpContext);
            order.OrderStatus = "Order Created";
            //process payment

            order.OrderStatus = "Payment Processed";
            orderService.CreateOrder(order, basketItems);
            basketService.ClearBasket(this.HttpContext);

            return RedirectToAction("Thankyou", new { OrderId = order.Id });
        }
        public ActionResult Thankyou(string OrderId)
        {
            ViewBag.OrderId = OrderId;
            return View();
        }
    }
}
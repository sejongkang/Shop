using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
    // 바스켓 컨트롤러에서 수행할 비즈니스 로직을 수행하는 서비스?
    // 인터페이스가 왜필요? - DI 의존성주입 이겠지? - 중복 코딩 해결. 수정 용이.
    public class BasketService : IBasketService
    {
        // DB의 Product 테이블에 대한 SQL을 수행하는 객체
        IRepository<Product> productContext;
        IRepository<Basket> basketContext;

        // 세션 이름
        public const string BasketSessionName = "eCommerceBasket";

        public BasketService(IRepository<Product> productContext, IRepository<Basket> basketContext)
        {
            this.basketContext = basketContext;
            this.productContext = productContext;
        }
        /// <summary>
        /// 바스켓 서비스에서만 사용됨 - 다른 곳에서 호출안됨
        /// 장바구니 목록 불러오기
        /// 쿠키 Get -> 바스켓 Id Get -> 바스켓 Get
        /// </summary>
        /// <param name="httpContext">
        /// 싱글톤으로 사용되는 현재 Context Get
        /// </param>
        /// <param name="createIfNull">
        /// 없을 시 생성하는 플래그?
        /// </param>
        /// <returns></returns>
        private Basket GetBasket(HttpContextBase httpContext, bool createIfNull)
        {
            // 클라이언트의 httpContext에 저장된 BasketSessionName 쿠키 Get
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);
            // 부를때 마다 새로운 바스켓 생성?
            Basket basket = new Basket();
            // 바스켓 쿠키가 있으면
            if (cookie != null)
            {
                // 쿠키에 저장된 바스켓 테이블의 Id Get
                // 왜 User로 사용 안할까? - 비회원의 바스켓 시 필요하구나
                string basketId = cookie.Value;
                // 바스켓 Id가 있으면
                
                if (!string.IsNullOrEmpty(basketId))
                {
                    // DB에서 바스켓 로드
                    basket = basketContext.Find(basketId);
                }
                // 쿠키는 있고 바스켓은 없을 시
                // 쿠키가 있으면 바스켓 Id도 있는거 아닌가?
                else
                {
                    // 생성하는 플래그이면
                    if (createIfNull)
                    {
                        // 새로운 바스켓 생성
                        basket = CreateNewBasket(httpContext);
                    }
                }
            }
            // 바스켓 쿠키가 없으면
            // 쿠키 만료?
            else
            {
                // 바스켓 쿠키가 없을 때 생성하는 플래그면
                if (createIfNull)
                {
                    // 현재 컨텍스트의 새로운 바스켓 생성
                    basket = CreateNewBasket(httpContext);
                }
            }
            return basket;
        }
        /// <summary>
        /// 새로운 바스켓을 생성해서 사용자 쿠키와 서버의 DB에 다 저장함
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        private Basket CreateNewBasket(HttpContextBase httpContext)
        {
            // 새 바스켓 생성
            Basket basket = new Basket();
            // 바스켓 테이블에 바스켓 추가
            basketContext.Insert(basket);

            // 바스켓 쿠키 생성
            // BasketSessionName이 Key 인거 같음
            HttpCookie cookie = new HttpCookie(BasketSessionName);
            // 바스켓 쿠키의 값은 바스켓의 아이디
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            // 클라이언트의 httpContext에 바스켓 쿠키 추가
            httpContext.Response.Cookies.Add(cookie);
            // 바스켓이 회원별로 DB에 저장되지 않고 하루짜리 서버의 쿠키로 생성됨
            // 서버 껏다 키면 쿠키 다 사라짐
            // DB에는 있음
            return basket;
        }
        public void AddToBasket(HttpContextBase httpContext, string productId)
        {
            // 현재 컨텍스트(클라이언트)의 바스켓 Get, 없으면 만들어라
            Basket basket = GetBasket(httpContext, true);
            // 바스켓에 있는 Products 중에서 해당 Product Id랑 일치하는 Product Get
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.ProductId == productId);
            // 바스켓에 해당 Product가 없으면
            if (item == null)
            {
                item = new BasketItem()
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quanity = 1
                };
                basket.BasketItems.Add(item);
            }
            // 바스켓에 있는 Product의 수 증가
            else
            {
                item.Quanity = item.Quanity + 1;
            }
            // 바스켓 아이템 수정됐으니 바스켓 테이블 커밋
            // 외래키로 바스켓 아이템 테이블이 자동 수정?
            basketContext.Commit();
        }
        public void RemoveFromBasket(HttpContextBase httpContext, string itemId)
        {
            Basket basket = GetBasket(httpContext, true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i => i.Id == itemId);
            if (itemId != null)
            {
                basket.BasketItems.Remove(item);
                basketContext.Commit();
            }
        }
        public List<BasketItemViewModel> GetBasketItem(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            if (basket != null)
            {
                // select가 구문 맨 뒤로 가는구나
                // select는 list로 반환
                var result = (from b in basket.BasketItems
                              join p in productContext.Collection() on b.ProductId equals p.Id
                              select new BasketItemViewModel()
                              {
                                  Id = b.Id,
                                  Quantity = b.Quanity,
                                  ProductName = p.Name,
                                  Image = p.Image,
                                  Price = p.Price
                              }).ToList();
                return result;
            }
            else
                return new List<BasketItemViewModel>();
        }
        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext, false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0, 0);
            if (basket != null)
            {
                // ?는 Null 가능
                int? basketCount = (from item in basket.BasketItems
                                    select item.Quanity).Sum();
                decimal? basketTotal = (from item in basket.BasketItems
                                        join p in productContext.Collection() on item.ProductId equals p.Id
                                        select item.Quanity * p.Price).Sum();
                // ??는 널이 아니면 앞의 값 대입
                model.BasketCount = basketCount ?? 0;
                model.BasketTotal = basketTotal ?? decimal.Zero;

                return model;
            }
            else
            {
                return model;
            }
        }
    }
}

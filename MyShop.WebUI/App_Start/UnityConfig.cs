using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.DataAccess.InMemory;
using MyShop.DataAccess.SQL;
using MyShop.Services;
using System;

using Unity;

namespace MyShop.WebUI
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>
        /// There is no need to register concrete types such as controllers or
        /// API controllers (unless you want to change the defaults), as Unity
        /// allows resolving a concrete type even if it was not previously
        /// registered.
        /// </remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below.
            // Make sure to add a Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your type's mappings here.
            // container.RegisterType<IProductRepository, ProductRepository>();
            
            // 인터페이스를 해당 클래스로 맵핑?(용어 치환 느낌?)
            // 인터페이스(공통 규칙)을 지키는 클래스인 경우 유지보수가 쉬움, 코드 중복 없음
            // 인터페이스 구현하는 클래스가 바뀔 경우 여기서만 수정하면 됨
            // MSSQL 쓰거나 Oracle, MySQL 등 바껴도 컨트롤러 말고 여기만 변경.
            container.RegisterType<IRepository<Product>, SQLRepository<Product>>();
            container.RegisterType<IRepository<ProductCategory>, SQLRepository<ProductCategory>>();
            container.RegisterType<IRepository<BasketItem>, SQLRepository<BasketItem>>();
            container.RegisterType<IRepository<Basket>, SQLRepository<Basket>>();
            container.RegisterType<IRepository<Customer>, SQLRepository<Customer>>();
            container.RegisterType<IRepository<Order>, SQLRepository<Order>>();

            container.RegisterType<IBasketService, BasketService>();
            container.RegisterType<IOrderService, OrderService>();
        }
    }
}
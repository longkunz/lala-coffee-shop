using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Unity;
using Unity.Lifetime;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;
using LalaCoffee.Services.Payment;

namespace LalaCoffee
{
    public class UnityDependencyResolver : IDependencyResolver
    {
        private readonly IUnityContainer _container;

        public UnityDependencyResolver(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null) return null;

            try
            {
                if (!_container.IsRegistered(serviceType) && (serviceType.IsAbstract || serviceType.IsInterface))
                {
                    return null;
                }
                return _container.Resolve(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (serviceType == null) return Enumerable.Empty<object>();

            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch
            {
                return Enumerable.Empty<object>();
            }
        }
    }

    public static class UnityConfig
    {
        private static Lazy<IUnityContainer> _container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        public static IUnityContainer Container => _container.Value;

        public static void RegisterComponents()
        {
            var container = Container;
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            // Register DbContext
            container.RegisterType<ApplicationDbContext>(new HierarchicalLifetimeManager());

            // Register Repositories
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));

            // Register Domain Services
            container.RegisterType<IEmailService, EmailService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICartService, CartService>(new HierarchicalLifetimeManager());
            container.RegisterType<IOrderService, OrderService>(new HierarchicalLifetimeManager());
            container.RegisterType<IPaymentService, PaymentService>(new ContainerControlledLifetimeManager());
        }

        public static void ResetContainer()
        {
            _container = new Lazy<IUnityContainer>(() =>
            {
                var container = new UnityContainer();
                RegisterTypes(container);
                return container;
            });
        }
    }
}

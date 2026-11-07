using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Unity;
using Unity.Lifetime;
using LalaCoffee.Data;
using LalaCoffee.Models;
using LalaCoffee.Services;
using LalaCoffee.Services.Email;

namespace LalaCoffee
{
    public class UnityDependencyResolver : IDependencyResolver, IDisposable
    {
        private const string PerRequestContainerKey = "PerRequestUnityChildContainer";
        private readonly IUnityContainer _container;
        private bool _disposed;

        public UnityDependencyResolver(IUnityContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IUnityContainer Container => _container;

        public IUnityContainer CurrentContainer
        {
            get
            {
                var httpContext = HttpContext.Current;
                if (httpContext == null)
                {
                    return _container;
                }

                if (httpContext.Items[PerRequestContainerKey] is IUnityContainer existingChild)
                {
                    return existingChild;
                }

                var childContainer = _container.CreateChildContainer();
                httpContext.Items[PerRequestContainerKey] = childContainer;

                try
                {
                    httpContext.AddOnRequestCompleted(ctx =>
                    {
                        DisposePerRequestContainer(ctx);
                    });
                }
                catch
                {
                    // Fall back to Application_EndRequest in unmocked/test environments
                }

                return childContainer;
            }
        }

        public static void DisposePerRequestContainer(HttpContext context)
        {
            if (context?.Items[PerRequestContainerKey] is IUnityContainer child)
            {
                context.Items.Remove(PerRequestContainerKey);
                child.Dispose();
            }
        }

        public static void DisposePerRequestContainer(HttpContextBase context)
        {
            if (context?.Items[PerRequestContainerKey] is IUnityContainer child)
            {
                context.Items.Remove(PerRequestContainerKey);
                child.Dispose();
            }
        }

        public IUnityContainer CreateChildContainer()
        {
            return _container.CreateChildContainer();
        }

        public UnityDependencyResolver CreateChildScope()
        {
            return new UnityDependencyResolver(_container.CreateChildContainer());
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null) return null;

            try
            {
                var container = CurrentContainer;
                if (!container.IsRegistered(serviceType) && (serviceType.IsAbstract || serviceType.IsInterface))
                {
                    return null;
                }
                return container.Resolve(serviceType);
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
                return CurrentContainer.ResolveAll(serviceType);
            }
            catch
            {
                return Enumerable.Empty<object>();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _container?.Dispose();
                }
                _disposed = true;
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

            // Register DbContext with HierarchicalLifetimeManager for per-request/child-container disposal
            container.RegisterType<ApplicationDbContext>(new HierarchicalLifetimeManager());

            // Register Repositories
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));

            // Register Domain Services
            container.RegisterType<IEmailService, EmailService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICartService, CartService>(new HierarchicalLifetimeManager());
            container.RegisterType<IOrderService, OrderService>(new HierarchicalLifetimeManager());
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

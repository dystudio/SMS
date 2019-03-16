using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using Ninject;
using System.Web.Http.Filters;
using System.Web.Routing;

namespace Sms.WebAdmin.App_Start
{
    public class MvcDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        private IKernel _ikernel;

        public MvcDependencyResolver(IKernel ikernel)
        {
            this._ikernel = ikernel;
        }


        #region IDependencyResolver Members

        public object GetService(Type serviceType)
        {
            try
            {
                return _ikernel.TryGet(serviceType);
            }
            catch (ActivationException ex)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _ikernel.GetAll(serviceType);
            }
            catch (ActivationException ex)
            {
                return Enumerable.Empty<object>();
            }
        }

        #endregion
    }




    public class WebApiDependencyResolver : WebApiDependencyScope, IDependencyResolver
    {
        private readonly IKernel _kernel;

        public WebApiDependencyResolver(IKernel kernel) : base(kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException(nameof(kernel));
            }
            this._kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new WebApiDependencyScope(_kernel);
        }
    }

    public class WebApiDependencyScope : IDependencyScope
    {
        private IKernel _ikernel;

        internal WebApiDependencyScope(IKernel resolver)
        {
            Contract.Assert(resolver != null);
            this._ikernel = resolver;
        }
        public void Dispose()
        {
            _ikernel = null;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return _ikernel.TryGet(serviceType);
            }
            catch (ActivationException ex)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _ikernel.GetAll(serviceType);
            }
            catch (ActivationException ex)
            {
                return Enumerable.Empty<object>();
            }
        }
    }

    public class WebApiActionFilterProvider : ActionDescriptorFilterProvider, IFilterProvider
    {
        private readonly IKernel _kernel;

        public WebApiActionFilterProvider(IKernel container)
        {
            this._kernel = container;
        }

        public new IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(configuration, actionDescriptor);
            foreach (var filter in filters)
            {
                _kernel.Inject(filter.Instance);
            }
            return filters;
        }
    }
}
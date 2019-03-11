using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sms.IRepository;
using Sms.Repository;
using System.Web.Http;
using System.Web.Mvc;

namespace Sms.WebAdmin.App_Start
{
    public class NinjectRegister
    {
        public static void Register(HttpConfiguration config)
        {
            Ninject.IKernel kernel = new Ninject.StandardKernel();
            //允许私有属性注入
            kernel.Settings.InjectNonPublic = true;

            //service binding
            kernel.Bind<IRepositoryFactory>().To<RepositoryFactory>();

            //mvc inject
            DependencyResolver.SetResolver(new MvcDependencyResolver(kernel));
            //webapi inject
            config.DependencyResolver = new WebApiDependencyResolver(kernel);

        }


        //private static readonly IKernel _kernel;

        //static NinjectRegister()
        //{
        //    //创建容器对象
        //    _kernel = new StandardKernel();
        //    //允许私有属性注入
        //    _kernel.Settings.InjectNonPublic = true;
        //}

        ///// <summary>
        ///// MVC项目的注册
        ///// </summary>
        //public static void RegisterForMvc()
        //{
        //    System.Web.Mvc.DependencyResolver.SetResolver(new DependencyResolver(_kernel));

        //    //binding service
        //    _kernel.Bind<IRepositoryFactory>().To<RepositoryFactory>();
        //}
    }
}
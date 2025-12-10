using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Concrete;
using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.Security.Jwt;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Http;

namespace Business.DependencyResolvers.Autofac;
public class AutofacBusinessModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<UserManager>().As<IUserService>().SingleInstance();
        builder.RegisterType<EfUserDal>().As<IUserDal>().SingleInstance();

        builder.RegisterType<OperationClaimManager>().As<IOperationClaimService>().SingleInstance();
        builder.RegisterType<EfOperationClaimDal>().As<IOperationClaimDal>().SingleInstance();

        builder.RegisterType<UserOperationClaimManager>().As<IUserOperationClaimService>().SingleInstance();
        builder.RegisterType<EfUserOperationClaimDal>().As<IUserOperationClaimDal>().SingleInstance();

        builder.RegisterType<MenuManager>().As<IMenuService>().SingleInstance();
        builder.RegisterType<EfMenuDal>().As<IMenuDal>().SingleInstance();

        builder.RegisterType<MenuOperationClaimManager>().As<IMenuOperationClaimService>().SingleInstance();
        builder.RegisterType<EfMenuOperationClaimDal>().As<IMenuOperationClaimDal>().SingleInstance();

        builder.RegisterType<EfRefreshTokenDal>().As<IRefreshTokenDal>().SingleInstance();

        builder.RegisterType<AuthManager>().As<IAuthService>().SingleInstance();
        builder.RegisterType<JwtHelper>().As<ITokenHelper>().SingleInstance();

        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
            .EnableInterfaceInterceptors(new ProxyGenerationOptions()
            {
                Selector = new AspectInterceptorSelector()
            }).SingleInstance();
    }
}

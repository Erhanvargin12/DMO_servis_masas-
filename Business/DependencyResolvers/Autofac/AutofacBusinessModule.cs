using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Abstract.TicketServices;
using Business.Concrete;
using Business.Concrete.TicketManagers;
using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.Security.Jwt;
using DataAccess.Abstract;
using DataAccess.Abstract.TicketDal;
using DataAccess.Concrete.EntityFramework;
using DataAccess.Concrete.EntityFramework.EfTicketDal;
using Microsoft.AspNetCore.Http;

namespace Business.DependencyResolvers.Autofac;
public class AutofacBusinessModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // System
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


        // Ticket 
        builder.RegisterType<TicketAssignmentManager>().As<ITicketAssignmentService>().SingleInstance();
        builder.RegisterType<EfTicketAssignmentDal>().As<ITicketAssignmentDal>().SingleInstance();

        builder.RegisterType<TicketAttachmentManager>().As<ITicketAttachmentService>().SingleInstance();
        builder.RegisterType<EfTicketAttachmentDal>().As<ITicketAttachmentDal>().SingleInstance();

        builder.RegisterType<TicketResponseManager>().As<ITicketResponseService>().SingleInstance();
        builder.RegisterType<EfTicketResponseDal>().As<ITicketResponseDal>().SingleInstance();

        builder.RegisterType<TicketManager>().As<ITicketService>().SingleInstance();
        builder.RegisterType<EfTicketDal>().As<ITicketDal>().SingleInstance();

        builder.RegisterType<TicketTransferManager>().As<ITicketTransferService>().SingleInstance();
        builder.RegisterType<EfTicketTransferDal>().As<ITicketTransferDal>().SingleInstance();

        
        // Assembly
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
            .EnableInterfaceInterceptors(new ProxyGenerationOptions()
            {
                Selector = new AspectInterceptorSelector()
            }).SingleInstance();
    }
}

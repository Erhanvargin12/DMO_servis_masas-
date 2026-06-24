using Autofac;
using Autofac.Core;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Abstract.TicketServices;
using Business.Concrete;
using Business.Concrete.TicketManagers;
using Castle.DynamicProxy;
using Core.Communication;
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
        builder.RegisterType<UserManager>().As<IUserService>().InstancePerLifetimeScope();
        builder.RegisterType<EfUserDal>().As<IUserDal>().InstancePerLifetimeScope();

        builder.RegisterType<OperationClaimManager>().As<IOperationClaimService>().InstancePerLifetimeScope();
        builder.RegisterType<EfOperationClaimDal>().As<IOperationClaimDal>().InstancePerLifetimeScope();

        builder.RegisterType<UserOperationClaimManager>().As<IUserOperationClaimService>().InstancePerLifetimeScope();
        builder.RegisterType<EfUserOperationClaimDal>().As<IUserOperationClaimDal>().InstancePerLifetimeScope();

        builder.RegisterType<MenuManager>().As<IMenuService>().InstancePerLifetimeScope();
        builder.RegisterType<EfMenuDal>().As<IMenuDal>().InstancePerLifetimeScope();

        builder.RegisterType<MenuOperationClaimManager>().As<IMenuOperationClaimService>().InstancePerLifetimeScope();
        builder.RegisterType<EfMenuOperationClaimDal>().As<IMenuOperationClaimDal>().InstancePerLifetimeScope();

        builder.RegisterType<EfRefreshTokenDal>().As<IRefreshTokenDal>().InstancePerLifetimeScope();

        builder.RegisterType<AuthManager>().As<IAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<JwtHelper>().As<ITokenHelper>().SingleInstance();

        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();

        builder.RegisterType<HttpServiceCommunication>().As<IServiceCommunication>().InstancePerLifetimeScope();

        // Ticket 
        builder.RegisterType<TicketAssignmentManager>().As<ITicketAssignmentService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketAssignmentDal>().As<ITicketAssignmentDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketAttachmentManager>().As<ITicketAttachmentService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketAttachmentDal>().As<ITicketAttachmentDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketResponseManager>().As<ITicketResponseService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketResponseDal>().As<ITicketResponseDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketManager>().As<ITicketService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketDal>().As<ITicketDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketTransferManager>().As<ITicketTransferService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketTransferDal>().As<ITicketTransferDal>().InstancePerLifetimeScope();

        
        // Assembly
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
            .EnableInterfaceInterceptors(new ProxyGenerationOptions()
            {
                Selector = new AspectInterceptorSelector()
            }).InstancePerLifetimeScope();
    }
}

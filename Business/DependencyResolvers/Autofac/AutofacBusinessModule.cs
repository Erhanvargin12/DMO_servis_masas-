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
using Business.Integrations;
using Business.Abstract.IntegrationServices;

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

        builder.RegisterType<BranchManager>().As<IBranchService>().InstancePerLifetimeScope();
        builder.RegisterType<EfBranchDal>().As<IBranchDal>().InstancePerLifetimeScope();

        builder.RegisterType<CompanyManager>().As<ICompanyService>().InstancePerLifetimeScope();
        builder.RegisterType<EfCompanyDal>().As<ICompanyDal>().InstancePerLifetimeScope();

        builder.RegisterType<AuthManager>().As<IAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<JwtHelper>().As<ITokenHelper>().SingleInstance();

        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();

        builder.RegisterType<HttpServiceCommunication>().As<IServiceCommunication>().InstancePerLifetimeScope();

        // Integrations
        builder.RegisterType<DmoAuthClient>().As<IDmoAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<DmoMainClient>().As<IDmoMainService>().InstancePerLifetimeScope();
        builder.RegisterType<DmoEbysClient>().As<IDmoEbysService>().InstancePerLifetimeScope();

        // AI Services
        builder.RegisterType<GeminiTicketAiManager>().As<ITicketAiService>().InstancePerLifetimeScope();

        // Ticket 
        builder.RegisterType<TicketAssignmentManager>().As<ITicketAssignmentService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketAssignmentDal>().As<ITicketAssignmentDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketAttachmentManager>().As<ITicketAttachmentService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketAttachmentDal>().As<ITicketAttachmentDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketResponseManager>().As<ITicketResponseService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketResponseDal>().As<ITicketResponseDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketManager>().As<ITicketService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketDal>().As<ITicketDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketRoutingService>().As<ITicketRoutingService>().InstancePerLifetimeScope();

        builder.RegisterType<TicketTransferManager>().As<ITicketTransferService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketTransferDal>().As<ITicketTransferDal>().InstancePerLifetimeScope();

        builder.RegisterType<TicketCategoryManager>().As<ITicketCategoryService>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketCategoryDal>().As<ITicketCategoryDal>().InstancePerLifetimeScope();
        builder.RegisterType<EfTicketCategoryBranchDal>().As<ITicketCategoryBranchDal>().InstancePerLifetimeScope();

        
        // Assembly
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
            .EnableInterfaceInterceptors(new ProxyGenerationOptions()
            {
                Selector = new AspectInterceptorSelector()
            }).InstancePerLifetimeScope();
    }
}

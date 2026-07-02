using Business.Abstract.IntegrationServices;
using Core.Communication;

namespace Business.Integrations;

public class DmoEbysClient : IDmoEbysService
{
    private readonly IServiceCommunication _serviceCommunication;

    public DmoEbysClient(IServiceCommunication serviceCommunication)
    {
        _serviceCommunication = serviceCommunication;
    }
}
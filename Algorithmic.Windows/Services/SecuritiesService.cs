using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI;

namespace ShareInvest.Services;

class SecuritiesService
{
    internal SecuritiesService(string id)
    {
        this.id = id;
    }
    internal ISecuritiesMapper<AxMessageEventArgs> GetSecurities()
    {
        return new AxKH(id);
    }
    readonly string id;
}
using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI;

namespace ShareInvest.Services
{
    class SecuritiesService
    {
        public SecuritiesService(string id)
        {
            this.id = id;
        }
        internal ISecuritiesMapper<MessageEventArgs> GetSecurities()
        {
            return new AxKH(id);
        }
        readonly string id;
    }
}
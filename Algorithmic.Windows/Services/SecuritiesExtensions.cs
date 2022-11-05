using ShareInvest.Mappers;
using ShareInvest.Observer;

namespace ShareInvest.Services;

static class SecuritiesExtensions
{
    public static ISecuritiesMapper<MessageEventArgs> ConfigureServices<T>(T param)
    {
        return param switch
        {
            _ => new AxKH()
        };
    }
}
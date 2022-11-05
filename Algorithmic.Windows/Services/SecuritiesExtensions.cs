using ShareInvest.Mappers;
using ShareInvest.Observers;

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
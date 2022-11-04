using AxKHOpenAPILib;

using ShareInvest.Infrastructure;

namespace ShareInvest;

abstract class TR
{
    internal abstract void OnReceiveTrData(ICoreClient api,
                                           AxKHOpenAPI ax,
                                           _DKHOpenAPIEvents_OnReceiveTrDataEvent param,
                                           Models.OpenAPI.TR? tr);
}
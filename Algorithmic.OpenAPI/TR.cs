using AxKHOpenAPILib;

namespace ShareInvest;

abstract class TR
{
    internal abstract IEnumerable<string> OnReceiveTrData(AxKHOpenAPI ax,
                                                          _DKHOpenAPIEvents_OnReceiveTrDataEvent param,
                                                          Models.OpenAPI.TR? tr);
}
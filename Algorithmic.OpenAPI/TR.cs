using AxKHOpenAPILib;

using System.Globalization;

namespace ShareInvest;

abstract class TR
{
    internal abstract IEnumerable<string> OnReceiveTrData(AxKHOpenAPI ax,
                                                          _DKHOpenAPIEvents_OnReceiveTrDataEvent e,
                                                          Models.OpenAPI.TR? tr);
    protected internal static CultureInfo Culture
    {
        get => new("ko-KR");
    }
}
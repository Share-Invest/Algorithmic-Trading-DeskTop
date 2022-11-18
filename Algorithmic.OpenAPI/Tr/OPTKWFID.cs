using AxKHOpenAPILib;

using Newtonsoft.Json;

using ShareInvest.Properties;

using System.Text;

namespace ShareInvest.Tr;

class OPTKWFID : TR
{
    internal static IEnumerable<Models.OpenAPI.TR> GetListOfStocks(IEnumerable<string> codeListByMarket)
    {
        int index = 0;
        var sb = new StringBuilder(0x100);
        var codeStack = new Stack<StringBuilder>(0x10);

        foreach (var code in codeListByMarket)
            if (string.IsNullOrEmpty(code) is false)
            {
                if (index++ % 0x63 == 0x62)
                {
                    codeStack.Push(sb.Append(code));

                    sb = new StringBuilder();
                }
                sb.Append(code).Append(';');
            }
        codeStack.Push(sb.Remove(sb.Length - 1, 1));

        while (codeStack.TryPop(out StringBuilder? pop))
            if (pop is not null && pop.Length > 5)
            {
                var listOfStocks = pop.ToString();

                yield return new Models.OpenAPI.Request.OPTKWFID
                {
                    Value = new[]
                    {
                        listOfStocks
                    },
                    PrevNext = listOfStocks.Split(';').Length
                };
            }
    }
    internal override IEnumerable<string> OnReceiveTrData(AxKHOpenAPI ax,
                                                          _DKHOpenAPIEvents_OnReceiveTrDataEvent e,
                                                          Models.OpenAPI.TR? tr)
    {
        if (tr?.Multiple is not null)

            for (int i = 0; i < ax?.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
            {
                var dic = new Dictionary<string, string>();

                for (int j = 0; j < tr.Multiple.Length; j++)
                    dic[tr.Multiple[j]] = ax.GetCommData(e.sTrCode, e.sRQName, i, tr.Multiple[j]).Trim();

                string code = dic[tr.Multiple[0]],
                       state = nameof(Models.OpenAPI.Response.OPTKWFID.State),
                       constructionSupervision = nameof(Models.OpenAPI.Response.OPTKWFID.ConstructionSupervision),
                       investmentCaution = nameof(Models.OpenAPI.Response.OPTKWFID.InvestmentCaution),
                       listingDate = nameof(Models.OpenAPI.Response.OPTKWFID.ListingDate);

                dic[state] = ax.GetMasterStockState(code);
                dic[tr.Multiple[0x24]] = ax.KOA_Functions(Resources.EX, code);
                dic[investmentCaution] = ax.KOA_Functions(Resources.WARNING, code);
                dic[listingDate] = ax.GetMasterListedStockDate(code);
                dic[constructionSupervision] = ax.KOA_Functions(Resources.INFO, code)
                                                 .Replace(';', '+');

                if (dic.Count > 0)

                    yield return JsonConvert.SerializeObject(dic);
            }
    }
}
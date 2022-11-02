using AxKHOpenAPILib;

using Newtonsoft.Json;

namespace ShareInvest.Tr;

class OPTKWFID : TR
{
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

                var code = dic[tr.Multiple[0]];
                string state = nameof(Models.OpenAPI.Response.OPTKWFID.State),
                       constructionSupervision = nameof(Models.OpenAPI.Response.OPTKWFID.ConstructionSupervision),
                       investmentCaution = nameof(Models.OpenAPI.Response.OPTKWFID.InvestmentCaution),
                       listingDate = nameof(Models.OpenAPI.Response.OPTKWFID.ListingDate);

                dic[state] = ax.GetMasterStockState(code);
                dic[tr.Multiple[0x24]] = ax.KOA_Functions(cnt, code);
                dic[investmentCaution] = ax.KOA_Functions(warning, code);
                dic[listingDate] = ax.GetMasterListedStockDate(code);
                dic[constructionSupervision] = ax.KOA_Functions(info, code)
                                                 .Replace(';', '+');

                yield return JsonConvert.SerializeObject(dic, Formatting.Indented);
            }
    }
    const string info = "GetMasterStockInfo";
    const string cnt = "GetMasterListedStockCntEx";
    const string warning = "IsOrderWarningStock";
}
using AxKHOpenAPILib;

using Newtonsoft.Json;

using ShareInvest.Models.OpenAPI.Response;

namespace ShareInvest.Tr;

class OPW00005 : TR
{
    internal override IEnumerable<string> OnReceiveTrData(AxKHOpenAPI ax,
                                                          _DKHOpenAPIEvents_OnReceiveTrDataEvent e,
                                                          Models.OpenAPI.TR? tr)
    {
        if (tr?.Value is not null)
        {
            Dictionary<string, string> dic;

            if (tr?.Single is not null)
            {
                dic = new Dictionary<string, string>
                {
                    {
                        tr.Id[0],
                        tr.Value[0]
                    }
                };
                for (int i = 0; i < tr.Single.Length; i++)
                {
                    dic[tr.Single[i]] = ax.GetCommData(e.sTrCode,
                                                       e.sRQName,
                                                       0,
                                                       tr.Single[i])
                                          .Trim();
                }
                if (dic.Count > 1)
                {
                    dic[nameof(AccountOPW00005.Date)] = DateTime.Now.ToString("d",
                                                                              Culture);

                    yield return JsonConvert.SerializeObject(dic);
                }
            }
            if (tr?.Multiple is not null)

                for (int i = 0; i < ax.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    dic = new Dictionary<string, string>
                    {
                        {
                            tr.Id[0],
                            tr.Value[0]
                        }
                    };
                    for (int j = 0; j < tr.Multiple.Length; j++)
                    {
                        dic[tr.Multiple[j]] = ax.GetCommData(e.sTrCode,
                                                             e.sRQName,
                                                             i,
                                                             tr.Multiple[j])
                                                .Trim();
                    }
                    if (dic.Count > 1)
                    {
                        dic[nameof(BalanceOPW00005.Date)] = DateTime.Now.ToString("d",
                                                                                  Culture);

                        yield return JsonConvert.SerializeObject(dic);
                    }
                }
        }
    }
}
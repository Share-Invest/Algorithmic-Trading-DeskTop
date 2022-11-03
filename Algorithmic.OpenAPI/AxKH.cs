using AxKHOpenAPILib;

using ShareInvest.Infrastructure.Http;
using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI.Observe;
using ShareInvest.Models.OpenAPI.Request;

using System.Reflection;

namespace ShareInvest;

public partial class AxKH : UserControl,
                            ISecuritiesMapper<AxMessageEventArgs>
{
    public event EventHandler<AxMessageEventArgs>? Send;

    public int ConnectState => axAPI.GetConnectState();

    public AxKH(CoreRestClient api, string id)
    {
        this.id = id;
        this.api = api;

        InitializeComponent();
    }
    public bool CommConnect()
    {
        axAPI.OnReceiveChejanData += OnReceiveChejanData;
        axAPI.OnReceiveRealData += OnReceiveRealData;
        axAPI.OnReceiveTrData += OnReceiveTrData;
        axAPI.OnEventConnect += OnEventConnect;
        axAPI.OnReceiveMsg += OnReceiveMessage;
        axAPI.OnReceiveConditionVer += OnReceiveConditionVersion;
        axAPI.OnReceiveRealCondition += OnReceiveRealCondition;
        axAPI.OnReceiveTrCondition += OnReceiveTrCondition;

        return axAPI.CommConnect() == 0;
    }
    void OnReceiveErrorMessage(string? sRQName, int error)
    {
        if (error < 0)
            Send?.Invoke(this,
            new AxMessageEventArgs(Status.Error[error],
                                                sRQName,
                                                Math.Abs(error).ToString("D4")));
    }
    void OnReceiveMessage(object sender,
                          _DKHOpenAPIEvents_OnReceiveMsgEvent e)
    {
        Send?.Invoke(this,
                     new AxMessageEventArgs(e.sMsg[9..],
                                            e.sRQName,
                                            e.sScrNo));
    }
    void OnEventConnect(object sender,
                        _DKHOpenAPIEvents_OnEventConnectEvent e)
    {
        if (e.nErrCode == 0)
        {
            var codeListByMarket = new List<string>(axAPI.GetCodeListByMarket("0")
                                                         .Split(';')
                                                         .OrderBy(o => Guid.NewGuid()));

            codeListByMarket.AddRange(axAPI.GetCodeListByMarket("10")
                                           .Split(';')
                                           .OrderBy(o => Guid.NewGuid()));

            foreach (var tr in Tr.OPTKWFID.GetListOfStocks(codeListByMarket))
            {
                var nCodeCount = tr.PrevNext;
                tr.PrevNext = 0;

                if (tr.Value is not null)
                    Delay.GetInstance(0x259)
                         .RequestTheMission(new Task(() =>
                         {
                             OnReceiveErrorMessage(tr.RQName,
                                                   axAPI.CommKwRqData(tr.Value[0],
                                                                      tr.PrevNext,
                                                                      nCodeCount,
                                                                      0,
                                                                      tr.RQName,
                                                                      tr.ScreenNo));
                         }));
            }
        }
        else
            OnReceiveErrorMessage(sender.GetType().Name, e.nErrCode);
    }
    void OnReceiveTrData(object sender,
                               _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
    {
        var name = string.Concat(typeof(TR).FullName, '.', e.sTrCode);

        if (Assembly.GetExecutingAssembly()
                    .CreateInstance(name, true) is TR ctor)

            foreach (var json in ctor.OnReceiveTrData(axAPI,
                                                      e,
                                                      Constructer.GetInstance(e.sTrCode)))
                _ = Task.Run(async() =>
                {
                    await api.PostExecuteAsync(json, "stock");
                });
    }
    void OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
    {
        throw new NotImplementedException();
    }
    void OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
    {
        throw new NotImplementedException();
    }
    void OnReceiveConditionVersion(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
    {
        throw new NotImplementedException();
    }
    void OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent e)
    {
        throw new NotImplementedException();
    }
    void OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
    {
        throw new NotImplementedException();
    }
    readonly string id;
    readonly CoreRestClient api;
}
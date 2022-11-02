using AxKHOpenAPILib;

using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI.Observe;

using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ShareInvest;

public partial class AxKH : UserControl,
                            ISecuritiesMapper<AxMessageEventArgs>
{
    public event EventHandler<AxMessageEventArgs>? Send;

    public int ConnectState => axAPI.GetConnectState();

    public AxKH(string id)
    {
        this.id = id;

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
    IEnumerable<Models.OpenAPI.TR> GetListOfStocks(IEnumerable<string> codeListByMarket)
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

            foreach (var tr in GetListOfStocks(codeListByMarket))
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
    void OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
    {
        var name = string.Concat(typeof(TR).FullName, '.', e.sTrCode);

        if (Assembly.GetExecutingAssembly()
                    .CreateInstance(name, true) is TR ctor)
        {
            var request = string.Concat(typeof(Models.OpenAPI.TR).Namespace,
                                        '.',
                                        nameof(Models.OpenAPI.Request),
                                        '.',
                                        e.sTrCode);

            foreach (var json in ctor.OnReceiveTrData(axAPI, e,
                                                      Assembly.GetExecutingAssembly()
                                                              .CreateInstance(request, true) as Models.OpenAPI.TR))
            {
                Debug.WriteLine(json);
            }
        }
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
}
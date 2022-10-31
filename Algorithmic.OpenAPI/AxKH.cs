using AxKHOpenAPILib;

using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI;

using System.Diagnostics;
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
    void GetInformationOfCode(IEnumerable<string> codeListByMarket)
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


                if (Status.IsDebugging)
                    Debug.WriteLine(pop);
            }
    }
    void OnReceiveErrorMessage(int error, string? sRQName)
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

            GetInformationOfCode(codeListByMarket);
        }
        else
            OnReceiveErrorMessage(e.nErrCode, sender.GetType().Name);
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
    void OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent e)
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
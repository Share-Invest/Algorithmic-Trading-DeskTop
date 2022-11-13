using ShareInvest.Identifies;
using ShareInvest.Infrastructure.Http;
using ShareInvest.Infrastructure.Socket;
using ShareInvest.Properties;

using System.Diagnostics;

namespace ShareInvest;

static class Program
{
    [STAThread]
    static void Main()
    {
        if (KeyDecoder.ProductKeyFromRegistry is string key)
        {
            Status.SetDebug();

            ApplicationConfiguration.Initialize();

            Application.Run(new Securities(new[]
            {
                Resources.bird_idle,
                Resources.bird_awake,
                Resources.bird_alert,
                Resources.bird_sleep,
                Resources.bird_invisible
            },
            new CoreRestClient(Status.Address),

            new CoreSignalR(string.Concat(Status.Address,
                                          Resources.KIWOOM)),
            Status.GetKey(key.Split('-'))));
        }
        GC.Collect();

        Process.GetCurrentProcess().Kill();
    }
}
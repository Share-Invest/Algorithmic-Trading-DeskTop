using ShareInvest.Identifies;
using ShareInvest.Services;

using System.Diagnostics;

namespace ShareInvest
{
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
                    Properties.Resources.bird_idle,
                    Properties.Resources.bird_awake,
                    Properties.Resources.bird_alert,
                    Properties.Resources.bird_sleep,
                    Properties.Resources.bird_invisible
                },
                new SecuritiesService(Status.GetId(key.Split('-')))));
            }
            GC.Collect();
            Process.GetCurrentProcess().Kill();
        }
    }
}
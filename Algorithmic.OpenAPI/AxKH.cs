using ShareInvest.Mappers;
using ShareInvest.Models.OpenAPI;

namespace ShareInvest
{
    public partial class AxKH : UserControl,
                                ISecuritiesMapper<MessageEventArgs>
    {
        public event EventHandler<MessageEventArgs>? Send;

        public AxKH(string id)
        {
            this.id = id;
            InitializeComponent();
        }
        public bool CommConnect()
        {
            return axAPI.CommConnect() == 0;
        }
        public int ConnectState => axAPI.GetConnectState();

        readonly string id;
    }
}
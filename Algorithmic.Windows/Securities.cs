using System.ComponentModel;

namespace ShareInvest
{
    partial class Securities : Form
    {
        internal Securities(Icon[] icons, string id)
        {
            this.id = id;
            this.icons = icons;
            InitializeComponent();
        }
        void Add(IComponent? component)
        {
            if (component is Control control)
            {
                Controls.Add(control);
                control.Dock = DockStyle.Fill;
                control.Show();
                FormBorderStyle = FormBorderStyle.None;
                CenterToScreen();
            }
            else
                Close();
        }
        void Dispose(IComponent? component)
        {
            if (component is Control control)
                control.Dispose();

            Dispose();
        }
        readonly Icon[] icons;
        readonly string id;
    }
}
﻿using Newtonsoft.Json;

using ShareInvest.Infrastructure;
using ShareInvest.Mappers;
using ShareInvest.Observer;
using ShareInvest.Observer.OpenAPI;
using ShareInvest.Properties;
using ShareInvest.Services;

using System.ComponentModel;
using System.Diagnostics;

namespace ShareInvest;

partial class Securities : Form
{
    internal Securities(Icon[] icons, ICoreClient client, string id)
    {
        this.id = id;
        this.icons = icons;
        this.client = client;

        securities = SecuritiesExtensions.ConfigureServices(id);

        InitializeComponent();

        timer.Start();
    }
    void OnReceiveMessage(AxMessageEventArgs e)
    {
        switch (e.Screen)
        {
            case "0106" or "0100":

                Dispose(securities as Control);
                break;
        }
        var param = $"{DateTime.Now:G}\n[{e.Code}] {e.Title}({e.Screen})";

        notifyIcon.Text = param.Length < 0x40 ? param : $"[{e.Code}] {e.Title}({e.Screen})";

#if DEBUG
        Debug.WriteLine(param);
#endif
    }
    async Task OnReceiveMessage(UserInfoEventArgs e)
    {
        e.UserInfo.Key = id;

#if DEBUG
        Debug.WriteLine(JsonConvert.SerializeObject(e.UserInfo,
                                                    Formatting.Indented));
#endif
        await client.PostAsync(e.UserInfo.GetType().Name, e.UserInfo);
    }
    async Task OnReceiveMessage(JsonMessageEventArgs e)
    {
#if DEBUG
        Debug.WriteLine(string.Concat(JsonConvert.SerializeObject(e.Convey,
                                                                  Formatting.Indented),
                                      '\n',
                                      JsonConvert.SerializeObject(e.Convey),
                                      '\n',
                                      e.Convey?.GetType().Name));
#endif
        if (e.Convey is not null)

            await client.PostAsync(e.Convey.GetType().Name, e.Convey);
    }
    void OnReceiveMessage(object? sender,
                          MessageEventArgs e)
    {
        _ = BeginInvoke(new Action(async () =>
        {
            switch (e.GetType().Name)
            {
                case nameof(JsonMessageEventArgs)
                when e is JsonMessageEventArgs convey:

                    await OnReceiveMessage(convey);

                    return;

                case nameof(AxMessageEventArgs)
                when e is AxMessageEventArgs ax:

                    OnReceiveMessage(ax);

                    return;

                case nameof(UserInfoEventArgs)
                when e is UserInfoEventArgs user:

                    await OnReceiveMessage(user);

                    return;
            };
        }));
    }
    void TimerTick(object sender, EventArgs e)
    {
        if (securities is null)
        {
            timer.Stop();

            strip.ItemClicked -= StripItemClicked;

            Dispose();
        }
        else if (FormBorderStyle.Equals(FormBorderStyle.Sizable) &&
                 WindowState.Equals(FormWindowState.Minimized) is false)

            WindowState = FormWindowState.Minimized;

        else
        {
            var now = DateTime.Now;

            if (IsConnected)
            {
                notifyIcon.Icon = icons[now.Second % 4];

                if (now.Hour == 8 && now.Minute == 1 && now.Second % 9 == 0 &&
                   (int)now.DayOfWeek > 0 && (int)now.DayOfWeek < 6 &&
                   securities is AxKH ax)

                    ax.Dispose();
            }
            else
                notifyIcon.Icon = icons[^1];

            if (now.Second == 0x3A && now.Minute % 2 == 0 &&
               (now.Hour == 5 || now.Hour == 6 && now.Minute < 0x35) is false)

                _ = BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (securities is AxKH ax &&
                            ax.ConnectState == 0 &&
                            Process.GetProcessesByName(Resources.OP).Length == 0)
                        {
                            IsConnected = ax.CommConnect();

                            if (IsConnected)
                            {
                                securities.Send += OnReceiveMessage;

                                Delay.Instance.Run();
                            }
                        }
                    }
                    catch
                    {
                        Dispose(securities as AxKH);
                    }
                }));
        }
    }
    void StripItemClicked(object? sender, ToolStripItemClickedEventArgs e)
    {
        if (e.ClickedItem.Name.Equals(reference.Name))

            _ = BeginInvoke(new Action(() =>
            {
                if (IsConnected)
                    _ = Process.Start(new ProcessStartInfo(Resources.URL)
                    {
                        UseShellExecute = true
                    });
                else
                {
                    IsConnected = securities switch
                    {
                        AxKH ax => ax.CommConnect(),

                        _ => false
                    };
                    if (IsConnected)
                    {
                        securities.Send += OnReceiveMessage;

                        Delay.Instance.Run();
                    }
                }
            }));
        else
            Close();
    }
    void SecuritiesResize(object sender, EventArgs e)
    {
        _ = BeginInvoke(new Action(() =>
        {
            SuspendLayout();

            Visible = false;
            ShowIcon = false;
            notifyIcon.Visible = true;

            ResumeLayout();

            Set(securities as Control);
        }));
    }
    void JustBeforeFormClosing(object sender, FormClosingEventArgs e)
    {
        if (CloseReason.UserClosing.Equals(e.CloseReason) &&
            DialogResult.Cancel.Equals(MessageBox.Show(Resources.WARNING.Replace('|', '\n'),
                                                       Text,
                                                       MessageBoxButtons.OKCancel,
                                                       MessageBoxIcon.Question,
                                                       MessageBoxDefaultButton.Button2)))
        {
            e.Cancel = true;

            return;
        }
        Dispose(securities as AxKH);
    }
    void Set(IComponent? component)
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
    bool IsConnected
    {
        get; set;
    }
    readonly ISecuritiesMapper<MessageEventArgs> securities;
    readonly ICoreClient client;
    readonly Icon[] icons;
    readonly string id;
}
using AudioQuickSwitch.Services;

namespace AudioQuickSwitch;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly Icon _appIcon;
    private readonly AudioDeviceService _audioDeviceService = new();
    private readonly DefaultDeviceService _defaultDeviceService = new();
    private readonly NotifyIcon _trayIcon;

    public TrayApplicationContext()
    {
        _appIcon = new Icon(Path.Combine(AppContext.BaseDirectory, "oneclick-audio-swap.ico"));
        _trayIcon = new NotifyIcon
        {
            Icon = _appIcon,
            Text = "AudioQuickSwitch",
            Visible = true,
            ContextMenuStrip = BuildContextMenu()
        };

        _trayIcon.MouseClick += OnTrayIconMouseClick;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.MouseClick -= OnTrayIconMouseClick;
            _trayIcon.Dispose();
            _appIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private static ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());
        return menu;
    }

    private void OnTrayIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        using var popup = new PopupForm(_audioDeviceService, _defaultDeviceService, _appIcon);
        popup.StartPosition = FormStartPosition.Manual;
        popup.Location = GetPopupLocation(popup.Size);
        popup.ShowDialog();
    }

    private static Point GetPopupLocation(Size popupSize)
    {
        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? SystemInformation.WorkingArea;
        return new Point(
            workingArea.Right - popupSize.Width - 12,
            workingArea.Bottom - popupSize.Height - 12);
    }
}

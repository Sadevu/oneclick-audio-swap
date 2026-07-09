namespace AudioQuickSwitch;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;

    public TrayApplicationContext()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "oneclick-audio-swap.ico")),
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

        using var popup = new PopupForm();
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

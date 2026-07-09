using System.Drawing.Drawing2D;
using OneclickAudioSwap.Services;

namespace OneclickAudioSwap;

internal sealed class PopupForm : Form
{
    private const double BackgroundOpacity = 0.70;

    private static readonly Color TransparentBack = Color.Fuchsia;
    private static readonly Color BackPanel = Color.FromArgb(32, 32, 32);
    private static readonly Color Border = Color.FromArgb(75, 75, 75);
    private static readonly Color TextMain = Color.FromArgb(245, 245, 245);
    private static readonly Color TextMuted = Color.FromArgb(196, 196, 196);

    private readonly AudioDeviceService _audioDeviceService;
    private readonly DefaultDeviceService _defaultDeviceService;
    private readonly PopupBackgroundForm _backgroundForm = new(BackPanel, BackgroundOpacity);
    private bool _readyToCloseOnDeactivate;

    public PopupForm(AudioDeviceService audioDeviceService, DefaultDeviceService defaultDeviceService, Icon appIcon)
    {
        _audioDeviceService = audioDeviceService;
        _defaultDeviceService = defaultDeviceService;

        Text = "OneclickAudioSwap";
        Icon = appIcon;
        BackColor = TransparentBack;
        TransparencyKey = TransparentBack;
        TopMost = true;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(360, 300);
        Padding = new Padding(1);
        DoubleBuffered = true;

        BuildDeviceList();
    }

    protected override void OnShown(EventArgs e)
    {
        SyncBackgroundForm();
        _backgroundForm.Show(this);
        base.OnShown(e);
        Activate();
        BringToFront();
        _readyToCloseOnDeactivate = true;
    }

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        SyncBackgroundForm();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        SyncBackgroundForm();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        if (_readyToCloseOnDeactivate)
        {
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _backgroundForm.Close();
        _backgroundForm.Dispose();
        base.OnClosed(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    private void SyncBackgroundForm()
    {
        if (!IsDisposed)
        {
            _backgroundForm.Bounds = Bounds;
        }
    }

    private void BuildDeviceList()
    {
        Controls.Clear();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(16, 12, 16, 6),
            BackColor = TransparentBack
        };

        header.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 20,
            Text = "Output device",
            Font = BoldFont(10.5f),
            ForeColor = TextMain,
            BackColor = TransparentBack
        });

        header.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 18,
            Text = "Select a device to use for all Windows output roles",
            Font = BoldFont(8.5f),
            ForeColor = TextMuted,
            BackColor = TransparentBack
        });

        var list = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(10, 4, 10, 10),
            BackColor = TransparentBack
        };

        Controls.Add(list);
        Controls.Add(header);

        IReadOnlyList<AudioDevice> devices;
        string? defaultDeviceId;
        try
        {
            devices = _audioDeviceService.GetOutputDevices();
            defaultDeviceId = _audioDeviceService.GetDefaultOutputDeviceId();
        }
        catch (Exception ex)
        {
            list.Controls.Add(CreateMessageLabel($"Failed to load output devices.\r\n{ex.Message}"));
            return;
        }

        if (devices.Count == 0)
        {
            list.Controls.Add(CreateMessageLabel("No output devices found."));
        }

        foreach (var device in devices)
        {
            list.Controls.Add(new DeviceItemControl(
                device,
                string.Equals(device.Id, defaultDeviceId, StringComparison.OrdinalIgnoreCase),
                OnDeviceSelected));
        }
    }

    private static Label CreateMessageLabel(string text)
    {
        return new Label
        {
            AutoSize = false,
            Width = 320,
            Height = 84,
            Margin = new Padding(4, 8, 4, 4),
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = BoldFont(9f),
            ForeColor = TextMain,
            BackColor = TransparentBack
        };
    }

    private static Font BoldFont(float size)
    {
        return new Font(FontFamily.GenericSansSerif, size, FontStyle.Bold);
    }

    private void OnDeviceSelected(AudioDevice device)
    {
        try
        {
            _defaultDeviceService.SetDefaultOutputDevice(device.Id);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "OneclickAudioSwap", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class PopupBackgroundForm : Form
    {
        public PopupBackgroundForm(Color backColor, double opacity)
        {
            BackColor = backColor;
            Opacity = opacity;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
        }
    }

    private sealed class DeviceItemControl : Control
    {
        private static readonly Color HoverBack = Color.FromArgb(58, 58, 58);
        private static readonly Color SelectedBack = Color.FromArgb(48, 68, 88);
        private static readonly Color SelectedMark = Color.FromArgb(96, 205, 255);

        private readonly AudioDevice _device;
        private readonly bool _selected;
        private readonly Action<AudioDevice> _onSelected;
        private bool _hovered;

        public DeviceItemControl(AudioDevice device, bool selected, Action<AudioDevice> onSelected)
        {
            _device = device;
            _selected = selected;
            _onSelected = onSelected;

            Width = 326;
            Height = 42;
            Margin = new Padding(0, 2, 0, 2);
            Cursor = Cursors.Hand;
            Font = BoldFont(9f);
            DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            _onSelected(_device);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using var background = new SolidBrush(_selected ? SelectedBack : _hovered ? HoverBack : TransparentBack);
            e.Graphics.FillRectangle(background, bounds);

            if (_selected)
            {
                using var mark = new SolidBrush(SelectedMark);
                e.Graphics.FillEllipse(mark, 12, 15, 12, 12);
            }

            var textBounds = new Rectangle(_selected ? 32 : 16, 0, Width - (_selected ? 44 : 28), Height);
            TextRenderer.DrawText(
                e.Graphics,
                _device.Name,
                Font,
                textBounds,
                TextMain,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }
    }
}

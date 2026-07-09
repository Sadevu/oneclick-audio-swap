using System.Drawing.Drawing2D;
using OneclickAudioSwap.Services;

namespace OneclickAudioSwap;

internal sealed class PopupForm : Form
{
    private static readonly Color BackPanel = Color.FromArgb(249, 249, 249);
    private static readonly Color Border = Color.FromArgb(218, 218, 218);
    private static readonly Color TextMain = Color.FromArgb(31, 31, 31);
    private static readonly Color TextMuted = Color.FromArgb(102, 102, 102);

    private readonly AudioDeviceService _audioDeviceService;
    private readonly DefaultDeviceService _defaultDeviceService;

    public PopupForm(AudioDeviceService audioDeviceService, DefaultDeviceService defaultDeviceService, Icon appIcon)
    {
        _audioDeviceService = audioDeviceService;
        _defaultDeviceService = defaultDeviceService;

        Text = "OneclickAudioSwap";
        Icon = appIcon;
        BackColor = BackPanel;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(360, 300);
        Padding = new Padding(1);
        DoubleBuffered = true;

        BuildDeviceList();
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    private void BuildDeviceList()
    {
        Controls.Clear();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(16, 12, 16, 6),
            BackColor = BackPanel
        };

        header.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 20,
            Text = "Output device",
            Font = new Font(Font.FontFamily, 10.5f, FontStyle.Bold),
            ForeColor = TextMain
        });

        header.Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 18,
            Text = "Select a device to use for all Windows output roles",
            Font = new Font(Font.FontFamily, 8.5f),
            ForeColor = TextMuted
        });

        var list = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(10, 4, 10, 10),
            BackColor = BackPanel
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
            ForeColor = TextMain
        };
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

    private sealed class DeviceItemControl : Control
    {
        private static readonly Color HoverBack = Color.FromArgb(237, 237, 237);
        private static readonly Color SelectedBack = Color.FromArgb(232, 240, 254);
        private static readonly Color SelectedMark = Color.FromArgb(0, 95, 184);

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
            using var background = new SolidBrush(_selected ? SelectedBack : _hovered ? HoverBack : BackPanel);
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

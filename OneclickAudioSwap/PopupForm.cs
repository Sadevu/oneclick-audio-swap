using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OneclickAudioSwap.Services;

namespace OneclickAudioSwap;

internal sealed class PopupForm : Form
{
    private const byte BackgroundAlpha = 178;
    private const int HeaderHeight = 62;
    private const int ListTop = 70;
    private const int RowHeight = 42;
    private const int RowGap = 4;
    private const int ItemLeft = 10;
    private const int ItemWidth = 326;

    private static readonly Color BackPanel = Color.FromArgb(BackgroundAlpha, 32, 32, 32);
    private static readonly Color Border = Color.FromArgb(255, 75, 75, 75);
    private static readonly Color TextMain = Color.White;
    private static readonly Color TextMuted = Color.FromArgb(255, 230, 230, 230);
    private static readonly Color HoverBack = Color.FromArgb(235, 58, 58, 58);
    private static readonly Color SelectedBack = Color.FromArgb(235, 48, 68, 88);
    private static readonly Color SelectedMark = Color.FromArgb(255, 96, 205, 255);

    private readonly AudioDeviceService _audioDeviceService;
    private readonly DefaultDeviceService _defaultDeviceService;
    private readonly Font _titleFont = new(FontFamily.GenericSansSerif, 10.5f, FontStyle.Bold);
    private readonly Font _captionFont = new(FontFamily.GenericSansSerif, 8.5f, FontStyle.Bold);
    private readonly Font _itemFont = new(FontFamily.GenericSansSerif, 9f, FontStyle.Bold);
    private IReadOnlyList<AudioDevice> _devices = Array.Empty<AudioDevice>();
    private string? _defaultDeviceId;
    private string? _errorMessage;
    private int _hoveredIndex = -1;

    public PopupForm(AudioDeviceService audioDeviceService, DefaultDeviceService defaultDeviceService, Icon appIcon)
    {
        _audioDeviceService = audioDeviceService;
        _defaultDeviceService = defaultDeviceService;

        Text = "OneclickAudioSwap";
        Icon = appIcon;
        TopMost = true;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(360, 300);
        DoubleBuffered = true;
        LoadDevices();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            const int wsExLayered = 0x00080000;
            var createParams = base.CreateParams;
            createParams.ExStyle |= wsExLayered;
            return createParams;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleFont.Dispose();
            _captionFont.Dispose();
            _itemFont.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
        BringToFront();
        RenderLayeredWindow();
    }

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        if (IsHandleCreated)
        {
            RenderLayeredWindow();
        }
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        Close();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var hoveredIndex = HitTestDevice(e.Location);
        if (hoveredIndex == _hoveredIndex)
        {
            return;
        }

        _hoveredIndex = hoveredIndex;
        RenderLayeredWindow();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoveredIndex == -1)
        {
            return;
        }

        _hoveredIndex = -1;
        RenderLayeredWindow();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        var index = HitTestDevice(e.Location);
        if (index < 0 || index >= _devices.Count)
        {
            return;
        }

        OnDeviceSelected(_devices[index]);
    }

    private void LoadDevices()
    {
        try
        {
            _devices = _audioDeviceService.GetOutputDevices();
            _defaultDeviceId = _audioDeviceService.GetDefaultOutputDeviceId();
            _errorMessage = null;
        }
        catch (Exception ex)
        {
            _devices = Array.Empty<AudioDevice>();
            _defaultDeviceId = null;
            _errorMessage = $"Failed to load output devices.\r\n{ex.Message}";
        }
    }

    private int HitTestDevice(Point point)
    {
        if (_errorMessage is not null || point.X < ItemLeft || point.X > ItemLeft + ItemWidth || point.Y < ListTop)
        {
            return -1;
        }

        var offset = point.Y - ListTop;
        var stride = RowHeight + RowGap;
        var index = offset / stride;
        var yInRow = offset % stride;
        return index >= 0 && index < _devices.Count && yInRow < RowHeight ? index : -1;
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

    private void RenderLayeredWindow()
    {
        if (!IsHandleCreated)
        {
            return;
        }

        using var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            DrawPopup(graphics);
        }

        var screenDc = GetDC(IntPtr.Zero);
        var memoryDc = CreateCompatibleDC(screenDc);
        var bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
        var oldBitmap = SelectObject(memoryDc, bitmapHandle);

        try
        {
            var top = new Point(Left, Top);
            var size = new Size(Width, Height);
            var source = Point.Empty;
            var blend = new BlendFunction
            {
                BlendOp = 0,
                BlendFlags = 0,
                SourceConstantAlpha = 255,
                AlphaFormat = 1
            };

            UpdateLayeredWindow(Handle, screenDc, ref top, ref size, memoryDc, ref source, 0, ref blend, 2);
        }
        finally
        {
            SelectObject(memoryDc, oldBitmap);
            DeleteObject(bitmapHandle);
            DeleteDC(memoryDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    private void DrawPopup(Graphics graphics)
    {
        using var background = new SolidBrush(BackPanel);
        graphics.FillRectangle(background, new Rectangle(0, 0, Width, Height));

        using var borderPen = new Pen(Border);
        graphics.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        using var titleBrush = new SolidBrush(TextMain);
        using var captionBrush = new SolidBrush(TextMuted);
        graphics.DrawString("Output device", _titleFont, titleBrush, new RectangleF(16, 12, Width - 32, 22));
        graphics.DrawString("Select a device to use for all Windows output roles", _captionFont, captionBrush, new RectangleF(16, 36, Width - 32, 18));

        if (_errorMessage is not null)
        {
            graphics.DrawString(_errorMessage, _itemFont, titleBrush, new RectangleF(16, ListTop + 8, Width - 32, 90));
            return;
        }

        if (_devices.Count == 0)
        {
            graphics.DrawString("No output devices found.", _itemFont, titleBrush, new RectangleF(16, ListTop + 8, Width - 32, 40));
            return;
        }

        for (var i = 0; i < _devices.Count; i++)
        {
            DrawDeviceItem(graphics, i, _devices[i]);
        }
    }

    private void DrawDeviceItem(Graphics graphics, int index, AudioDevice device)
    {
        var selected = string.Equals(device.Id, _defaultDeviceId, StringComparison.OrdinalIgnoreCase);
        var hovered = index == _hoveredIndex;
        var top = ListTop + index * (RowHeight + RowGap);
        var bounds = new Rectangle(ItemLeft, top, ItemWidth, RowHeight);

        if (selected || hovered)
        {
            using var itemBackground = new SolidBrush(selected ? SelectedBack : HoverBack);
            graphics.FillRectangle(itemBackground, bounds);
        }

        var textLeft = selected ? ItemLeft + 32 : ItemLeft + 16;
        if (selected)
        {
            using var mark = new SolidBrush(SelectedMark);
            graphics.FillEllipse(mark, ItemLeft + 12, top + 15, 12, 12);
        }

        using var textBrush = new SolidBrush(TextMain);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        graphics.DrawString(device.Name, _itemFont, textBrush, new RectangleF(textLeft, top, ItemLeft + ItemWidth - textLeft - 12, RowHeight), format);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("user32.dll")]
    private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BlendFunction pblend, int dwFlags);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }
}

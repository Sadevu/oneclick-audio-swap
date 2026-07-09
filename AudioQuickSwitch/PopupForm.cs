using AudioQuickSwitch.Services;

namespace AudioQuickSwitch;

internal sealed class PopupForm : Form
{
    private readonly AudioDeviceService _audioDeviceService;
    private readonly DefaultDeviceService _defaultDeviceService;

    public PopupForm(AudioDeviceService audioDeviceService, DefaultDeviceService defaultDeviceService, Icon appIcon)
    {
        _audioDeviceService = audioDeviceService;
        _defaultDeviceService = defaultDeviceService;

        Text = "AudioQuickSwitch";
        Icon = appIcon;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(360, 260);

        BuildDeviceList();
    }

    private void BuildDeviceList()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8)
        };

        IReadOnlyList<AudioDevice> devices;
        try
        {
            devices = _audioDeviceService.GetOutputDevices();
        }
        catch (Exception ex)
        {
            panel.Controls.Add(CreateMessageLabel($"Failed to load output devices.\r\n{ex.Message}"));
            Controls.Add(panel);
            return;
        }

        if (devices.Count == 0)
        {
            panel.Controls.Add(CreateMessageLabel("No output devices found."));
        }

        foreach (var device in devices)
        {
            var button = new Button
            {
                Width = 320,
                Height = 36,
                Text = device.Name,
                TextAlign = ContentAlignment.MiddleLeft,
                Tag = device.Id
            };

            button.Click += OnDeviceButtonClick;
            panel.Controls.Add(button);
        }

        Controls.Add(panel);
    }

    private static Label CreateMessageLabel(string text)
    {
        return new Label
        {
            AutoSize = false,
            Width = 320,
            Height = 80,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private void OnDeviceButtonClick(object? sender, EventArgs e)
    {
        if (sender is not Button { Tag: string deviceId })
        {
            return;
        }

        try
        {
            _defaultDeviceService.SetDefaultOutputDevice(deviceId);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "AudioQuickSwitch", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

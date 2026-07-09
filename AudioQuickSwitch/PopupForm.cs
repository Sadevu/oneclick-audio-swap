namespace AudioQuickSwitch;

internal sealed class PopupForm : Form
{
    public PopupForm()
    {
        Text = "AudioQuickSwitch";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(320, 160);

        var label = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Audio device list will appear here.",
            TextAlign = ContentAlignment.MiddleCenter
        };

        Controls.Add(label);
    }
}

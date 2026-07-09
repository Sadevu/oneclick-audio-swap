using System.Runtime.InteropServices;

namespace OneclickAudioSwap.Native;

[ComImport]
[Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
internal sealed class PolicyConfigClient
{
}

[ComImport]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    int GetMixFormat();
    int GetDeviceFormat();
    int ResetDeviceFormat();
    int SetDeviceFormat();
    int GetProcessingPeriod();
    int SetProcessingPeriod();
    int GetShareMode();
    int SetShareMode();
    int GetPropertyValue();
    int SetPropertyValue();
    int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string deviceId, ERole role);
    int SetEndpointVisibility();
}


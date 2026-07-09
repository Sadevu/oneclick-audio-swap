using System.Runtime.InteropServices;
using OneclickAudioSwap.Native;

namespace OneclickAudioSwap.Services;

internal sealed class DefaultDeviceService
{
    private static readonly Guid PolicyConfigClientClsid = new("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9");

    public void SetDefaultOutputDevice(string deviceId)
    {
        var policyConfig = CreateComInstance<IPolicyConfig>(PolicyConfigClientClsid);

        try
        {
            SetDefaultEndpoint(policyConfig, deviceId, ERole.eConsole);
            SetDefaultEndpoint(policyConfig, deviceId, ERole.eMultimedia);
            SetDefaultEndpoint(policyConfig, deviceId, ERole.eCommunications);
        }
        finally
        {
            Marshal.ReleaseComObject(policyConfig);
        }
    }

    private static void SetDefaultEndpoint(IPolicyConfig policyConfig, string deviceId, ERole role)
    {
        Marshal.ThrowExceptionForHR(policyConfig.SetDefaultEndpoint(deviceId, role));
    }

    private static T CreateComInstance<T>(Guid classId)
    {
        var type = Type.GetTypeFromCLSID(classId, throwOnError: true)!;
        return (T)Activator.CreateInstance(type)!;
    }
}


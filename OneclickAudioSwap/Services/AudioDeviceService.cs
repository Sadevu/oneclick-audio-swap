using System.Runtime.InteropServices;
using OneclickAudioSwap.Native;

namespace OneclickAudioSwap.Services;

internal sealed unsafe class AudioDeviceService
{
    private static readonly Guid MMDeviceEnumeratorClsid = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
    private static readonly Guid IMMDeviceEnumeratorIid = new("A95664D2-9614-4F35-A746-DE8DB63617E6");

    public IReadOnlyList<AudioDevice> GetOutputDevices()
    {
        var enumerator = IntPtr.Zero;
        var collection = IntPtr.Zero;

        try
        {
            Marshal.ThrowExceptionForHR(NativeMethods.CoCreateInstance(
                in MMDeviceEnumeratorClsid,
                IntPtr.Zero,
                1,
                in IMMDeviceEnumeratorIid,
                out enumerator));

            Marshal.ThrowExceptionForHR(EnumAudioEndpoints(enumerator, EDataFlow.eRender, DeviceStates.Active, out collection));
            Marshal.ThrowExceptionForHR(CollectionGetCount(collection, out var count));

            var devices = new List<AudioDevice>((int)count);
            for (uint i = 0; i < count; i++)
            {
                Marshal.ThrowExceptionForHR(CollectionItem(collection, i, out var device));
                try
                {
                    devices.Add(ReadDevice(device));
                }
                finally
                {
                    Marshal.Release(device);
                }
            }

            return devices;
        }
        finally
        {
            if (collection != IntPtr.Zero)
            {
                Marshal.Release(collection);
            }

            if (enumerator != IntPtr.Zero)
            {
                Marshal.Release(enumerator);
            }
        }
    }

    private static AudioDevice ReadDevice(IntPtr device)
    {
        Marshal.ThrowExceptionForHR(DeviceGetId(device, out var idPointer));
        var id = Marshal.PtrToStringUni(idPointer) ?? string.Empty;
        Marshal.FreeCoTaskMem(idPointer);

        Marshal.ThrowExceptionForHR(DeviceOpenPropertyStore(device, StorageAccessMode.Read, out var propertyStore));
        try
        {
            Marshal.ThrowExceptionForHR(PropertyStoreGetValue(propertyStore, PropertyKeys.DeviceFriendlyName, out var value));
            try
            {
                return new AudioDevice(id, value.GetString() ?? id);
            }
            finally
            {
                value.Clear();
            }
        }
        finally
        {
            Marshal.Release(propertyStore);
        }
    }

    private static int EnumAudioEndpoints(IntPtr self, EDataFlow dataFlow, uint stateMask, out IntPtr devices)
    {
        var method = GetMethod<EnumAudioEndpointsDelegate>(self, 3);
        return method(self, dataFlow, stateMask, out devices);
    }

    private static int CollectionGetCount(IntPtr self, out uint count)
    {
        var method = GetMethod<CollectionGetCountDelegate>(self, 3);
        return method(self, out count);
    }

    private static int CollectionItem(IntPtr self, uint index, out IntPtr device)
    {
        var method = GetMethod<CollectionItemDelegate>(self, 4);
        return method(self, index, out device);
    }

    private static int DeviceGetId(IntPtr self, out IntPtr id)
    {
        var method = GetMethod<DeviceGetIdDelegate>(self, 5);
        return method(self, out id);
    }

    private static int DeviceOpenPropertyStore(IntPtr self, StorageAccessMode access, out IntPtr propertyStore)
    {
        var method = GetMethod<DeviceOpenPropertyStoreDelegate>(self, 4);
        return method(self, access, out propertyStore);
    }

    private static int PropertyStoreGetValue(IntPtr self, PropertyKey key, out PropVariant value)
    {
        var method = GetMethod<PropertyStoreGetValueDelegate>(self, 5);
        return method(self, in key, out value);
    }

    private static T GetMethod<T>(IntPtr self, int index)
    {
        var vtable = Marshal.ReadIntPtr(self);
        var pointer = Marshal.ReadIntPtr(vtable, index * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int EnumAudioEndpointsDelegate(IntPtr self, EDataFlow dataFlow, uint stateMask, out IntPtr devices);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CollectionGetCountDelegate(IntPtr self, out uint count);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int CollectionItemDelegate(IntPtr self, uint index, out IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int DeviceGetIdDelegate(IntPtr self, out IntPtr id);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int DeviceOpenPropertyStoreDelegate(IntPtr self, StorageAccessMode access, out IntPtr propertyStore);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int PropertyStoreGetValueDelegate(IntPtr self, in PropertyKey key, out PropVariant value);
}


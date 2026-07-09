using System.Runtime.InteropServices;

namespace AudioQuickSwitch.Native;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct PropertyKey
{
    public PropertyKey(Guid formatId, int propertyId)
    {
        FormatId = formatId;
        PropertyId = propertyId;
    }

    public Guid FormatId { get; }
    public int PropertyId { get; }
}

internal static class PropertyKeys
{
    public static readonly PropertyKey DeviceFriendlyName = new(
        new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"),
        14);
}

[StructLayout(LayoutKind.Sequential)]
internal struct PropVariant
{
    private ushort _valueType;
    private ushort _reserved1;
    private ushort _reserved2;
    private ushort _reserved3;
    private IntPtr _value;
    private IntPtr _reserved4;

    public string? GetString()
    {
        const ushort vtLpWStr = 31;
        return _valueType == vtLpWStr && _value != IntPtr.Zero
            ? Marshal.PtrToStringUni(_value)
            : null;
    }

    public void Clear()
    {
        NativeMethods.PropVariantClear(ref this);
    }
}

[ComImport]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out uint propertyCount);

    [PreserveSig]
    int GetAt(uint propertyIndex, out PropertyKey key);

    [PreserveSig]
    int GetValue(in PropertyKey key, out PropVariant value);

    [PreserveSig]
    int SetValue(in PropertyKey key, in PropVariant value);

    [PreserveSig]
    int Commit();
}

internal static class NativeMethods
{
    [DllImport("Ole32.dll")]
    internal static extern int PropVariantClear(ref PropVariant propVariant);

    [DllImport("Ole32.dll")]
    internal static extern int CoCreateInstance(
        in Guid classId,
        IntPtr outer,
        uint context,
        in Guid interfaceId,
        out IntPtr instance);
}

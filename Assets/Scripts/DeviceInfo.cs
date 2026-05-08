using UnityEngine;

public class DeviceInfo
{
    public static string GetDeviceId()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }
}

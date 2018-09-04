using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace VoiceDetection
{
  [StructLayout(LayoutKind.Sequential)]
  public class DEV_BROADCAST_HDR
  {
    public int dbch_size;
    public int dbch_devicetype;
    public int dbch_reserved;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct DEV_BROADCAST_DEVICEINTERFACE
  {
    public int dbcc_size;
    public int dbcc_devicetype;
    public int dbcc_reserved;
    public Guid dbcc_classguid;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
    public char[] dbcc_name;
  }

  public struct DeviceInfo_t
  {
    public string DevType;
    public string DeviceInstanceId;
    public string DeviceUniqueID;
  }

  class USBPnpDetection
  {
    public const ushort WM_DEVICECHANGE = 0x0219;
    public const ushort DBT_DEVICEARRIVAL = 0x8000;
    public const ushort DBT_DEVICEREMOVECOMPLETE = 0x8004;
    public const ushort DBT_DEVTYP_DEVICEINTERFACE = 0x0005;
    public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x0000;

    private IntPtr m_hNotifyDevNode;
    public static USBPnpDetection instance;

    public static USBPnpDetection GetInstance()
    {
      if (instance == null)
      {
        instance = new USBPnpDetection();
      }
      return instance;
    }

    //Windows API
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint UnregisterDeviceNotification(IntPtr Handle);

    public  void RegisterDeviceNotification(Guid guid, WindowInteropHelper windowInteropHelper)
    {
      DEV_BROADCAST_DEVICEINTERFACE devIF = new DEV_BROADCAST_DEVICEINTERFACE();
      IntPtr devIFBuffer;

      //Set to HID GUID
      devIF.dbcc_size = Marshal.SizeOf(devIF);
      devIF.dbcc_devicetype = USBPnpDetection.DBT_DEVTYP_DEVICEINTERFACE;
      devIF.dbcc_reserved = 0;
      devIF.dbcc_classguid = guid;

      //AloCATE A buffer for DLL call
      devIFBuffer = Marshal.AllocHGlobal(devIF.dbcc_size);

      //Copy devIF to buffer
      Marshal.StructureToPtr(devIF, devIFBuffer, true);

      //Register for HID device notification
      m_hNotifyDevNode = USBPnpDetection.RegisterDeviceNotification(windowInteropHelper.Handle, devIFBuffer, USBPnpDetection.DEVICE_NOTIFY_WINDOW_HANDLE);

      //Copy buffer to devIF
      devIF = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(devIFBuffer, typeof(DEV_BROADCAST_DEVICEINTERFACE));

      //Free buffer
      Marshal.FreeHGlobal(devIFBuffer);
    }

    public  void UnregisterNotification()
    {
      uint ret = USBPnpDetection.UnregisterDeviceNotification(m_hNotifyDevNode);
    }

    public  DeviceInfo_t GetHIDDeviceInfo(DEV_BROADCAST_DEVICEINTERFACE dvi)
    {
      DeviceInfo_t deviceInfo = new DeviceInfo_t();

      string dbcc_name = new string(dvi.dbcc_name);

      dbcc_name = RemoveNullTerminatedFromString(dbcc_name);

      string[] parts = dbcc_name.Split('#');
      if (parts.Length >= 3)
      {
        deviceInfo.DevType = parts[0].Substring(parts[0].IndexOf(@"?") + 2);
        deviceInfo.DeviceInstanceId = parts[1];
        deviceInfo.DeviceUniqueID = parts[2];

      }
      return deviceInfo;
    }

    private  string RemoveNullTerminatedFromString(string str)
    {
      if (str == null) return null;

      int pos = str.IndexOf((char)0);
      if (pos != -1)
      {
        str = str.Substring(0, pos);
      }
      return str;
    }

  }
}

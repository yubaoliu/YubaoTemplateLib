# USB

# USB Plug and Play Detection on Windows

## Platform

- Win10
- Visual Studio 2017

## C# WPF Example code

**Function**: 

​	MPF main window will be invisible until the USB HID device is plugged in



**MainWindow.xaml.cs**:

This is an example to show how to use the class: [**USBPnpDetection**](.\Windows\USB\USBPnpDetection.cs) 

```c#
    private USBPnpDetection instance;

    public MainWindow()
    {
      InitializeComponent();
      //USB (HID) plug and play
      instance = USBPnpDetection.GetInstance();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);
      HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
      source.AddHook(WndProc);
      Guid hidGuid = new Guid("4d1e55b2-f16f-11cf-88cb-001111000030");

      WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
      instance.RegisterDeviceNotification(hidGuid,windowInteropHelper);
      //hide main window
      this.Visibility = Visibility.Hidden; 
    }

    protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == USBPnpDetection.WM_DEVICECHANGE)
      {
        //Get the massage event type
        int nEventType = wParam.ToInt32();
        //Check for devices being connected or disconnected
        if (nEventType == USBPnpDetection.DBT_DEVICEARRIVAL ||
          nEventType == USBPnpDetection.DBT_DEVICEREMOVECOMPLETE)
        {
          DEV_BROADCAST_HDR hdr = new DEV_BROADCAST_HDR();

          //Convert lparam to DEV_BROADCAST_HDR structure
          Marshal.PtrToStructure(lParam, hdr);

          if (hdr.dbch_devicetype == USBPnpDetection.DBT_DEVTYP_DEVICEINTERFACE)
          {
            DEV_BROADCAST_DEVICEINTERFACE devIF = new DEV_BROADCAST_DEVICEINTERFACE();
            devIF.dbcc_name = new char[255];

            //Convert lparam to DEV_BROADCAST_DEVICEINTERFACE structure
            devIF = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

            DeviceInfo_t devInfo = new DeviceInfo_t();
            devInfo = instance.GetHIDDeviceInfo(devIF);
            if (devInfo.DeviceInstanceId != "VID_0B0E&PID_0422&MI_03&Col01")//jabra
              return IntPtr.Zero;

            //An HID device was connected or removed
            switch (nEventType)
            {
              case USBPnpDetection.DBT_DEVICEARRIVAL:
                this.Visibility = Visibility.Visible; //Show the Window
                break;
              case USBPnpDetection.DBT_DEVICEREMOVECOMPLETE:
                this.Visibility = Visibility.Hidden;
                break;
              default:
                break;
            }
          }
        }
      }
      return IntPtr.Zero;
    }

```


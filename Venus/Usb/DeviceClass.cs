using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Venus.Usb {
    /// <summary>
    /// A generic base class for physical device classes.
    /// </summary>
    public abstract class DeviceClass : IDisposable {
        private IntPtr deviceInfoSet;
        private Guid classGuid;
        private List<Device> devices;

        protected DeviceClass(Guid classGuid) : this(classGuid, IntPtr.Zero) {}

        internal virtual Device CreateDevice(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index) {
            return new Device(deviceClass, deviceInfoData, path, index);
        }

        /// <summary>
        /// Initializes a new instance of the DeviceClass class.
        /// </summary>
        /// <param name="classGuid">A device class Guid.</param>
        /// <param name="hwndParent">The handle of the top-level window to be used for any user interface or IntPtr.Zero for no handle.</param>
        protected DeviceClass(Guid classGuid, IntPtr hwndParent) {
            this.classGuid = classGuid;

            deviceInfoSet = Native.SetupDiGetClassDevs(ref this.classGuid, 0, hwndParent, Native.DIGCF_DEVICEINTERFACE | Native.DIGCF_PRESENT);
            if (deviceInfoSet.ToInt32() == Native.INVALID_HANDLE_VALUE)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            if (deviceInfoSet != IntPtr.Zero) {
                Native.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                deviceInfoSet = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Gets the device class's guid.
        /// </summary>
        public Guid ClassGuid {
            get { return classGuid; }
        }

        /// <summary>
        /// Gets the list of devices of this device class.
        /// </summary>
        public List<Device> Devices {
            get {
                if (devices == null) {
                    devices = new List<Device>();
                    int index = 0;
                    while (true) {
                        var interfaceData = new Native.SP_DEVICE_INTERFACE_DATA();

                        if (!Native.SetupDiEnumDeviceInterfaces(deviceInfoSet, null, ref classGuid, index, interfaceData)) {
                            int error = Marshal.GetLastWin32Error();
                            if (error != Native.ERROR_NO_MORE_ITEMS)
                                throw new Win32Exception(error);
                            break;
                        }

                        var devData = new Native.SP_DEVINFO_DATA();
                        int size = 0;
                        if (!Native.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, interfaceData, IntPtr.Zero, 0, ref size, devData)) {
                            int error = Marshal.GetLastWin32Error();
                            if (error != Native.ERROR_INSUFFICIENT_BUFFER)
                                throw new Win32Exception(error);
                        }

                        IntPtr buffer = Marshal.AllocHGlobal(size);
                        var detailData = new Native.SP_DEVICE_INTERFACE_DETAIL_DATA();
                        detailData.cbSize = Marshal.SizeOf(typeof (Native.SP_DEVICE_INTERFACE_DETAIL_DATA));
                        Marshal.StructureToPtr(detailData, buffer, false);

                        if (!Native.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, interfaceData, buffer, size, ref size, devData)) {
                            Marshal.FreeHGlobal(buffer);
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }

                        var pDevicePath = (IntPtr) ((int) buffer + Marshal.SizeOf(typeof (int)));
                        string devicePath = Marshal.PtrToStringAuto(pDevicePath);
                        Marshal.FreeHGlobal(buffer);

                        Device device = CreateDevice(this, devData, devicePath, index);
                        devices.Add(device);

                        index++;
                    }
                    devices.Sort();
                }
                return devices;
            }
        }

        internal Native.SP_DEVINFO_DATA GetInfo(int dnDevInst) {
            var sb = new StringBuilder(1024);
            int hr = Native.CM_Get_Device_ID(dnDevInst, sb, sb.Capacity, 0);
            if (hr != 0)
                throw new Win32Exception(hr);

            var devData = new Native.SP_DEVINFO_DATA();
            devData.cbSize = Marshal.SizeOf(typeof (Native.SP_DEVINFO_DATA));
            if (!Native.SetupDiOpenDeviceInfo(deviceInfoSet, sb.ToString(), IntPtr.Zero, 0, devData))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return devData;
        }

        internal string GetProperty(Native.SP_DEVINFO_DATA devData, int property, string defaultValue) {
            if (devData == null)
                throw new ArgumentNullException("devData");

            int propertyRegDataType = 0;
            int requiredSize;
            int propertyBufferSize = 1024;

            IntPtr propertyBuffer = Marshal.AllocHGlobal(propertyBufferSize);
            if (
                !Native.SetupDiGetDeviceRegistryProperty(deviceInfoSet, devData, property, out propertyRegDataType, propertyBuffer, propertyBufferSize,
                                                         out requiredSize)) {
                Marshal.FreeHGlobal(propertyBuffer);
                int error = Marshal.GetLastWin32Error();
                if (error != Native.ERROR_INVALID_DATA)
                    throw new Win32Exception(error);
                return defaultValue;
            }

            string value = Marshal.PtrToStringAuto(propertyBuffer);
            Marshal.FreeHGlobal(propertyBuffer);
            return value;
        }

        internal int GetProperty(Native.SP_DEVINFO_DATA devData, int property, int defaultValue) {
            if (devData == null)
                throw new ArgumentNullException("devData");

            int propertyRegDataType = 0;
            int requiredSize;
            int propertyBufferSize = Marshal.SizeOf(typeof (int));

            IntPtr propertyBuffer = Marshal.AllocHGlobal(propertyBufferSize);
            if (
                !Native.SetupDiGetDeviceRegistryProperty(deviceInfoSet, devData, property, out propertyRegDataType, propertyBuffer, propertyBufferSize,
                                                         out requiredSize)) {
                Marshal.FreeHGlobal(propertyBuffer);
                int error = Marshal.GetLastWin32Error();
                if (error != Native.ERROR_INVALID_DATA)
                    throw new Win32Exception(error);
                return defaultValue;
            }

            var value = (int) Marshal.PtrToStructure(propertyBuffer, typeof (int));
            Marshal.FreeHGlobal(propertyBuffer);
            return value;
        }

        internal Guid GetProperty(Native.SP_DEVINFO_DATA devData, int property, Guid defaultValue) {
            if (devData == null)
                throw new ArgumentNullException("devData");

            int propertyRegDataType = 0;
            int requiredSize;
            int propertyBufferSize = Marshal.SizeOf(typeof (Guid));

            IntPtr propertyBuffer = Marshal.AllocHGlobal(propertyBufferSize);
            if (
                !Native.SetupDiGetDeviceRegistryProperty(deviceInfoSet, devData, property, out propertyRegDataType, propertyBuffer, propertyBufferSize,
                                                         out requiredSize)) {
                Marshal.FreeHGlobal(propertyBuffer);
                int error = Marshal.GetLastWin32Error();
                if (error != Native.ERROR_INVALID_DATA)
                    throw new Win32Exception(error);
                return defaultValue;
            }

            var value = (Guid) Marshal.PtrToStructure(propertyBuffer, typeof (Guid));
            Marshal.FreeHGlobal(propertyBuffer);
            return value;
        }
    }
}
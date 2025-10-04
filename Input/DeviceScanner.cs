using System;
using System.Collections.Generic;
using SharpDX.DirectInput;

namespace Arcadia.Input {
    public static class DeviceScanner {
        public static List<string> ScanDevices() {
            var directInput = new DirectInput();
            var devices = new List<string>();

            foreach (var device in directInput.GetDevices(DeviceType.GameControl, DeviceEnumerationFlags.AttachedOnly)) {
                devices.Add($"{device.Type}: {device.InstanceName}");
            }

            return devices;
        }
    }
}

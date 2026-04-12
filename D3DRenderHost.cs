using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Arcadia.UI.Rendering
{
    /// <summary>
    /// Bridges Direct3D11 rendering into the WPF composition engine.
    /// Defaults to a windowed rendering context.
    /// </summary>
    public class D3DRenderHost : IDisposable
    {
        private Device _device;
        private Texture2D _renderTarget;
        private RenderTargetView _renderTargetView;
        private D3DImage _d3dImage;

        public D3DImage ImageSource => _d3dImage;
        public Device Device => _device;

        public D3DRenderHost(int width, int height)
        {
            _d3dImage = new D3DImage();
            InitializeD3D(width, height);
        }

        private void InitializeD3D(int width, int height)
        {
            // Create Device with BGRA support for D3DImage compatibility
            _device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);

            var colordesc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            _renderTarget = new Texture2D(_device, colordesc);
            _renderTargetView = new RenderTargetView(_device, _renderTarget);

            _d3dImage.Lock();
            // In a real scenario, a DX9/DX11 shared handle or interop library is used here
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, GetSharedHandle());
            _d3dImage.Unlock();
        }

        private IntPtr GetSharedHandle()
        {
            using (var resource = _renderTarget.QueryInterface<SharpDX.DXGI.Resource>())
            {
                return resource.SharedHandle;
            }
        }

        public void Render(Action<DeviceContext, RenderTargetView> renderCallback)
        {
            if (_d3dImage.IsFrontBufferAvailable)
            {
                _d3dImage.Lock();
                
                renderCallback(_device.ImmediateContext, _renderTargetView);

                _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
                _d3dImage.Unlock();
            }
        }

        public void Dispose()
        {
            _renderTargetView?.Dispose();
            _renderTarget?.Dispose();
            _device?.Dispose();
        }
    }
}
using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Arcadia.UI.Controls
{
    public class D3D11Host : Control
    {
        public Device? Device { get; private set; }
        public DeviceContext? Context { get; private set; }
        public SwapChain? SwapChain { get; private set; }
        public RenderTargetView? RenderTargetView { get; private set; }
        private Texture2D? _backBuffer;

        public event Action<DeviceContext>? RenderFrame;

        public D3D11Host()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint, true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitializeD3D11();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            CleanupD3D11();
            base.OnHandleDestroyed(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Device != null && SwapChain != null && ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                RenderTargetView?.Dispose();
                _backBuffer?.Dispose();
                
                SwapChain.ResizeBuffers(1, ClientSize.Width, ClientSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
                
                _backBuffer = Texture2D.FromSwapChain<Texture2D>(SwapChain, 0);
                RenderTargetView = new RenderTargetView(Device, _backBuffer);
                Context?.Rasterizer.SetViewport(new Viewport(0, 0, ClientSize.Width, ClientSize.Height));
            }
        }

        private void InitializeD3D11()
        {
            int w = Math.Max(ClientSize.Width, 1);
            int h = Math.Max(ClientSize.Height, 1);
            
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(w, h, new Rational(60, 1), Format.B8G8R8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, desc, out var device, out var swapChain);
            Device = device;
            SwapChain = swapChain;
            Context = Device.ImmediateContext;

            var factory = SwapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);

            _backBuffer = Texture2D.FromSwapChain<Texture2D>(SwapChain, 0);
            RenderTargetView = new RenderTargetView(Device, _backBuffer);
            Context.Rasterizer.SetViewport(new Viewport(0, 0, w, h));
        }

        private void CleanupD3D11()
        {
            RenderTargetView?.Dispose();
            _backBuffer?.Dispose();
            Context?.Dispose();
            SwapChain?.Dispose();
            Device?.Dispose();
        }

        public void Render()
        {
            if (Device != null && RenderTargetView != null && Context != null && SwapChain != null)
            {
                Context.OutputMerger.SetRenderTargets(RenderTargetView);
                // Clear to black/transparent background
                Context.ClearRenderTargetView(RenderTargetView, new Color4(0.0f, 0.0f, 0.0f, 0.0f));

                RenderFrame?.Invoke(Context);

                SwapChain.Present(1, PresentFlags.None);
            }
        }
    }
}

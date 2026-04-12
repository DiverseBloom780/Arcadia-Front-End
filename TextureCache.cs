using System;
using System.Collections.Generic;
using System.IO;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.WIC;

namespace Arcadia.UI.Rendering
{
    /// <summary>
    /// Efficiently loads and caches game media as Direct3D ShaderResourceViews.
    /// </summary>
    public class TextureCache : IDisposable
    {
        private readonly Device _device;
        private readonly ImagingFactory _factory;
        private readonly Dictionary<string, ShaderResourceView> _cache = new();

        public TextureCache(Device device)
        {
            _device = device;
            _factory = new ImagingFactory();
        }

        public ShaderResourceView? GetTexture(string? path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            if (_cache.TryGetValue(path, out var srv)) return srv;

            try
            {
                using var bitmapDecoder = new BitmapDecoder(_factory, path, DecodeOptions.CacheOnDemand);
                using var frame = bitmapDecoder.GetFrame(0);
                using var formatConverter = new FormatConverter(_factory);
                
                // Convert to a format D3D11 likes (BGRA 32-bit)
                formatConverter.Initialize(frame, PixelFormat.Format32bppPBGRA);

                int width = formatConverter.Size.Width;
                int height = formatConverter.Size.Height;
                int stride = width * 4;

                using var buffer = new DataStream(height * stride, true, true);
                formatConverter.CopyPixels(stride, buffer);
                buffer.Position = 0;

                var texture = new Texture2D(_device, new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    MipLevels = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
                }, new DataRectangle(buffer.DataPointer, stride));

                var srv = new ShaderResourceView(_device, texture);
                _cache[path] = srv;
                
                texture.Dispose();
                return srv;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            foreach (var texture in _cache.Values) texture.Dispose();
            _cache.Clear();
            _factory.Dispose();
        }
    }
}
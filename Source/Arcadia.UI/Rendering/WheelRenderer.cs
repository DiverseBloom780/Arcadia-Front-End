using System;
using System.Numerics;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;
using System.Collections.ObjectModel;
using Arcadia.Core.Models;
using System.Runtime.InteropServices;

namespace Arcadia.UI.Rendering
{
    /// <summary>
    /// Handles the Direct3D math for drawing the curved game wheel.
    /// </summary>
    public class WheelRenderer
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public SharpDX.Vector4 Position;
            public SharpDX.Vector2 TexCoord;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PerObjectBuffer
        {
            public System.Numerics.Matrix4x4 WorldViewProj;
            public float Opacity;
            public float IsSelected; // 1.0 if selected, 0.0 otherwise
            private SharpDX.Vector2 _padding; // 16-byte alignment for D3D11
            public SharpDX.Vector4 ColorTint; // Dynamic color customization
        }

        private SharpDX.Direct3D11.Buffer? _vertexBuffer;
        private SharpDX.Direct3D11.Buffer? _constantBuffer;
        private VertexShader? _vertexShader;
        private PixelShader? _pixelShader;
        private InputLayout? _inputLayout;

        // Wheel Configuration
        private const int VisibleItems = 11;
        public float WheelRadius { get; set; } = 500f;
        public float ItemSpacing { get; set; } = 0.25f; // Radians
        public float LinearSpacing { get; set; } = 150f; // Pixels for non-curved
        public float WheelXOffset { get; set; } = 200f;
        public float WheelYOffset { get; set; } = 500f;
        public float LogoWidth { get; set; } = 300f;
        public float LogoHeight { get; set; } = 150f;

        // Customization Properties
        public WheelMode CurrentMode { get; set; } = WheelMode.Angled;
        public float TiltAngle { get; set; } = -0.5f; // Radians for 3D tilt
        public Color4 AccentColor { get; set; } = Color4.White;

        public void Initialize(SharpDX.Direct3D11.Device device, byte[] vertexShaderByteCode, byte[] pixelShaderByteCode)
        {
            _vertexShader = new VertexShader(device, vertexShaderByteCode);
            _pixelShader = new PixelShader(device, pixelShaderByteCode);

            var vertices = new Vertex[]
            {
                new Vertex { Position = new SharpDX.Vector4(-0.5f,  0.5f, 0f, 1f), TexCoord = new SharpDX.Vector2(0, 0) },
                new Vertex { Position = new SharpDX.Vector4( 0.5f,  0.5f, 0f, 1f), TexCoord = new SharpDX.Vector2(1, 0) },
                new Vertex { Position = new SharpDX.Vector4(-0.5f, -0.5f, 0f, 1f), TexCoord = new SharpDX.Vector2(0, 1) },
                new Vertex { Position = new SharpDX.Vector4( 0.5f, -0.5f, 0f, 1f), TexCoord = new SharpDX.Vector2(1, 1) }
            };

            _vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);
            _constantBuffer = new SharpDX.Direct3D11.Buffer(device, SharpDX.Utilities.SizeOf<PerObjectBuffer>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            _inputLayout = new InputLayout(device, vertexShaderByteCode, new[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoord)).ToInt32(), 0)
            });
        }

        public void Render(DeviceContext context, double scrollOffset, ObservableCollection<Game> games, TextureCache textureCache)
        {
            if (_vertexBuffer == null || _inputLayout == null) return;
            int totalGames = games.Count;
            if (totalGames == 0) return;

            context.InputAssembler.InputLayout = _inputLayout;
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, SharpDX.Utilities.SizeOf<Vertex>(), 0));
            
            context.VertexShader.Set(_vertexShader);
            context.VertexShader.SetConstantBuffer(0, _constantBuffer);
            context.PixelShader.Set(_pixelShader);

            for (int i = -VisibleItems / 2; i <= VisibleItems / 2; i++)
            {
                // Calculate wrapped index for infinite circular scrolling
                double itemIndex = scrollOffset + i;
                int wrappedIndex = ((int)Math.Round(itemIndex) % totalGames + totalGames) % totalGames;
                float relativePos = (float)(i - (scrollOffset % 1));

                var game = games[wrappedIndex];
                var texture = textureCache.GetTexture(game.LogoPath);
                
                DrawWheelItem(context, relativePos, texture);
            }
        }

        private void DrawWheelItem(DeviceContext context, float relativeIndex, ShaderResourceView? texture)
        {
            float x = WheelXOffset, y = WheelYOffset; 

            switch (CurrentMode)
            {
                case WheelMode.Curved:
                    float angle = relativeIndex * ItemSpacing;
                    x = (float)(Math.Cos(angle) * WheelRadius) - WheelRadius + WheelXOffset; 
                    y = (float)(Math.Sin(angle) * WheelRadius) + WheelYOffset;
                    break;

                case WheelMode.Vertical:
                    x = WheelXOffset;
                    y = WheelYOffset + relativeIndex * LinearSpacing;
                    break;

                case WheelMode.Horizontal:
                    x = WheelXOffset + relativeIndex * LinearSpacing;
                    y = WheelYOffset;
                    break;
            }
            
            // Calculate Scale: Selected item is larger
            float scale = 1.0f - (Math.Abs(relativeIndex) * 0.15f);
            scale = Math.Max(0.5f, scale);

            // Calculate Opacity: Items fade as they move away from center
            float opacity = 1.0f - (Math.Abs(relativeIndex) / (VisibleItems / 2f));

            // Apply the game logo texture to the pixel shader
            context.PixelShader.SetShaderResource(0, texture);

            bool isSelected = Math.Abs(relativeIndex) < 0.4f;
            
            // Build 3D Transform
            var transform = Matrix4x4.CreateScale(scale * LogoWidth, scale * LogoHeight, 1f);
            
            if (CurrentMode == WheelMode.Angled)
            {
                // Apply 3D rotation for the "premium" curved-away look
                transform *= Matrix4x4.CreateRotationY(TiltAngle * (1.0f - scale));
            }
            
            transform *= Matrix4x4.CreateTranslation(x, y, 0);

            var cbData = new PerObjectBuffer
            {
                WorldViewProj = transform,
                Opacity = opacity,
                IsSelected = isSelected ? 1.0f : 0.0f,
                ColorTint = isSelected ? new SharpDX.Vector4(1.2f, 1.2f, 1.2f, 1f) : new SharpDX.Vector4(AccentColor.R, AccentColor.G, AccentColor.B, AccentColor.A)
            };

            DataStream mappedResource;
            context.MapSubresource(_constantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);
            mappedResource.Write(cbData);
            context.UnmapSubresource(_constantBuffer, 0);

            context.Draw(4, 0);
        }
    }
}
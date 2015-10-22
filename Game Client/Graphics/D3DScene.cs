using System;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.Direct3D10_1;
using Device = SlimDX.Direct3D10_1.Device1;
using SlimDX.DXGI;

namespace Game_Client.Graphics
{
    class D3DScene : IDisposable
    {
        private Device              d3ddevice;
        private RenderTargetView    renderview;
        private Int32               viewwidth;
        private Int32               viewheight;

        public Texture2D SharedTexture {
            get;
            set;
        }

        public D3DScene(Int32 width, Int32 height) {
            viewwidth = width;
            viewheight = height;
            InitD3D();
        }

        public void Dispose() {
            DestroyD3D();
        }

        public void Render(int arg) {
            d3ddevice.ClearRenderTargetView(renderview, new Color4(1.0f, 0f, 0f, 0f));


            d3ddevice.Flush();
        }

        void InitD3D() {
            d3ddevice = new Device(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, FeatureLevel.Level_10_0);

            var colordesc = new Texture2DDescription();
            colordesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
            colordesc.Format = Format.B8G8R8A8_UNorm;
            colordesc.Width = viewwidth;
            colordesc.Height = viewheight;
            colordesc.MipLevels = 1;
            colordesc.SampleDescription = new SampleDescription(1, 0);
            colordesc.Usage = ResourceUsage.Default;
            colordesc.OptionFlags = ResourceOptionFlags.Shared;
            colordesc.CpuAccessFlags = CpuAccessFlags.None;
            colordesc.ArraySize = 1;

            SharedTexture = new Texture2D(d3ddevice, colordesc);
            renderview = new RenderTargetView(d3ddevice, SharedTexture);

            d3ddevice.Flush();
        }

        void DestroyD3D() {
            if (SharedTexture != null) {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (d3ddevice != null) {
                d3ddevice.Dispose();
                d3ddevice = null;
            }
        }
    }
}

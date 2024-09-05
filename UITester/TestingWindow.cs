using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SharkUI;

namespace UITester
{
    public class TestingWindow : GameWindow
    {
        private UIBatcher uibatcher = UIBatcher.Instance;
        private const string TestString = "The quick brown fox jumps over the lazy dog";
        public static TestingWindow GetTester() => new(GameWindowSettings.Default, new() { ClientSize = (960, 540), Title = "Testing Window" });
        public TestingWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            uibatcher.initBatch();
            uibatcher.Width = 64;
            uibatcher.ScreenSize = (Size.X, Size.Y);
            //uibatcher.UseScreenSpace = true;

            //TODO: add text once here
            //uibatcher.addText(TestString, new(.2f, .45f), new(1f), 1f);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (!IsFocused) return;

            KeyboardState input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape)) Close();

        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            if (uibatcher.UseScreenSpace)
            {
                //uibatcher.insertChar('T', new(200, 200), new(1f, 1f, 1f, 1f), 1f);
                //uibatcher.insertChar('E', new(236, 200), new(1f, 0f, 0f, 1f), 1f);
                //uibatcher.insertChar('S', new(272, 200), new(0f, 1f, 0f, 1f), 1f);
                //uibatcher.insertChar('T', new(308, 200), new(0f, 0f, 1f, 1f), 1f);

                //Console.WriteLine("--- IGNORE ABOVE ---\n");

                uibatcher.addText("TESTING", new(200, 250), new(1f), 1f);
            } else
            {//TODO: make this in 0-1 range

                //Console.WriteLine("--- IGNORE ABOVE ---\n");
                //uibatcher.addText("TESTING", new(.2f, .3f), new(1f), 1f);
                uibatcher.insertChar('T', new(.2f, .3f), new(1f), 1f);
            }

            uibatcher.flushBatch();

            SwapBuffers();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if(!IsFocused) return;
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e) { base.OnFramebufferResize(e); GL.Viewport(0, 0, e.Width, e.Height); }
    }
}

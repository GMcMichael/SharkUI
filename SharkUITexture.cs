using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace SharkUI
{
    internal class SharkUITexture
    {
        public int Handle { get { return _handle; } private set { _handle = value; } }
        private int _handle = -1;
        public int width = -1, height = -1;
        public SharkUITexture() {}
        public void Init(string texturePath, TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _handle = GL.GenTexture();
            GL.ActiveTexture(textureSlot);
            Enable();
            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult image = ImageResult.FromStream(File.OpenRead(texturePath), ColorComponents.RedGreenBlueAlpha);

            width = image.Width;
            height = image.Height;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            if(isPowerOfTwo(image.Width) && isPowerOfTwo(image.Height))
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public float AspectRation => width / height;

        public void TestPowerOfTwo()
        {
            foreach (int num in (List<int>)[0, 1, 2, 3, 4, 6, 7, 8, 9, 15, 16, 17, 31, 32, 33, 63, 64, 65, 127, 128, 129, 255, 256, 257, 511, 512, 513, 1023, 1024, 1025])
                Console.WriteLine(num + ": " + isPowerOfTwo(num));
        }

        private bool isPowerOfTwo(int n) => (n != 0 && (n & (n - 1)) == 0);

        public void Enable() => GL.BindTexture(TextureTarget.Texture2D, _handle);
    }
}

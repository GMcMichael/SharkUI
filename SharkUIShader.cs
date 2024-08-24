using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SharkUI
{
    internal class SharkUIShader
    {
        public string Name;
        private int _handle = -1;
        private bool __disposedValue = false;
        private readonly Dictionary<string, int> _uniformLocations = [];
        private List<SharkUITexture> _textures = [];
        public int Handle() { return _handle; }
        public SharkUIShader(string name, string vertexPath, string fragmentPath, List<SharkUITexture> textures)
        {
            Name = name;
            _textures = textures;
            try {
                Console.WriteLine($"Reading {Name} shader from sources:\nVertex: {vertexPath}\nFragment: {fragmentPath}\n");
                string vertexSource = File.ReadAllText(vertexPath);
                string fragmentSource = File.ReadAllText(fragmentPath);
                int[] InternalShaders = [GL.CreateShader(ShaderType.VertexShader), GL.CreateShader(ShaderType.FragmentShader)];

                GL.ShaderSource(InternalShaders[0], vertexSource);
                GL.ShaderSource(InternalShaders[1], fragmentSource);

                foreach (int shaderHandle in InternalShaders)
                {
                    GL.CompileShader(shaderHandle);

                    GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int compSuccess);
                    if (compSuccess == 0)
                    {
                        Console.WriteLine($"ERROR: {Name} found errors in source file(s)\n" + GL.GetShaderInfoLog(shaderHandle));
                        DeleteShaders(InternalShaders);
                        throw new Exception();
                    }
                }

                _handle = GL.CreateProgram();

                foreach (int shaderHandle in InternalShaders)
                    GL.AttachShader(_handle, shaderHandle);

                GL.LinkProgram(_handle);

                DeleteShaders(InternalShaders);

                GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int linkSuccess);
                if (linkSuccess == 0)
                {
                    Console.WriteLine(GL.GetProgramInfoLog(_handle));
                    throw new Exception();
                }

                GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

                _uniformLocations = [];
                for (var i = 0; i < numberOfUniforms; i++)
                {
                    // get the name of this uniform,
                    var key = GL.GetActiveUniform(_handle, i, out _, out _);

                    // get the location,
                    var location = GL.GetUniformLocation(_handle, key);

                    // and then add it to the dictionary.
                    _uniformLocations.Add(key, location);
                }
            } catch
            {
                DeleteProgram();
                Console.WriteLine($"Error creating {Name} shader from sources:\nVertex: {vertexPath}\nFragment: {fragmentPath}\n");
            }
        }

        private void trySend(string uniformName, Action sendAction) { try { sendAction?.Invoke(); } catch { Console.WriteLine($"ERROR: Shader '{Name}' setting uniform using name: '{uniformName}'\n"); } }

        public void SetInt(string uniformName, int num) => trySend(uniformName, () => GL.Uniform1(_uniformLocations[uniformName], num));
        public void SetFloat(string uniformName, float num) => trySend(uniformName, () => GL.Uniform1(_uniformLocations[uniformName], num));
        public void SetTexture(string uniformName, int textureHandle) => SetInt(uniformName, textureHandle);
        public void SetVec2(string uniformName, Vector2 vector) => trySend(uniformName, () => GL.Uniform2(_uniformLocations[uniformName], vector));
        public void SetVec4(string uniformName, Vector4 vector) => trySend(uniformName, () => GL.Uniform4(_uniformLocations[uniformName], vector));
        public void SetMat4(string uniformName, Matrix4 matrix4) => trySend(uniformName, () => GL.UniformMatrix4(_uniformLocations[uniformName], true, ref matrix4));

        //when aspectRatio = width / height, you scale the x down to get to lower aspect ratios and you scale y down to get to higher aspect ratios
        public Matrix4 GetAspectRatio(float windowWidth, float windowHeight, int targetTexture = 0)
        {
            int targetIndex = Math.Max(0, targetTexture);
            float initialAspectRatio = windowWidth / windowHeight;
            float targetAspectRatio = _textures[targetIndex].Width / _textures[targetIndex].Height;

            float xScale = 1f;
            float yScale = 1f;

            if (initialAspectRatio > targetAspectRatio)
                xScale = windowHeight * targetAspectRatio / windowWidth;           //decrease aspect ratio by shrinking x
            else
                yScale = windowWidth * (1f / targetAspectRatio) / windowHeight;    //increase aspect ratio by shrinking y

            return Matrix4.CreateScale(xScale, yScale, 1);
        }

        public void SetAspectRatio(float windowWidth, float windowHeight, int targetTexture = 0) => SetMat4("model", GetAspectRatio(windowWidth, windowHeight, targetTexture));

        public SharkUIShader Enable()
        {
            GL.UseProgram(_handle);

            _textures.ForEach(t => t.Enable());
            return this;
        }

        public void Disable() => GL.UseProgram(0);

        private void DeleteShaders(int[] shaders) { foreach (int shaderHandle in shaders) { GL.DetachShader(_handle, shaderHandle); GL.DeleteShader(shaderHandle); } }

        private void DeleteProgram()
        {
            Delete();
            if (_handle != -1) GL.DeleteProgram(_handle);
            _handle = -1;
        }

        protected virtual void Delete()
        {
            if (!__disposedValue)
            {
                GL.DeleteProgram(_handle);
                __disposedValue = true;
            }
        }
    }
}

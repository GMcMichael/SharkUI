using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SharkUI
{
    public class UIBatcher//https://www.khronos.org/opengl/wiki/Vertex_Specification_Best_Practices
    {
        public static UIBatcher Instance { get { return instance; } }
        private static UIBatcher instance = new();

        private readonly int INFO_STRIDE = 7 * sizeof(float);//x,y,  r,g,b,a,  scale
        private readonly int INDEX_STRIDE = sizeof(int);//charIndex
        public int BATCH_SIZE = 100;//TODO: number of chars per batch, 10kb / 32 bytes = 3,125
        public int BatchSize
        {
            get { return BATCH_SIZE; }
            set
            {
                flushBatch();
                BATCH_SIZE = value;
                initVertices();
            }
        }

        private int WIDTH = 32;
        private int SPACING = 4;
        public int Width { get { return WIDTH; } set { WIDTH = value; } }
        public int Spacing { get { return SPACING; } set {  SPACING = value; } }

        private bool SCREEN_SPACE = false;
        private bool DYNAMIC_SPACING = false;
        public bool UseScreenSpace { get { return SCREEN_SPACE; } set { SCREEN_SPACE = value;} }
        public bool UseDynamicSpacing { get { return DYNAMIC_SPACING; } set { DYNAMIC_SPACING = value;} }

        private Vector2? SCREEN_SIZE = null;
        public Vector2 ScreenSize { set { SCREEN_SIZE = value; } }

        private byte[] vertexInfo = [];
        private int[] vertexIndices = [];
        private int size;

        private int vao, infoBuffer, indexBuffer, charUVs;

        private readonly string root = "./SharkUI Resources/";
        private SharkUIShader? shader;
        private TextureAtlas? atlas;
        private TextureAtlas defaultAtlas;
        public TextureAtlas Atlas {
            get { return atlas ?? defaultAtlas; }
            set {
                atlas = value;
                UpdateTexture(value);
            }
        }

        private UIBatcher()
        {
            initVertices();
            defaultAtlas = new(root + "/Fonts/Courier.png", 16, 16, new(1f/17f, 1f/17f), new Dictionary<char, Vector2> {
                {  '!', new( 1, 13) },
                { '\"', new( 2, 13) },
                {  '#', new( 3, 13) },
                {  '$', new( 4, 13) },
                {  '%', new( 5, 13) },
                {  '&', new( 6, 13) },
                { '\'', new( 7, 13) },
                {  '(', new( 8, 13) },
                {  ')', new( 9, 13) },
                {  '*', new(10, 13) },
                {  '+', new(11, 13) },
                {  ',', new(12, 13) },
                {  '-', new(13, 13) },
                {  '.', new(14, 13) },
                {  '/', new(15, 13) },

                {  '0', new( 0, 12) },
                {  '1', new( 1, 12) },
                {  '2', new( 2, 12) },
                {  '3', new( 3, 12) },
                {  '4', new( 4, 12) },
                {  '5', new( 5, 12) },
                {  '6', new( 6, 12) },
                {  '7', new( 7, 12) },
                {  '8', new( 8, 12) },
                {  '9', new( 9, 12) },
                {  ':', new(10, 12) },
                {  ';', new(11, 12) },
                {  '<', new(12, 12) },
                {  '=', new(13, 12) },
                {  '>', new(14, 12) },
                {  '?', new(15, 12) },

                {  '@', new( 0, 11) },
                {  'A', new( 1, 11) },
                {  'B', new( 2, 11) },
                {  'C', new( 3, 11) },
                {  'D', new( 4, 11) },
                {  'E', new( 5, 11) },
                {  'F', new( 6, 11) },
                {  'G', new( 7, 11) },
                {  'H', new( 8, 11) },
                {  'I', new( 9, 11) },
                {  'J', new(10, 11) },
                {  'K', new(11, 11) },
                {  'L', new(12, 11) },
                {  'M', new(13, 11) },
                {  'N', new(14, 11) },
                {  'O', new(15, 11) },

                {  'P', new( 0, 10) },
                {  'Q', new( 1, 10) },
                {  'R', new( 2, 10) },
                {  'S', new( 3, 10) },
                {  'T', new( 4, 10) },
                {  'U', new( 5, 10) },
                {  'V', new( 6, 10) },
                {  'W', new( 7, 10) },
                {  'X', new( 8, 10) },
                {  'Y', new( 9, 10) },
                {  'Z', new(10, 10) },
                {  '[', new(11, 10) },
                { '\\', new(12, 10) },
                {  ']', new(13, 10) },
                {  '^', new(14, 10) },
                {  '_', new(15, 10) },

                {  '`', new( 0, 9) },
                {  'a', new( 1, 9) },
                {  'b', new( 2, 9) },
                {  'c', new( 3, 9) },
                {  'd', new( 4, 9) },
                {  'e', new( 5, 9) },
                {  'f', new( 6, 9) },
                {  'g', new( 7, 9) },
                {  'h', new( 8, 9) },
                {  'i', new( 9, 9) },
                {  'j', new(10, 9) },
                {  'k', new(11, 9) },
                {  'l', new(12, 9) },
                {  'm', new(13, 9) },
                {  'n', new(14, 9) },
                {  'o', new(15, 9) },

                {  'p', new( 0, 8) },
                {  'q', new( 1, 8) },
                {  'r', new( 2, 8) },
                {  's', new( 3, 8) },
                {  't', new( 4, 8) },
                {  'u', new( 5, 8) },
                {  'v', new( 6, 8) },
                {  'w', new( 7, 8) },
                {  'x', new( 8, 8) },
                {  'y', new( 9, 8) },
                {  'z', new(10, 8) },
                {  '{', new(11, 8) },
                {  '|', new(12, 8) },
                {  '}', new(13, 8) },
                {  '~', new(14, 8) },
            });
        }
        private void initVertices()
        {
            vertexInfo = new byte[INFO_STRIDE * BATCH_SIZE];
            vertexIndices = new int[BATCH_SIZE];
            size = 0;
        }

        private void UpdateTexture(TextureAtlas newAtlas)
        {
            //if shader and GL aren't initialized, return
            if (shader == null) return;

            shader.ChangeTexture(0, newAtlas.Texture);
            shader.SetTexture("fontAtlas", newAtlas.Texture.Handle);

            //send char data to buffer
            GL.BindBuffer(BufferTarget.UniformBuffer, charUVs);
            GL.BufferData(BufferTarget.UniformBuffer, sizeof(float) * 2 * newAtlas.UVs.Count, newAtlas.UVs.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            //attach buffer to uniform block binding
            shader.SetUniformBlock("Atlas", 0, charUVs);
        }

        public void initBatch()
        {
            //TODO: free current gpu mem if exists
            defaultAtlas.Init();

            //make batch text shader
            charUVs = GL.GenBuffer();
            shader = new("BatchTextShader", root + "Shaders/TestUIText.vert", root + "Shaders/TestUIText.frag", [Atlas.Texture]);
            UpdateTexture(Atlas);

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            
            GL.BindBuffer(BufferTarget.UniformBuffer, charUVs);

            infoBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, infoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, BATCH_SIZE * INFO_STRIDE, (nint)null, BufferUsageHint.DynamicDraw);

            //x,y
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, INFO_STRIDE, 0);
            GL.EnableVertexAttribArray(0);

            //r,g,b,a
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, INFO_STRIDE, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            //scale
            GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Float, false, INFO_STRIDE, 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, BATCH_SIZE * INDEX_STRIDE, (nint)null, BufferUsageHint.DynamicDraw);

            //charIndex
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Int, false, INFO_STRIDE, 0);
            GL.EnableVertexAttribArray(3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void flushBatch()//TODO: test having a static buffer, one large (like 1 kb, 10 kb, ..., 1 mb) buffer thats reused through sub buffer
        {
            if (size <= 0) return;

            // clear gpu buffer, upload cpu
            GL.BindBuffer(BufferTarget.ArrayBuffer, infoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, INFO_STRIDE * BATCH_SIZE, (nint)null, BufferUsageHint.DynamicDraw);//just clear buffer?
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, size * INFO_STRIDE, vertexInfo);

            GL.BindBuffer(BufferTarget.ArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, INDEX_STRIDE * BATCH_SIZE, (nint)null, BufferUsageHint.DynamicDraw);
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, size * INDEX_STRIDE, vertexIndices);

            //TODO: check if I need any parts of these anywhere
            //FontAtlasShader?.SetAspectRatio(WindowSize.X, WindowSize.Y);
            //modelMatrix = Matrix4.CreateScale(scale.X, scale.Y, 0) * Matrix4.CreateTranslation(position);

            //set GL state
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.One);   //set blending mode
            GL.Enable(EnableCap.Blend);                                                                         //enable blending
            GL.Disable(EnableCap.ProgramPointSize);                                                             //stop shader from setting point size
            GL.PointSize(WIDTH);                                                                                //set point size
            GL.PointParameter(PointParameterName.PointSpriteCoordOrigin, (float)All.LowerLeft);                 //set uv origin to lower left
            shader?.Enable();                                                                                   //enable text shader

            shader?.SetVec2("CharMask", Atlas.charMask);

            //draw uploaded buffer
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Points, 0, size);
            shader?.Disable();

            size = 0;

            //TODO: read index buffer back and check values
            //maybe, try and see if senfin uv floats is easier
        }

        private void insert(int charIndex, Vector2 xy, Vector4 rgba, float scale)
        {
            if (size >= BATCH_SIZE)
                flushBatch();

            Vector2 XY = (SCREEN_SPACE ? xy / SCREEN_SIZE ?? Vector2.One : xy) + (new Vector2(WIDTH / 2) / SCREEN_SIZE ?? Vector2.One);
            XY *= 2;
            XY -= Vector2.One;

            //Console.WriteLine($"param: {xy}, proc: {XY}, reversed: {(XY + Vector2.One)/2 * ScreenSize}");

            int index = size * INFO_STRIDE;//TODO: am I writing the buyes to the correct spots? go debug and watch memory
            int floatSize = sizeof(float);
            BitConverter.GetBytes(XY.X).CopyTo(vertexInfo, index);
            BitConverter.GetBytes(XY.Y).CopyTo(vertexInfo, index + floatSize);

            BitConverter.GetBytes(rgba.X).CopyTo(vertexInfo, index + (2 * floatSize));
            BitConverter.GetBytes(rgba.Y).CopyTo(vertexInfo, index + (3 * floatSize));
            BitConverter.GetBytes(rgba.Z).CopyTo(vertexInfo, index + (4 * floatSize));
            BitConverter.GetBytes(rgba.W).CopyTo(vertexInfo, index + (5 * floatSize));

            BitConverter.GetBytes(scale).CopyTo(vertexInfo, index + (6 * floatSize));

            //BitConverter.GetBytes(charIndex).CopyTo(vertexIndices, size * INDEX_STRIDE);
            vertexIndices[size] = charIndex;
            size++;
        }

        public void insertChar(int charCode, Vector2 xy, Vector4 rgba, float scale) => insert(charCode, xy, rgba, scale);

        public void addText(string text, Vector2 xy, Vector4 rgba, float scale)
        {
            char c;
            TextureAtlas texAtlas = atlas ?? defaultAtlas;
            Vector2 offset = new Vector2(WIDTH + SPACING, 0) * scale / (SCREEN_SPACE ? Vector2.One : SCREEN_SIZE ?? Vector2.One);
            for (int i = 0; i < text.Length; i++)
            {
                c = text[i];
                if (!texAtlas.UVIndices.TryGetValue(c, out int index))
                {
                    Console.WriteLine($"Unknown character ({c})");
                    continue;
                }

                //Console.WriteLine($"i: {i}, char: {c}, index: {index}");
                insert(index, xy + (offset*i), rgba, scale);
            }
        }

        /*TODO:
         * make texures apply
         * add z levels
         * buttons though buttonclick captures? (eg. add button at xy with width and height, then if a click happens I can check the bounding box)
         * split into static and dynamic buffers
         * add menus (rows and cols with some cols/rows being dynamic)
         */
    }
}

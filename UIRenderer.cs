using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SharkUI
{
    //Font atlas gotten from: https://lucide.github.io/Font-Atlas-Generator/

    struct UIElement
    {
        public Matrix4 modelMatrix;
        public List<string> text;
        public List<int> lengths;

        public UIElement(Vector3 position, Vector2 scale, List<string>? text = null)
        {
            modelMatrix = Matrix4.CreateScale(scale.X, scale.Y, 0) * Matrix4.CreateTranslation(position);
            this.text = text ?? [];
            lengths = [];
            foreach (string t in this.text)
                lengths.Add(t.Length);
        }
    }
    struct UIList
    {
        public int size = 32;
        public int head = 0;
        public int VBO = -1;
        public Queue<int> queue = new();
        public int INSTANCE_BUFFER_SIZE = -1;

        public UIList(int size)
        {
            this.size = size;
        }

        public void ExpandList()
        {
            size *= 2;
            Tuple<float, float, int, int>[] data = new Tuple<float, float, int, int>[size];
            //expand and copy data on GPU side
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.GetBufferSubData(BufferTarget.ArrayBuffer, 0, size * INSTANCE_BUFFER_SIZE, data);//HERE: should I split the arrays into a vec2 and int2 arrays?

            GL.BufferData(BufferTarget.ArrayBuffer, uiList.size * INSTANCE_BUFFER_SIZE, Array.Empty<int>(), BufferUsageHint.DynamicRead);
        }

        public void Free(int i) => queue.Enqueue(i);

        public int Reserve()
        {
            int index = queue.Count > 0 ? queue.Dequeue() : -1;
            if (index < 0 && head >= size) ExpandList();
            return head++;
        }
    }
    public class UIRenderer
    {
        private const int OFFSET_BUFFER_STRIDE = 2 * sizeof(float);
        private const int STRING_INFO_BUFFER_STRIDE = 2 * sizeof(int);

        private const string printName = "UIRenderer";
        private SharkUIShader? TextShader;
        private int UITextOffsetBuffer;
        private int UITextStringInfoBuffer;
        private UIList uiList = new();
        private SharkUIShader? FontAtlasShader;
        private List<Tuple<Guid, UIElement, int>> _elements = new();
        private TextureAtlas _fontAtlas = new();
        private int displayKey = -1;
        private string root = "./SharkUI Resources/";
        private bool _canRender = false;

        private bool _debugMode;
        private bool _showAtlas = false;

        private KeyValuePair<char, Vector2> _defaultChar = new('*', new(0,0));

        private int fullScreenVAO;
        private int UITextVAO;
        private readonly float[] squareVertices =
        {
            //Position      Uvs
            -1f, -1f, 0f,   0f, 0f, // bottom left
             1f,  1f, 0f,   1f, 1f, // top right
            -1f,  1f, 0f,   0f, 1f, // top left
                         
            -1f, -1f, 0f,   0f, 0f, // bottom left
             1f, -1f, 0f,   1f, 0f, // bottom right
             1f,  1f, 0f,   1f, 1f, // top right
        };


        public UIRenderer(bool debugMode = false) { _debugMode = debugMode; }

        /// <summary>
        /// Initalizes the font atlas and UI shader, call wherever your shaders are made
        /// </summary>
        public void Init()
        {
            _canRender = true;
            //set up fullscreen object
            if (_debugMode) Console.WriteLine(printName + ": Generating fullscreenVAO");
            fullScreenVAO = GL.GenVertexArray();
            GL.BindVertexArray(fullScreenVAO);

            if (_debugMode) Console.WriteLine(printName + ": Generating squareVBO");
            int squareVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, squareVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, squareVertices.Length * sizeof(float), squareVertices, BufferUsageHint.StaticDraw);

            if (_debugMode) Console.WriteLine(printName + ": Setting fullscreenVAO vertex attrib pointers");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            if (_debugMode) Console.WriteLine(printName + ": Enabling fullscreenVAO vertex attrib arrays");
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            if (_debugMode) Console.WriteLine(printName + ": Generating UITextVAO");
            UITextVAO = GL.GenVertexArray();
            GL.BindVertexArray(UITextVAO);

            if (_debugMode) Console.WriteLine(printName + ": Setting UITextVAO vertex attrib pointers");
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            if (_debugMode) Console.WriteLine(printName + ": Generating UITextVBO");
            UITextOffsetBuffer = GL.GenBuffer();
            UITextStringInfoBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, UITextOffsetBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, uiList.size * OFFSET_BUFFER_STRIDE, Array.Empty<float>(), BufferUsageHint.DynamicRead);

            GL.BindBuffer(BufferTarget.ArrayBuffer, UITextStringInfoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, uiList.size * STRING_INFO_BUFFER_STRIDE, Array.Empty<int>(), BufferUsageHint.DynamicRead);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, OFFSET_BUFFER_STRIDE,      0);
            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Int,   false, STRING_INFO_BUFFER_STRIDE, 0);
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Int,   false, STRING_INFO_BUFFER_STRIDE, sizeof(int));

            if (_debugMode) Console.WriteLine(printName + ": Enabling UITextVAO vertex attrib arrays");
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);

            //Set UITextVBO to change every instance instead of vertex
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);

            //unbind any VAO
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            if (_debugMode) Console.WriteLine(printName + ": Initalizing font atlas");
            //set up font texture map
            _fontAtlas.Init(root + "/Fonts/Courier.png", 16, 16, TextureUnit.Texture0, new(0.25f, 0.25f, 0.2f, 0.2f), new Dictionary<char, Vector2> {
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
            
            if (_debugMode) Console.WriteLine(printName + ": Generating shader\n");
            //make UI shader
            TextShader = new("TextShader", root + "Shaders/SharkUIText.vert", root + "Shaders/SharkUIText.frag", [_fontAtlas.texture]);
            TextShader.SetTexture("fontAtlas", _fontAtlas.texture.Handle);
            if (TextShader.Handle() == -1) _canRender = false;
            //TODO put the char pos into a buffer to index into eg: FontAtlasShader.CreateBuffer("name?", charPositions);

            FontAtlasShader = new("TextureShader", root + "Shaders/SharkUIModelTexture.vert", root + "Shaders/SharkUITextureShader.frag", [_fontAtlas.texture]);
            FontAtlasShader.SetTexture("textureSlot", _fontAtlas.texture.Handle);
            if (FontAtlasShader.Handle() == -1) _canRender = false;

            if (_debugMode) Console.WriteLine(printName + ": Clearing UI elements");
            _elements.Clear();

            if (_debugMode) Console.WriteLine(printName + (_canRender ? ": Done Initalizing" : ": Initialization Failed"));
        }

        /// <summary>
        /// Renders all UI elements sorted by the transparency mode
        /// </summary>
        public void Render(Vector2 WindowSize)
        {
            if (!_canRender) return;
            GL.DepthMask(false);                                                                                // disable writing to depth buffer
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.One);   // set blending mode
            GL.Enable(EnableCap.Blend);                                                                         // enable blending

            if (_showAtlas)
            {
                FontAtlasShader?.Enable();                                                                      // enable font atlas shader
                FontAtlasShader?.SetFloat("opacity", 1f);                                                       // set opacity
                FontAtlasShader?.SetAspectRatio(WindowSize.X, WindowSize.Y);                                    // set aspect ratio
                //TODO for debug purposes FontAtlasShader?.SetInt("displayKey", displayKey);                    // set display key (-1 for none)

                GL.BindVertexArray(fullScreenVAO);                                                              // bind full screen VAO
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);                                                   // draw font atlas to framebuffer
            }

            TextShader?.Enable();                                                                               // enable text rendering shader
            //TextShader?.SetVec4("charMask", _fontAtlas.charMask);                                               // set font atlas char mask
            TextShader?.SetVec2("gridDims", new(_fontAtlas.gridWidth, _fontAtlas.gridHeight));                  // set font atlas grid dimensions
            TextShader?.SetFloat("opacity", 1f);                                                                // set opacity

            foreach (var entry in _elements)                                                                    // for each UI element
            {
                //Guid guid = entry.Item1;
                UIElement element = entry.Item2;
                int IntstanceBufferIndex = entry.Item3; // Do I need this?

                //set shader char position
                TextShader?.SetVec2("charPos", GetCharacterPos(element.text[0][0]));                            // set character to be drawn
                TextShader?.SetMat4("model", TextShader.GetAspectRatio(WindowSize.X, WindowSize.Y) * element.modelMatrix);                                            // set element scale and position

                //render square to screen position
                GL.BindVertexArray(fullScreenVAO);                                                              // bind full screen VAO
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);                                                   // draw element to framebuffer
                //GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, _elements.Count);                       // draw instanced elements to framebuffer
            }

            TextShader?.Disable();
            GL.Disable(EnableCap.Blend);
        }

        public Vector2 GetCharacterPos(char c) => _fontAtlas.characterInfo.TryGetValue(c, out var pos) ? pos : _fontAtlas.characterInfo.FirstOrDefault(_defaultChar).Value;

        private void RecalculateUIBuffer()
        {
            foreach (var entry in _elements)
            {
                UIElement element = entry.Item2;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, UITextOffsetBuffer);
            //GL.BufferData(BufferTarget.ArrayBuffer, uiList.size * OFFSET_BUFFER_STRIDE, Array.Empty<float>(), BufferUsageHint.DynamicRead);
            //GL.BufferSubData(BufferTarget.ArrayBuffer, );

            GL.BindBuffer(BufferTarget.ArrayBuffer, UITextStringInfoBuffer);
            //.BufferData(BufferTarget.ArrayBuffer, uiList.size * STRING_INFO_BUFFER_STRIDE, Array.Empty<int>(), BufferUsageHint.DynamicRead);
            //GL.BufferSubData(BufferTarget.ArrayBuffer, );

            //GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, OFFSET_BUFFER_STRIDE,      0);
            //GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Int,   false, STRING_INFO_BUFFER_STRIDE, 0);
            //GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Int,   false, STRING_INFO_BUFFER_STRIDE, sizeof(int));

            UIList uIList = new UIList();
            uIList
        }

        //TODO: Add 2 functions to batch do below
        public Guid AddUIElement(Vector3 offset, Vector2 scale, List<string> text)
        {
            Guid guid = Guid.NewGuid();
            _elements.Add(new(guid,new(offset, scale, text),-1));
            RecalculateUIBuffer();
            return guid;
        }

        public void RemoveUIElement(Guid id) {
            Tuple<Guid, UIElement, int>? item = _elements.Find(e => e.Item1.Equals(id));
            if (item == null) return;

            _elements.Remove(item);
            // free the items slot in the uiList
            uiList.Free(item.Item3);
            RecalculateUIBuffer();
        }

        public void ShowAtlas(bool b) { _showAtlas = b; }
    }
}

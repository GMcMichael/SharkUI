using OpenTK.Mathematics;

namespace SharkUI
{
    public class TextureAtlas//TODO make this user friendly
    {
        public string path = "";
        public int gridWidth, gridHeight;
        public List<(float, float)> UVs = [];
        public Dictionary<char, int> UVIndices = [];
        public SharkUITexture Texture { get { return texture; } }
        private SharkUITexture texture = new();
        private Dictionary<char, Vector2> characterInfo = [];
        public Vector2 charMask;

        public TextureAtlas() {}
        public TextureAtlas(string path, int gridWidth, int gridHeight, Vector2 charMask, Dictionary<char, Vector2> characterInfo)
        {
            this.path = path;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.charMask = charMask;
            this.characterInfo = characterInfo;
        }
        public void Init() => Init(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
        public void Init(OpenTK.Graphics.OpenGL4.TextureUnit textureSlot) => Init(path, textureSlot);
        public void Init(string path, int gridWidth, int gridHeight, Vector2 charMask, Dictionary<char, Vector2> characterInfo) => Init(path, gridWidth, gridHeight, OpenTK.Graphics.OpenGL4.TextureUnit.Texture0, charMask, characterInfo);
        public void Init(string path, int gridWidth, int gridHeight, OpenTK.Graphics.OpenGL4.TextureUnit textureSlot, Vector2 charMask, Dictionary<char, Vector2> characterInfo)
        {
            this.path = path;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.charMask = charMask;
            this.characterInfo = characterInfo;
            Init(path, textureSlot);
        }
        public void Init(string path, OpenTK.Graphics.OpenGL4.TextureUnit textureSlot)
        {
            texture.Init(path, textureSlot);
            FillUVArray();
        }

        private void FillUVArray()
        {
            UVIndices.Clear();
            UVs.Clear();

            Vector2 gridStride = new(1f / gridWidth, 1f / gridHeight);
            foreach (var item in characterInfo)
            {
                UVIndices.Add(item.Key, UVs.Count);
                UVs.Add((item.Value.X * gridStride.X, item.Value.Y * gridStride.Y));
            }
        }

        public override string ToString()
        {
            string charInfo = "";
            foreach (var info in characterInfo)
                charInfo += "Char: " + info.Key + ", Row: " + info.Value.Y + ", Col: " + info.Value.X + "\n"; //TODO x and y in print may be wrong

            return string.Format("Path: \"{0}\", Grid Width: {1} Grid Height: {2}\nCharInfo:\n{3}", path, gridWidth, gridHeight, charInfo);
        }

        public string CompactInfo()
        {
            string charInfo = "";
            foreach (var info in characterInfo)
                charInfo += info.Key;

            return string.Format("Path: \"{0}\",Grid  Width: {1} Grid Height: {2} Possible Chars: {3}", path, gridWidth, gridHeight, charInfo);
        }
    }
}

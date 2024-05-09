using OpenTK.Mathematics;

namespace SharkUI
{
    internal class TextureAtlas
    {
        public string path = "";
        public int gridWidth, gridHeight;
        public Dictionary<char, Vector2> characterInfo = [];
        public SharkUITexture texture = new();
        public Vector4 charMask;

        public TextureAtlas() {}
        public void Init(string path, int gridWidth, int gridHeight, Vector4 charMask, Dictionary<char,Vector2> characterInfo) => Init(path, gridWidth, gridHeight, OpenTK.Graphics.OpenGL4.TextureUnit.Texture0, charMask, characterInfo);
        public void Init(string path, int gridWidth, int gridHeight, OpenTK.Graphics.OpenGL4.TextureUnit textureSlot, Vector4 charMask, Dictionary<char,Vector2> characterInfo)
        {
            this.path = path;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.charMask = charMask;
            this.characterInfo = characterInfo;
            texture.Init(path, textureSlot);
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

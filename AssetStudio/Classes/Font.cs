namespace AssetStudio;

public sealed class Font : NamedObject
{
    public byte[] m_FontData;

    public Font(ObjectReader reader) : base(reader)
    {
        if (version[0] == 5 && version[1] >= 5 || version[0] > 5) //5.5 and up
        {
            var m_LineSpacing = reader.ReadSingle();
            var m_DefaultMaterial = new PPtr<Material>(reader);
            var m_FontSize = reader.ReadSingle();
            var m_Texture = new PPtr<Texture>(reader);
            var m_AsciiStartOffset = reader.ReadInt32();
            var m_Tracking = reader.ReadSingle();
            var m_CharacterSpacing = reader.ReadInt32();
            var m_CharacterPadding = reader.ReadInt32();
            var m_ConvertCase = reader.ReadInt32();
            var m_CharacterRects_size = reader.ReadInt32();
            for (var i = 0; i < m_CharacterRects_size; i++)
                reader.Position += 44; //CharacterInfo data 41
            var m_KerningValues_size = reader.ReadInt32();
            for (var i = 0; i < m_KerningValues_size; i++)
                reader.Position += 8;
            var m_PixelScale = reader.ReadSingle();
            var m_FontData_size = reader.ReadInt32();
            if (m_FontData_size > 0)
                m_FontData = reader.ReadBytes(m_FontData_size);
        }
        else
        {
            var m_AsciiStartOffset = reader.ReadInt32();

            if (version[0] <= 3)
            {
                var m_FontCountX = reader.ReadInt32();
                var m_FontCountY = reader.ReadInt32();
            }

            var m_Kerning = reader.ReadSingle();
            var m_LineSpacing = reader.ReadSingle();

            if (version[0] <= 3)
            {
                var m_PerCharacterKerning_size = reader.ReadInt32();
                for (var i = 0; i < m_PerCharacterKerning_size; i++)
                {
                    var first = reader.ReadInt32();
                    var second = reader.ReadSingle();
                }
            }
            else
            {
                var m_CharacterSpacing = reader.ReadInt32();
                var m_CharacterPadding = reader.ReadInt32();
            }

            var m_ConvertCase = reader.ReadInt32();
            var m_DefaultMaterial = new PPtr<Material>(reader);

            var m_CharacterRects_size = reader.ReadInt32();
            for (var i = 0; i < m_CharacterRects_size; i++)
            {
                var index = reader.ReadInt32();
                //Rectf uv
                var uvx = reader.ReadSingle();
                var uvy = reader.ReadSingle();
                var uvwidth = reader.ReadSingle();
                var uvheight = reader.ReadSingle();
                //Rectf vert
                var vertx = reader.ReadSingle();
                var verty = reader.ReadSingle();
                var vertwidth = reader.ReadSingle();
                var vertheight = reader.ReadSingle();
                var width = reader.ReadSingle();

                if (version[0] >= 4)
                {
                    var flipped = reader.ReadBoolean();
                    reader.AlignStream();
                }
            }

            var m_Texture = new PPtr<Texture>(reader);

            var m_KerningValues_size = reader.ReadInt32();
            for (var i = 0; i < m_KerningValues_size; i++)
            {
                int pairfirst = reader.ReadInt16();
                int pairsecond = reader.ReadInt16();
                var second = reader.ReadSingle();
            }

            if (version[0] <= 3)
            {
                var m_GridFont = reader.ReadBoolean();
                reader.AlignStream();
            }
            else
            {
                var m_PixelScale = reader.ReadSingle();
            }

            var m_FontData_size = reader.ReadInt32();
            if (m_FontData_size > 0)
                m_FontData = reader.ReadBytes(m_FontData_size);
        }
    }
}

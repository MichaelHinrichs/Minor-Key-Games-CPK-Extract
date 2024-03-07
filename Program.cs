//Written for games in the Rosa engine, by minorkeygames. https://minorkeygames.com/
//Eldrich https://store.steampowered.com/app/252630/
//NEON STRUCT https://store.steampowered.com/app/310740/
//Slayer Shock https://store.steampowered.com/app/501860/
//EPONYMOUS https://store.steampowered.com/app/655270/
//NEON STRUCT Carrion Carrier https://store.steampowered.com/app/1417090/
//NEON STRUCT Desperation Column https://store.steampowered.com/app/1848200/

using System.IO;
using System.IO.Compression;

namespace Minor_Key_Games_CPK_Extract
{
    class Program
    {
        static void Main(string[] args)
        {
            using FileStream input = File.OpenRead(args[0]);
            BinaryReader br = new(input);

            if (new(br.ReadChars(4) != "DCPK")
                throw new System.Exception("Not a Minor Key Games CPK file.");

            int fileNum = br.ReadInt32();
            uint fileDataOffset = br.ReadUInt32();
            uint fileDataSize = br.ReadUInt32();
            System.Collections.Generic.List<SUBFILE> subfiles = new();
            for (int i = 0; i < fileNum; i++)
                subfiles.Add(Subfile(br));

            foreach (SUBFILE subfile in subfiles)
            {
                br.BaseStream.Position = subfile.Offset + fileDataOffset;

                Directory.CreateDirectory(Path.GetDirectoryName(input.Name) + "//" + Path.GetDirectoryName(subfile.Name));
                using FileStream FS = File.Create(Path.GetDirectoryName(input.Name) + "//" + subfile.Name);
                BinaryWriter bw = new(FS);

                if (subfile.isCompressed == 1)
                {
                    MemoryStream ms = new();
                    br.ReadInt16();
                    using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(subfile.sizeCompressed)), CompressionMode.Decompress))
                        ds.CopyTo(ms);
                    br = new(ms);
                    br.BaseStream.Position = 0;
                }

                bw.Write(br.ReadBytes(subfile.sizeUncompressed));
                bw.Close();
                br = new(input);
            }
            br.Close();
        }

        public static SUBFILE Subfile(BinaryReader br)
        {
            string Name = new(br.ReadChars(br.ReadInt32()));
            uint Offset = br.ReadUInt32();
            int sizeCompressed = br.ReadInt32();
            int sizeUncompressed = br.ReadInt32();

            if (sizeUncompressed == -1)//padding in Neon STRUCT
                sizeUncompressed = br.ReadInt32();

            return new SUBFILE()
            {
                Name = Name.TrimEnd((char)0x00),
                Offset = Offset,
                sizeCompressed = sizeCompressed,
                sizeUncompressed = sizeUncompressed,
                isCompressed = br.ReadInt32()
            };
        }

        public struct SUBFILE
        {
            public string Name;
            public uint Offset;
            public int sizeCompressed;
            public int sizeUncompressed;
            public int isCompressed;
        }
    }
}

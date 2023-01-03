using Force.Crc32;
using K4os.Compression.LZ4;
using System;
using System.IO;
using System.Text;
using System.Linq;

namespace WoTB_Mod_Creater
{
    public class DVPL
    {
        public class DVPLFooterData
        {
            public uint OSize { get; set; }
            public uint CSize { get; set; }
            public uint Crc32 { get; set; }
            public uint Type { get; set; }
        }
        public static void Resources_Extract(string FileName)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/dll"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/dll");
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("WoTB_Voice_Mod_Creater.Resources." + FileName))
            {
                using (FileStream bw = new FileStream(Directory.GetCurrentDirectory() + "/dll/" + FileName, FileMode.Create))
                {
                    while (stream.Position < stream.Length)
                    {
                        byte[] bits = new byte[stream.Length];
                        stream.Read(bits, 0, (int)stream.Length);
                        bw.Write(bits, 0, (int)stream.Length);
                    }
                    bw.Close();
                }
                stream.Close();
            }
        }
        private static DVPLFooterData ReadDVPLFooter(byte[] buffer)
        {
            byte[] footerBuffer = buffer.Reverse().Take(20).Reverse().ToArray();
            byte[] DVPLTypeBytes = footerBuffer.Reverse().Take(4).Reverse().ToArray();
            string DVPLTypeCheck = Encoding.UTF8.GetString(DVPLTypeBytes);
            if (DVPLTypeCheck != "DVPL")
                throw new Exception("Invalid DVPL Footer");
            DVPLFooterData dataThatWereRead = new DVPLFooterData
            {
                OSize = BitConverter.ToUInt32(footerBuffer, 0),
                CSize = BitConverter.ToUInt32(footerBuffer, 4),
                Crc32 = BitConverter.ToUInt32(footerBuffer, 8),
                Type = BitConverter.ToUInt32(footerBuffer, 12)
            };
            return dataThatWereRead;
        }
        public static bool DVPL_Pack(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (!File.Exists(From_File))
                return false;
            try
            {
                if (Path.GetExtension(From_File) == ".tex")
                    CREATE_DVPL(LZ4Level.L00_FAST, From_File, To_File, IsFromFileDelete);
                else
                    CREATE_DVPL(LZ4Level.L03_HC, From_File, To_File, IsFromFileDelete);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool DVPL_Pack(byte[] File_Buffer, string To_File, bool IsTex)
        {
            try
            {
                if (IsTex)
                    CREATE_DVPL(LZ4Level.L00_FAST, File_Buffer, To_File);
                else
                    CREATE_DVPL(LZ4Level.L03_HC, File_Buffer, To_File);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static void DVPL_UnPack(string From_File, string To_File, bool IsFromFileDelete)
        {
            if (File.Exists(From_File))
            {
                DVPL_UnPack(File.ReadAllBytes(From_File), To_File);
                if (IsFromFileDelete)
                    File.Delete(From_File);
            }
        }
        public static void DVPL_UnPack(byte[] Buffer, string To_File)
        {
            DVPLFooterData footerData = ReadDVPLFooter(Buffer);
            byte[] targetBlock = Buffer.Reverse().Skip(20).Reverse().ToArray();
            if (targetBlock.Length != footerData.CSize)
                throw new Exception("DVPL Size Mismatch");
            if (Crc32Algorithm.Compute(targetBlock) != footerData.Crc32)
                throw new Exception("DVPL CRC32 Mismatch");
            if (footerData.Type == 0)
            {
                if (!(footerData.OSize == footerData.CSize && footerData.Type == 0))
                    throw new Exception("DVPL(圧縮タイプ)のサイズが不一致です。");
                File.WriteAllBytes(To_File, targetBlock);
            }
            else if (footerData.Type == 1 || footerData.Type == 2)
            {
                byte[] deDVPLBlock = new byte[footerData.OSize];
                int i = LZ4Codec.Decode(targetBlock, deDVPLBlock);
                if (i == -1)
                    throw new Exception("DVPL(圧縮タイプ)のデコードサイズが不一致です。");
                File.WriteAllBytes(To_File, deDVPLBlock);
            }
            else throw new Exception("フォーマットが正しくありません。");
        }
        public static void CREATE_DVPL(LZ4Level COMPRESSION_TYPE, string From_File, string ToFile, bool IsFromFileDelete)
        {
            byte[] ORIGINAL_DATA = File.ReadAllBytes(From_File);
            CREATE_DVPL(COMPRESSION_TYPE, ORIGINAL_DATA, ToFile);
            if (IsFromFileDelete)
                File.Delete(From_File);
        }
        public static void CREATE_DVPL(LZ4Level COMPRESSION_TYPE, byte[] buffer, string ToFile)
        {
            int ORIGINAL_SIZE = buffer.Length;
            byte[] LZ4_CONTENT = new byte[LZ4Codec.MaximumOutputSize(ORIGINAL_SIZE)];
            int LZ4_SIZE = LZ4Codec.Encode(buffer, LZ4_CONTENT, COMPRESSION_TYPE);
            if (COMPRESSION_TYPE == LZ4Level.L00_FAST)
            {
                Buffer.BlockCopy(LZ4_CONTENT, 2, LZ4_CONTENT, 0, LZ4_CONTENT.Length - 2);
                LZ4_SIZE -= 2;
            }
            Array.Resize(ref LZ4_CONTENT, LZ4_SIZE);
            byte[] DVPL_CONTENT = FORMAT_WG_DVPL(LZ4_CONTENT, LZ4_SIZE, ORIGINAL_SIZE, COMPRESSION_TYPE);
            File.WriteAllBytes(ToFile, DVPL_CONTENT);
        }
        public static byte[] FORMAT_WG_DVPL(byte[] LZ4_CONTENT, int LZ4_SIZE, int ORIGINAL_SIZE, LZ4Level COMPRESSION_TYPE)
        {
            uint LZ4_CRC32 = Crc32Algorithm.Compute(LZ4_CONTENT);
            byte[] DVPL_TEXT = Encoding.UTF8.GetBytes("DVPL");
            ushort COMPRESSION_TYPE_USHORT = (ushort)COMPRESSION_TYPE;
            if (COMPRESSION_TYPE != LZ4Level.L00_FAST)
                COMPRESSION_TYPE_USHORT -= 1;
            byte[] DVPL_CONTENT = new byte[LZ4_CONTENT.Length + sizeof(uint) * 3 + sizeof(ushort) * 2 + DVPL_TEXT.Length];
            int OFFSET_ACCUMULATOR = 0;
            Buffer.BlockCopy(LZ4_CONTENT, 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, LZ4_CONTENT.Length);
            OFFSET_ACCUMULATOR += LZ4_CONTENT.Length;
            Buffer.BlockCopy(BitConverter.GetBytes((uint)ORIGINAL_SIZE), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(uint));
            OFFSET_ACCUMULATOR += sizeof(uint);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)LZ4_SIZE), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(uint));
            OFFSET_ACCUMULATOR += sizeof(uint);
            Buffer.BlockCopy(BitConverter.GetBytes(LZ4_CRC32), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(uint));
            OFFSET_ACCUMULATOR += sizeof(uint);
            Buffer.BlockCopy(BitConverter.GetBytes(COMPRESSION_TYPE_USHORT), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(ushort));
            OFFSET_ACCUMULATOR += sizeof(ushort);
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)0), 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, sizeof(ushort));
            OFFSET_ACCUMULATOR += sizeof(ushort);
            Buffer.BlockCopy(DVPL_TEXT, 0, DVPL_CONTENT, OFFSET_ACCUMULATOR, DVPL_TEXT.Length);
            return DVPL_CONTENT;
        }
    }
}
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace WoTB_Mod_Creater.Class
{
    public enum Message_Length
    {
        Long,
        Short
    }
    public interface IMessage
    {
        void Message_Alert(string Message, Message_Length Length_Type);
    }
    public interface IFileSystem
    {
        string GetExDir();
        void UnZip(string filePath, string folderPath);
        void ExtractToFile(ZipArchiveEntry entry, string filePath);
    }
    public interface IAndroidSystem
    {
        string Get_Device_ID();
        bool Get_File_Permission();
        List<string> Get_Applications();
        bool Get_WoTB_Exist();
        string Get_WoTB_Path();
        Stream Get_Drive_API();
    }
}
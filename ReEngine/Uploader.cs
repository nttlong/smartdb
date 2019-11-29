using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReEngine
{
    public static class Uploader
    {
        static Dictionary<string, UploadEntry> Cache = new Dictionary<string, UploadEntry>();
        public static string RegisterUpload(string Token,string AppDirectory, string tempDir, string action,int FileSize)
        {
            var uploadEntry = new UploadEntry(tempDir, action);
            uploadEntry.Id = Guid.NewGuid().ToString();
            uploadEntry.FileSize = FileSize;
            uploadEntry.Token = Token;
            uploadEntry.AppDirectory = AppDirectory;
            uploadEntry.ActionPath = action;
            Cache.Add(uploadEntry.Id, uploadEntry);
            return uploadEntry.Id;
        }
        public static UpLoadInfo UpLoadChunk(string RegisterId,string Data)
        {
            var bff = Convert.FromBase64String(Data);
            UploadEntry entry = GetEntry(RegisterId);
            string path = Path.Join(entry.UploadDirectory,RegisterId);
            long Length = 0;
            if (!File.Exists(path))
            {
                using (FileStream sw = File.Create(path))
                {
                    sw.Write(bff,0,bff.Length);
                    Length = bff.Length;
                }
            }
            else
            {
                using (FileStream sw = File.OpenWrite(path))
                {
                    sw.Position = sw.Length;
                    sw.Write(bff, 0, bff.Length);
                    Length = sw.Length;
                }
                
            }
            if (Length == entry.FileSize)
            {
                var scriptRunner = ScriprLoader.Load(entry.AppDirectory, entry.ActionPath);
                
            }
            return new UpLoadInfo
            {
                Id=RegisterId,
                Length=Length,
                Percent=(entry.FileSize/Length)*100
            };

        }

        private static UploadEntry GetEntry(string registerId)
        {
            return Cache[registerId];
        }
    }
}

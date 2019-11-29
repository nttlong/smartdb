using System.IO;

namespace ReEngine
{
    internal class UploadEntry
    {
        private string tempDir;
        private string action;

        public UploadEntry(string tempDir, string action)
        {
            this.tempDir = tempDir;
            this.action = action;
        }

        public string Id { get; internal set; }
        public string UploadDirectory
        {
            get
            {
                return this.tempDir;
            }
        }

        public int FileSize { get; internal set; }
        public string Token { get; internal set; }
        public string AppDirectory { get; internal set; }
        public string ActionPath { get; internal set; }
    }
}
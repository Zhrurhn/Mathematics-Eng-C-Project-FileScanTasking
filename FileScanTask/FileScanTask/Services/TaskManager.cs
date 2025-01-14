using System.IO;
using System.Security.Cryptography;

namespace FileScanTask
{
    public class TaskManager
    {
        private readonly List<FileScan> _queue = new();

        public void AddToQueue(string filePath)
        {
            var hash = ComputeFileHash(filePath);
            if (string.IsNullOrEmpty(hash))
            {
                throw new Exception($"Hash could not be generated for file: {Path.GetFileName(filePath)}");
            }

            _queue.Add(new FileScan(filePath, hash));
        }

        public IEnumerable<FileScan> GetAllFiles()
        {
            return _queue;
        }

        public void RemoveFromQueue(FileScan file)
        {
            _queue.Remove(file);
        }

        public bool IsQueueEmpty()
        {
            return _queue.Count == 0;
        }

        private string ComputeFileHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = new BufferedStream(File.OpenRead(filePath), 1200000);
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        }
    }
}

using System.IO;

namespace FileScanTask
{
    public class FileScan
    {
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public string Hash { get; }

        public FileScan(string filePath, string hash)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        public string GetDisplayText()
        {
            return $"{FileName} - {GetMaskedHash()}";
        }
        public string GetMaskedHash()
        {
            if (string.IsNullOrEmpty(Hash) || Hash.Length < 8)
            {
                return "*****"; // Eğer hash bilgisi geçerli değilse, bir varsayılan değer döndür
            }

            return $"{Hash.Substring(0, 4)}***{Hash.Substring(Hash.Length - 4)}"; // İlk 4 ve son 4 karakteri göster
        }
    }




}

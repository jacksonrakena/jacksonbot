using System.IO;

namespace Abyss.Core.Results
{
    public struct FileAttachment
    {
        private FileAttachment(Stream stream, string filename)
        {
            Stream = stream;
            Filename = filename;
        }

        public Stream Stream { get; }
        public string Filename { get; }

        public static FileAttachment FromStream(Stream stream, string filename)
        {
            return new FileAttachment(stream, filename);
        }

        public override bool Equals(object? obj)
        {
            return obj is FileAttachment fa && fa.Filename == Filename;
        }

        public static bool operator ==(FileAttachment left, FileAttachment right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FileAttachment left, FileAttachment right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Filename.GetHashCode();
        }
    }
}
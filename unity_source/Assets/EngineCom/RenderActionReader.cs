using AbyssCLI.ABI;
using System;
using System.IO;

namespace EngineCom
{
    public class RenderActionReader
    {
        private readonly BinaryReader _reader;
        public RenderActionReader(Stream stream)
        {
            _reader = new(stream);
        }

        public bool TryRead(out RenderAction result)
        {
            try
            {
                int length = _reader.ReadInt32();
                if (length <= 0)
                    throw new Exception("invalid length message");

                byte[] data = _reader.ReadBytes(length);
                result = RenderAction.Parser.ParseFrom(data);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
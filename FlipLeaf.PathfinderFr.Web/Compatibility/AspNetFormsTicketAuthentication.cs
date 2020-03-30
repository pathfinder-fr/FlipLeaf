using System;
using System.IO;

namespace FlipLeaf.Compatibility
{
    public sealed class AspNetFormsTicketAuthentication
    {
        public AspNetFormsTicketAuthentication(int version, string name, DateTime issueDateUtc, DateTime expirationUtc, bool isPersistent, string userData, string cookiePath)
        {
            Version = version;
            Name = name;
            IssueDateUtc = issueDateUtc;
            ExpirationUtc = expirationUtc;
            IsPersistent = isPersistent;
            UserData = userData;
            CookiePath = cookiePath;
        }
        public int Version { get; }

        public string Name { get; }

        public DateTime IssueDateUtc { get; }

        public DateTime ExpirationUtc { get; }

        public bool IsPersistent { get; }

        public string UserData { get; }

        public string CookiePath { get; }

        private sealed class SerializingBinaryReader : BinaryReader
        {
            public SerializingBinaryReader(Stream input)
                : base(input)
            {
            }

            public string ReadBinaryString()
            {
                var num = Read7BitEncodedInt();
                var array = ReadBytes(num * 2);
                var array2 = new char[num];

                for (var i = 0; i < array2.Length; i++)
                {
                    array2[i] = (char)(array[2 * i] | (array[2 * i + 1] << 8));
                }

                return new string(array2);
            }

            public override string ReadString()
            {
                throw new NotImplementedException();
            }
        }

        public static AspNetFormsTicketAuthentication Deserialize(byte[] serializedTicket, int serializedTicketLength)
        {
            using var memoryStream = new MemoryStream(serializedTicket);
            using var serializingBinaryReader = new SerializingBinaryReader(memoryStream);

            var separator = serializingBinaryReader.ReadByte();
            if (separator != 1)
            {
                return null;
            }

            int version = serializingBinaryReader.ReadByte();
            var ticks = serializingBinaryReader.ReadInt64();
            var issueDateUtc = new DateTime(ticks, DateTimeKind.Utc);
            var dateTime = issueDateUtc.ToLocalTime();

            separator = serializingBinaryReader.ReadByte();
            if (separator != 254)
            {
                return null;
            }

            var ticks2 = serializingBinaryReader.ReadInt64();
            var expirationUtc = new DateTime(ticks2, DateTimeKind.Utc);
            var dateTime2 = expirationUtc.ToLocalTime();

            bool isPersistent;
            switch (serializingBinaryReader.ReadByte())
            {
                case 0:
                    isPersistent = false;
                    break;
                case 1:
                    isPersistent = true;
                    break;
                default:
                    return null;
            }

            var name = serializingBinaryReader.ReadBinaryString();
            var userData = serializingBinaryReader.ReadBinaryString();
            var cookiePath = serializingBinaryReader.ReadBinaryString();
            separator = serializingBinaryReader.ReadByte();

            if (separator != byte.MaxValue)
            {
                return null;
            }

            if (memoryStream.Position != serializedTicketLength)
            {
                return null;
            }

            return new AspNetFormsTicketAuthentication(version, name, issueDateUtc, expirationUtc, isPersistent, userData, cookiePath);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSME_Websocket
{
    internal static class BinaryReaderExtensions
    {
        public static MessageData ReadData(this BinaryReader reader)
        {
            var dataType = reader.ReadByte();
            var length = reader.ReadByte();
            var date = reader.ReadBytes(length);
            return new MessageData(dataType, length, date);
        }
    }
}

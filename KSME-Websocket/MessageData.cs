using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSME_Websocket
{
    internal class MessageData
    {
        internal MessageData(byte dataType, byte length, byte[] data)
        {
            DataType = dataType;
            Length = length;
            Data = data;
        }

        public byte DataType { get; set; }
        public byte Length { get; set; }

        public byte[] Data { get; set; }




        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('(');
            stringBuilder.Append(DataType);
            stringBuilder.Append(':');
            stringBuilder.Append(Length);
            stringBuilder.Append('\t');


            //DataType 26 = TimeStamp with value?
            if (DataType == 26)
            {
                var timestamp1 = BitConverter.ToUInt16(Data[0..2].Reverse().ToArray());

                var timeStamp = BitConverter.ToUInt32(Data, 1);
                var timeStampValue = BitConverter.ToUInt32(Data, 7);
                var timeStampPhaseId = Length >= 12 ? (byte?)Data[11] : null;

                stringBuilder.Append(Data[0]);
                stringBuilder.Append('\t');
                stringBuilder.Append(timeStamp);
                stringBuilder.Append('\t');
                stringBuilder.Append(Data[5]);
                stringBuilder.Append('\t');
                stringBuilder.Append(Data[6]);
                stringBuilder.Append('\t');
                stringBuilder.Append(timeStampValue);
                stringBuilder.Append('\t');
                stringBuilder.Append(timeStampPhaseId);
                stringBuilder.Append('\t');
            }
            else if (DataType == 34)
            {
                stringBuilder.Append(BitConverter.ToString(Data));
            }

            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }
    }
}

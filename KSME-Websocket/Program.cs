// See https://aka.ms/new-console-template for more information
using KSEM_Client;
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using System.Threading;



var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var password = configuration["KsemPassword"];
if (password == null) throw new ArgumentNullException(nameof(password));

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;




var ksemClient = new KSEMClient("http://ksem-76555758");

await ksemClient.LoginAsync(password);
if (ksemClient._loginResponseData == null || string.IsNullOrEmpty(ksemClient._loginResponseData.AccessToken)) throw new Exception("you have to login first");


var config = await ksemClient.GetProtoBuf(cancellationToken);
var configFS = new FileStream("config.dat", FileMode.Create);
await configFS.WriteAsync(config, cancellationToken);
configFS.Close();
await configFS.DisposeAsync();

var socket = new ClientWebSocket();
await socket.ConnectAsync(new Uri("ws://ksem-76555758/api/data-transfer/ws/protobuf/gdr/local/values/smart-meter"), cancellationToken);

var data = System.Text.Encoding.UTF8.GetBytes("Bearer " + ksemClient._loginResponseData.AccessToken);
await socket.SendAsync(data, WebSocketMessageType.Text, true, cancellationToken);

var buff = new byte[1024];


var fs = new StreamWriter("dataList.binary");
cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(120));
ushort prevTimestamp = 0;

try
{

    while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buff, cancellationToken);
        var count = result.Count;



        var hexString = string.Join(' ', buff.Take(count));
        await fs.WriteLineAsync(hexString);


        var ms = new MemoryStream(buff, 0, count);

        var reader = new BinaryReader(ms);

        //smartmeter pos1
        var lineFeed1 = reader.ReadChar();

        //134 ? ???
        //135 ? ???
        //136 ? ???
        //137 = EnergyPhase?
        var messageType1 = reader.ReadByte();
        //if (messageType1 != 137) continue;


        if (messageType1 == 134)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
        else if (messageType1 == 135)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
        }
        else if (messageType1 == 136)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
        }
        else if (messageType1 == 137)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
        }
        else
        {
            Console.ResetColor();
        }

        Console.Write(DateTime.Now);
        Console.Write('\t');
        Console.Write(messageType1);
        Console.Write('\t');




        var accept1 = reader.ReadByte();
        var lineFeed2 = reader.ReadChar();

        //as erste mal smart-meter
        var smartMeterText1 = reader.ReadString();

        var steuerrkennzeichen1 = reader.ReadByte();
        var messageType2 = reader.ReadByte();
        var steuerrkennzeichen2 = reader.ReadByte();

        var lineFeed3 = reader.ReadChar();

        //as zweite mal smart-meter
        var smartMeterText2 = reader.ReadString();

        var dle1 = reader.ReadByte();
        var startOfHeading1 = reader.ReadByte();


        //DataType 26 = TimeStamp with value?
        var dataType1 = reader.ReadByte();
        Console.Write(dataType1);
        Console.Write('\t');


        var lengthTimestamp = reader.ReadByte();
        var timeStampBuffer = reader.ReadBytes(lengthTimestamp);

        var timestamp1 = BitConverter.ToUInt16(timeStampBuffer[0..2].Reverse().ToArray());
        //if (timestamp1 == prevTimestamp) continue;
        prevTimestamp = timestamp1;

        var timeStamp = BitConverter.ToUInt32(timeStampBuffer, 1);
        var timeStampValue = BitConverter.ToUInt32(timeStampBuffer, 7);
        var timeStampPhaseId = lengthTimestamp >= 12 ? (byte?)timeStampBuffer[11] : null;

        Console.Write(timeStampBuffer[0]);
        Console.Write('\t');
        Console.Write(timeStamp);
        Console.Write('\t');
        Console.Write(timeStampBuffer[5]);
        Console.Write('\t');
        Console.Write(timeStampBuffer[6]);
        Console.Write('\t');
        Console.Write(timeStampValue);
        Console.Write('\t');
        Console.Write(timeStampPhaseId);
        Console.Write('\t');



        //DataType 34 = ???
        var dataType2 = reader.ReadByte();
        Console.Write(dataType2);
        Console.Write('\t');
        var lengthDataType2 = reader.ReadByte();
        Console.Write(lengthDataType2);
        Console.Write('\t');
        var dataType2Buffer = reader.ReadBytes(lengthDataType2);

        Console.Write('\t');
        Console.Write(BitConverter.ToString(dataType2Buffer));
        Console.Write('\t');




        Console.WriteLine();

        //Console.WriteLine(reader.ReadInt16() + ", " + reader.ReadInt16() + ", " + reader.ReadInt16());

        //ms.Position = 0;
        //Console.WriteLine(reader.ReadUInt16() + ", " + reader.ReadUInt16() + ", " + reader.ReadUInt16());

        //ms.Position = 0;
        //Console.WriteLine(reader.ReadInt32() + ", " + reader.ReadInt32() + ", " + reader.ReadInt32());

        //ms.Position = 0;
        //Console.WriteLine(reader.ReadUInt32() + ", " + reader.ReadUInt32() + ", " + reader.ReadUInt32());

    }

}
catch (TaskCanceledException) { }
catch (Exception)
{
    throw;
}
finally
{
    fs.Close();
    await fs.DisposeAsync();
}


// See https://aka.ms/new-console-template for more information
using KSEM_Client;
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using System.Threading;

Console.WriteLine("Hello, World!");


var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var password = configuration["KsemPassword"];
if (password == null) throw new ArgumentNullException(nameof(password));

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;




var ksmeClient = new KSEMClient("http://ksem-76555758");

await ksmeClient.LoginAsync(password);
if (ksmeClient._loginResponseData == null || string.IsNullOrEmpty(ksmeClient._loginResponseData.AccessToken)) throw new Exception("you have to login first");



var socket = new ClientWebSocket();
await socket.ConnectAsync(new Uri("ws://ksem-76555758/api/data-transfer/ws/protobuf/gdr/local/values/smart-meter"), cancellationToken);

var data = System.Text.Encoding.UTF8.GetBytes("Bearer " + ksmeClient._loginResponseData.AccessToken);
await socket.SendAsync(data, WebSocketMessageType.Text, true, cancellationToken);

var buff = new byte[1024];

while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
{
    var result = await socket.ReceiveAsync(buff, cancellationToken);
    var count = result.Count;

    var fs = new FileStream("data.binary", FileMode.Create);
    await fs.WriteAsync(buff, 0, count);
    fs.Close();
    await fs.DisposeAsync();


    var ms = new MemoryStream(buff, 0, count);

    var reader = new BinaryReader(ms);

    //smartmeter pos1
    ms.Position = 4;
    var smartMeterText1 = reader.ReadString();
    Console.Write(smartMeterText1);
    Console.Write('\t');
    Console.Write(reader.ReadInt64());

    //smartmeter pos2
    ms.Position = 20;
    var smartMeterText2 = reader.ReadString();
    Console.Write(smartMeterText2);
    Console.Write('\t');
    ms.Position = 96;
    Console.Write(reader.ReadHalf());


    Console.WriteLine();

    //Console.WriteLine(reader.ReadInt16() + ", " + reader.ReadInt16() + ", " + reader.ReadInt16());

    //ms.Position = 0;
    //Console.WriteLine(reader.ReadUInt16() + ", " + reader.ReadUInt16() + ", " + reader.ReadUInt16());

    //ms.Position = 0;
    //Console.WriteLine(reader.ReadInt32() + ", " + reader.ReadInt32() + ", " + reader.ReadInt32());

    //ms.Position = 0;
    //Console.WriteLine(reader.ReadUInt32() + ", " + reader.ReadUInt32() + ", " + reader.ReadUInt32());

}

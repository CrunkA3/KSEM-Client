﻿// See https://aka.ms/new-console-template for more information
using KSEM_Client;
using KSME_Websocket;
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


cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(120));

try
{

    while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
    {
        var result = await socket.ReceiveAsync(buff, cancellationToken);
        var count = result.Count;



        var hexString = string.Join(' ', buff.Take(count));


        var ms = new MemoryStream(buff, 0, count);

        var reader = new BinaryReader(ms);

        //smartmeter pos1
        var lineFeed1 = reader.ReadChar();


        //137 = EnergyPhase?
        var messageType1 = reader.ReadByte();
        //if (messageType1 != 137) continue;
        var fs = new StreamWriter($"messages_{messageType1}.dat", true);

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
        fs.Write(DateTime.Now.ToString());
        fs.Write('\t');

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


        while (reader.BaseStream.Position < count)
        {
            var messageData = reader.ReadData();
            fs.Write('\t');
            fs.Write(messageData.ToString());
            Console.Write('\t');
            Console.Write(messageData.ToString());
        }
        fs.WriteLine();
        Console.WriteLine();
        fs.Close();
    }

}
catch (TaskCanceledException) { }
catch (Exception)
{
    throw;
}
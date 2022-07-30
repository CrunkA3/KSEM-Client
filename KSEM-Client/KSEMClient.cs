using KSEM_Client.Data;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security;
using System.Text.Json;

namespace KSEM_Client;


public class KSEMClient
{
    private string _hostEndpoint;
    public LoginResponseData? _loginResponseData;

    public KSEMClient(string hostEndpoint)
    {
        _hostEndpoint = hostEndpoint + "/api/";
    }


    public async Task LoginAsync(string password, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "emos" },
            { "client_secret", "56951025" },
            { "username", "admin" },
            { "password", password },
        });

        HttpClient httpClient = new();
        Uri requestUri = new(_hostEndpoint + "web-login/token");
        var httpResponse = await httpClient.PostAsync(requestUri, content, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
        _loginResponseData = await httpResponse.Content.ReadFromJsonAsync<LoginResponseData>(cancellationToken: cancellationToken);

    }

    public async Task<DeviceStatus?> GetDeviceStatusAsync(CancellationToken cancellationToken = default)
    {
        EnsureLogin();

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _loginResponseData!.AccessToken);
        Uri requestUri = new(_hostEndpoint + "device-settings/devicestatus");
        var httpResponse = await httpClient.GetAsync(requestUri, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadFromJsonAsync<DeviceStatus>(cancellationToken: cancellationToken);
    }



    public async Task<byte[]> GetProtoBuf(CancellationToken cancellationToken = default)
    {
        EnsureLogin();

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _loginResponseData!.AccessToken);
        Uri requestUri = new(_hostEndpoint + "data-transfer/protobuf/gdr/local/config/smart-meter");
        var httpResponse = await httpClient.GetAsync(requestUri, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken);
    }

    public async Task StartSocketAsync(CancellationToken cancellationToken)
    {
        EnsureLogin();

        var socket = new ClientWebSocket();
        await socket.ConnectAsync(new Uri("ws://ksem-76555758/api/data-transfer/ws/protobuf/gdr/local/values/smart-meter"), cancellationToken);

        var data = System.Text.Encoding.UTF8.GetBytes("Bearer " + _loginResponseData!.AccessToken);
        await socket.SendAsync(data, WebSocketMessageType.Text, true, cancellationToken);

        var buff = new byte[1024];

        var result = await socket.ReceiveAsync(buff, cancellationToken);
        var count = result.Count;
        var resultString = System.Text.Encoding.UTF8.GetString(buff, 0, count);
        Console.WriteLine(resultString);
    }



    private void EnsureLogin()
    {
        if (_loginResponseData == null || string.IsNullOrEmpty(_loginResponseData.AccessToken)) throw new Exception("you have to login first");
    }
}

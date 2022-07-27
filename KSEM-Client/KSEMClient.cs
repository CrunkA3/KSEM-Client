using KSEM_Client.Data;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Security;
using System.Text.Json;

namespace KSEM_Client;


public class KSEMClient
{
    private string _hostEndpoint;
    private LoginResponseData? _loginResponseData;

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
        if (_loginResponseData == null) throw new Exception("you have to login first");

        HttpClient httpClient = new();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _loginResponseData.AccessToken);
        Uri requestUri = new(_hostEndpoint + "device-settings/devicestatus");
        var httpResponse = await httpClient.GetAsync(requestUri, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadFromJsonAsync<DeviceStatus>(cancellationToken: cancellationToken);
    }

}

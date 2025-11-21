using API.DTOs;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
public class DeviceUpdateService( HttpClient _httpClient) : IDeviceUpdateService
{    public async Task<bool> UpdateDeviceAsync(DeviceUpdateDto payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, "api/device/update")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}


using API.DTOs;

public interface IDeviceUpdateService
{
    Task<bool> UpdateDeviceAsync(DeviceUpdateDto payload);
}

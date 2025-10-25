using System;
using System.Linq;
using System.Threading.Tasks;
using API.Common;
using API.DTOs;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Endpoints
{
    public static class DeviceRegistrationEndpoint
    {
        public static RouteGroupBuilder MapDeviceRegistrationEndpoint(this WebApplication app)
        {
            var group = app.MapGroup("/api/device").WithTags("device");

            // POST /api/device/register
            group.MapPost("/register", async (
                AppDbContext db,
                [FromBody] DeviceRegistrationDto dto
            ) =>
            {
                // Validate
                if (string.IsNullOrEmpty(dto.DeviceId) || string.IsNullOrEmpty(dto.UserId))
                {
                    return Results.BadRequest(Response<string>.Failure("DeviceId and UserId are required."));
                }

                var existing = await db.Devices.FirstOrDefaultAsync(d => d.DeviceId == dto.DeviceId);
                if (existing != null)
                {
                    return Results.BadRequest(Response<string>.Failure("Device already registered."));
                }

                var device = new Device
                {
                    DeviceId = dto.DeviceId,
                    DeviceName = dto.DeviceName,
                    UserId = dto.UserId,
                    CameraEnabled = dto.CameraEnabled,
                    MicEnabled = dto.MicEnabled,
                    LocationEnabled = dto.LocationEnabled,
                    SoftwareVersion = dto.SoftwareVersion,
                    RegisteredAt = DateTime.UtcNow
                };

                db.Devices.Add(device);
                await db.SaveChangesAsync();

                return Results.Ok(Response<string>.Success(device.DeviceId, "Device registered successfully."));
            }).DisableAntiforgery();

            // GET /api/device/all
            group.MapGet("/all", async (AppDbContext db) =>
            {
                var devices = await db.Devices
                    .Include(d => d.User)
                    .Select(d => new DevicePermissionsDto
                    {
                        Id = d.DeviceId, // or d.Id, depending on which "id" your frontend expects
                        CameraEnabled = d.CameraEnabled,
                        MicEnabled = d.MicEnabled,
                        LocationEnabled = d.LocationEnabled,
                        SoftwareVersion = d.SoftwareVersion
                    })
                    .ToListAsync();

                return Results.Ok(Response<List<DevicePermissionsDto>>.Success(devices, "Devices fetched successfully."));
            });


            // PATCH /api/device/update
            group.MapPatch("/update", async (
                AppDbContext db,
                [FromBody] DeviceUpdateDto dto
            ) =>
            {
                if (string.IsNullOrEmpty(dto.DeviceId))
                    return Results.BadRequest(Response<string>.Failure("Device ID is required."));

                // Query the device using the UUID string
                var device = await db.Devices
                    .FirstOrDefaultAsync(d => d.DeviceId == dto.DeviceId);

                if (device == null)
                    return Results.NotFound(Response<string>.Failure("Device not found."));

                // Update only provided fields
                if (dto.CameraEnabled.HasValue)
                    device.CameraEnabled = dto.CameraEnabled.Value;

                if (dto.MicEnabled.HasValue)
                    device.MicEnabled = dto.MicEnabled.Value;

                if (dto.LocationEnabled.HasValue)
                    device.LocationEnabled = dto.LocationEnabled.Value;

                if (dto.SoftwareVersion.HasValue)
                    device.SoftwareVersion = dto.SoftwareVersion.Value;

                await db.SaveChangesAsync();

                return Results.Ok(Response<object>.Success(device, "Device updated successfully."));
            }).DisableAntiforgery();



            return group;
        }
    }
}

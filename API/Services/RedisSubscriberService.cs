using API.Data;
using API.DTOs;
using API.Hubs;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Text.Json;

public class RedisSubscriberService(IHubContext<DeltaPayloadHub> _deltaHubContext, RedisPubSubService redisService, IDeviceUpdateService _updateService) : BackgroundService
{ //The job of this service is to listen for messages on a Redis channel in the background and process them accordingly.

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        redisService.Subscribe("DeltaChannel", async (channel, message) =>
        { //basically im writing the handler function here that will be called when a message is received on the subscribed channel by the server instance from redis
            var payload = JsonSerializer.Deserialize<DeviceUpdateDto>(message!)!;
            Console.WriteLine($"Received message from Redis subscription {message}"); //can i also track here which server instance sent the message?

            var updated = await _updateService.UpdateDeviceAsync(payload);
            if (!updated)
            {
                Console.WriteLine("Update API failed.");
                return;
            }

            await _deltaHubContext.Clients.All.SendAsync("ApplyDelta", payload);
        });

        Console.WriteLine("Subscribed to Redis channel 'DeltaChannel'");

        return Task.CompletedTask;
    }
}


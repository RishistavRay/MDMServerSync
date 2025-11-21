using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;


namespace API.Hubs
{
    public class DeltaPayloadHub( RedisPubSubService _redisService, IDeviceUpdateService _updateService) : Hub
    {
        public static readonly ConcurrentDictionary<string, OnlineClientDto> onlineClients = new(); //we are referring to online clients for local server instance.
        public override async Task OnConnectedAsync() //this has to be be here too its a lifecycle method for hub connections
        {
            var httpContext = Context.GetHttpContext();
            var clientId = httpContext?.Request.Query["clientId"].ToString()!;
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrWhiteSpace(clientId))
            {
                Console.WriteLine("Connection Failed");
                await Clients.Caller.SendAsync("Error", "Missing clientId in connection request.");
                Context.Abort(); // disconnects the client
                return;
            }
            Console.WriteLine("Connection Attempt Successful", connectionId);

            if (onlineClients.ContainsKey(clientId))
            {
                //client is already online
                onlineClients[clientId].ConnectionId = connectionId;
            }
            else
            {
                var client = new OnlineClientDto
                {
                    ConnectionId = connectionId,
                    ClientId = clientId,
                    IsOnline = true
                };
                onlineClients.TryAdd(clientId, client);
                await Clients.Caller.SendAsync("RequestReceivedNotify", clientId); //sned clientId as payload
            }

        }


        public async Task applyAndPublishDelta(string clientId, DeviceUpdateDto payload)
        {
            Console.WriteLine("Invoked applyAndPublishDelta method in DeltaPayloadHub");
            Console.WriteLine($"Received delta payload from client {clientId}: {JsonSerializer.Serialize(payload)}");

            //Send to Redis
            await _redisService.PublishAsync("DeltaChannel", payload);

        }
    }
}

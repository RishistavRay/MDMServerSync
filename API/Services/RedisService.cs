using API.DTOs;
using API.Models;
using StackExchange.Redis;
using System.Text.Json;

public class RedisPubSubService //Thenjob of this server is to publish and subscribe to messages in Redis
{
    private readonly IConnectionMultiplexer _redis;
    //IconnectionMultiplexer is used to connect to the Redis server
    //it has functions like GetSubscriber which is used to get the subscriber instance
    private readonly ISubscriber _subscriber;
    //ISubscriber is used to publish and subscribe to messages in Redis


    public RedisPubSubService(IConnectionMultiplexer redis)
    //constructor that takes IConnectionMultiplexer as a parameter
    {
        _redis = redis;
        _subscriber = redis.GetSubscriber();
    }

    public async Task PublishAsync(string channel, DeviceUpdateDto payload)
    {
        var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal); //Pattern is other mode after Literal which is for listening to multiple channels 
        var message = JsonSerializer.Serialize(payload);

        //return _subscriber.PublishAsync(redisChannel, message);

        long count = await _subscriber.PublishAsync(redisChannel, message);

        Console.WriteLine($"[Redis] Published to '{channel}', subscriber count = {count}");
    }
    public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler) //will be called through a background service to listen for messages
    {
        var redisChannel = new RedisChannel(channel, RedisChannel.PatternMode.Literal);
        _subscriber.Subscribe(redisChannel, handler);
    } //handler is simply a function that will be called when a message is received on the subscribed channel
}

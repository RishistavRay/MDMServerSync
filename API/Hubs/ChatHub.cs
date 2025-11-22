using System;
using System.Collections.Concurrent;
using API.Data;
using API.DTOs;
using API.Extenions;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Hubs;

[Authorize]

//Why can this class accept arguments?
//Thats just Syntax sugar for dependency injection for this scenario its the same as defining private readonly fields _userManager and _context and a constructor to initialize them
public class ChatHub(UserManager<AppUser> userManager, AppDbContext context) : Hub
{//UserManager is provided by ASP.NET Identity to manage user related operations like creating user updating user deleting user etc
    //it has functions like FindByNameAsync, FindByIdAsync etc

    public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();
    //onlineUsers is a thread-safe dictionary to store online users with their connection IDs and other details
    //OnlineUserDto is a data transfer object defined in API/DTOs/OnlineUserDto.cs to represent online user details
    //onlineUsers contains all people CURRENTLY online
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext(); //to understand the http request that initiated the connection
        var recevierId = httpContext?.Request.Query["senderId"].ToString();
        var userName = Context.User!.Identity!.Name!; //getting the username of the connected user from the hub context
        var currentUser = await userManager.FindByNameAsync(userName);
        // returns AppUser object if found otherwise null
        var connectionId = Context.ConnectionId;

        if (onlineUsers.ContainsKey(userName))
        {
            //user is already online
            onlineUsers[userName].ConnectionId = connectionId;
        }
        else
        {
            var user = new OnlineUserDto
            {   //user just came online
                ConnectionId = connectionId,
                UserName = userName,
                ProfilePicture = currentUser!.ProfileImage,
                FullName = currentUser!.FullName
            };

            onlineUsers.TryAdd(userName, user);
            //TryAdd is a thread-safe way to add an item to the ConcurrentDictionary
            //Basically if user is already added then it will not add again and continue without throwinf an exception


            await Clients.Others.SendAsync("Notify", currentUser);
            //Notify is the event name that both server and client agree on to communicate
            // currentUser is the payload that will be sent to the client based on their connection
            //Clients is HubCallerContext property to send messages to connected clients
            //look for connection.on("Notify", function(user){}) in frontend
        }

        if (!string.IsNullOrEmpty(recevierId))
        {
            await LoadMessages(recevierId);
        }

        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
        //just sends an event at the end of OnConnectedAsync method
        //look out for connection.on("OnlineUsers", function(users){}) in frotend.
    }

    public async Task LoadMessages(string recipientId, int pageNumber = 1)
    {

        int pageSize = 10;
        var username = Context.User!.Identity!.Name;
        var currentUser = await userManager.FindByNameAsync(username!);

        if (currentUser is null)
        {
            return;
        }

        List<MessageResponseDto> messages = await context.Messages
        .Where(x => x.ReceiverId == currentUser!.Id && x.SenderId == recipientId || x.SenderId == currentUser!.Id && x.ReceiverId == recipientId)
        .OrderByDescending(x => x.CreatedDate)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .OrderBy(x => x.CreatedDate)
        .Select(x => new MessageResponseDto
        {
            Id = x.Id,
            Content = x.Content,
            CreatedDate = x.CreatedDate,
            ReceiverId = x.ReceiverId,
            SenderId = x.SenderId
        })
        .ToListAsync();


        foreach (var message in messages)
        {
            var msg = await context.Messages.FirstOrDefaultAsync(x => x.Id == message.Id);

            if (msg != null && msg.ReceiverId == currentUser.Id)
            {
                msg.IsRead = true;
                await context.SaveChangesAsync();
            }
        }

        await Task.Delay(1000);
        await Clients.User(currentUser.Id)
        .SendAsync("ReceiveMessageList", messages);
    }

    public async Task SendMessage(MessageRequestDto message)
    {
        var senderId = Context.User!.Identity!.Name;
        var recipientId = message.ReceiverId;

        var newMsg = new Message
        {
            Sender = await userManager.FindByNameAsync(senderId!),
            Receiver = await userManager.FindByIdAsync(recipientId!),
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            Content = message.Content
        };

        context.Messages.Add(newMsg);
        //all the messages are stored in a single database for user which contains receiver id and sender id
        //they are then filtered based on the user to show as different chats
        //context is different instance for every hub connection
        await context.SaveChangesAsync(); //takae all the changes and write the changes into database (or db context)

        await Clients.User(recipientId!).SendAsync("ReceiveNewMessage", newMsg);
    }

    public async Task NotifyTyping(string recipientUserName)
    {
        var senderUserName = Context.User!.Identity!.Name;

        if (senderUserName is null)
        {
            return;
        }

        var connectionId = onlineUsers.Values.FirstOrDefault(x => x.UserName == recipientUserName)?.ConnectionId;
        //finding the connection id of the recipient user from the onlineUsers dictionary
        if (connectionId != null)
        {
            await Clients.Client(connectionId).SendAsync("NotifyTypingToUser", senderUserName);
            //if that username is connected to the hub then mention that i am typing because person I'm viewing is the future recepient so tell him im typing even if im not literally typing.
            //send event to that specific client using connectionId
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User!.Identity!.Name;
        onlineUsers.TryRemove(username!, out _); //remove entry from onlineUsers List
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }

    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        var username = Context.User!.GetUserName();

        var onlineUsersSet = new HashSet<string>(onlineUsers.Keys);
        //put all currently online usernames in a hashset for quick lookup
        var users = await userManager.Users.Select(u => new OnlineUserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            ProfilePicture = u.ProfileImage,
            IsOnline = onlineUsersSet.Contains(u.UserName!),
            UnreadCount = context.Messages.Count(x => x.ReceiverId == username && x.SenderId == u.Id && !x.IsRead)
        }).OrderByDescending(u => u.IsOnline)
        .ToListAsync();
        //problem is this function does not filter for conversation histry so it will return all users but ordered by online status.
        //It should also be sorted by time of latest message but thats not the case here.
        return users;
    }


}

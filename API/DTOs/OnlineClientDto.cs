using System;

namespace API.DTOs;

public class OnlineClientDto
{
    public string? ClientId { get; set; }
    public string? ConnectionId { get; set; }
    public bool IsOnline { get; set; }
}

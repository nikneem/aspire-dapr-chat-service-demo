using System.Text.Json.Serialization;
using HexMaster.Chat.Members.Abstractions.DTOs;
using HexMaster.Chat.Members.Abstractions.Events;
using HexMaster.Chat.Members.Abstractions.Requests;
using HexMaster.Chat.Shared.Models;

namespace HexMaster.Chat.Members.Api;

[JsonSerializable(typeof(RegisterMemberRequest))]
[JsonSerializable(typeof(ChatMemberDto))]
[JsonSerializable(typeof(MemberJoinedEvent))]
[JsonSerializable(typeof(MemberLeftEvent))]
[JsonSerializable(typeof(ChatMember))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(DateTimeOffset?))]
[JsonSourceGenerationOptions(WriteIndented = false)]
public partial class MembersApiJsonSerializationContext : JsonSerializerContext
{
}

using HexMaster.Chat.Members.Abstractions.Interfaces;
using HexMaster.Chat.Members.Abstractions.Requests;
using HexMaster.Chat.Members.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Azure Table Storage
builder.AddAzureTableClient("tables");

// Add chat members services
builder.AddChatMembers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

// API endpoints
app.MapPost("/members", async (RegisterMemberRequest request, IMemberService memberService) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
    {
        return Results.BadRequest("Name and email are required");
    }

    var member = await memberService.RegisterMemberAsync(request);
    return Results.Created($"/members/{member.Id}", member);
});

app.MapGet("/members/{id}", async (string id, IMemberService memberService) =>
{
    var member = await memberService.GetMemberAsync(id);
    return member != null ? Results.Ok(member) : Results.NotFound();
});

app.MapPut("/members/{id}/activity", async (string id, IMemberService memberService) =>
{
    await memberService.UpdateLastActivityAsync(id);
    return Results.Ok();
});

app.Run();

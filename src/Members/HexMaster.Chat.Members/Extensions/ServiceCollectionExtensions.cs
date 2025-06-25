using HexMaster.Chat.Members.Abstractions.Interfaces;
using HexMaster.Chat.Members.BackgroundServices;
using HexMaster.Chat.Members.Repositories;
using HexMaster.Chat.Members.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HexMaster.Chat.Members.Extensions;

public static class ServiceCollectionExtensions
{
    public static TBuilder AddChatMembers<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Add services
        builder.Services.AddScoped<IMemberRepository, MemberRepository>();
        builder.Services.AddScoped<IMemberService, MemberService>();

        // Add background services
        builder.Services.AddHostedService<MemberCleanupService>();

        return builder;
    }
}

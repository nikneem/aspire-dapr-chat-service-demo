using HexMaster.Chat.Messages.Abstractions.Interfaces;
using HexMaster.Chat.Messages.BackgroundServices;
using HexMaster.Chat.Messages.Repositories;
using HexMaster.Chat.Messages.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HexMaster.Chat.Messages.Extensions;

public static class ServiceCollectionExtensions
{
    public static TBuilder AddChatMessages<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Add services
        builder.Services.AddScoped<IMessageRepository, MessageRepository>();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IMemberStateService, MemberStateService>();

        // Add background services
        builder.Services.AddHostedService<MessageCleanupService>();

        return builder;
    }
}

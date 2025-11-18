using CommunityToolkit.Aspire.Hosting.Dapr;
using HexMaster.Chat.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(options =>
    {
        options.WithLifetime(ContainerLifetime.Persistent);
    });

var membersTables = storage.AddTables("memberstables");
var messagesTables = storage.AddTables("messagestables");

var redis = builder.AddRedis("redis").WithRedisInsight();

var stateStore = builder.AddDaprStateStore("chatservice-statestore")
    .WaitFor(redis);

var redisHost = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
var redisPort = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);

var pubSub = builder
  .AddDaprPubSub("chatservice-pubsub")
  .WithMetadata(
    "redisHost",
    ReferenceExpression.Create(
      $"{redisHost}:{redisPort}"
    )
  )
  .WaitFor(redis);


// Add microservices with Dapr sidecars
var membersApi = builder.AddProject<Projects.HexMaster_Chat_Members_Api>(AspireConstants.MembersApiName)
    .WithReference(membersTables)
    .WaitFor(membersTables)
    .WaitFor(redis)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(pubSub)
            .WithReference(stateStore);
    });

var messagesApi = builder.AddProject<Projects.HexMaster_Chat_Messages_Api>(AspireConstants.MessagesApiName)
    .WithReference(messagesTables)
    .WaitFor(messagesTables)
    .WaitFor(redis)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(pubSub)
            .WithReference(stateStore);
    });


var realtimeApi = builder.AddProject<Projects.HexMaster_Chat_Realtime_Api>(AspireConstants.RealtimeApiName)
    .WaitFor(redis)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(pubSub);
    });

// Add Node.js Chat Client
builder.AddNpmApp(AspireConstants.ChatClientName, "../../../ChatClient")
    .WaitFor(membersApi)
    .WaitFor(messagesApi)
    .WaitFor(realtimeApi)
    .WithHttpEndpoint(env: "PORT", name: "Chat-Client")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

await builder.Build().RunAsync();

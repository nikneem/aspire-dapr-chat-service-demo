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

// ======================================================================
// INFRASTRUCTURE RESOURCES
// ======================================================================
var redis = builder.AddRedis("redis").WithRedisInsight();

// ======================================================================
// MESSAGING PROVIDER CONFIGURATION
// ======================================================================
// Choose your messaging provider by uncommenting ONE of the options below:

// OPTION 1: Redis Pub/Sub (Currently Active)
var redisHost = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
var redisPort = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);

var pubSub = builder
    .AddDaprPubSub("chatservice-pubsub")
    .WithMetadata(
        "redisHost",
        ReferenceExpression.Create($"{redisHost}:{redisPort}")
    )
    .WaitFor(redis);

var messagingResource = redis; // Reference for WaitFor dependencies

// OPTION 2: RabbitMQ Pub/Sub (Commented Out)
//var username = builder.AddParameter("username", secret: true);
//var password = builder.AddParameter("password", secret: true);
//var rabbitmq = builder.AddRabbitMQ("messaging", username, password)
//    .WithManagementPlugin();

//var rabbitmqHost = rabbitmq.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
//var rabbitmqPort = rabbitmq.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);

//var pubSub = builder
//    .AddDaprPubSub("chatservice-pubsub")
//    .WithMetadata(
//        "host",
//        ReferenceExpression.Create($"amqp://{rabbitmqHost}:{rabbitmqPort}")
//    )
//    .WaitFor(rabbitmq);

//var messagingResource = rabbitmq; // Reference for WaitFor dependencies

// ======================================================================
// DAPR STATE STORE (Always uses Redis)
// ======================================================================
var stateStore = builder.AddDaprStateStore("chatservice-statestore")
    .WaitFor(redis);

// ======================================================================
// MICROSERVICES WITH DAPR SIDECARS
// ======================================================================
var membersApi = builder.AddProject<Projects.HexMaster_Chat_Members_Api>(AspireConstants.MembersApiName)
    .WithReference(membersTables)
    .WaitFor(membersTables)
    .WaitFor(messagingResource)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(pubSub)
            .WithReference(stateStore);
    });

var messagesApi = builder.AddProject<Projects.HexMaster_Chat_Messages_Api>(AspireConstants.MessagesApiName)
    .WithReference(messagesTables)
    .WaitFor(messagesTables)
    .WaitFor(messagingResource)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(pubSub)
            .WithReference(stateStore);
    });


var realtimeApi = builder.AddProject<Projects.HexMaster_Chat_Realtime_Api>(AspireConstants.RealtimeApiName)
    .WaitFor(messagingResource)
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

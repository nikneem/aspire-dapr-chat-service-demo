using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(options =>
    {
        options.WithLifetime(ContainerLifetime.Persistent);
    });

var tables = storage.AddTables("tables");

// Add Dapr
builder.AddDapr();

var daprOptions = new DaprSidecarOptions
{
    ResourcesPaths = [Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "../../../../dapr"))]
};

// Add microservices with Dapr sidecars
var membersApi = builder.AddProject<Projects.HexMaster_Chat_Members_Api>(AspireConstants.MembersApiName)
    .WithReference(tables)
    .WithDaprSidecar(daprOptions);

var messagesApi = builder.AddProject<Projects.HexMaster_Chat_Messages_Api>(AspireConstants.MessagesApiName)
    .WithReference(tables)
    .WithDaprSidecar(daprOptions);


var realtimeApi = builder.AddProject<Projects.HexMaster_Chat_Realtime_Api>(AspireConstants.RealtimeApiName)
    .WithDaprSidecar(daprOptions);


// Add Node.js Chat Client
var chatClient = builder.AddNpmApp(AspireConstants.ChatClientName, "../../../ChatClient")
    .WithReference(membersApi)
    .WithReference(messagesApi)
    .WithReference(realtimeApi)
    .WaitFor(membersApi)
    .WaitFor(messagesApi)
    .WaitFor(realtimeApi)
    .WithHttpEndpoint(env: "PORT", name: "Chat-Client")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();

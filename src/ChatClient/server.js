const express = require('express');
const cors = require('cors');
const path = require('path');
const configLoader = require('./config-loader');

const app = express();
const port = process.env.PORT || 3000;

// Enable CORS for all origins
app.use(cors());

// Serve static files from the public directory
app.use(express.static(path.join(__dirname, 'public')));

// Serve the main chat client
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Configuration endpoint to provide service URLs to the client
app.get('/api/config', (req, res) => {
    const serviceUrls = configLoader.getServiceUrls();
    const clientConfig = configLoader.getClientConfig();
    
    const config = {
        membersApiUrl: serviceUrls.membersApiUrl,
        messagesApiUrl: serviceUrls.messagesApiUrl,
        realtimeApiUrl: serviceUrls.realtimeApiUrl,
        client: clientConfig
    };
    
    res.json(config);
});

// Health check endpoint
app.get('/health', (req, res) => {
    const config = configLoader.getConfig();
    res.json({ 
        status: 'healthy', 
        timestamp: new Date().toISOString(),
        environment: configLoader.getEnvironment(),
        services: configLoader.getServiceUrls()
    });
});

app.listen(port, '0.0.0.0', () => {
    const config = configLoader.getConfig();
    const serviceUrls = configLoader.getServiceUrls();
    
    console.log(`Chat client server running on port ${port}`);
    console.log(`Environment: ${configLoader.getEnvironment()}`);
    console.log(`Title: ${config.client.title}`);
    console.log(`Debug mode: ${config.client.debug}`);
    console.log(`Members API: ${serviceUrls.membersApiUrl}`);
    console.log(`Messages API: ${serviceUrls.messagesApiUrl}`);
    console.log(`Realtime API: ${serviceUrls.realtimeApiUrl}`);
});

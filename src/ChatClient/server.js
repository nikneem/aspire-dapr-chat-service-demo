const express = require('express');
const cors = require('cors');
const path = require('path');

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
    const config = {
        membersApiUrl: process.env.services__membersapi__http__0 || 'http://localhost:5129',
        messagesApiUrl: process.env.services__messagesapi__http__0 || 'http://localhost:5227',
        realtimeApiUrl: process.env.services__realtimeapi__http__0 || 'http://localhost:5206',
    };
    
    res.json(config);
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'healthy', timestamp: new Date().toISOString() });
});

app.listen(port, '0.0.0.0', () => {
    console.log(`Chat client server running on port ${port}`);
    console.log(`Environment: ${process.env.NODE_ENV || 'development'}`);
    if (process.env.services__membersapi__http__0) {
        console.log(`Members API: ${process.env.services__membersapi__http__0}`);
    }
    if (process.env.services__messagesapi__http__0) {
        console.log(`Messages API: ${process.env.services__messagesapi__http__0}`);
    }
    if (process.env.services__realtimeapi__http__0) {
        console.log(`Realtime API: ${process.env.services__realtimeapi__http__0}`);
    }
});

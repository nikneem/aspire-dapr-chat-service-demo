// Build script for production deployment
// This script is used in GitHub Actions to build the Docker image with production configuration

const fs = require('fs');
const path = require('path');

console.log('Starting production build...');

// Set production environment
process.env.NODE_ENV = 'production';

// Verify configuration files exist
const devConfigPath = path.join(__dirname, 'config', 'development.json');
const prodConfigPath = path.join(__dirname, 'config', 'production.json');

if (!fs.existsSync(devConfigPath)) {
    console.error('ERROR: Development configuration file not found!');
    process.exit(1);
}

if (!fs.existsSync(prodConfigPath)) {
    console.error('ERROR: Production configuration file not found!');
    process.exit(1);
}

// Test configuration loading
console.log('Testing configuration loading...');
try {
    const configLoader = require('./config-loader');
    
    console.log('Environment:', configLoader.getEnvironment());
    console.log('Production URLs:', configLoader.getServiceUrls());
    
    const urls = configLoader.getServiceUrls();
    
    if (urls.membersApiUrl !== 'https://members.aspirichat.com' || 
        urls.messagesApiUrl !== 'https://messages.aspirichat.com' ||
        urls.realtimeApiUrl !== 'https://realtime.aspirichat.com') {
        console.error('ERROR: Production configuration not loaded correctly!');
        console.error('Expected production URLs, but got:', urls);
        process.exit(1);
    }
    
    console.log('✅ Production configuration loaded correctly');
    console.log('✅ Production build preparation complete');
    console.log('Ready to build Docker image with production configuration');
    
} catch (error) {
    console.error('ERROR: Configuration test failed!', error);
    process.exit(1);
}

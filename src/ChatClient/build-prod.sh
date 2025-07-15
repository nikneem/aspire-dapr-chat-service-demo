#!/bin/bash

# Build script for production deployment
# This script is used in GitHub Actions to build the Docker image with production configuration

echo "Starting production build..."

# Set production environment
export NODE_ENV=production

# Verify configuration files exist
if [ ! -f "config/development.json" ]; then
    echo "ERROR: Development configuration file not found!"
    exit 1
fi

if [ ! -f "config/production.json" ]; then
    echo "ERROR: Production configuration file not found!"
    exit 1
fi

# Test configuration loading
echo "Testing configuration loading..."
node -e "
const configLoader = require('./config-loader');
console.log('Environment:', configLoader.getEnvironment());
console.log('Production URLs:', configLoader.getServiceUrls());
if (configLoader.getServiceUrls().membersApiUrl !== 'https://members.aspirichat.com') {
    console.error('ERROR: Production configuration not loaded correctly!');
    process.exit(1);
}
console.log('✓ Production configuration loaded correctly');
"

echo "✓ Production build preparation complete"
echo "Ready to build Docker image with production configuration"

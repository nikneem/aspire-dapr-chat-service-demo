@echo off
REM Build script for production deployment
REM This script is used in GitHub Actions to build the Docker image with production configuration

echo Starting production build...

REM Set production environment
set NODE_ENV=production

REM Verify configuration files exist
if not exist "config\development.json" (
    echo ERROR: Development configuration file not found!
    exit /b 1
)

if not exist "config\production.json" (
    echo ERROR: Production configuration file not found!
    exit /b 1
)

REM Test configuration loading
echo Testing configuration loading...
node -e "const configLoader = require('./config-loader'); console.log('Environment:', configLoader.getEnvironment()); console.log('Production URLs:', configLoader.getServiceUrls()); if (configLoader.getServiceUrls().membersApiUrl !== 'https://members.aspirichat.com') { console.error('ERROR: Production configuration not loaded correctly!'); process.exit(1); } console.log('✓ Production configuration loaded correctly');"

if %errorlevel% neq 0 (
    echo ERROR: Configuration test failed!
    exit /b 1
)

echo ✓ Production build preparation complete
echo Ready to build Docker image with production configuration

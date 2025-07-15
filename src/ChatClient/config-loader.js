const fs = require('fs');
const path = require('path');

/**
 * Configuration loader that handles environment-specific settings
 */
class ConfigLoader {
    constructor() {
        this.environment = process.env.NODE_ENV || 'development';
        this.config = null;
        this.loadConfig();
    }

    loadConfig() {
        try {
            // Try to load environment-specific configuration
            const configPath = path.join(process.cwd(), 'config', `${this.environment}.json`);
            
            if (fs.existsSync(configPath)) {
                const configData = fs.readFileSync(configPath, 'utf8');
                this.config = JSON.parse(configData);
                console.log(`Loaded configuration for environment: ${this.environment}`);
            } else {
                console.warn(`Configuration file not found for environment: ${this.environment}`);
                console.warn(`Looked in: ${configPath}`);
                this.loadFallbackConfig();
            }

            // Override with environment variables if they exist (for Aspire integration)
            this.applyEnvironmentOverrides();
            
        } catch (error) {
            console.error('Error loading configuration:', error);
            this.loadFallbackConfig();
        }
    }

    loadFallbackConfig() {
        console.log('Loading fallback configuration...');
        this.config = {
            environment: this.environment,
            services: {
                membersApiUrl: "http://localhost:5129",
                messagesApiUrl: "http://localhost:5227",
                realtimeApiUrl: "http://localhost:5206"
            },
            client: {
                title: "AspireChat - Fallback",
                description: "Chat application with fallback configuration",
                debug: true
            }
        };
    }

    applyEnvironmentOverrides() {
        if (!this.config) return;

        // Override service URLs with Aspire environment variables if they exist
        if (process.env.services__membersapi__http__0) {
            this.config.services.membersApiUrl = process.env.services__membersapi__http__0;
            console.log(`Overriding Members API URL with Aspire value: ${this.config.services.membersApiUrl}`);
        }

        if (process.env.services__messagesapi__http__0) {
            this.config.services.messagesApiUrl = process.env.services__messagesapi__http__0;
            console.log(`Overriding Messages API URL with Aspire value: ${this.config.services.messagesApiUrl}`);
        }

        if (process.env.services__realtimeapi__http__0) {
            this.config.services.realtimeApiUrl = process.env.services__realtimeapi__http__0;
            console.log(`Overriding Realtime API URL with Aspire value: ${this.config.services.realtimeApiUrl}`);
        }

        // Override other environment-specific settings
        if (process.env.CLIENT_TITLE) {
            this.config.client.title = process.env.CLIENT_TITLE;
        }

        if (process.env.CLIENT_DEBUG) {
            this.config.client.debug = process.env.CLIENT_DEBUG === 'true';
        }
    }

    getConfig() {
        return this.config;
    }

    getServiceUrls() {
        return this.config?.services || {};
    }

    getClientConfig() {
        return this.config?.client || {};
    }

    getEnvironment() {
        return this.environment;
    }

    isProduction() {
        return this.environment === 'production';
    }

    isDevelopment() {
        return this.environment === 'development';
    }
}

module.exports = new ConfigLoader();

const configLoader = require('./config-loader');

console.log('Testing configuration loader...');
console.log('Environment:', configLoader.getEnvironment());
console.log('Configuration:', configLoader.getConfig());
console.log('Service URLs:', configLoader.getServiceUrls());
console.log('Client config:', configLoader.getClientConfig());

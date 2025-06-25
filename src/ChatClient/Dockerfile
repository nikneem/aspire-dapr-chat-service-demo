# Multi-stage build for Node.js chat client
FROM node:20-alpine AS base

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source files
COPY . .

# Expose the port
EXPOSE 3000

# Start the Node.js server
CMD ["npm", "start"]

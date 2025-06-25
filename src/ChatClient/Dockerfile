# Multi-stage build for Node.js chat client
FROM node:20-alpine AS base
WORKDIR /app
COPY package*.json ./
RUN npm ci --ignore-scripts
USER nonroot
COPY . .
EXPOSE 3000
CMD ["npm", "start"]

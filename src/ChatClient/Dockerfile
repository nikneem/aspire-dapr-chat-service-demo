FROM node:20-alpine AS base
WORKDIR /app
COPY package*.json ./
RUN npm ci --ignore-scripts
COPY . .
ENV NODE_ENV=production
RUN addgroup -g 1001 -S nodejs \
&& adduser -S nodejs -u 1001 \
&& chown -R nodejs:nodejs /app
USER nodejs
EXPOSE 3000
CMD ["npm", "start"]

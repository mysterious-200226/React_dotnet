FROM node:18-alpine

WORKDIR /app

COPY package*.json ./

# ✅ ADD THIS LINE: Install curl so the AWS ECS health check succeeds
RUN apk add --no-cache curl

RUN npm install --production

COPY . .

EXPOSE 3001

# ✅ Change index.js to server.js
CMD ["node", "server.js"]
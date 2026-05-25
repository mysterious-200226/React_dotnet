# ─── Stage 1: Build Client ───────────────────────────────────────────────────
FROM node:18 AS build-client

WORKDIR /app/client
COPY Client/package*.json ./
RUN npm install
COPY Client/ .

ENV PUBLIC_URL=/
RUN npm run build


# ─── Stage 2: Build AvailityAccessSimulationTool ─────────────────────────────
FROM node:18 AS build-availity

WORKDIR /app/availity
COPY AvailityAccessSimulationTool/package*.json ./
RUN npm install
COPY AvailityAccessSimulationTool/ .

ENV PUBLIC_URL=/
RUN npm run build


# ─── Stage 3: Serve both SPAs via Nginx ──────────────────────────────────────
FROM nginx:alpine

# Needed if ECS container health check uses curl against /health
RUN apk add --no-cache curl

# Copy built apps to separate folders
COPY --from=build-client /app/client/build /usr/share/nginx/html/ui/
COPY --from=build-availity /app/availity/build /usr/share/nginx/html/availity/

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
	CMD curl -f http://localhost/health || exit 1

CMD ["nginx", "-g", "daemon off;"]
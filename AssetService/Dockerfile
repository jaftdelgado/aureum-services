FROM node:22-alpine AS builder

WORKDIR /usr/src/app

COPY package*.json ./

RUN npm install

COPY . .

RUN npm run build

FROM node:22-alpine AS runner

WORKDIR /usr/src/app

COPY package*.json ./
COPY --from=builder /usr/src/app/dist ./dist

RUN npm install

ENV NODE_ENV=development
ENV PORT=3000

EXPOSE 3000

CMD ["node", "dist/main.js"]

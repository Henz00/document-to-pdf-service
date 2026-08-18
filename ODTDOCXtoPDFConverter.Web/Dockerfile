FROM node:22 AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

RUN npm run build -- --configuration production


FROM nginx:alpine
COPY --from=build /app/dist/ODTDOCXtoPDFConverter.Web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
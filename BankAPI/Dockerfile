# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

# Стадия сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только .csproj из корня (поскольку проект в корне)
COPY *.csproj ./
RUN if [ ! -f "BankAPI.csproj" ]; then \
    echo "Available files:" && ls -la && \
    echo "Файл проекта должен быть в /src/!" && exit 1; \
    fi
RUN dotnet restore "BankAPI.csproj"

# Копируем остальной код из корня
COPY . ./
RUN dotnet publish "BankAPI.csproj" -c Release -o /app/publish --no-restore

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Production
# ENV ASPNETCORE_URLS=http://+:80

# Открываем порт
EXPOSE 80

# Добавляем проверку работоспособности (добавил установку curl, так как alpine не имеет его по умолчанию)
RUN apk add --no-cache curl
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl --fail http://localhost || exit 1

ENTRYPOINT ["dotnet", "BankAPI.dll"]
# Базовый образ для выполнения
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

# Стадия сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем только .csproj
COPY *.csproj ./
RUN if [ ! -f "BankAPI.csproj" ]; then \
    echo "Available files:" && ls -la && \
    echo "Файл проекта должен быть в /src/!" && exit 1; \
    fi

# Очищаем NuGet кэш и восстанавливаем пакеты
RUN dotnet nuget locals all --clear
RUN dotnet restore "BankAPI.csproj" --no-cache

# Копируем остальной код
COPY . ./

# Публикуем проект
RUN dotnet publish "BankAPI.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

# Открываем порт
EXPOSE 80

# Добавляем проверку работоспособности
RUN apt-get update && apt-get install -y curl
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl --fail http://localhost || exit 1

ENTRYPOINT ["dotnet", "BankAPI.dll"]
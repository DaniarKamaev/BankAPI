# BankAPI - Банковское REST API

## Описание проекта

BankAPI - это RESTful API для управления банковскими счетами с поддержкой событийной архитектуры через RabbitMQ. Позволяет:

- Создавать новые счета разных типов (текущий, вклад, кредитный)
- Пополнять счета и выполнять переводы между счетами
- Публиковать доменные события для интеграции с другими системами
- Обрабатывать входящие события (блокировка/разблокировка клиентов)
- Авторизацию через JWT токены

## 🚀 Технологии

- **.NET 9.0** с ASP.NET Core
- **MediatR** (CQRS паттерн)
- **FluentValidation** (валидация запросов)
- **Entity Framework Core** (PostgreSQL)
- **RabbitMQ** (асинхронная коммуникация)
- **Hangfire** (фоновые задачи)
- **JWT Bearer Authentication**
- **Docker & Docker Compose** (контейнеризация)

## 📋 Функциональность

### Пользовательские истории
- ✅ Открытие текущего счета и вклада с публикацией событий
- ✅ Пополнение счета с уведомлениями
- ✅ Переводы между счетами с гарантированной доставкой событий
- ✅ Начисление процентов по вкладам
- ✅ Обработка блокировок клиентов от antifraud-системы

### Публикуемые события
- `AccountOpened` - открытие счета → CRM
- `MoneyCredited` - пополнение → Уведомления
- `TransferCompleted` - перевод → Аудит
- `InterestAccrued` - начисление процентов

### Потребляемые события
- `ClientBlocked` - блокировка операций
- `ClientUnblocked` - разблокировка операций

## 🛠 Установка и запуск

### Предварительные требования
- Docker Desktop для Windows
- Docker Compose

### Быстрый запуск

1. **Клонируйте и запустите:**
```bash
git clone <repository-url>
cd BankAPI
docker-compose up --build
```

2. **Проверьте работоспособность:**
   - API: http://localhost:80
   - RabbitMQ Management: http://localhost:15672 (guest/guest)
   - База данных: localhost:5432 (postgres/1234)

3. **Получите JWT токен:**
```bash
curl -X POST http://localhost:80/api/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

## 🔧 Конфигурация

### Переменные окружения (через docker-compose.yml)
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=bankdb;Username=postgres;Password=1234
  - Jwt__Key=YourSuperSecretKeyForJWTEncryption
  - Jwt__Issuer=bank-api
  - Jwt__Audience=bank-clients
  - RabbitMQ__Host=rabbitmq
  - RabbitMQ__Username=guest
  - RabbitMQ__Password=guest
```

### Топики RabbitMQ
- **account.events** (topic exchange)
- **account.crm** ← account.*
- **account.notifications** ← money.*  
- **account.antifraud** ← client.*
- **account.audit** ← #

## 📡 API Endpoints

### 🔐 Аутентификация
- `POST /api/login` - Получение JWT токена
```json
{"username": "admin", "password": "admin123"}
```

### 📊 Управление счетами
- `POST /api/accounts` - Создание счета
- `GET /api/accounts/owner/{ownerId}` - Счета клиента
- `POST /api/accounts/{accountId}/deposit` - Пополнение
- `POST /api/transfer` - Перевод между счетами
- `POST /api/accounts/{accountId}/close` - Закрытие счета
- `GET /api/accounts/{accountId}/status` - Статус счета

## 🗄 Модели данных

### Типы счетов (AccountType)
- `0` - Checking (Текущий)
- `1` - Deposit (Вклад) 
- `2` - Credit (Кредитный)

### Валюты (CurrencyType)
- `0` - RUB
- `1` - USD
- `2` - EUR

## 🔐 Безопасность

- Все бизнес-эндпоинты требуют JWT авторизации
- События не содержат персональных данных

## 📊 Мониторинг

- Structured logging с correlationId
- Health checks для БД и RabbitMQ
- Мониторинг очереди Outbox сообщений

## 🔧 Разработка

### Локальная разработка
```bash
docker-compose up -d db rabbitmq
dotnet run
```

### Миграции базы данных
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## 📝 Примеры событий

### AccountOpened
```json
{
  "eventId": "b5f3a7f6-2f4e-4b1a-9f3a-2b0c1e7c1a11",
  "occurredAt": "2025-08-12T21:00:00Z",
  "meta": {
    "version": "v1",
    "source": "account-service",
    "correlationId": "11111111-1111-1111-1111-111111111111",
    "causationId": "22222222-2222-2222-2222-222222222222"
  },
  "accountId": "9c3f3f5d-7c2e-4a1a-9f5a-1b3a7e9d2f11",
  "ownerId": "2a7e9d2f-9f5a-4a1a-7c2e-9c3f3f5d1b3a",
  "currency": "RUB",
  "type": "Checking"
}
```

## 🆘 Поддержка

При возникновении проблем:
1. Проверьте логи: `docker-compose logs bankapi`
2. Убедитесь, что все контейнеры запущены: `docker-compose ps`
3. Проверьте подключение к БД и RabbitMQ

## Тестирование

Для тестирования API можно использовать:
- Postman

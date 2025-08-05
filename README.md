# BankAPI - Банковское REST API

## Описание проекта

BankAPI - это RESTful API для управления банковскими счетами, позволяющее:
- Создавать новые счета разных типов (текущий, вклад, кредитный)
- Пополнять счета
- Переводить средства между счетами
- Просматривать информацию о счетах клиента
- Аутентифицировать пользователей через JWT

## Технологии

- .NET 9.0
- ASP.NET Core
- MediatR (реализация паттерна CQRS)
- FluentValidation (валидация запросов)
- JWT Bearer Authentication
- PostgreSQL (хранение данных)
- Docker (контейнеризация)

## Установка и запуск

1. Убедитесь, что у вас установлены:
   - Docker
   - Docker Compose

2. Клонируйте репозиторий:
   ```bash
   git clone <repository-url>
   cd <project-directory>
   ```

3. Запустите проект с помощью Docker Compose:
   ```bash
   docker-compose up --build
   ```

4. API будет доступно по адресу: `http://localhost:8080`

## Конфигурация

Настройки JWT можно изменить в файле `appsettings.json`:
```json
"Jwt": {
  "Key": "your-secret-key",
  "Issuer": "your-issuer",
  "Audience": "your-audience"
}
```

## API Endpoints

### Аутентификация

- `POST /api/login` - Получение JWT токена
  - Тело запроса:
    ```json
    {
      "username": "admin",
      "password": "admin123"
    }
    ```

### Управление счетами

- `POST /api/accounts` - Создание нового счета (требует авторизации)
  - Тело запроса:
    ```json
    {
      "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "type": "Checking",
      "currency": "RUB",
      "balance": 1000,
      "interestRate": null
    }
    ```

- `GET /api/accounts/owner/{ownerId}` - Получение всех счетов клиента (требует авторизации)

- `POST /api/accounts/{accountId}/deposit` - Пополнение счета (требует авторизации)
  - Тело запроса: сумма пополнения (decimal)

- `POST /api/accounts/transfer` - Перевод между счетами (требует авторизации)
  - Тело запроса:
    ```json
    {
      "fromAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "toAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "amount": 500
    }
    ```

## Модели данных

### Типы счетов (AccountType)
- `Checking` - Текущий счет - 0
- `Deposit` - Вклад - 1
- `Credit` - Кредитный счет - 2

### Типы валют (CurrencyType)
- `RUB` - Российский рубль - 0
- `USD` - Доллар США - 1
- `EUR` - Евро - 2

### Типы транзакций (TransactionType)
- `Credit` - Зачисление - 0
- `Debit` - Списание - 1

## Тестирование

Для тестирования API можно использовать:
- Postman

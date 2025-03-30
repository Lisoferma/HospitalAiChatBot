# HospitalAIChatbot

# Инструкция по запуску чат-бота

## Требования/Зависимости

- СУБД `Mongo DB` сервер. Версия 6.0+;
- .NET Runtime 9.0+ & .NET .ASP Runtime 9.0+;
- Модель для обработки голосового ввода;

## Инструкция 

### Клонирование репозитория

При налиичии утилиты `git`
`git clone https://github.com/Lisoferma/HospitalAiChatBot.git`

Иначе скачайте репозиторий в удобном формате с сайта `https://github.com/Lisoferma/HospitalAiChatBot.git`.

### Настройка параметров ПО

1. Создайте файл `secrets.json` для проекта `TelegramBot` с помощью IDE. [_Материалы_](https://learn.microsoft.com/ru-ru/aspnet/core/security/app-secrets?view=aspnetcore-9.0&tabs=linux);
2. Задайте в файле секретов следующие поля:

   1. "botToken": <telegram_bot_api_token>;
   2. "specialistChatId": <specialist_telegram_account_id>. _При этом специалист должен как минимум раз написать чат-боту_;
3. Запустите сервер чат-бота с помощью команды `dotnet run <путь_к_проекту_HospitalAiChatBox>`;
4. Запустите клиент чат-бота - TelegramBot с помощью команды `dotnet run <путь_к_проекту_TelegramBot>`;

# Дополнительные материалы проекта

## UML-диаграмма классов
![uml-class-diagram](https://github.com/user-attachments/assets/d90c0d23-3983-44ae-891e-82abf4d77665)

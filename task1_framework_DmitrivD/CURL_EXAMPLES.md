# Примеры cURL команд для тестирования API

## Базовая информация

**Base URL:** `http://localhost:5000`  
**API Path:** `/api/items`

## 1. Получение списка всех товаров

### Простой запрос
```bash
curl http://localhost:5000/api/items
```

### С форматированием (требуется jq)
```bash
curl -s http://localhost:5000/api/items | jq '.'
```

### С заголовками ответа
```bash
curl -i http://localhost:5000/api/items
```

**Ожидаемый ответ:**
```json
[]
```

---

## 2. Создание товара

### Базовый пример
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ноутбук Lenovo ThinkPad",
    "price": 89999.99
  }'
```

### С выводом заголовков
```bash
curl -i -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Мышь Logitech MX Master 3",
    "price": 7999.00
  }'
```

### С кастомным Request ID
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -H "X-Request-Id: my-custom-id-12345" \
  -d '{
    "name": "Клавиатура Keychron K2",
    "price": 8999.00
  }'
```

**Ожидаемый ответ (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Ноутбук Lenovo ThinkPad",
  "price": 89999.99
}
```

**Заголовок:**
```
Location: /api/items/550e8400-e29b-41d4-a716-446655440000
X-Request-Id: ...
```

---

## 3. Получение товара по ID

### Простой запрос
```bash
# Замените {id} на реальный ID из предыдущего шага
curl http://localhost:5000/api/items/550e8400-e29b-41d4-a716-446655440000
```

### С форматированием
```bash
curl -s http://localhost:5000/api/items/550e8400-e29b-41d4-a716-446655440000 | jq '.'
```

**Ожидаемый ответ (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Ноутбук Lenovo ThinkPad",
  "price": 89999.99
}
```

---

## 4. Сценарии обработки ошибок

### Ошибка: Пустое имя
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "price": 100
  }'
```

**Ожидаемый ответ (400 Bad Request):**
```json
{
  "code": "validation",
  "message": "Поле name не должно быть пустым",
  "requestId": "abc123def456"
}
```

### Ошибка: Отрицательная цена
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Товар с отрицательной ценой",
    "price": -100
  }'
```

**Ожидаемый ответ (400 Bad Request):**
```json
{
  "code": "validation",
  "message": "Поле price не может быть отрицательным",
  "requestId": "def456ghi789"
}
```

### Ошибка: Слишком длинное имя
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"$(printf 'a%.0s' {1..201})\",
    \"price\": 100
  }"
```

**Ожидаемый ответ (400 Bad Request):**
```json
{
  "code": "validation",
  "message": "Поле name не должно превышать 200 символов",
  "requestId": "ghi789jkl012"
}
```

### Ошибка: Несуществующий ID
```bash
curl http://localhost:5000/api/items/00000000-0000-0000-0000-000000000000
```

**Ожидаемый ответ (404 Not Found):**
```json
{
  "code": "not_found",
  "message": "Элемент с ID 00000000-0000-0000-0000-000000000000 не найден",
  "requestId": "jkl012mno345"
}
```

### Ошибка: Множественные нарушения валидации
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "price": -50
  }'
```

**Ожидаемый ответ (400 Bad Request):**
```json
{
  "code": "validation",
  "message": "Поле name не должно быть пустым",
  "requestId": "mno345pqr678"
}
```
*(Валидация прекращается на первой ошибке)*

---

## 5. Граничные значения

### Цена = 0 (допустимо)
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Бесплатная доставка",
    "price": 0
  }'
```

**Ожидается:** 201 Created ✅

### Имя = 200 символов (максимум, допустимо)
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"$(printf 'a%.0s' {1..200})\",
    \"price\": 100
  }"
```

**Ожидается:** 201 Created ✅

### Имя = 201 символ (превышение, недопустимо)
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d "{
    \"name\": \"$(printf 'a%.0s' {1..201})\",
    \"price\": 100
  }"
```

**Ожидается:** 400 Bad Request ❌

### Имя с пробелами по краям (триммируется)
```bash
curl -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "  Товар с пробелами  ",
    "price": 100
  }'
```

**Ожидаемый результат:**
```json
{
  "id": "...",
  "name": "Товар с пробелами",  // Без пробелов по краям
  "price": 100
}
```

---

## 6. Полные сценарии

### Сценарий: Создание и получение
```bash
#!/bin/bash

echo "=== 1. Создаём товар ==="
RESPONSE=$(curl -s -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Монитор Dell 27 дюймов",
    "price": 25999.00
  }')

echo "$RESPONSE" | jq '.'

# Извлекаем ID из ответа
ID=$(echo "$RESPONSE" | jq -r '.id')

echo ""
echo "=== 2. Получаем товар по ID: $ID ==="
curl -s http://localhost:5000/api/items/$ID | jq '.'

echo ""
echo "=== 3. Получаем все товары ==="
curl -s http://localhost:5000/api/items | jq '.'
```

### Сценарий: Создание нескольких товаров
```bash
#!/bin/bash

echo "=== Создаём 5 товаров ==="
for i in {1..5}; do
  echo "Создаём товар $i..."
  curl -s -X POST http://localhost:5000/api/items \
    -H "Content-Type: application/json" \
    -d "{
      \"name\": \"Товар №$i\",
      \"price\": $((i * 1000))
    }" | jq -r '.id'
done

echo ""
echo "=== Получаем полный список ==="
curl -s http://localhost:5000/api/items | jq '.'
```

### Сценарий: Проверка валидации
```bash
#!/bin/bash

echo "=== Тест 1: Пустое имя ==="
curl -s -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{"name": "", "price": 100}' | jq '.'

echo ""
echo "=== Тест 2: Отрицательная цена ==="
curl -s -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{"name": "Товар", "price": -100}' | jq '.'

echo ""
echo "=== Тест 3: Корректные данные ==="
curl -s -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{"name": "Корректный товар", "price": 100}' | jq '.'
```

---

## 7. Проверка Request ID

### Отправка с кастомным Request ID
```bash
curl -i -X GET http://localhost:5000/api/items \
  -H "X-Request-Id: my-trace-id-2025"
```

**Проверьте заголовок ответа:**
```
X-Request-Id: my-trace-id-2025
```

### Проверка Request ID в логах ошибок
```bash
curl -s -X POST http://localhost:5000/api/items \
  -H "Content-Type: application/json" \
  -H "X-Request-Id: error-trace-123" \
  -d '{
    "name": "",
    "price": 100
  }' | jq '.'
```

**Ожидается в ответе:**
```json
{
  "code": "validation",
  "message": "...",
  "requestId": "error-trace-123"  // Тот же ID, что мы отправили
}
```

**Проверьте логи приложения:**
```
info: TimingAndLogMiddleware[0]
      Запрос обработан. requestId=error-trace-123 ...
```

---

## 8. Продвинутые примеры

### Измерение времени ответа
```bash
curl -w "\nВремя ответа: %{time_total}s\n" \
  -o /dev/null -s \
  http://localhost:5000/api/items
```

### Сохранение ответа в файл
```bash
curl -s http://localhost:5000/api/items > items.json
cat items.json | jq '.'
```

### Параллельные запросы (тест конкурентности)
```bash
#!/bin/bash

echo "=== Создаём 10 товаров параллельно ==="
for i in {1..10}; do
  (
    curl -s -X POST http://localhost:5000/api/items \
      -H "Content-Type: application/json" \
      -d "{\"name\": \"Параллельный товар $i\", \"price\": $((i * 100))}"
  ) &
done

wait
echo "=== Готово ==="

sleep 1

echo "=== Проверяем результат ==="
curl -s http://localhost:5000/api/items | jq 'length'
```

### Извлечение конкретных полей с jq
```bash
# Получить только названия товаров
curl -s http://localhost:5000/api/items | jq '.[].name'

# Получить товары дороже 5000
curl -s http://localhost:5000/api/items | jq '.[] | select(.price > 5000)'

# Посчитать общую стоимость всех товаров
curl -s http://localhost:5000/api/items | jq '[.[].price] | add'

# Получить самый дорогой товар
curl -s http://localhost:5000/api/items | jq 'max_by(.price)'
```

---

## 9. Тестирование с Postman

### Импорт коллекции

Создайте файл `postman_collection.json`:

```json
{
  "info": {
    "name": "Mini Web Service API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get All Items",
      "request": {
        "method": "GET",
        "header": [],
        "url": "http://localhost:5000/api/items"
      }
    },
    {
      "name": "Get Item By ID",
      "request": {
        "method": "GET",
        "header": [],
        "url": "http://localhost:5000/api/items/{{itemId}}"
      }
    },
    {
      "name": "Create Item",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"name\": \"Test Item\",\n  \"price\": 99.99\n}"
        },
        "url": "http://localhost:5000/api/items"
      }
    }
  ]
}
```

Импортируйте в Postman: File → Import → выберите файл

---

## 10. Troubleshooting

### Проблема: Connection refused
```bash
curl: (7) Failed to connect to localhost port 5000: Connection refused
```

**Решение:**
1. Убедитесь, что приложение запущено: `dotnet run`
2. Проверьте порт в `Properties/launchSettings.json`

### Проблема: 404 Not Found на все запросы
```bash
404 Not Found
```

**Решение:**
1. Проверьте URL: должен быть `/api/items`, а не `/items`
2. Убедитесь, что порт правильный: `http://localhost:5000`

### Проблема: Invalid JSON
```bash
{"code":"internal_error","message":"Внутренняя ошибка сервера",...}
```

**Решение:**
1. Проверьте JSON на валидность: https://jsonlint.com
2. Убедитесь в наличии `Content-Type: application/json`
3. Используйте двойные кавычки в JSON, не одинарные

---

## Полезные команды

### Проверить, запущено ли приложение
```bash
curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/items
# Ожидается: 200
```

### Получить только HTTP статус
```bash
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/api/items
```

### Показать только заголовки
```bash
curl -I http://localhost:5000/api/items
```

### Verbose режим (для отладки)
```bash
curl -v http://localhost:5000/api/items
```

---

## Заключение

Все примеры можно скопировать и выполнить напрямую в терминале. Для лучшего опыта рекомендуется установить:

- **jq** - для форматирования JSON: `sudo apt install jq`
- **Postman** - для GUI тестирования API
- **HTTPie** - альтернатива curl с лучшим UX: `pip install httpie`

Пример с HTTPie:
```bash
# Вместо curl
http POST http://localhost:5000/api/items name="Test" price:=99.99

# Автоматическое форматирование и подсветка синтаксиса!
```

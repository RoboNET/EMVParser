# RoboNet.EMVParser

[![NuGet Version](https://img.shields.io/nuget/v/RoboNet.EMVParser.svg?style=flat)](https://www.nuget.org/packages/RoboNet.EMVParser/)

RoboNet.EMVParser - это высокопроизводительная библиотека для работы с EMV данными в форматах TLV и DOL, оптимизированная для использования в .NET приложениях.

## Основные возможности

- 🚀 Высокопроизводительный парсинг с использованием Span API
- 📦 Поддержка как Memory<byte>, так и Span<byte>
- 🔍 Автоматическое определение и парсинг составных тегов
- 🎯 Поддержка длинных тегов и многобайтовых длин
- 🛠️ Удобные методы расширения для работы со списками тегов
- 💡 Оптимизация производительности и минимальное выделение памяти
- 🔢 Поддержка парсинга DOL (Data Object List) для структур CDOL1, CDOL2 и DDOL

## Установка

```bash
dotnet add package RoboNet.EMVParser
```

## Быстрый старт

```csharp
// Пример EMV данных
var emvData = "9F2608AABBCCDDEE12345695050000000000";
var data = Convert.FromHexString(emvData);

// Получение всех тегов
IReadOnlyList<TagPointer> tagsList = EMVTLVParser.ParseTagsList(data);

// Поиск конкретного тега
var tag = tagsList.GetTag("9F26");
if (tag != null)
{
    Console.WriteLine($"Значение тега: {tag.ValueHex}");
}

// Парсинг DOL (Data Object List) - содержит пары тег-длина без значений
var dolData = "9F02069F1D029F03069F1A0295055F2A029A039C019F37049F21039F7C14";
IReadOnlyList<TagLengthPointer> dolTags = DOLParser.ParseTagsList(dolData);

foreach (var dolTag in dolTags)
{
    Console.WriteLine($"DOL Тег: {dolTag.TagHex}, Ожидаемая длина: {dolTag.Length}");
}
```

# Работа с TLV тегами

TLV (Tag-Length-Value) - это формат данных, используемый в EMV транзакциях. Библиотека RoboNet.EMVParser предоставляет удобный API для работы с этим форматом.

## Основные концепции

TLV состоит из трех частей:
- Tag (Тег) - идентификатор данных
- Length (Длина) - длина значения в байтах
- Value (Значение) - сами данные

## BER-TLV формат

BER-TLV (Basic Encoding Rules - Tag Length Value) - это специфический формат кодирования данных, используемый в EMV. Он основан на правилах базового кодирования ASN.1 и имеет следующие особенности:

### Структура тега (Tag)
- Первый байт содержит информацию о:
  - Классе тега (биты 7-8)
  - Типе данных (бит 6): примитивный или составной
  - Номере тега (биты 1-5)
- Если номер тега = 31 (11111), то тег продолжается в следующем байте

### Структура длины (Length)
- Если первый бит = 0: следующие 7 бит содержат длину
- Если первый бит = 1: следующие 7 бит указывают количество байт, которые содержат длину

### Значение (Value)
- Для примитивных тегов: содержит данные
- Для составных тегов: содержит вложенные TLV структуры

### Пример
```
Тег: 9F 26 (два байта)
Длина: 08 (один байт)
Значение: 01 23 45 67 89 AB CD EF (8 байт)
```

## Длинные теги и длины

### Многобайтовые теги
Если первые 5 бит тега равны '11111' (31 в десятичной системе), это означает, что тег продолжается в следующем байте. Формат следующих байтов:
- Бит 8: если 1, тег продолжается в следующем байте
- Биты 7-1: часть номера тега

Пример:
```
1F 81 80 - трехбайтовый тег, где:
1F = первый байт (11111 в младших битах)
81 = второй байт (1 в старшем бите означает продолжение)
80 = последний байт (0 в старшем бите означает конец тега)
```

### Многобайтовые длины
Если первый бит байта длины равен 1, то оставшиеся 7 бит указывают количество следующих байтов, которые содержат значение длины.

Примеры:
```
82 01 00 - трехбайтовая длина, где:
82 = первый байт (1 в старшем бите, 2 в оставшихся битах означает 2 байта длины)
01 00 = значение длины (256 в десятичной системе)

81 FF - двухбайтовая длина, где:
81 = первый байт (1 в старшем бите, 1 в оставшихся битах означает 1 байт длины)
FF = значение длины (255 в десятичной системе)
```

### Обработка в библиотеке
Библиотека RoboNet.EMVParser автоматически обрабатывает многобайтовые теги и длины:
```csharp
// Пример с длинным тегом
var longTagData = "1F818001020304"; // Тег: 1F8180, Длина: 01, Значение: 02 03 04
var parsedTag = EMVTLVParser.ParseTagsList(Convert.FromHexString(longTagData));

// Пример с длинной длиной
var longLengthData = "9F26820100" + new string('FF', 256); // 256 байт данных
var parsedLength = EMVTLVParser.ParseTagsList(Convert.FromHexString(longLengthData));
```

## Работа с классом EMVTags

Класс `EMVTags` предоставляет удобный способ работы с EMV тегами, используя предопределенные константы и описания тегов. Класс автоматически генерируется на основе файла well-known-tags.txt, который содержит стандартные EMV теги и их описания.

### Получение описания тега

Для получения описания тега по его номеру используйте метод `GetTagName`:

```csharp
var tagName = EMVTags.GetTagName("5F2A");
// Вернет: "Transaction Currency Code"
```

### Использование предопределенных констант

Вместо использования строковых литералов для тегов, вы можете использовать предопределенные константы:

```csharp
// Использование константы вместо строкового литерала
var tag = tagsList.GetTag(EMVTags.TransactionCurrencyCode); // вместо "5F2A"
if (tag != null)
{
    Console.WriteLine($"Значение тега валюты: {tag.ValueHex}");
}
```

### Преимущества использования EMVTags

1. **Типобезопасность**: Использование предопределенных констант вместо строковых литералов помогает избежать опечаток
2. **Документация**: Каждая константа содержит XML-документацию с описанием назначения тега
3. **Удобство сопровождения**: При изменении номеров тегов достаточно обновить well-known-tags.txt
4. **IntelliSense**: IDE предоставляет автодополнение и подсказки при работе с тегами

### Пример комплексного использования

```csharp
var emvData = "9F2608AABBCCDDEE12345695050000000000";
var data = Convert.FromHexString(emvData);
var tagsList = EMVTLVParser.ParseTagsList(data);

// Получение значения тега по константе
var applicationCryptogram = tagsList.GetTag(EMVTags.ApplicationCryptogram);

// Получение описания тега
var tagDescription = EMVTags.GetTagName(applicationCryptogram.TagHex);

// Вывод информации
Console.WriteLine($"Тег: {applicationCryptogram.TagHex}");
Console.WriteLine($"Описание: {tagDescription}");
Console.WriteLine($"Значение: {applicationCryptogram.ValueHex}");
```

## Основные классы

### TagPointer

`TagPointer` - основной класс для работы с TLV данными. Содержит следующие свойства:

- `TLV` - полные TLV данные
- `Tag` - байты тега
- `Value` - байты значения
- `Length` - длина значения
- `InternalTags` - внутренние теги (для составных тегов)
- `TagDataType` - тип данных тега
- `TagClassType` - тип класса тега

Дополнительные свойства для удобства:
- `TLVHex` - полные TLV данные в HEX формате
- `TagHex` - тег в HEX формате
- `ValueHex` - значение в HEX формате
- `ValueString` - значение в ASCII кодировке
- `ValueNumeric` - значение как число

## Основные методы

### Парсинг всех тегов

```csharp
var data = Convert.FromHexString(emvData);
IReadOnlyList<TagPointer> tagsList = EMVTLVParser.ParseTagsList(data);

foreach (var tag in tagsList)
{
    Console.WriteLine($"Tag: {tag.TagHex}, Value: {tag.ValueHex}");
}
```

### Поиск конкретного тега

```csharp
// Поиск по списку тегов
var tag = tagsList.GetTag("5F2A");
if (tag != null)
{
    Console.WriteLine($"Found tag value: {tag.ValueHex}");
}

// Прямой поиск в данных
var tagValue = EMVTLVParser.GetTagValue(data, "5F2A");
Console.WriteLine($"Tag value: {Convert.ToHexString(tagValue.Span)}");
```

### Получение значений

```csharp
// Получение значения в HEX
string hexValue = tagsList.GetTagValueHex("5F2A");

// Получение значения как массива байт
Memory<byte> byteValue = tagsList.GetTagValue("5F2A");

// Получение строкового значения (для ASCII данных)
string stringValue = tag.ValueString;

// Получение числового значения
long numericValue = tag.ValueNumeric;
```

## Типы тегов

Теги могут быть двух типов:
- `PrimitiveDataObject` - содержит простые данные
- `ConstructedDataObject` - содержит другие TLV структуры (доступны через `InternalTags`)

## Классы тегов

Теги могут принадлежать к разным классам:
- `ApplicationClass`
- `ContextSpecificClass`
- И другие, определенные в `ClassType`

## Примечания

1. Библиотека оптимизирована для работы с Span API
2. Поддерживает работу как с Memory<byte>, так и с Span<byte>
3. Предоставляет удобные методы расширения для работы со списками тегов
4. Автоматически определяет составные теги и парсит их внутреннюю структуру

## Лицензия

Этот проект распространяется под лицензией MIT. Подробности смотрите в файле LICENSE.

## Поддержка

Если у вас возникли проблемы или есть предложения по улучшению библиотеки, пожалуйста, создайте issue в репозитории проекта.

## Работа с предпочтительным именем приложения

Для корректного отображения названия приложения в EMV часто используется тег `9F12` (Application Preferred Name) вместе с тегом кодировки `9F11` (Issuer Code Table Index). Библиотека предоставляет удобный способ получения этого значения с учетом правильной кодировки через класс `PreferredApplicationNameUtility`.

### Пример использования

```csharp
// Получение списка тегов из EMV данных
var data = Convert.FromHexString(emvData);
var tagsList = EMVTLVParser.ParseTagsList(data);

// Получение предпочтительного имени приложения с учетом кодировки
string? applicationName = tagsList.GetApplicationPreferredName();
// Например: "МИР Классик" или "Visa Debit"
```

### Как это работает

1. Метод ищет тег `9F12` (Application Preferred Name) в списке тегов
2. Если найден тег `9F11` (Issuer Code Table Index), его значение используется для определения правильной кодировки ISO-8859-X
3. Если тег кодировки не найден, используется ASCII кодировка
4. Возвращает декодированное значение имени приложения или null, если тег не найден

### Примечания

- Метод автоматически обрабатывает различные кодировки, что особенно важно для карт с нелатинскими символами
- Поддерживает все стандартные кодировки ISO-8859
- Особенно полезен для корректного отображения названий приложений на платежных терминалах

# Работа с DOL (Data Object List)

DOL (Data Object List) — это специальная структура данных EMV, которая определяет список объектов данных и их ожидаемые длины, но не содержит фактических значений. Структуры DOL обычно используются в EMV-транзакциях для указания того, какие данные должны быть включены в различные операции.

## Основные типы DOL

### CDOL1 (Card Risk Management Data Object List 1)
- **Тег**: `8C`
- Используется во время первой команды GENERATE AC
- Определяет элементы данных, необходимые карте для управления рисками

### CDOL2 (Card Risk Management Data Object List 2)  
- **Тег**: `8D`
- Используется во время второй команды GENERATE AC (если присутствует)
- Содержит дополнительные элементы данных для окончательной авторизации

### DDOL (Dynamic Data Authentication Data Object List)
- **Тег**: `9F49`
- Используется для динамической аутентификации данных
- Определяет элементы данных для криптографической проверки

## Структура DOL

В отличие от формата TLV, DOL использует формат TL (Tag-Length):
- **Tag (Тег)**: Идентифицирует элемент данных
- **Length (Длина)**: Указывает ожидаемую длину в байтах
- **Нет значения**: DOL только определяет структуру, не фактические данные

## Использование DOLParser

### Базовый парсинг DOL

```csharp
// Пример данных CDOL1 - содержит пары тег-длина
var cdol1Data = "9F02069F1D029F03069F1A0295055F2A029A039C019F37049F21039F7C14";
IReadOnlyList<TagLengthPointer> dolTags = DOLParser.ParseTagsList(cdol1Data);

// Отображение структуры DOL
foreach (var dolTag in dolTags)
{
    string tagName = EMVTags.GetTagName(dolTag.TagHex) ?? "Неизвестный";
    Console.WriteLine($"Тег: {dolTag.TagHex} ({tagName})");
    Console.WriteLine($"Ожидаемая длина: {dolTag.Length} байт");
    Console.WriteLine($"Тип данных: {dolTag.TagDataType}");
    Console.WriteLine("---");
}
```

### Свойства TagLengthPointer

Класс `TagLengthPointer` обеспечивает доступ к информации о записи DOL:

```csharp
var dolTag = dolTags.First();

// Доступ к исходным данным
Memory<byte> tlData = dolTag.TL;        // Полные байты TL
Memory<byte> tagBytes = dolTag.Tag;     // Только байты тега
int expectedLength = dolTag.Length;     // Ожидаемая длина данных

// Удобные свойства
string tlHex = dolTag.TLHex;           // TL данные как hex строка
string tagHex = dolTag.TagHex;         // Тег как hex строка
string? tagName = dolTag.Name;         // Читаемое название тега

// Метаданные
DataType dataType = dolTag.TagDataType;    // Примитивный/Составной
ClassType classType = dolTag.TagClassType; // Универсальный/Приложение/и т.д.
```

### Работа с различными типами ввода

```csharp
// Из hex строки
var dolTags1 = DOLParser.ParseTagsList("9F02069F1D02");

// Из массива байт
byte[] dolBytes = Convert.FromHexString("9F02069F1D02");
var dolTags2 = DOLParser.ParseTagsList(dolBytes);

// Из Memory<byte>
Memory<byte> dolMemory = dolBytes.AsMemory();
var dolTags3 = DOLParser.ParseTagsList(dolMemory);
```

### Практический пример: Обработка CDOL1

```csharp
// Парсинг CDOL1 из ответа карты
var cdol1Hex = "9F02069F1D029F03069F1A0295055F2A029A039C019F37049F21039F7C14";
var cdol1Structure = DOLParser.ParseTagsList(cdol1Hex);

Console.WriteLine("Структура CDOL1:");
Console.WriteLine("================");

int totalLength = 0;
foreach (var entry in cdol1Structure)
{
    var tagName = EMVTags.GetTagName(entry.TagHex) ?? "Неизвестный тег";
    Console.WriteLine($"{entry.TagHex}: {tagName} ({entry.Length} байт)");
    totalLength += entry.Length;
}

Console.WriteLine($"\nОбщая ожидаемая длина данных: {totalLength} байт");

// Теперь вы знаете, какие данные собирать для команды GENERATE AC
// Например, если CDOL1 содержит 9F02 (Amount, Authorized), вам нужно предоставить 6 байт,
// представляющих сумму транзакции
```

## Сравнение DOL и TLV

| Аспект | Формат TLV | Формат DOL |
|--------|------------|------------|
| Структура | Tag-Length-Value | Только Tag-Length |
| Назначение | Содержит фактические данные | Определяет структуру данных |
| Использование | Передача данных | Спецификация данных |
| Парсер | `TLVParser` | `DOLParser` |
| Тип результата | `TagPointer` | `TagLengthPointer` |

## Характеристики производительности

Как и основной парсер TLV, `DOLParser` оптимизирован для производительности:
- Использует `Span<byte>` и `Memory<byte>` для операций без копирования
- Минимальные выделения памяти во время парсинга
- Эффективные операции на уровне байтов
- Нет ненужных преобразований строк во время парсинга

## Обработка ошибок

Парсер DOL корректно обрабатывает некорректные данные:
- Неверные структуры тегов пропускаются
- Парсинг продолжается, даже если отдельные записи некорректны
- Используйте блоки try-catch для надежной обработки ошибок в продакшн коде

```csharp
try
{
    var dolTags = DOLParser.ParseTagsList(dolData);
    // Обработка структуры DOL
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка парсинга DOL: {ex.Message}");
}
``` 
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logic.Domain.Level5Management.Contract.DataClasses;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace CfgBinEditor.Converter;

internal sealed class T2bEntryValueJsonConverter : JsonConverter<T2bEntryValue>
{
    public override T2bEntryValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected a T2b entry value object.");

        ValueType type = default;
        JsonElement valueElement = default;
        var hasValue = false;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            ReadProperty(ref reader, ref type, ref valueElement, ref hasValue);

        return new T2bEntryValue
        {
            Type = type,
            Value = hasValue ? ConvertValue(type, valueElement) : null
        };
    }

    public override void Write(Utf8JsonWriter writer, T2bEntryValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(T2bEntryValue.Type), (int)value.Type);
        writer.WritePropertyName(nameof(T2bEntryValue.Value));
        WriteValue(writer, value);
        writer.WriteEndObject();
    }

    private static void ReadProperty(ref Utf8JsonReader reader, ref ValueType type, ref JsonElement valueElement, ref bool hasValue)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException("Expected a T2b entry value property.");

        string propertyName = reader.GetString()!;
        reader.Read();

        if (propertyName.Equals(nameof(T2bEntryValue.Type), StringComparison.OrdinalIgnoreCase))
        {
            type = ReadType(ref reader);
            return;
        }

        if (!propertyName.Equals(nameof(T2bEntryValue.Value), StringComparison.OrdinalIgnoreCase))
        {
            reader.Skip();
            return;
        }

        valueElement = JsonElement.ParseValue(ref reader);
        hasValue = true;
    }

    private static ValueType ReadType(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return (ValueType)reader.GetInt32();

        if (reader.TokenType == JsonTokenType.String && Enum.TryParse(reader.GetString(), true, out ValueType type))
            return type;

        throw new JsonException("Expected a T2b value type.");
    }

    private static object? ConvertValue(ValueType type, JsonElement valueElement)
    {
        if (valueElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;

        return type switch
        {
            ValueType.String => valueElement.GetString(),
            ValueType.Integer => valueElement.GetInt64(),
            ValueType.FloatingPoint => valueElement.GetDouble(),
            _ => throw new JsonException($"Unknown value type {type}.")
        };
    }

    private static void WriteValue(Utf8JsonWriter writer, T2bEntryValue entryValue)
    {
        if (entryValue.Value is null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (entryValue.Type)
        {
            case ValueType.String:
                writer.WriteStringValue((string)entryValue.Value);
                break;
            case ValueType.Integer:
                WriteInteger(writer, entryValue.Value);
                break;
            case ValueType.FloatingPoint:
                WriteFloatingPoint(writer, entryValue.Value);
                break;
            default:
                throw new JsonException($"Unknown value type {entryValue.Type}.");
        }
    }

    private static void WriteInteger(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case long longValue:
                writer.WriteNumberValue(longValue);
                break;
            default:
                throw new JsonException($"Unsupported integer value type {value.GetType()}.");
        }
    }

    private static void WriteFloatingPoint(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case float floatValue:
                writer.WriteNumberValue(floatValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            default:
                throw new JsonException($"Unsupported floating point value type {value.GetType()}.");
        }
    }
}

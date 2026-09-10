using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using UnityEngine;

public static class SaveJsonCodec
{
    public const int CurrentVersion = 1;

    public static bool TryDeserialize(string json, out SaveData data, out string error)
    {
        data = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = "Save JSON is empty.";
            return false;
        }

        if (!TryPrecheckV1(json, out error)) return false;

        // 每次都创建独立候选对象；缺失字段保留非法值，不能继承旧存档。
        var candidate = new SaveData
        {
            version = 0,
            coinNum = -1,
            positionX = float.NaN,
            positionY = float.NaN,
            positionZ = float.NaN
        };

        try
        {
            JsonUtility.FromJsonOverwrite(json, candidate);
        }
        catch (ArgumentException exception)
        {
            error = "Cannot deserialize save JSON: " + exception.Message;
            return false;
        }

        if (!TryValidate(candidate, out error)) return false;

        data = candidate;
        return true;
    }

    public static bool TrySerialize(SaveData data, out string json, out string error)
    {
        json = null;
        if (!TryValidate(data, out error)) return false;

        try
        {
            json = JsonUtility.ToJson(data, true);
            return true;
        }
        catch (ArgumentException exception)
        {
            error = "Cannot serialize save JSON: " + exception.Message;
            return false;
        }
    }

    public static bool TryValidate(SaveData data, out string error)
    {
        if (data == null)
        {
            error = "Save data is null.";
            return false;
        }
        if (data.version != CurrentVersion)
        {
            error = "Save version is missing or unsupported: " + data.version;
            return false;
        }
        if (data.coinNum < 0)
        {
            error = "Save coinNum is missing or negative.";
            return false;
        }
        if (!IsFinite(data.positionX) || !IsFinite(data.positionY) || !IsFinite(data.positionZ))
        {
            error = "Save position is missing or contains a non-finite value.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool TryPrecheckV1(string json, out string error)
    {
        error = null;
        try
        {
            using (XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(
                Encoding.UTF8.GetBytes(json), new XmlDictionaryReaderQuotas()))
            {
                // 由框架解析 JSON；这里只检查 V1 顶层字段，不自行解析 token。
                if (reader.MoveToContent() != XmlNodeType.Element || reader.GetAttribute("type") != "object")
                {
                    error = "Save JSON root must be an object.";
                    return false;
                }

                int presentFields = 0;
                reader.ReadStartElement();
                while (reader.MoveToContent() == XmlNodeType.Element)
                {
                    string field = reader.LocalName;
                    int fieldBit = GetV1FieldBit(field);
                    if (fieldBit == 0)
                    {
                        reader.Skip();
                        continue;
                    }
                    if (reader.GetAttribute("type") != "number")
                    {
                        error = "Save field must be a JSON number: " + field;
                        return false;
                    }

                    string number = reader.ReadElementContentAsString();
                    if (field == "version" || field == "coinNum")
                    {
                        if (!int.TryParse(number, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
                        {
                            error = "Save field must be an Int32 integer: " + field;
                            return false;
                        }
                    }
                    else if (!float.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out float coordinate) ||
                        !IsFinite(coordinate))
                    {
                        error = "Save field must be a finite float: " + field;
                        return false;
                    }

                    presentFields |= fieldBit;
                }
                reader.ReadEndElement();
                if (reader.MoveToContent() != XmlNodeType.None)
                {
                    error = "Save JSON contains trailing content.";
                    return false;
                }
                if (presentFields != 31)
                {
                    error = "Save JSON is missing required V1 fields.";
                    return false;
                }
            }
            return true;
        }
        catch (Exception exception) when (exception is XmlException || exception is ArgumentException ||
            exception is FormatException || exception is SerializationException)
        {
            error = "Cannot read save JSON structure: " + exception.Message;
            return false;
        }
    }

    private static int GetV1FieldBit(string field)
    {
        switch (field)
        {
            case "version": return 1;
            case "positionX": return 2;
            case "positionY": return 4;
            case "positionZ": return 8;
            case "coinNum": return 16;
            default: return 0;
        }
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

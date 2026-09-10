using System;
using System.IO;
using System.Security;
using System.Text;

public static class SaveFileStore
{
    public static bool TryRead(string path, out SaveData data, out string error)
    {
        data = null;
        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            return SaveJsonCodec.TryDeserialize(json, out data, out error);
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            error = "Cannot read save file: " + exception.Message;
            return false;
        }
    }

    public static bool TryWrite(string path, SaveData data, out string error)
    {
        // 校验和序列化必须先完成，避免非法快照覆盖已有文件。
        if (!SaveJsonCodec.TrySerialize(data, out string json, out error)) return false;

        try
        {
            File.WriteAllText(path, json, Encoding.UTF8);
            return true;
        }
        catch (Exception exception) when (IsFileException(exception))
        {
            error = "Cannot write save file: " + exception.Message;
            return false;
        }
    }

    private static bool IsFileException(Exception exception)
    {
        return exception is IOException || exception is UnauthorizedAccessException ||
            exception is ArgumentException || exception is NotSupportedException ||
            exception is SecurityException;
    }
}

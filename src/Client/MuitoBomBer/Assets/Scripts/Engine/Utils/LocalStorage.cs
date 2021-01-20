using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class LocalStorage
{
#if UNITY_EDITOR
    private const string FILE_NAME = "storage_.dat";
#else
    private const string FILE_NAME = "storage.dat";
#endif
    private static Dictionary<string, string> _dict;
    private static Dictionary<string, string> Dict
    {
        get
        {
            if (_dict == null)
                LoadDict();

            return _dict;
        }
    }

    private static void LoadDict()
    {
        var path = Application.persistentDataPath + "/" + FILE_NAME;

        if (File.Exists(path))
        {
            byte[] buffer = File.ReadAllBytes(path);

            using (var stream = new MemoryStream(buffer))
            {
                _dict = Serializer.Deserialize<Dictionary<string, string>>(stream);
            }
        }
        else
        {
            _dict = new Dictionary<string, string>();
        }
    }

    private static void SaveDict()
    {
        if (_dict == null)
            return;

        var path = Application.persistentDataPath + "/" + FILE_NAME;

        byte[] buffer = new byte[20480]; //20k
        var len = 0L;

        using (var stream = new MemoryStream(buffer))
        {
            Serializer.Serialize(stream, _dict);
            len = stream.Position;
        }

        File.WriteAllBytes(path, buffer.Take((int)len).ToArray());
    }

    public static string GetString(string name)
    {
        string res = null;
        Dict.TryGetValue(name, out res);
        return res;
    }

    public static void SetString(string name, string value, bool autoSave = true)
    {
        Dict[name] = value;

        if (autoSave)
            SaveDict();
    }
}

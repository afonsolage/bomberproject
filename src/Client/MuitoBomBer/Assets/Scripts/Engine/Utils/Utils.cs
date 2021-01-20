using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    /// <summary>
    /// Get the absolute path of the project's StreamingAssets.
    /// </summary>
    /// <returns>Project path</returns>
    public static string SteamingAssetsPath(string path = "")
    {
        //if (Application.isEditor)
        //    return "file:" + Application.dataPath + "/StreamingAssets" + path;
        //else if (Application.platform == RuntimePlatform.Android)
        //    return "jar:file://" + Application.dataPath + "!/assets/" + path;
        //else if (Application.platform == RuntimePlatform.IPhonePlayer)
        //    return "file:" + Application.dataPath + "/Raw" + path;
        //else
        //    return "file:" + Application.dataPath + "/StreamingAssets" + path;

        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            return /*"file:///" + */Application.dataPath + "/StreamingAssets/" + path;
        else if (Application.platform == RuntimePlatform.Android)
            return Application.streamingAssetsPath + "/" + path;
        else
            return "file:///" + Application.streamingAssetsPath + "/" + path;
    }

    /// <summary>
    /// Persistent storage path.
    /// </summary>
    /// <param name="path">Relative path</param>
    /// <returns>Persistent storage path</returns>
    public static string GetPersistentPath(string path)
    {
        return Application.persistentDataPath + "/" + path;
    }

    /// <summary>
    /// Get the path that can be passed to the WWW object
    /// </summary>
    /// <param name='path'>Relative path or full path</param>
    /// <returns>Target path</returns>
    public static string GetWWWPath(string path)
    {
        if (path.StartsWith("http://") || path.StartsWith("ftp://") || path.StartsWith("https://") || path.StartsWith("file://") || path.StartsWith("jar:file://"))
            return path;

        return (Application.platform == RuntimePlatform.Android) ? path.Insert(0, "file://") : path.Insert(0, "file:///");
    }

    public static string GetPlatformFolderForAssetBundles()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.IPhonePlayer:
                return "iPhone";
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.Android:
                return "Android";
        }

        Debug.LogError("Out of Range... RuntimePlatform: " + Application.platform);
        return null;
    }

    /* --------------------------------------------------------- */

    private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
    public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
    {
        if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
        if (value < 0) { return "-" + SizeSuffix(-value); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }
}

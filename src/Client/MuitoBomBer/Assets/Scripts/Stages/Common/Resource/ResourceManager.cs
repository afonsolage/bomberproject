using CommonLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceManager
{
    #region Config XML
    private XElement _configXML = null;
    public XElement ConfigXML { get { return _configXML; } }

    private string _applicationVersion = "0.00";
    public string ApplicationVersion
    {
        get { return _applicationVersion; }
    }

    private string _resourceVersion = "0.00";
    public string ResourceVersion
    {
        get { return _resourceVersion; }
    }

    private string _patchURL = "";
    public string PatchURL
    {
        get { return _patchURL; }
    }

    private string _serverIP = "";
    public string ServerIP
    {
        get { return _serverIP; }
    }

    private uint _serverPort = 0;
    public uint ServerPort
    {
        get { return _serverPort; }
    }

    #endregion

    #region PatchList XML
    private XElement _patchListXML = null;
    public XElement PatchListXML { get { return _configXML; } }
    #endregion

    internal IEnumerator LoadBaseConfigClient(Action<bool> loadedCallback)
    {
        /* ----------------------------------------- */
        // Load Config.xml
        var pathXML = Utils.SteamingAssetsPath("Config.xml");

        using (var www = UnityWebRequest.Get(pathXML))
        {
            //www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            //www.SetRequestHeader("Pragma", "no-cache");
            //www.timeout = 10000;

            AsyncOperation op = www.SendWebRequest();
            while (!op.isDone)
                yield return 0;

            if (www.isNetworkError)
            {
                CLog.W("WWW download had an error: {0} : {1}", www.error, pathXML);

                loadedCallback(false);
            }
            else
            {
                bool finished = false;

                if (www.downloadHandler != null)
                {
                    MemoryStream ms = new MemoryStream(www.downloadHandler.data);
                    _configXML = XElement.Load(ms);
                    if (_configXML == null)
                    {
                        CLog.E("Failed to get xml.");
                        finished = false;
                    }
                    else
                    {
                        var application = XMLHelper.GetXElement(_configXML, "Application");
                        _applicationVersion = XMLHelper.GetSafeAttributeStr(application, "Version");

                        var resource = XMLHelper.GetXElement(_configXML, "Resource");
                        _resourceVersion = XMLHelper.GetSafeAttributeStr(resource, "Version");

                        var info = XMLHelper.GetXElement(_configXML, "Info");
                        _serverIP = XMLHelper.GetSafeAttributeStr(info, "ServerIP");
                        _serverPort = (uint)XMLHelper.GetSafeAttribute(info, "ServerPort");

                        var patch = XMLHelper.GetXElement(_configXML, "Patch");
                        _patchURL = XMLHelper.GetSafeAttributeStr(patch, "URL");

                        finished = true;
                    }
                }
                else
                {
                    CLog.W(string.Format("Failed to download file {0}.", pathXML));
                    finished = false;
                }

                if(!finished) loadedCallback(finished);
            }
        }

        /* ----------------------------------------- */
        // Load PatchList.xml

        pathXML = Utils.SteamingAssetsPath("PatchList.xml");
        using (var www = UnityWebRequest.Get(pathXML))
        {
            //www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            //www.SetRequestHeader("Pragma", "no-cache");
            //www.timeout = 10000;

            AsyncOperation op = www.SendWebRequest();
            while (!op.isDone)
                yield return 0;

            if (www.isNetworkError)
            {
                CLog.W("WWW download had an error: {0} : {1}", www.error, pathXML);

                loadedCallback(false);
            }
            else
            {
                bool finished = false;

                if (www.downloadHandler != null)
                {
                    MemoryStream ms = new MemoryStream(www.downloadHandler.data);
                    _patchListXML = XElement.Load(ms);
                    if (_patchListXML == null)
                    {
                        CLog.E("Failed to get xml.");
                        finished = false;
                    }
                    else
                    {
                        finished = true;
                    }
                }
                else
                {
                    CLog.W(string.Format("Failed to download file {0}.", pathXML));
                    finished = false;
                }

                loadedCallback(finished);
            }
        }
    }
}

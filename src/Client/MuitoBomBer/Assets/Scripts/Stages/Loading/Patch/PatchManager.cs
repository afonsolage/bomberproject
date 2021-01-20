using CommonLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class PatchManager : MonoBehaviour
{
    private delegate void OnCompleteDownload(bool success, long fileSize, string errorMsg, bool retry, int downloadIdx);

    private struct FileData
    {
        public uint crc;
        public long length;
    }

    private struct ConfigData
    {
        public string applicationVersion;
        public string resourceVersion;

        public string _infoServerIP;
        public int _infoServerPort;

        public string _patchUrl;
    }

    private struct PatchListData
    {
        public ConfigData configData;

        public Dictionary<string, FileData> clientPatchList;
        public Dictionary<string, FileData> serverPatchList;
    }

    private struct WebRequest
    {
        public UnityWebRequest www;
        public AsyncOperation op;

        public WebRequest(UnityWebRequest www, AsyncOperation op)
        {
            this.www = www;
            this.op = op;
        }
    }

    /// <summary>
    /// Patch URL used to access the web server with resources/updates.
    /// </summary>
    private string _patchURL = "";
    public string PatchURL
    {
        get { return _patchURL; }
        set { _patchURL = value; }
    }

    /// <summary>
    /// Path of the Config.xml.
    /// </summary>
    private readonly string _configURL = "Config.xml";
    public string ConfigURL
    {
        get { return PatchURL + Utils.GetPlatformFolderForAssetBundles() + "/" + _configURL; }
    }

    /// <summary>
    /// Local Config XML data.
    /// </summary>
    private byte[] _webConfigXML = null;

    /// <summary>
    /// Path of the PatchList.xml.
    /// </summary>
    private readonly string _patchListURL = "PatchList.xml";
    public string PatchListURL
    {
        get { return PatchURL + Utils.GetPlatformFolderForAssetBundles() + "/" + _patchListURL; }
    }

    /// <summary>
    /// Local Patch List XML data.
    /// </summary>
    private byte[] _webPatchListXML = null;

    /// <summary>
    /// Patch List Data.
    /// </summary>
    private PatchListData _patchListData;

    /// <summary>
    /// Pointer of the current Loading Stage.
    /// </summary>
    private LoadingStage _loadingStage;
    public LoadingStage LoadingStage
    {
        get { return _loadingStage; }
        set { _loadingStage = value; }
    }

    /// <summary>
    /// Download File Index, used to get next list item to be downloaded.
    /// </summary>
    private int _downloadFileIndex = 0;

    /// <summary>
    /// List all new files needed to be downloaded.
    /// </summary>
    private Dictionary<int, string> _dicDownloadFile = null;

    /// <summary>
    /// List all new files needed to be deleted.
    /// </summary>
    private List<string> _listDeleteFile = null;

    /// <summary>
    /// Length of all files need to download.
    /// </summary>
    private float _downloadFileLength = 0f;

    /// <summary>
    /// Current download length has already been downloaded.
    /// </summary>
    private float _curDownloadFileLength = 0f;

    /// <summary>
    /// Maximum count that can be downloaded simultaneously.
    /// </summary>
    private int _maxDownloadCount = 1;
    private int _curDownloadCount = 0;

    /// <summary>
    /// If need to stop download.
    /// </summary>
    private bool _downloadStop = false;

    /// <summary>
    /// List all files that are in the queue being downloaded.
    /// </summary>
    private Dictionary<int, WebRequest> _downloadQueue = new Dictionary<int, WebRequest>();

    /// <summary>
    /// Interface of Loading Window.
    /// </summary>
    private LoadingWindow _loadingWindow = null;

    private void Awake()
    {
        _patchListData.configData.applicationVersion = "";
        _patchListData.configData.resourceVersion = "";
    }

    /// <summary>
    /// Start patch manager.
    /// </summary>
    public void ConnectPatch()
    {
        _loadingWindow = LoadingStage.UIManager.FindInstance(WindowType.LOADING) as LoadingWindow;
        if (_loadingWindow == null)
        {
            CLog.E("Failed to get LoadingWindow!");
            return;
        }

        _loadingWindow.SetStatus(LoadingWindow.LoadingStatus.CHECKING_PATCH_DETAILS);

        // Get Config.xml from the Web Server.
        CoroutineUtils.StartThrowingCoroutine(this,
            GetConfigFile(new Action<int>(CheckConfigFile)),
            ex =>
            {
                if (ex != null)
                {
                    CLog.Catch(ex);
                    CheckConfigFile(1);
                }
            }
        );
    }

    /// <summary>
    /// Obtain the Config.xml from the web server.
    /// </summary>
    /// <param name="FinishConfigFile"></param>
    /// <returns></returns>
    private IEnumerator GetConfigFile(Action<int> FinishConfigFile)
    {
        using (var www = UnityWebRequest.Get(ConfigURL))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            www.timeout = 10000;

            AsyncOperation op = www.SendWebRequest();
            while (!op.isDone)
            {
                yield return 0;
            }

            if (www.isNetworkError)
            {
                CLog.W("WWW download had an error: {0} : {1}", www.error, ConfigURL);

                if (www.error == "Cannot resolve destination host")
                {
                    FinishConfigFile(3);
                }
                else
                {
                    FinishConfigFile(1);
                }
            }
            else
            {
                bool finished = false;

                if (www.downloadHandler != null)
                {
                    _webConfigXML = www.downloadHandler.data;
                    var ms = new MemoryStream(_webConfigXML);

                    var configXML = XElement.Load(ms);
                    if (configXML == null)
                    {
                        CLog.E("Failed to get xml.");
                        finished = false;
                    }
                    else
                    {
                        var application = XMLHelper.GetXElement(configXML, "Application");
                        _patchListData.configData.applicationVersion = XMLHelper.GetSafeAttributeStr(application, "Version");

                        var resource = XMLHelper.GetXElement(configXML, "Resource");
                        _patchListData.configData.resourceVersion = XMLHelper.GetSafeAttributeStr(resource, "Version");

                        var info = XMLHelper.GetXElement(configXML, "Info");
                        _patchListData.configData._infoServerIP = XMLHelper.GetSafeAttributeStr(info, "ServerIP");
                        _patchListData.configData._infoServerPort = (int)XMLHelper.GetSafeAttribute(info, "ServerPort");

                        var patch = XMLHelper.GetXElement(configXML, "Patch");
                        _patchListData.configData._patchUrl = XMLHelper.GetSafeAttributeStr(patch, "URL");

                        finished = true;
                    }
                }
                else
                {
                    CLog.W(string.Format("Failed to download file {0} from {1}.", _patchListURL, PatchListURL));
                    finished = false;
                }

                FinishConfigFile((finished) ? 0 : 2);
            }
        }
    }

    /// <summary>
    /// Obtain the PathList.xml from the web server.
    /// </summary>
    /// <param name="FinishConfigFile"></param>
    /// <returns></returns>
    private IEnumerator GetPatchFile(Action<int> FinishPatchFile)
    {
        // Process PatchList.xml from the StreamingAssets.
        var localPatchList = Main.Current.ResourceManager.PatchListXML;
        if (localPatchList == null)
        {
            CLog.E("PatchList.xml from StreamingAssets is not loaded.");
            FinishPatchFile(2);
        }
        else
        {
            var fileList = localPatchList.Elements("File");
            if (fileList != null)
            {
                _patchListData.clientPatchList = new Dictionary<string, FileData>();

                foreach (var file in fileList)
                {
                    FileData patch;
                    string name = XMLHelper.GetSafeAttributeStr(file, "Name");
                    patch.length = XMLHelper.GetSafeAttributeLong(file, "Size");
                    patch.crc = (uint)XMLHelper.GetSafeAttribute(file, "CRC");

                    _patchListData.clientPatchList.Add(name, patch);
                }
            }
        }

        // Get PatchList.xml from Web Server.
        using (var www = UnityWebRequest.Get(PatchListURL))
        {
            www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            www.SetRequestHeader("Pragma", "no-cache");
            www.timeout = 10000;

            AsyncOperation op = www.SendWebRequest();
            while (!op.isDone)
                yield return 0;

            if (www.isNetworkError)
            {
                CLog.W("WWW download had an error: {0} : {1}", www.error, PatchListURL);

                if (www.error == "Cannot resolve destination host")
                {
                    FinishPatchFile(3);
                }
                else
                {
                    FinishPatchFile(1);
                }
            }
            else
            {
                bool finished = false;

                if (www.downloadHandler != null)
                {
                    _webPatchListXML = www.downloadHandler.data;
                    var ms = new MemoryStream(_webPatchListXML);
                    var patchListXML = XElement.Load(ms);
                    if (patchListXML == null)
                    {
                        CLog.E("Failed to get xml.");
                        finished = false;
                    }
                    else
                    {
                        var fileList = patchListXML.Elements("File");
                        if (fileList != null)
                        {
                            _patchListData.serverPatchList = new Dictionary<string, FileData>();

                            foreach (var file in fileList)
                            {
                                FileData patch;
                                string name = XMLHelper.GetSafeAttributeStr(file, "Name");
                                patch.length = XMLHelper.GetSafeAttributeLong(file, "Size");
                                patch.crc = (uint)XMLHelper.GetSafeAttribute(file, "CRC");

                                _patchListData.serverPatchList.Add(name, patch);
                            }
                        }

                        finished = true;
                    }
                }
                else
                {
                    CLog.W(string.Format("Failed to download file {0} from {1}.", _patchListURL, PatchListURL));
                    finished = false;
                }

                FinishPatchFile((finished) ? 0 : 2);
            }
        }
    }

    /// <summary>
    /// Check Config file.
    /// </summary>
    /// <param name="connectNumber"></param>
    private void CheckConfigFile(int connectNumber)
    {
        if (connectNumber == 1)
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "Failed to download patch. Please try again.", MessageBox.MSG_BOX_STYLE.OK, (result =>
                {
                    ConnectPatch();
                }));
            }

            return;
        }
        else if (connectNumber == 2)
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "Failed to download patch. Please try again.", MessageBox.MSG_BOX_STYLE.OK, (result =>
                {
                    Application.Quit();
                }));
            }

            return;
        }
        else if (connectNumber == 3)
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "The internet connection is not stable. \nPlease check out your connection. \nWould you like to try it again?", MessageBox.MSG_BOX_STYLE.YES_NO, (result =>
                {
                    if (result)
                    {
                        ConnectPatch();
                    }
                    else
                    {
                        Application.Quit();
                    }
                }));
            }

            return;
        }

        // Verify that the application is in the latest version.
        if (!CheckApplicationVersion())
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "There is a newer version of the game.\nPlease update to the newest version.", MessageBox.MSG_BOX_STYLE.YES_NO, (result =>
                {
                    if (result)
                    {
                        string link = "";
                        switch (Application.platform)
                        {
                            case RuntimePlatform.OSXEditor:
                            case RuntimePlatform.OSXPlayer:
                            case RuntimePlatform.IPhonePlayer:
                                link = "https://play.google.com/store";
                                break;
                            case RuntimePlatform.WindowsPlayer:
                            case RuntimePlatform.WindowsEditor:
                            case RuntimePlatform.Android:
                                link = "https://itunes.apple.com/";
                                break;
                        }

                        Application.OpenURL(link);
                        Application.Quit();
                    }
                    else
                    {
                        Application.Quit();
                    }
                }));
            }

            return;
        }

        // Get PathFiles.xml from the Web Server.
        CoroutineUtils.StartThrowingCoroutine(this,
            GetPatchFile(new Action<int>(CheckPatchFiles)),
            ex =>
            {
                if (ex != null)
                {
                    CLog.Catch(ex);
                    CheckPatchFiles(1);
                }
            }
        );
    }

    /// <summary>
    /// Checks if need to update client resources.
    /// </summary>
    /// <param name="connectNumber"></param>
    private void CheckPatchFiles(int connectNumber)
    {
        if (connectNumber == 1)
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "Failed to download patch. Please try again.", MessageBox.MSG_BOX_STYLE.OK, (result =>
                {
                    ConnectPatch();
                }));
            }

            return;
        }
        else if (connectNumber == 2)
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "Failed to download patch. Please try again.", MessageBox.MSG_BOX_STYLE.OK, (result =>
                {
                    Application.Quit();
                }));
            }

            return;
        }
        else if (connectNumber == 3)
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                msgBox.CreateMsgBox("Error", "The internet connection is not stable. \nPlease check out your connection. \nWould you like to try it again?", MessageBox.MSG_BOX_STYLE.YES_NO, (result =>
                {
                    if (result)
                    {
                        ConnectPatch();
                    }
                    else
                    {
                        Application.Quit();
                    }
                }));
            }

            return;
        }

        StartCoroutine(CheckUpdate());
    }

    private IEnumerator CheckUpdate()
    {
        // Change loading status "checking patch data".
        _loadingWindow.SetStatus(LoadingWindow.LoadingStatus.CHECKING_PATCH_DATA);
        yield return new WaitForSeconds(1);

        // Check if it's necessary to update resource files from Client.
        if (UpdateResource())
        {
            var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
            if (msgBox)
            {
                string size = Utils.SizeSuffix((long)_downloadFileLength);
                string msg = string.Format("There is update pending, resources waiting to download. ({0})\nDownloading resources will only be done once.\n[ff0000]Please connect to a WIFI network or additional charges may incur depending on your data plan.", size);
                msgBox.CreateMsgBox("Information", msg, MessageBox.MSG_BOX_STYLE.YES_NO, (result =>
                {
                    if (result)
                    {
                        _loadingWindow.SetStatus(LoadingWindow.LoadingStatus.CHECKING_DOWNLOAD_PATCH_DATA);

                        // Delete the old files, which do not exist in the current resource version.
                        StartDelete();

                        // Download the new files.
                        StartDownload();
                    }
                    else
                    {
                        Application.Quit();
                    }
                }));
            }

            yield break;
        }
        else
        {
            // Change loading status "done".
            _loadingWindow.SetStatus(LoadingWindow.LoadingStatus.DONE);
            yield return new WaitForSeconds(1);

            // The client does not have any pending updates, it is already in the latest version.
            _loadingStage.FinishPatchCheck();
        }
    }

    /// <summary>
    /// Verify that the application is in the latest version.
    /// </summary>
    /// <returns></returns>
    private bool CheckApplicationVersion()
    {
        if (_patchListData.configData.applicationVersion == "")
        {
            CLog.E("Check Patch System");
            return false;
        }

        return float.Parse(Main.Current.ResourceManager.ApplicationVersion) >= float.Parse(_patchListData.configData.applicationVersion);
    }

    /// <summary>
    /// Verify that the resource is in the latest version.
    /// </summary>
    /// <returns></returns>
    private bool CheckResourceVersion()
    {
        if (_patchListData.configData.resourceVersion == "")
        {
            CLog.E("Check Patch System");
            return false;
        }

        return float.Parse(Main.Current.ResourceManager.ResourceVersion) >= float.Parse(_patchListData.configData.resourceVersion);
    }

    /// <summary>
    /// Check if it's encessary to update resource in the Client.
    /// </summary>
    /// <returns></returns>
    private bool UpdateResource()
    {
        // Verify that the client resource's version in on the save on web server version.
        if (!CheckResourceVersion())
        {
            // Every new file need to be update, will to have your own index.
            int downloadIndex = 0;

            // Initialization of the Dictionary and List of the files.
            _dicDownloadFile = new Dictionary<int, string>();
            _listDeleteFile = new List<string>();

            string fileName = "";
            FileData currentFileData;

            // Check files that need to be updated.
            foreach (var file in _patchListData.serverPatchList)
            {
                fileName = file.Key;
                currentFileData = file.Value;

                // Output.
                FileData clientFileData;

                // Check if file already exist file in Client and compare with lastest version on web server.
                if (_patchListData.clientPatchList.TryGetValue(fileName, out clientFileData))
                {
                    if (clientFileData.length != currentFileData.length ||
                        clientFileData.crc != currentFileData.crc ||
                        !File.Exists(Utils.SteamingAssetsPath(fileName)))
                    {
                        // Update lenght of files for be downloaded.
                        _downloadFileLength += currentFileData.length;

                        // Add in dictionary file necessary to update.
                        _dicDownloadFile.Add(downloadIndex, fileName);

                        // Increase file index for be used in next file.
                        _downloadFileIndex++;
                    }
                }
                else // If not found file in Client (resource) is because is a new file added.
                {
                    // Update lenght of files for be downloaded.
                    _downloadFileLength += currentFileData.length;

                    // Add in dictionary file necessary to update.
                    _dicDownloadFile.Add(downloadIndex, fileName);

                    // Increase file index for be used in next file.
                    _downloadFileIndex++;
                }
            }

            // Check files that need to be deleted.
            foreach(var file in _patchListData.clientPatchList)
            {
                if(!_patchListData.serverPatchList.ContainsKey(fileName))
                {
                    // Not found that file, let to delete.
                    _listDeleteFile.Add(fileName);
                }
            }

            // Check if need to send client update for lastest version.
            return (_dicDownloadFile.Count > 0 || _listDeleteFile.Count > 0) ? true : false;
        }

        // Not found new update.
        return false;
    }

    /// <summary>
    /// Process all old files that need to be deleted.
    /// </summary>
    private void StartDelete()
    {
        if (_listDeleteFile != null && _listDeleteFile.Count > 0)
        {
            foreach (var file in _listDeleteFile)
            {
                if (!DeleteFile(PatchURL + Utils.GetPlatformFolderForAssetBundles() + "/" + file))
                    CLog.E("Failed in to delete file: {0}", file);
            }
        }
    }

    /// <summary>
    /// Process files that need to be downloaded.
    /// </summary>
    private void StartDownload()
    {
        // Set fps to 60.
        Application.targetFrameRate = 60;

        _downloadStop = false;

        // Set max download count.
        _maxDownloadCount = (SystemInfo.systemMemorySize <= 1024) ? 3 : (SystemInfo.systemMemorySize <= 2048) ? 5 : 7;

        _loadingWindow.SetStatus(LoadingWindow.LoadingStatus.DOWNLOADING_PATCH_DATA);
        _loadingWindow.SetDownloadStatus(_curDownloadFileLength, _downloadFileLength);

        // Start download of new files.
        StartCoroutine("Co_Download");
        StartCoroutine(Co_DownloadQueueCheck(new Action<float, long>(OnUpdateDownloadProgress), new OnCompleteDownload(OnCompleteDownloadFile)));
    }

    /// <summary>
    /// Process all files that need to be downloaded.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Co_Download()
    {
        // Current count of files already downloaded.
        _curDownloadCount = 0;

        while (_curDownloadCount < _dicDownloadFile.Count)
        {
            yield return 0;
            DownloadQueue();
        }

        // All files pending already downloaded.
        CompleteDownload();
        yield break;
    }

    /// <summary>
    /// Process all the files that are in the download queue.
    /// </summary>
    /// <param name="onDownloadProgress"></param>
    /// <param name="onCompleteDownload"></param>
    /// <returns></returns>
    private IEnumerator Co_DownloadQueueCheck(Action<float, long> onDownloadProgress, OnCompleteDownload onCompleteDownload)
    {
        List<int> removeList = new List<int>();

        for (;;)
        {
            yield return true;

            foreach (var download in _downloadQueue)
            {
                int downloadIndex = download.Key;

                UnityWebRequest www = download.Value.www;
                AsyncOperation op = download.Value.op;

                FileData patchData;
                string fileName = _dicDownloadFile[downloadIndex];
                _patchListData.serverPatchList.TryGetValue(fileName, out patchData);

                // Check if downloaded file is already done.
                if (!op.isDone)
                {
                    onDownloadProgress?.Invoke(op.progress, patchData.length);
                }
                else
                {
                    // Check if has successfully downloaded the file.
                    if (www.isNetworkError)
                    {
                        // Failed to download file, try to download again.
                        onCompleteDownload(false, patchData.length, string.Format("{0}\nThere is an error. Would you like to try again?", www.error), true, downloadIndex);
                    }
                    else
                    {
                        try
                        {
                            // Check if buffer is not null.
                            if (www.downloadHandler != null)
                            {
                                // Save new file downloaded.
                                SaveBytesToFile(Utils.SteamingAssetsPath(fileName), www.downloadHandler.data);

                                onCompleteDownload(true, patchData.length, null, false, -1);

                                // Clear web download.
                                www.Dispose();
                                www = null;
                            }
                            else
                            {
                                // Failed to download file, try to download again.
                                onCompleteDownload(false, patchData.length, string.Format("Patch file error: cannot connect to patch server. Please try again later. There may be time difference by location in patch server synchronization. The app will shut down."), false, -1);
                            }
                        }
                        catch
                        {
                            // Failed to download file, try to download again.
                            onCompleteDownload(false, patchData.length, string.Format("{0}\nThere is an error. Would you like to try again?", "AssetBundle. Download Error"), true, downloadIndex);
                        }

                    }

                    // File downloaded with successfully, now it's necessary to remove from download queue.
                    removeList.Add(downloadIndex);
                }
            }

            // Process all files need be removed form download queue.
            if (removeList.Count > 0)
            {
                for (int i = 0; i < removeList.Count; i++)
                {
                    _downloadQueue.Remove(removeList[i]);
                }

                removeList.Clear();
            }
        }
    }

    /// <summary>
    /// Checks can already add a new file to download queue.
    /// </summary>
    private void DownloadQueue()
    {
        // Checks if already have maximum number of files to downloading.
        if (_downloadQueue.Count >= _maxDownloadCount)
            return;

        // Checks if already downloaded all the files that needed to be downloaded.
        if (_dicDownloadFile.Count <= _downloadFileIndex)
            return;

        // Checks if download is in "stop".
        if (_downloadStop)
            return;

        // Add file to the download queue.
        AddDownloadQueue(_downloadFileIndex);

        // Increase file index.
        _downloadFileIndex++;
    }

    /// <summary>
    /// Add new file to the download queue.
    /// </summary>
    private void AddDownloadQueue(int downloadIndex)
    {
        var file = _dicDownloadFile[downloadIndex];
        var filePatchUrl = PatchURL + Utils.GetPlatformFolderForAssetBundles() + "/" + file;

        //UnityWebRequest www = UnityWebRequest.GetAssetBundle(filePatchUrl);
        UnityWebRequest www = UnityWebRequest.Get(filePatchUrl);
        www.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
        www.SetRequestHeader("Pragma", "no-cache");
        www.timeout = 10000;

        AsyncOperation op = www.SendWebRequest();
        _downloadQueue.Add(downloadIndex, new WebRequest(www, op));
    }

    /// <summary>
    /// Process file that was downloaded or had a failure
    /// </summary>
    /// <param name="success"></param>
    /// <param name="fileSize"></param>
    /// <param name="errorMsg"></param>
    /// <param name="retry"></param>
    /// <param name="downloadIdx"></param>
    private void OnCompleteDownloadFile(bool success, long fileSize, string errorMsg, bool retry, int downloadIdx)
    {
        // If the file has not been downloaded successfully.
        if (!success)
        {
            // Check if can to retry download file again.
            if (retry)
            {
                var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
                if (msgBox)
                {
                    msgBox.CreateMsgBox("Error", errorMsg, MessageBox.MSG_BOX_STYLE.YES_NO, (result =>
                    {
                        if (result)
                        {
                            _downloadStop = false;
                            AddDownloadQueue(downloadIdx);
                        }
                        else
                        {
                            Application.Quit();
                        }
                    }));

                    return;
                }
            }
            else
            {
                _downloadStop = true;

                var msgBox = LoadingStage.UIManager.FindInstance(WindowType.MSG_BOX, true, true) as MessageBox;
                if (msgBox)
                {
                    msgBox.CreateMsgBox("Error", errorMsg, MessageBox.MSG_BOX_STYLE.OK, (result =>
                    {
                        Application.Quit();
                    }));
                }
            }

            return;
        }

        // Update current download file length with new file downloaded.
        _curDownloadFileLength += fileSize;

        // Update current count of files already downloaded.
        _curDownloadCount++;

        // Update in Loading Window current status of Patch.
        _loadingWindow.SetDownloadStatus(_curDownloadFileLength, _downloadFileLength);
    }

    /// <summary>
    /// Update the current progress of downloads in the interface.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="fileSize"></param>
    private void OnUpdateDownloadProgress(float value, long fileSize)
    {
        _loadingWindow.SetDownloadStatus(_curDownloadFileLength, _downloadFileLength);
    }

    /// <summary>
    /// Update was finalized, all files needed to be downloaded were finalized, finished step.
    /// </summary>
    private void CompleteDownload()
    {
        // Back for original FPS.
        Application.targetFrameRate = Main.Current.FPS;

        // Update Config.xml in the Client.
        SaveBytesToFile(Utils.SteamingAssetsPath("Config.xml"), _webConfigXML);

        // Update PatchList.xml in the Client.
        SaveBytesToFile(Utils.SteamingAssetsPath("PatchList.xml"), _webPatchListXML);

        // Patch is done.
        _loadingStage.FinishPatchCheck();
    }

    /// <summary>
    /// Saves the bytes to file.
    /// </summary>
    /// <param name='path'>Path</param>
    /// <param name='root'>Root</param>
    private bool SaveBytesToFile(string path, byte[] bytes)
    {
        try
        {
            // Check if file already exists, if yes, delete old file.
            if (File.Exists(path))
            {
                // Delete file.
                DeleteFile(path);
            }

            // Check if has sub folder necessary for be create.
            int index = path.LastIndexOf('/');
            if (-1 != index)
            {
                // Create directory.
                Directory.CreateDirectory(path.Substring(0, index));
            }

            // Create file new file.
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                // Write the bytes of file.
                fs.Write(bytes, 0, bytes.Length);

                // Flush file.
                fs.Flush();

                // Close and dispose file.
                fs.Close();
                fs.Dispose();
            }

            return true;
        }
        catch (Exception ex)
        {
            CLog.E("Catching error in to save bytes in file.");
            CLog.Catch(ex);
        }

        return false;
    }

    /// <summary>
    /// Delete the file.
    /// </summary>
    /// <param name='path'>Path</param>
    private bool DeleteFile(string path)
    {
        try
        {
            // Check if file already exists, if yes, delete file.
            if (File.Exists(path))
            {
                CLog.I("Deleted file:" + path);

                // Delete file.
                File.Delete(path);

                // File deleted.
                return true;
            }
        }
        catch (Exception ex)
        {
            CLog.E("Catching error in to delete file.");
            CLog.Catch(ex);
        }

        // File don't deleted.
        return false;
    }
}

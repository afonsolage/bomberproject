using CommonLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadingStage : BaseStage
{
    private UIManager _uiManager;
    internal UIManager UIManager { get { return _uiManager; } }

    private PatchManager _patchManager;
    internal PatchManager PatchManager { get { return _patchManager; } }

    public override void Init(params object[] args)
    {
        var uiRoot = GameObject.Find("UI Root");
        if (uiRoot == null)
        {
            CLog.E("An UI Root must exists on this scene.");
            return;
        }

        _uiManager = uiRoot.GetComponent<UIManager>();
        _uiManager.Setup(this);

        var loadingWindow = _uiManager.Instanciate(WindowType.LOADING) as LoadingWindow;
        loadingWindow?.SetStatus(LoadingWindow.LoadingStatus.LOADING_PATCH_DATA);
    }

    public void InitPatch(string patchUrl)
    {
        if (_patchManager == null)
        {
            var patch = GameObject.Find("PatchManager");
            if (patch != null)
            {
                _patchManager = patch.GetComponent<PatchManager>();
                if(_patchManager == null)
                {
                    CLog.W("PatchManager (GameObject) found but don't has PatchManager Component. Adding component by manually.");
                    _patchManager = patch.AddComponent<PatchManager>();
                }
            }
            else
            {
                CLog.W("Failed to find PatchManager (GameObject). Creating GameObject and Component by manually.");

                patch = new GameObject("PatchManager");
                _patchManager = patch.AddComponent<PatchManager>();
            }
        }

        _patchManager.LoadingStage = this;
        _patchManager.PatchURL = patchUrl;

        FinishPatchCheck();

        //#if UNITY_EDITOR
        //        FinishPatchCheck();
        //#else
        //        _patchManager.ConnectPatch();
        //#endif
    }

    public void FinishPatchCheck()
    {
        // Destroy Patch Manager from scene.
        UnityEngine.Object.Destroy(_patchManager);
        _patchManager = null;

        // Let to load all resources of the Client.
        if (!LoadResource())
        {
            CLog.E("Failed to read Resources from Client.");
            return;
        }

        // Go to main stage.
        StageManager.ChangeStage(StageType.Lobby, Main.Current.ResourceManager.ServerIP, Main.Current.ResourceManager.ServerPort);
    }

    public bool LoadResource()
    {
        return true;
    }

    public override void OnDispose()
    {
        var patch = GameObject.Find("PatchManager");
        if (patch != null)
        {
            if (Application.isEditor)
                UnityEngine.Object.DestroyImmediate(patch);
            else
                UnityEngine.Object.Destroy(patch);
        }
    }

    public override void OnTick(float delta)
    {

    }

    protected override void ProcessMessage(string message)
    {

    }
}

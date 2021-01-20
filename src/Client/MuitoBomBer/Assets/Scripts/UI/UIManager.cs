using CommonLib.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class UIComponent : MonoBehaviour
{
    protected UIManager _parent;
    internal UIManager Parent { get { return _parent; } }

    internal void Setup(UIManager parent)
    {
        _parent = parent;
    }

    protected T CurrentStage<T>() where T : BaseStage
    {
        return _parent.Stage as T;
    }

    internal virtual void OnBeforeDestroy() { }
}

/// <summary>
/// Enumaration with all windows types.
/// </summary>
public enum WindowType
{
    LOGIN,
    PLAYER_CREATION,
    MAIN,
    LOBBY,
    ROOM,
    FRIEND,
    MSG_BOX,
    CHAT,
    MSG_HINT,
    WAITING,
    WINNER,
    ROOM_OPTION,
    ROOM_PASSWORD,
    ROOM_PLAYER_MENU,
    HUD_ROOT,
    HUD_TEXT,
    FRIEND_ADD,
    LOADING,

    MAX
}

[Serializable]
public class WindowPrefab
{
    public WindowType windowType;
    public GameObject gameObject;
}

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Camera of the UI Root.
    /// </summary>
    public Camera _camera;

    /// <summary>
    /// Prefabs of the Windows.
    /// </summary>
    [Header("Windows Prefabs")]
    public WindowPrefab[] prefabs;

    /// <summary>
    /// Stage.
    /// </summary>
    private BaseStage _stage;
    internal BaseStage Stage { get { return _stage; } }

    /// <summary>
    /// Dictionary with all windows instancieds.
    /// </summary>
    private readonly Dictionary<WindowType, UIComponent> _windows = new Dictionary<WindowType, UIComponent>();

    /// <summary>
    /// Get count of instancied windows.
    /// </summary>
    internal int InstanciedWindows
    {
        get
        {
            lock (_windows)
            {
                return _windows.Count;
            }
        }
    }

    /// <summary>
    /// Setup.
    /// </summary>
    /// <param name="stage"></param>
    internal void Setup(BaseStage stage)
    {
        DestroyAll();

        _stage = stage;
    }

    /// <summary>
    /// Instanciate windows.
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    internal UIComponent Instanciate(WindowType window)
    {
        var prefab = Array.Find(prefabs, (w) => w.windowType == window);

#if DEBUG
        if (prefab == null)
        {
            CLog.E("Failed to find prefab for window {0}. Did you setup it on inspector?", window);
            return null;
        }
#endif

        var instance = NGUITools.AddChild(_camera.gameObject, prefab.gameObject);
        var uiComponent = InitComponentWindow(instance);

        lock (_windows)
        {
            _windows.Add(window, uiComponent);
        }

        return uiComponent;
    }

    private UIComponent InitComponentWindow(GameObject window)
    {
        var component = window.GetComponent<UIComponent>();

#if DEBUG
        if (component == null)
        {
            CLog.E("All Windows instanciated by UIManager should inherits from UIComponent.");
            return null;
        }
#endif

        component.Setup(this);

        return component;
    }

    /// <summary>
    /// Find instancied window by type, will return windows if this windows is instancied.
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    internal UIComponent FindInstance(WindowType window)
    {
        UIComponent res = null;

        lock (_windows)
        {
            _windows.TryGetValue(window, out res);
        }

        return res;
    }

    /// <summary>
    /// Find instancied window by type, will return windows if this windows is instancied.
    /// </summary>
    /// <param name="window"></param>
    /// <param name="init"></param>
    /// <param name="destroy"></param>
    /// <returns></returns>
    internal UIComponent FindInstance(WindowType window, bool init = false, bool destroy = false)
    {
        UIComponent res = null;

        lock (_windows)
        {
            _windows.TryGetValue(window, out res);
        }

        if (res && destroy)
        {
            Destroy(window);
            return (init) ? res = Instanciate(window) : null;
        }

        if (res == null && init)
        {
            res = Instanciate(window);
        }

        return res;
    }

    internal GameObject GetWindowGameObject(WindowType window)
    {
        return Array.Find(prefabs, (w) => w.windowType == window)?.gameObject;
    }

    /// <summary>
    /// Destroy all windows instancied.
    /// </summary>
    internal void DestroyAll()
    {
        lock (_windows)
        {
            foreach (var component in _windows.Values)
            {
                component.OnBeforeDestroy();
                if (component != null && component.gameObject != null)
                {
                    NGUITools.Destroy(component.gameObject);
                }
            }

            _windows.Clear();
        }
    }

    /// <summary>
    /// Destroy window instancied by type.
    /// </summary>
    /// <param name="window"></param>
    internal void Destroy(WindowType window)
    {
        var component = FindInstance(window);
        if (component == null)
        {
            CLog.D("Unable to destroy window {0}. Instance wasn't found.", window);
            return;
        }

        lock (_windows)
        {
            _windows.Remove(window);
        }

        component.OnBeforeDestroy();
        NGUITools.Destroy(component.gameObject);
    }

    internal static GameObject FindObjectInChildren(GameObject gameObject, string name)
    {
        foreach (var t in gameObject.GetComponentsInChildren<Transform>())
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }
}

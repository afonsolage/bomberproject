using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputJoystickManager : MonoBehaviour
{
    public const float HALF_PI = Mathf.PI / 2;

    public enum Constraints
    {
        None,
        Horizontal,
        Vertical
    }

    public Transform _background;
    public Transform _center;

    public float _radius = 0.2f;

    public bool _isFloating = true;
    public bool _isResetPos = true;

    private bool _isChecked = true;
    private bool _isDrag = false;
    private bool _isReverse = false;

    public Constraints constraint = Constraints.None;

    public float _smoothReverseTime = 0.3f;

    public List<EventDelegate> onJoystick = new List<EventDelegate>();
    public static InputJoystickManager current;

    public Vector2 _parameterDelta = Vector2.zero;

    private Vector3 _startPos = Vector3.zero;
    private Vector3 _prevPos = Vector3.zero;

    private Vector3 _defaultBackgroundPos = Vector3.zero;
    private Vector3 _defaultCenterPos = Vector3.zero;

    private float _timer = 0;

    [SerializeField]
    private float _scaleUIRoot = 1;

    [Header("Fade Out Effect")]
    /// <summary>
    /// Fade effects.
    /// </summary>
    public bool _isFadeOutEffect = true;
    public UIWidget[] widgetsToFade;
    public float fadeOutDelay = 1f;
    public float fadeOutAlpha = 0.2f;

    private void Awake()
    {
        if (_scaleUIRoot != transform.root.localScale.x)
        {
            _radius = _radius / _scaleUIRoot * transform.root.localScale.x;
            _scaleUIRoot = transform.root.localScale.x;
        }

        _defaultBackgroundPos = _background.position;
        _defaultCenterPos = _center.position;
    }

    private void Start()
    {
        _scaleUIRoot = transform.root.localScale.x;

        if (_background == null)
        {
            Debug.LogError("Child object 'Background' did not find. Fix it!");
        }

        if (_center == null)
        {
            Debug.LogError("Child object 'Center' did not find. Fix it!");
        }

        if (_background != null && _center != null)
        {
            if (_radius == 0f)
            {
                Debug.LogError("Radius is zero. Fix it!");
                return;
            }
            else
            {
                _isChecked = true;
            }
        }

        if (_isChecked && _isFloating)
        {
            ResetPosition();

            if (_isFadeOutEffect)
            {
                StartCoroutine(FadeOutJoystick());
            }
            else
            {
                _background.gameObject.SetActive(false);
                _center.gameObject.SetActive(false);
            }
        }
    }

    private void ResetPosition()
    {
        if(_isResetPos)
        {
            _background.position = _defaultBackgroundPos;
            _center.position = _defaultCenterPos;
        }
    }

    private void OnPress(bool isPressed)
    {
        if (_isChecked)
        {
            Vector3 lastWorldPosition = UICamera.lastWorldPosition;

            if (isPressed)
            {
                PointerDown(lastWorldPosition);
            }
            else
            {
                PointerUp(lastWorldPosition);
            }
        }
    }

    private void PointerDown(Vector3 pos)
    {
        if (_isFloating)
        {
            _startPos = pos;
            _background.position = _startPos;
            _center.position = _startPos;

            if (_isFadeOutEffect)
            {
                for (int i = 0; i < widgetsToFade.Length; ++i)
                {
                    TweenColor.Begin(widgetsToFade[i].gameObject, 0.1f, Color.white).method = UITweener.Method.EaseIn;
                }
            }
            else
            {
                _background.gameObject.SetActive(true);
                _center.gameObject.SetActive(true);
            }

            _prevPos = _startPos;
        }

        _startPos = _background.position;
        _center.position = _startPos;
        _prevPos = _startPos;

        _isDrag = true;
        _isReverse = false;
    }

    private void OnDrag(Vector2 delta)
    {
        if (_isChecked)
        {
            Vector3 newPos = new Vector3(UICamera.lastEventPosition.x, UICamera.lastEventPosition.y, 0);
            _prevPos = UICamera.mainCamera.ScreenToWorldPoint(newPos);
        }
    }

    private void PointerUp(Vector3 eventData)
    {
        _isDrag = false;
        _isReverse = true;
        _timer = 0;
    }

    private void Update()
    {
        if (_isChecked)
        {
            if (_isDrag)
            {
                _timer += Time.deltaTime;
                CalcPositionCenter(_prevPos);
            }
            else if (_isReverse)
            {
                _timer += Time.deltaTime;

                if (_timer > _smoothReverseTime)
                {
                    ReverseEnd();
                }
                else
                {
                    CalcPositionCenter(_prevPos);
                }
            }
        }
    }

    private void ReverseEnd()
    {
        if (_isChecked)
        {
            if (_isFloating)
            {
                ResetPosition();

                if (_isFadeOutEffect)
                {
                    StartCoroutine(FadeOutJoystick());
                }
                else
                {
                    _background.gameObject.SetActive(false);
                    _center.gameObject.SetActive(false);
                }
            }
            else
            {
                _center.position = _startPos;
            }

            SendMessageOnJoystick(Vector3.zero);
            _isReverse = false;
        }
    }

    private void CalcPositionCenter(Vector3 pos)
    {
        Vector3 curPos = pos - _startPos;
        curPos = Vector3.Lerp(Vector3.zero, curPos, _timer / _smoothReverseTime);

        if (curPos.magnitude > _radius)
            curPos = curPos.normalized * _radius;

        if (_isReverse)
        {
            float lerp = _radius - Mathf.Lerp(0f, _radius, _timer / _smoothReverseTime);
            curPos = curPos.normalized * lerp;
        }

        if (constraint == Constraints.Horizontal)
        {
            curPos.y = 0f;
        }
        else if (constraint == Constraints.Vertical)
        {
            curPos.x = 0f;
        }

        _center.position = _startPos + curPos;
        SendMessageOnJoystick(curPos);
    }

    private void SendMessageOnJoystick(Vector3 newPos)
    {
        float x = newPos.x / _radius;
        float y = newPos.y / _radius;

        Vector2 pos = new Vector2((x <= 1) ? x : 1, (y <= 1) ? y : 1);

        if (EventDelegate.IsValid(onJoystick))
        {
            OnJoystick(pos);
        }
        else
        {
            gameObject.SendMessage("OnJoystick", pos, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnJoystick(Vector2 delta)
    {
        if (current != null) return;

        current = this;
        _parameterDelta = delta;

        EventDelegate.Execute(onJoystick);

        current = null;
    }

    IEnumerator FadeOutJoystick()
    {
        yield return new WaitForSeconds(fadeOutDelay);

        for (int j = 0; j < widgetsToFade.Length; ++j)
        {
            UIWidget widget = widgetsToFade[j];
            Color lastColor = widget.color;
            Color newColor = lastColor;
            newColor.a = fadeOutAlpha;

            TweenColor.Begin(widget.gameObject, 0.5f, newColor).method = UITweener.Method.EaseOut;
        }
    }

    private float GetAngle(float a, float b)
    {
        if (a > 0)
            return (Mathf.Atan(b / a)) * Mathf.Rad2Deg;
        else if (a < 0 && b >= 0)
            return (Mathf.Atan(b / a) + Mathf.PI) * Mathf.Rad2Deg;
        else if (a < 0 && b < 0)
            return (Mathf.Atan(b / a) - Mathf.PI) * Mathf.Rad2Deg;
        else if (a == 0 && b > 0)
            return HALF_PI * Mathf.Rad2Deg;
        else if (a == 0 && b < 0)
            return -HALF_PI * Mathf.Rad2Deg;

        return 0;
    }

    private Vector3 GetPosition(float i)
    {
        Vector2 pos = new Vector2(1, 0);

        if (i > 0 && i <= 0.5)
        {
            float a = i * Mathf.PI;

            pos.x = Mathf.Cos(HALF_PI - a);
            pos.y = Mathf.Cos(a);
        }
        else if (i > 0.5 && i <= 1f)
        {
            float a = i * Mathf.PI - HALF_PI;

            pos.x = Mathf.Cos(a);
            pos.y = -(Mathf.Cos(HALF_PI - a));
        }
        else if (i < 0 && i >= -0.5)
        {
            float a = Mathf.Abs(i * Mathf.PI);

            pos.x = -(Mathf.Cos(HALF_PI - a));
            pos.y = Mathf.Cos(a);
        }
        else if (i < -0.5 && i >= -1)
        {
            float a = i * Mathf.PI - HALF_PI;

            pos.x = Mathf.Cos(a);
            pos.y = -(Mathf.Cos(HALF_PI - a));
        }

        return new Vector3(pos.x, pos.y, 0);
    }

    private float ConversionAngleToUnity(float angle)
    {
        if (angle <= 0 && angle >= -180)
        {
            return angle * -1f;
        }
        else if (angle >= 0 && angle <= 180)
        {
            return 360 - angle;
        }

        return 0;
    }

    public void OnMove(Vector2 delta)
    {
        if (!_isDrag) return;
        
        if(_parameterDelta.x != 0 || _parameterDelta.y != 0)
        {
            float angle = ConversionAngleToUnity(GetAngle(_parameterDelta.x, _parameterDelta.y));

            if ((angle <= 360 && angle > 320) || (angle >= 0 && angle <= 40))
                Debug.Log("RIGHT");
            else if (angle > 40 && angle <= 140)
                Debug.Log("DOWN");
            else if (angle > 140 && angle <= 220)
                Debug.Log("LEFT");
            else if (angle > 220 && angle <= 320)
                Debug.Log("UP");
            else
                Debug.LogError("FUCK YOU");
        }
    }
}

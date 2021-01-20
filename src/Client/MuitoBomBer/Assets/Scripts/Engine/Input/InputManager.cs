using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    /// <summary>
    ///  Modes of input.
    /// </summary>
    public enum InputForcedMode
    {
        None,
        Mobile,
        Desktop
    }

    /// <summary>
    /// Kind of control used for movement.
    /// </summary>
    public enum MovementControls
    {
        Joystick,
        Arrows
    }

    /// <summary>
    /// Prevent input to be detected.
    /// </summary>
    public bool _inputDetectionActive = true;

    /// <summary>
    /// Auto dectect if you is playing in some mobile platform.
    /// </summary>
    public bool _autoMobileDetection = true;

    /// <summary>
    /// Mobile controls will be hidden in editor mode, regardless of the current build target or the forced mode.
    /// </summary>
    public bool _hideMobileControlsInEditor = false;

    /// <summary>
    /// Use this to force desktop (keyboard, pad) or mobile (touch) mode.
    /// </summary>
    public InputForcedMode _forcedMode;

    /// <summary>
    /// Use this to specify whether you want to use the default joystick or arrows to move your character.
    /// </summary>
    public MovementControls _movementControl = MovementControls.Joystick;

    /// <summary>
    /// Currently in mobile mode.
    /// </summary>
    public bool IsMobile { get; protected set; }

    /// <summary>
    /// Movement value (used to move the character around).
    /// </summary>
    protected Vector2 _movement = Vector2.zero;
    public Vector2 Movement { get { return _movement; } }

    /// <summary>
    /// Acceleration / deceleration will take place when moving / stopping
    /// Turn SmoothMovement on to have inertia in your controls (meaning there'll be a small delay between a press/release of a direction and your character moving/stopping). 
    /// You can also define here the horizontal and vertical thresholds.
    /// </summary>
    public bool SmoothMovement = true;

    /// <summary>
    /// Object of joystick.
    /// </summary>
    public GameObject _joystick;

    /// <summary>
    /// Object of arrows (up, down, left and right).
    /// </summary>
    public GameObject _arrows;

    protected static readonly string _axisHorizontal = "Horizontal";
    protected static readonly string _axisVertical = "Vertical";

    protected virtual void Start()
    {
        ControlsModeDetection();
    }

    private void ControlsModeDetection()
    {
        SetMobileControlsActive(false);

        IsMobile = false;

#if UNITY_ANDROID || UNITY_IPHONE
        //if(AutoMobileDetection)
        //{
            SetMobileControlsActive(true);
            IsMobile = true;
        //}
#endif

        if (_forcedMode == InputForcedMode.Mobile)
        {
            SetMobileControlsActive(true);
            IsMobile = true;
        }

        if (_forcedMode == InputForcedMode.Desktop)
        {
            SetMobileControlsActive(false);
            IsMobile = false;
        }

#if UNITY_EDITOR
        if (_hideMobileControlsInEditor)
        {
            SetMobileControlsActive(false);
            IsMobile = false;
        }
#endif
    }

    private void SetMobileControlsActive(bool active)
    {
        _joystick.SetActive((active && _movementControl == MovementControls.Joystick) ? true : false);
        _arrows.SetActive((active && _movementControl == MovementControls.Arrows) ? true : false);
    }

    /// <summary>
    /// Process button states.
    /// </summary>
    protected virtual void LateUpdate()
    {
    }

    /// <summary>
    /// Check the various commands and update our values and states accordingly.
    /// </summary>
    protected virtual void Update()
    {
        if (!IsMobile && _inputDetectionActive)
        {
            SetMovement();
        }
    }

    /// <summary>
    /// Called every frame, if not on mobile, gets primary movement values from input.
    /// </summary>
    public virtual void SetMovement()
    {
        if (!IsMobile && _inputDetectionActive)
        {
            if (SmoothMovement)
            {
                _movement.x = Input.GetAxis(_axisHorizontal);
                _movement.y = Input.GetAxis(_axisVertical);
            }
            else
            {
                _movement.x = Input.GetAxisRaw(_axisHorizontal);
                _movement.y = Input.GetAxisRaw(_axisVertical);
            }
        }
    }

    /// <summary>
    /// When is using touch arrows, bind left/right arrows to this method.
    /// </summary>
    /// <param name="">.</param>
    public virtual void SetHorizontalMovement(float horizontalInput)
    {
        if (IsMobile && _inputDetectionActive)
        {
            _movement.x = horizontalInput;
        }
    }

    /// <summary>
    /// When is using touch arrows, bind secondary down/up arrows to this method.
    /// </summary>
    /// <param name="">.</param>
    public virtual void SetVerticalMovement(float verticalInput)
    {
        if (IsMobile && _inputDetectionActive)
        {
            _movement.y = verticalInput;
        }
    }
}

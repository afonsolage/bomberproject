using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageHintComponent : MonoBehaviour
{
    public UILabel _label;

    void Awake()
    {
        TweenAlpha alpha = gameObject.AddComponent<TweenAlpha>() as TweenAlpha;
        alpha.from = 0f;
        alpha.to = 1f;
        alpha.duration = 1f;
        alpha.style = UITweener.Style.Once;
        alpha.ignoreTimeScale = false;
        alpha.useFixedUpdate = true;

        TweenPosition position = gameObject.AddComponent<TweenPosition>() as TweenPosition;
        position.from = new Vector3(0, -100, 0);
        position.to = new Vector3(0, 50, 0);
        position.duration = 1f;
        position.style = UITweener.Style.Once;
        position.ignoreTimeScale = false;
        position.useFixedUpdate = true;

        position.AddOnFinished(OnTweenComplete);
    }

    public void SetText(string text)
    {
        if (_label)
            _label.text = text;
    }

    private void OnTweenComplete()
    {
        TweenAlpha alpha = gameObject.GetComponent<TweenAlpha>() as TweenAlpha;
        alpha.from = 1f;
        alpha.to = 0f;
        alpha.delay = 0.8f;
        alpha.ResetToBeginning();

        TweenPosition position = gameObject.GetComponent<TweenPosition>() as TweenPosition;
        position.from = new Vector3(0, 100, 0);
        position.to = new Vector3(0, 50, 0);
        position.delay = 1.0f;
        position.PlayReverse();

        position.AddOnFinished(RemoveTweenComplete);
    }

    private void RemoveTweenComplete()
    {
        GameObject.Destroy(gameObject);
    }
}

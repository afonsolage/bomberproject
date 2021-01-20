using CommonLib.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListComponent : MonoBehaviour
{
    public UI2DSprite _background;
    public UILabel _name;
    public UILabel _level;

    public UI2DSprite _maleSprite;
    public UI2DSprite _femaleSprite;

    public void SetBackgroundAlpha(bool odd)
    {
        var color = _background.color;
        color.a = ((odd) ? 50f : 120f) / 255f;

        _background.color = color;
    }

    public void SetInformations(string name, uint level, PlayerGender gender)
    {
        _name.text = name;
        _level.text = string.Format("{0}", level);

        if(gender == PlayerGender.Female)
        {
            _maleSprite.gameObject.SetActive(false);
            _femaleSprite.gameObject.SetActive(true);
        }
        else
        {
            _maleSprite.gameObject.SetActive(true);
            _femaleSprite.gameObject.SetActive(false);
        }
    }
}

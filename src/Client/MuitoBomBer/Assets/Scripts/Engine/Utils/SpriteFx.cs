using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpriteFx
{
    internal static IEnumerator FlashSprites(GameObject go, int numTimes, float delay, bool disable = false, bool destroy = true)
    {
        var sprite = go.GetComponentInChildren<SpriteRenderer>();

        if (sprite == null)
            yield break;

        var normalScale = go.transform.localScale;
        var bigScale = normalScale * 1.7f;

        // number of times to loop
        for (int loop = 0; loop < numTimes; loop++)
        {
            if (disable)
            {
                // for disabling
                sprite.enabled = false;
            }
            else
            {
                // for changing the alpha
                sprite.color = Color.red;
                go.transform.DOScale(bigScale, 1.0f);
            }

            // delay specified amount
            yield return new WaitForSeconds(delay);

            if (disable)
            {
                // for disabling
                sprite.enabled = true;
            }
            else
            {
                // for changing the alpha
                sprite.color = Color.white;
                go.transform.DOScale(normalScale, 1.0f);
            }

            // delay specified amount
            yield return new WaitForSeconds(delay);
        }

        if (destroy)
            Object.Destroy(go);
    }
}

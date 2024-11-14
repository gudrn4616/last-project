using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Diagnostics.Tracing;
using static UnityEngine.UI.Button;

[Serializable]
public class AnimationSpriteGroup
{
    public string key;
    public List<Sprite> sprites;
    public bool isLoop = true;
    public ButtonClickedEvent endCallback;
}

public class SpriteAnimation :
#if USE_AUTO_CACHING
    MonoAutoCaching
#else
    MonoBehaviour
#endif
{
    [SerializeField] private List<AnimationSpriteGroup> animations;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveToFrame = 20;
    private float nowFrame;
    private int nowIndex;
    private AnimationSpriteGroup nowAnimation;

    private async void Start()
    {
        bool isEmptySlot = animations.Find(obj => obj.sprites.Count == 0) != null;
        if (isEmptySlot)
        {
            var sprites = await ResourceManager.instance.LoadAssets<Sprite>(name.Replace("(Clone)", ""), eAddressableType.Images);
            foreach (var ani in animations)
            {
                ani.sprites = sprites.FindAll(obj => obj.name.Contains(ani.key));
            }
        }
        ChangeAnimation(animations[0].key);
    }

    private void Update()
    {
        if (nowAnimation == null) return;
        if (nowAnimation.sprites.Count == 0) return;
        nowFrame++;
        if(nowFrame > moveToFrame)
        {
            nowFrame = 0;
            nowIndex = Util.Next(nowIndex, 0, nowAnimation.sprites.Count, false);
            if (nowIndex == 0 && !nowAnimation.isLoop)
            {
                nowAnimation.endCallback.Invoke();
                nowAnimation = null;
            }
            else
            {
                spriteRenderer.sprite = nowAnimation.sprites[nowIndex];
            }
        }
    }

    public void ChangeAnimation(string key)
    {
        nowAnimation = animations.Find(obj => obj.key == key);
        if (nowAnimation != null)
        {
            nowFrame = 0;
            nowIndex = 0;
            spriteRenderer.sprite = nowAnimation.sprites[nowIndex];
            if(nowAnimation.sprites.Count > 10)
            {
                //moveToFrame = 20f - 20f / ((float)nowAnimation.sprites.Count - 10f);
                moveToFrame = 20f / ((float)nowAnimation.sprites.Count - 10f) + 2;
            }
        }
    }

    public bool IsAnim(string key)
    {
        return nowAnimation.key == key;
    }
    
    public void SetFlip(bool isFlip)
    {
        spriteRenderer.flipX = isFlip;
    }
}
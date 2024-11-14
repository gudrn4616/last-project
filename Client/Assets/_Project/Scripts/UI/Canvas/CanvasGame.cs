using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;

public class CanvasGame : CanvasBase<CanvasGame>
{
    protected override void Awake()
    {
        base.Awake();
        UIManager.Show<UIGame>();
    }
}
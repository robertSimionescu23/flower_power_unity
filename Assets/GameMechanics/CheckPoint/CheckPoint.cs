using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite debugSprite;
     [ContextMenu("Debug off")]
    public void TurnOffDebugMode()
    {
        print("Debug mode off");
        spriteRenderer.sprite = null;
    }
    [ContextMenu("Debug on")]
    public void TurnOnDebugMode()
    {
        print("Debug mode on");
        spriteRenderer.sprite = debugSprite;
    }





}

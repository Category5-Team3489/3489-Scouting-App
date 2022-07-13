using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingTexture : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed;
    [SerializeField] private Image image;

    private Vector2 offset = Vector2.zero;

    private void Update()
    {
        image.material.SetTextureOffset("_MainTex", offset);
        offset += scrollSpeed * Time.deltaTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(RectTransform))]
public class EnableSlideInAnim : MonoBehaviour
{
    [System.Serializable]
    public enum Direction
    {
        Top,
        Bottom,
        Left,
        Right
    }
    [SerializeField] Ease easeType = Ease.OutBack;
    [SerializeField] Direction slideFrom = Direction.Right;
    [SerializeField] float distance=200;
    float time = 0.3f;
    RectTransform rect;
    Vector2 startPos;
    Tween movementTween = null;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }
    private void OnEnable()
    {
        InitPos();
        SlideIn();
    }
    void InitPos()
    {
        switch (slideFrom)
        {
            case Direction.Top:
                {
                    rect.position += new Vector3(0, distance, 0);
                    break;
                }
            case Direction.Bottom:
                {
                    rect.position += new Vector3(0, -distance, 0);
                    break;
                }
            case Direction.Left:
                {
                    rect.position += new Vector3(-distance, 0, 0);
                    break;
                }
            case Direction.Right:
                {
                    rect.position += new Vector3(distance, 0, 0);
                    break;
                }
        }
    }
    void SlideIn()
    {
        movementTween?.Kill();
        movementTween= rect.DOAnchorPos(startPos,time).SetEase(easeType).SetUpdate(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(RectTransform))]
public class SlideInAnim : MonoBehaviour
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
    Vector3 startPos;
    Tween movementTween = null;
    public void PlayAnimationWithDelay(float delay)
    {
        InitPos();
        SlideIn(delay);
    }
    public void Init()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    #region Animation

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
    void SlideIn(float delay)
    {
        movementTween?.Kill();
        movementTween = rect.DOAnchorPos(startPos, time).SetEase(easeType).SetDelay(delay);
    }

    #endregion
}

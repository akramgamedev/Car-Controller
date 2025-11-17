using UnityEngine;
using DG.Tweening;

public class MarkerAnimationHelper : MonoBehaviour
{
    public static void AnimateMarkerDisappearance(Transform marker, System.Action onComplete = null)
    {
        if (marker == null) return;

        Vector3 startPos = marker.position;
        Vector3 targetPos = startPos + Vector3.down * 1.5f;

        Sequence markerDisappear = DOTween.Sequence();

        markerDisappear.Append(marker.DOMoveY(targetPos.y, 0.2f).SetEase(Ease.InCubic));
        markerDisappear.Join(marker.DOScale(0.1f, 0.2f).SetEase(Ease.InCubic));

        Renderer rend = marker.GetComponent<Renderer>();
        CanvasGroup cg = marker.GetComponent<CanvasGroup>();

        if (rend != null)
        {
            markerDisappear.Join(rend.material.DOFade(0f, 0.18f));
        }
        else if (cg != null)
        {
            markerDisappear.Join(cg.DOFade(0f, 0.18f));
        }

        markerDisappear.OnComplete(() =>
        {
            marker.gameObject.SetActive(false);
            marker.position = startPos;
            marker.localScale = Vector3.one;

            if (rend != null) rend.material.DOFade(1f, 0f);
            else if (cg != null) cg.alpha = 1f;

            onComplete?.Invoke();
        });
    }

}

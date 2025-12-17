using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
[RequireComponent(typeof(RectTransform))]
public class EnablePanelPopupChildGrow : MonoBehaviour
{
    [SerializeField] Ease easeTypeGrow = Ease.OutBack;
    [SerializeField] Ease easeTypeShrink = Ease.InBack;
    [SerializeField] Vector3 startScale = Vector3.zero;
    [SerializeField] Vector3 endScale = Vector3.one;
    [SerializeField] Transform[] childsToGrow;
    public UnityEvent callBack;
    float time = 0.3f;
    Tween panelTween = null;
    List<Tween> childTweens = new List<Tween>();

    private void OnEnable()
    {
        Popup();
    }
    void Popup()
    {
        panelTween?.Kill();
        KillAllChildTween();
        childTweens = new List<Tween>();
        transform.localScale = startScale;
        for (int i = 0; i < childsToGrow.Length; i++)
        {
            childsToGrow[i].localScale = Vector3.zero;
        }
        panelTween = transform.DOScale(endScale, time).SetEase(easeTypeGrow).SetUpdate(UpdateType.Normal, true).OnComplete(()=>
        {
            float delay = 0;
            for (int i = 0; i < childsToGrow.Length; i++)
            {
                GrowChild(childsToGrow[i],delay);
                delay += time * 0.4f;
            }   
        } );
    }
    void GrowChild(Transform child,float delay)
    {
        Tween tween = child.DOScale(1, time * 0.8f).SetUpdate(UpdateType.Normal, true).SetEase(easeTypeGrow).SetDelay(delay);
        childTweens.Add(tween);
    }
    void ShrinkChild(Transform child, float delay)
    {
        Tween tween = child.DOScale(0, time * 0.8f).SetUpdate(UpdateType.Normal, true).SetEase(easeTypeShrink).SetDelay(delay);
        childTweens.Add(tween);
    }
    void KillAllChildTween()
    {
        for (int i = 0; i < childTweens.Count; i++)
        {
            childTweens[i]?.Kill();
        }
    }
    public void ShowPanel()
    {
        Popup();
    }
    public void HidePanel()
    {
        panelTween?.Kill();
        KillAllChildTween();
        childTweens = new List<Tween>();
        float delay = 0;
        for (int i = childsToGrow.Length-1; i >=0 ; i--)
        {
            ShrinkChild(childsToGrow[i],delay);
            delay += time * 0.4f;
        }
        delay += time * 0.6f;
        panelTween = transform.DOScale(startScale, time).SetUpdate(UpdateType.Normal, true).SetEase(easeTypeShrink).SetDelay(delay).OnComplete(() => callBack.Invoke());
    }
}

using Unity.VisualScripting;
using UnityEngine;

public class PageManager : MonoBehaviour
{
    [SerializeField] VibrationController vibrationController;
    public GameObject[] pages;
    private int currentPageIndex = 0;

    void Start()
    {
        ShowPage(0);
    }

    public void ShowPage(int pageIndex)
    {
        vibrationController?.ButtonVibration();
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        if (pageIndex >= 0 && pageIndex < pages.Length)
        {
            pages[pageIndex].SetActive(true);
            currentPageIndex = pageIndex;
        }
    }

    public void NextPage()
    {
        vibrationController?.ButtonVibration();
        if (currentPageIndex < pages.Length - 1)
        {
            ShowPage(currentPageIndex + 1);
        }
    }

    public void PreviousPage()
    {
        vibrationController?.ButtonVibration();
        if (currentPageIndex > 0)
        {
            ShowPage(currentPageIndex - 1);
        }
    }

}

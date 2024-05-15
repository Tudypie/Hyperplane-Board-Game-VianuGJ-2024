using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tutorialPages;
    [SerializeField] private int currentPage = 0;

    public void ChangePage(int value)
    {
        tutorialPages[currentPage].SetActive(false);
        currentPage = Mathf.Clamp(currentPage + value, 0, tutorialPages.Length);
        if (currentPage >= tutorialPages.Length) 
        {
            tutorialPages[0].transform.parent.gameObject.SetActive(false);
            if (!GameManager.instance.gameStarted)
                GameManager.instance.OnStartGame();
            else
                UIManager.instance.ActivateGamePanel(true);

            return;
        }
        Invoke(nameof(PageSetActive), 0.01f);
    }

    private void PageSetActive() => tutorialPages[currentPage].SetActive(true);

    public void OpenTutorialPanel()
    {
        currentPage = 0;
        tutorialPages[currentPage].transform.parent.gameObject.SetActive(true);
        tutorialPages[currentPage].SetActive(true);
        UIManager.instance.ActivateGamePanel(false);    
    }
}

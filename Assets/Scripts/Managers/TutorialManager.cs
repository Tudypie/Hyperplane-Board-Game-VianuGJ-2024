using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tutorialPages;
    [SerializeField] private int currentPage = 0;

    public void NextPage()
    {
        tutorialPages[currentPage].SetActive(false);
        currentPage++;
        if (currentPage >= tutorialPages.Length) 
        {
            tutorialPages[0].transform.parent.gameObject.SetActive(false);
            GameManager.instance.OnStartGame();
            return;
        }
        Invoke(nameof(PageSetActive), 0.01f);
    }

    private void PageSetActive() => tutorialPages[currentPage].SetActive(true);
}

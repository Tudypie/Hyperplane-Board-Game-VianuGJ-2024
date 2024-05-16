using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tutorialPages;
    [SerializeField] private GameObject[] beginButton;
    [SerializeField] private GameObject[] nextArrow;
    [SerializeField] private GameObject cardDeck;
    [SerializeField] private int currentPage = 0;
    public bool tutorialIsOpen = false;
    public bool hasFinished = false;

    private Animator anim;

    public static TutorialManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }

    public void ChangePage(int value)
    {
        tutorialPages[currentPage].SetActive(false);
        currentPage = Mathf.Clamp(currentPage + value, 0, tutorialPages.Length);
        if (currentPage >= tutorialPages.Length) 
        {
            cardDeck.SetActive(true);
            UIManager.instance.ActivateKnowledgeText(true);
            hasFinished = true;
            CloseTutorialPanel();
            return;
        }
        Invoke(nameof(PageSetActive), 0.01f);
        AudioManager.instance.PlaySound(AudioManager.instance.pieceMove);
    }

    private void PageSetActive() => tutorialPages[currentPage].SetActive(true);

    public void OpenTutorialPanel(bool resetCurrentPage)
    {
        if(resetCurrentPage)
        {
            currentPage = 0;
        }
        else
        {
            tutorialPages[currentPage].SetActive(false);
            currentPage++;
        }

        for(int i = 0; i < currentPage; i++)
        {
            if (beginButton[i] != null)
                beginButton[i].SetActive(false);
            if (nextArrow[i] != null)
                nextArrow[i].SetActive(true);
        }

        //if (currentPage >= 6) cardDeck.SetActive(true);

        tutorialIsOpen = true;
        tutorialPages[currentPage].transform.parent.gameObject.SetActive(true);
        tutorialPages[currentPage].SetActive(true);
        UIManager.instance.ActivateGamePanel(false);
        anim.Play("TutorialShow");
    }

    public void CloseTutorialPanel()
    {
        tutorialIsOpen = false;
        //tutorialPages[0].transform.parent.gameObject.SetActive(false);
        anim.Play("TutorialHide");
        if (!GameManager.instance.gameStarted)
            GameManager.instance.OnStartGame();
        else
            UIManager.instance.ActivateGamePanel(true);

        if (currentPage >= tutorialPages.Length-1)
        {
            cardDeck.SetActive(true);
            UIManager.instance.ActivateKnowledgeText(true);
            hasFinished = true;
        }
    }
}

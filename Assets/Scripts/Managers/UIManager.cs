using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text turnText;
    public Text movesLeftText;
    public Image playerVolumeFill;
    public Image opponentVolumeFill;

    public static UIManager instance {  get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void SetTurnText(bool isPlayerTurn) => turnText.text = isPlayerTurn ? "player turn" : "enemy turn";

    public void SetMovesLeftText(int amount)
    {
        movesLeftText.text = amount + " move" + (amount > 1 ? "s" : "") + " left";
    }

    public void UpdatePlayerVolumeFill(float amount, float max) => playerVolumeFill.fillAmount = amount / max;

    public void UpdateOpponentVolumeFill(float amount, float max) => opponentVolumeFill.fillAmount = amount / max;


}

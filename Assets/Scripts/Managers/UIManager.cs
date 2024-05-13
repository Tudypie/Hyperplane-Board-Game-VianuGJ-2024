using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Serializable]
    public struct ActionText
    {
        public string name;
        public string description;
    }

    public ActionText[] actionsText;
    public Dictionary<string, string> actionsMap = new Dictionary<string, string>();
    public Text currentActionText;
    public Text turnText;
    public Text movesLeftText;
    public Image playerVolumeFill;
    public Image opponentVolumeFill;

    public static UIManager instance {  get; private set; }

    private void Awake()
    {
        instance = this;

        foreach(var action in actionsText)
            actionsMap.Add(action.name, action.description);
    }

    public void SetTurnText(bool isPlayerTurn) => turnText.text = isPlayerTurn ? "randul tau" : "randul inamicului";

    public void SetMovesLeftText(int amount)
    {
        movesLeftText.text = amount + (amount > 1 ? " mutari ramase" : " mutare ramasa");
    }

    public void SetPlaceText(bool isPrismSelected)
    {
        currentActionText.text = actionsMap["place"] + (isPrismSelected ? "\n" + actionsMap["rotate"] : "");
        turnText.gameObject.SetActive(false);
        movesLeftText.gameObject.SetActive(false);
    }

    public void SetAttackText()
    {
        currentActionText.text = actionsMap["attack"] + "\n" + actionsMap["rotate"];
        turnText.gameObject.SetActive(false);
        movesLeftText.gameObject.SetActive(false);
    }

    public void SetCardText()
    {
        currentActionText.text = actionsMap["card"];
        turnText.gameObject.SetActive(false);
        movesLeftText.gameObject.SetActive(false);
    }

    public void ClearActionText()
    {
        currentActionText.text = "";
        turnText.gameObject.SetActive(true);
        movesLeftText.gameObject.SetActive(true);
    }

    public void UpdatePlayerVolumeFill(float amount, float max) => playerVolumeFill.fillAmount = amount / max;

    public void UpdateOpponentVolumeFill(float amount, float max) => opponentVolumeFill.fillAmount = amount / max;


}

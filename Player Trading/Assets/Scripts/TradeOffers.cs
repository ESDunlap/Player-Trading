using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class TradeOfferInfo
{
    public List<string> playerIds;
    public List<string> playerDisplayNames;
    public List<string> tradeIds;
}

[System.Serializable]
public class TradeOffers : MonoBehaviour
{
    public Button[] tradeOfferButtons;
    public List<TradeInfo> tradeOffers;
    public TradeOfferInfo tradeOfferInfo;
    private int numTradeOffers;

    public static TradeOffers instance;
    void Awake() { instance = this; }
    public void UpdateTradeOffers()
    {
        DisableAllTradeOfferButtons();
        ExecuteCloudScriptRequest getTradeOffersRequest = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTradeIDs"
        };
        PlayFabClientAPI.ExecuteCloudScript(getTradeOffersRequest,
        result =>
        {
            string rawData = result.FunctionResult.ToString();
            tradeOfferInfo = JsonUtility.FromJson<TradeOfferInfo>(rawData);
            GetTradeInfo();
        },
        error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        );
    }

    void DisableAllTradeOfferButtons()
    {
        foreach (Button button in tradeOfferButtons)
            button.gameObject.SetActive(false);
    }

    void GetTradeInfo()
    {
        numTradeOffers = tradeOfferInfo.playerIds.Count;
        tradeOffers = new List<TradeInfo>();
        if (numTradeOffers == 0)
            UpdateTradeOffersUI();
        for (int x = 0; x < tradeOfferInfo.playerIds.Count; ++x)
        {
            GetTradeStatusRequest tradeStatusRequest = new GetTradeStatusRequest
            {
                OfferingPlayerId = tradeOfferInfo.playerIds[x],
                TradeId = tradeOfferInfo.tradeIds[x]
            };
            PlayFabClientAPI.GetTradeStatus(tradeStatusRequest,
            result =>
            {
                tradeOffers.Add(result.Trade);
                if (tradeOffers.Count == numTradeOffers)
                    UpdateTradeOffersUI();
            },
            error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
            );
        }
    }

    void UpdateTradeOffersUI()
    {
        for (int x = 0; x < tradeOfferButtons.Length; ++x)
        {
            tradeOfferButtons[x].gameObject.SetActive(x < tradeOffers.Count);
            if (tradeOfferButtons[x].gameObject.activeInHierarchy)
                continue;
            tradeOfferButtons[x].onClick.RemoveAllListeners();
            int tradeIndex = x;
            tradeOfferButtons[x].onClick.AddListener(() => OnTradeOfferButton(tradeIndex));
            tradeOfferButtons[x].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tradeOfferInfo.playerDisplayNames[x];
        }
    }

    public void OnTradeOfferButton(int tradeIndex)
    {
        ViewTradeWindow.instance.SetTradeWindow(tradeIndex);
    }
}

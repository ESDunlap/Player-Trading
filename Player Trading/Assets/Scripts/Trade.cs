using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;

public class Trade : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public GameObject tradeCanvas;
    public TextMeshProUGUI inventoryText;

    [HideInInspector]
    public List<ItemInstance> inventory;
    [HideInInspector]
    public List<CatalogItem> catalog;

    public static Trade instance;
    void Awake() { instance = this; }

    public UnityEvent onRefreshUI;

    public void OnLoggedIn()
    {
        tradeCanvas.SetActive(true);
        if (onRefreshUI != null)
            onRefreshUI.Invoke();
    }

    public void GetInventory()
    {
        inventoryText.text = "";

        GetPlayerCombinedInfoRequest getInvRequest = new GetPlayerCombinedInfoRequest
        {
            PlayFabId = LoginRegister.instance.playFabId,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserInventory = true
            }
        };
        PlayFabClientAPI.GetPlayerCombinedInfo(getInvRequest,
            result =>
            {
                inventory = result.InfoResultPayload.UserInventory;

                foreach (ItemInstance item in inventory)
                    inventoryText.text += item.DisplayName + ", ";
            },
            error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        );
    }

    public void GetCatalog()
    {
        GetCatalogItemsRequest getCatalogRequest = new GetCatalogItemsRequest
        {
            CatalogVersion = "PlayerItems"
        };

        PlayFabClientAPI.GetCatalogItems(getCatalogRequest,
            result => catalog = result.Catalog,
            error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        );
    }

    public void SetDisplayText(string text, bool isError)
    {
        displayText.text = text;
        if (isError)
            displayText.color = Color.red;
        else
            displayText.color = Color.green;
        Invoke("HideDisplayText", 2.0f);
    }
    void HideDisplayText()
    {
        displayText.text = "";
    }
}

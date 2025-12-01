using NUnit;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreateTrade : MonoBehaviour
{
    public TradeItem[] offeringItems;
    public TradeItem[] requestingItems;

    public static CreateTrade instance;
    void Awake() { instance = this; }
    public void OnCreateTradeButton()
    {
        List<ItemInstance> tempInventory = Trade.instance.inventory;
        List<string> itemsToOffer = new List<string>();
        foreach (TradeItem item in offeringItems)
        {
            for (int x = 0; x < item.value; ++x)
            {
                ItemInstance i = tempInventory.Find(y => y.DisplayName == item.itemName);
                if (i == null)
                {
                    Trade.instance.SetDisplayText("You don't have the offered items in your inventory.", false);
                    return;
                }
                else
                {
                    itemsToOffer.Add(i.ItemInstanceId);
                    tempInventory.Remove(i);
                }
            }
        }
        if(itemsToOffer.Count == 0)
        {
            Trade.instance.SetDisplayText("You can't trade nothing.", false);
            return;
        }

        List<string> itemsToRequest = new List<string>();
        foreach(TradeItem item in requestingItems)
        {
            string itemId = Trade.instance.catalog.Find(y => y.DisplayName == item.itemName).ItemId;
            for (int x = 0; x < item.value; ++x)
                itemsToRequest.Add(itemId);
        }
        OpenTradeRequest tradeRequest = new OpenTradeRequest
        {
            OfferedInventoryInstanceIds = itemsToOffer,
            RequestedCatalogItemIds = itemsToRequest,
        };
        PlayFabClientAPI.OpenTrade(tradeRequest,
            result => AddTradeToGroup(result.Trade.TradeId),
            error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        );
    }
    void AddTradeToGroup(string tradeId)
    {
        ExecuteCloudScriptRequest executeRequest = new ExecuteCloudScriptRequest
        {
            FunctionName = "AddNewTradeOffer",
            FunctionParameter = new { tradeID = tradeId }
        };
        PlayFabClientAPI.ExecuteCloudScript(executeRequest,
        result =>
        {
            Trade.instance.SetDisplayText("Trade offer created.", false);
            if (Trade.instance.onRefreshUI != null)
                Trade.instance.onRefreshUI.Invoke();
        },
        error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        );
    }

    public void ResetItemValues()
    {
        foreach (TradeItem item in offeringItems)
            item.ResetValue();
        foreach (TradeItem item in requestingItems)
            item.ResetValue();
    }
}

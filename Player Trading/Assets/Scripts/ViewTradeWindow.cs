using NUnit;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ViewTradeWindow : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI offeredItemsText;
    public TextMeshProUGUI requestedItemsText;
    private TradeInfo curTradeOffer;

    public static ViewTradeWindow instance;
    void Awake() { instance = this; }

    public void SetTradeWindow(int tradeOfferIndex)
    {
        curTradeOffer = TradeOffers.instance.tradeOffers[tradeOfferIndex];
        Dictionary<string, int> offeredItemsCount = new Dictionary<string, int>();
        Dictionary<string, int> requestingItemsCount = new Dictionary<string, int>();
        headerText.text = TradeOffers.instance.tradeOfferInfo.playerDisplayNames[tradeOfferIndex] + " wants to trade...";
        foreach (string itemId in curTradeOffer.OfferedCatalogItemIds)
        {
            if (!offeredItemsCount.ContainsKey(itemId))
                offeredItemsCount.Add(itemId, 0);
            offeredItemsCount[itemId]++;
        }
        offeredItemsText.text = "";
        foreach (KeyValuePair<string, int> item in offeredItemsCount)
        {
            string itemName = Trade.instance.catalog.Find(y => y.ItemId == item.Key).DisplayName;
            offeredItemsText.text += string.Format("x{0} {1}\n", item.Value, itemName);
        }

        foreach (string itemId in curTradeOffer.RequestedCatalogItemIds)
        {
            if (!requestingItemsCount.ContainsKey(itemId))
                requestingItemsCount.Add(itemId, 0);
            requestingItemsCount[itemId]++;
        }

        requestedItemsText.text = "";

        foreach (KeyValuePair<string, int> item in requestingItemsCount)
        {
            string itemName = Trade.instance.catalog.Find(y => y.ItemId == item.Key).DisplayName;
            requestedItemsText.text += string.Format("x{0} {1}\n", item.Value, itemName);
        }
    }

    public void OnAcceptTradeButton()
    {
        List<string> inventoryItemsToSend = new List<string>();
        List<ItemInstance> tempInventory = Trade.instance.inventory;
        for (int x = 0; x < curTradeOffer.RequestedCatalogItemIds.Count; ++x)
        {
            ItemInstance item = tempInventory.Find(y => y.ItemId == curTradeOffer.RequestedCatalogItemIds[x]);
            if (item == null)
            {
                Trade.instance.SetDisplayText("You don't have the requested items in your inventory.", false);
                return;
            }
            inventoryItemsToSend.Add(item.ItemInstanceId);
            tempInventory.Remove(item);
        }
        AcceptTradeRequest acceptTradeRequest = new AcceptTradeRequest
        {
            TradeId = curTradeOffer.TradeId,
            OfferingPlayerId = curTradeOffer.OfferingPlayerId,
            AcceptedInventoryInstanceIds = inventoryItemsToSend
        };
        PlayFabClientAPI.AcceptTrade(acceptTradeRequest,
        result => RemoveTradeOwnerFromGroup(result.Trade.OfferingPlayerId),
        error => Trade.instance.SetDisplayText(error.ErrorMessage, true)
        );
    }

    void RemoveTradeOwnerFromGroup(string offeringPlayerId)
    {

    }

    public void ResetUI()
    {
        headerText.text = "";
        offeredItemsText.text = "";
        requestedItemsText.text = "";
    }
}

using System;
using Plugin.InAppBilling;

namespace Eventos.Common.Interfaces;

public interface ISubscriptionService
{
    Task<bool> PurchaseSubscriptionAsync(string subscriptionId);
    Task<List<InAppBillingPurchase>> GetActiveSubscriptionsAsync();
    List<string> GetAvailableSubscriptions();
}

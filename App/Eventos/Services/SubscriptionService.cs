using Plugin.InAppBilling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventos.Common.Interfaces;
namespace Eventos.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly List<string> subscriptionIds = new()
    {
        "852576",
        "852577",
        "852578"
    };

        public async Task<bool> PurchaseSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var billing = CrossInAppBilling.Current;

                if (!await billing.ConnectAsync())
                {
                    Console.WriteLine("No se pudo conectar a la facturación.");
                    return false;
                }

                var purchase = await billing.PurchaseAsync(subscriptionId, ItemType.Subscription);

                if (purchase == null)
                {
                    Console.WriteLine("La compra fue cancelada.");
                    return false;
                }

                if (purchase.State == PurchaseState.Purchased || purchase.State == PurchaseState.Restored)
                {
                    Console.WriteLine($"Compra exitosa: {purchase.ProductId}");
                    return true;
                }
            }
            catch (InAppBillingPurchaseException ex)
            {
                Console.WriteLine($"Error en la compra: {ex.PurchaseError}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }

            return false;
        }

        public async Task<List<InAppBillingPurchase>> GetActiveSubscriptionsAsync()
        {
            var activeSubscriptions = new List<InAppBillingPurchase>();

            try
            {
                var billing = CrossInAppBilling.Current;

                if (!await billing.ConnectAsync())
                {
                    Console.WriteLine("No se pudo conectar a la facturación.");
                    return activeSubscriptions;
                }

                var purchases = await billing.GetPurchasesAsync(ItemType.Subscription);

                if (purchases != null)
                {
                    activeSubscriptions.AddRange(purchases);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener suscripciones activas: {ex.Message}");
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }

            return activeSubscriptions;
        }

        public List<string> GetAvailableSubscriptions() => subscriptionIds;
    }
}

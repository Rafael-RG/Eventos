using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eventos.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos.ViewModels
{
    public partial class SubscriptionViewModel : ObservableObject
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly ReceiptValidationService _receiptValidationService;

        public ObservableCollection<string> ActiveSubscriptions { get; } = new();
        public ObservableCollection<string> AvailableSubscriptions { get; } = new();

        public SubscriptionViewModel(SubscriptionService subscriptionService, ReceiptValidationService receiptValidationService)
        {
            _subscriptionService = subscriptionService;
            _receiptValidationService = receiptValidationService;

            // Cargar suscripciones disponibles
            foreach (var subscription in _subscriptionService.GetAvailableSubscriptions())
            {
                AvailableSubscriptions.Add(subscription);
            }
        }

        [RelayCommand]
        public async Task PurchaseSubscription(string subscriptionId)
        {
            var success = await _subscriptionService.PurchaseSubscriptionAsync(subscriptionId);
            if (success)
            {
                await App.Current.MainPage.DisplayAlert("Compra", $"Suscripción {subscriptionId} activada con éxito.", "OK");
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", $"La compra de {subscriptionId} falló o fue cancelada.", "OK");
            }
        }

        [RelayCommand]
        public async Task ValidateSubscriptions()
        {
            ActiveSubscriptions.Clear();

            var activePurchases = await _subscriptionService.GetActiveSubscriptionsAsync();
            foreach (var purchase in activePurchases)
            {
                var isValid = await _receiptValidationService.ValidateReceiptAsync(purchase.PurchaseToken);
                if (isValid)
                {
                    ActiveSubscriptions.Add(purchase.ProductId);
                }
            }

            if (!ActiveSubscriptions.Any())
            {
                await App.Current.MainPage.DisplayAlert("Validación", "No hay suscripciones activas.", "OK");
            }
        }
    }
}

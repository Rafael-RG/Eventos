using Eventos.Common.Interfaces;
using Microsoft.Extensions.Localization;
using Eventos.Helpers;
using Eventos.Resources.Strings;

namespace Eventos.Services
{
    public class LocalizationService : ILocalizationService
    {
        private IStringLocalizer<Texts> localizer;

        public LocalizationService()
        {
            this.localizer = ServiceHelper.GetService<IStringLocalizer<Texts>>();
        }


        public string GetText(string text)
        {
            var localizedText = this.localizer[text];
            return localizedText;
        }
    }
}

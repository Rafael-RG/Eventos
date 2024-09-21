using System;
using CoreGraphics;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace Eventos.Platforms.iOS
{
	public class CustomShellHandler : ShellRenderer
	{
        protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
        {
            return new CustomShellTabBarAppearanceTracker();
        }

        public class CustomShellTabBarAppearanceTracker : IShellTabBarAppearanceTracker
        {
            public void Dispose()
            {
                //throw new NotImplementedException();
            }

            public void ResetAppearance(UITabBarController controller)
            {
                //throw new NotImplementedException();
            }

            public void SetAppearance(UITabBarController controller, ShellAppearance appearance)
            {
                //throw new NotImplementedException();
            }

            public void UpdateLayout(UITabBarController controller)
            {
                controller.TabBar.TintColor = UIColorExtensions.FromHex("#D2E898");
                controller.TabBar.UnselectedItemTintColor = UIColorExtensions.FromHex("#FFFFFF");

                foreach (var tabbarItem in controller.TabBar.Items)
                {
                    tabbarItem.Image = tabbarItem.Image?.Scale(new CGSize(24, 24));
                }
            }

            public static class UIColorExtensions
            {
                public static UIColor FromHex(string hex)
                {
                    var r = Convert.ToByte(hex.Substring(1, 2), 16);
                    var g = Convert.ToByte(hex.Substring(3, 2), 16);
                    var b = Convert.ToByte(hex.Substring(5, 2), 16);
                    return UIColor.FromRGB(r, g, b);
                }
            }

        }
    }
}


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
                foreach (var tabbarItem in controller.TabBar.Items)
                {
                    tabbarItem.Image = tabbarItem.Image?.Scale(new CGSize(24, 24));
                }
            }
        }
    }
}


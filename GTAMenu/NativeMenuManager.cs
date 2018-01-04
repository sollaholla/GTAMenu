using System.Collections.Generic;
using System.Linq;

namespace GTAMenu
{
    public class NativeMenuManager
    {
        private readonly List<NativeMenu> _menus = new List<NativeMenu>();

        public void AddMenu(NativeMenu menu)
        {
            if (_menus.Contains(menu)) return;
            _menus.Add(menu);
        }

        public void RemoveMenu(NativeMenu menu)
        {
            if (!_menus.Contains(menu)) return;
            _menus.Remove(menu);
        }

        public void ProcessMenus()
        {
            var menusCopy = _menus.ToArray();

            foreach (var menu in menusCopy)
                if (menu.Visible)
                    menu.Draw();
        }

        public bool IsAnyMenuOpen()
        {
            var menusCopy = _menus.ToArray();

            return menusCopy.Any(x => x.Visible);
        }

        public NativeMenu AddSubMenu(string menuTitle, string menuDescription, string itemText, string itemDescription, NativeMenu parent)
        {
            var item = new NativeMenuItemBase(itemText, itemDescription);
            var menu = new NativeMenu(menuTitle, menuDescription, parent.BannerType)
            {
                AcceleratedScrolling = parent.AcceleratedScrolling,
                DescriptionColor = parent.DescriptionColor,
                AllowClickOut = parent.AllowClickOut,
                MaxDrawableItems = parent.MaxDrawableItems,
                MenuWidth = parent.MenuWidth,
                NavigationMode = parent.NavigationMode,
                OffsetX = parent.OffsetX,
                OffsetY = parent.OffsetY,
                SoundSet = parent.SoundSet
            };

            menu.MenuBack += (nativeMenu, eventArgs) =>
            {
                parent.SupressAudioNextCall();
                parent.Visible = true;
            };

            parent.MenuItems.Add(item);
            item.Selected += (sender, args) =>
            {
                parent.SupressAudioNextCall();
                parent.Visible = false;
                menu.Visible = true;
            };
            AddMenu(menu);
            return menu;
        }
    }
}
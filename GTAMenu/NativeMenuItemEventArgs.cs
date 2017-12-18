using System;

namespace GTAMenu
{
    public class NativeMenuItemEventArgs : EventArgs
    {
        public NativeMenuItemEventArgs(NativeMenuItemBase menuItem, int menuItemIndex)
        {
            MenuItem = menuItem;
            MenuItemIndex = menuItemIndex;
        }

        public NativeMenuItemBase MenuItem { get; }

        public int MenuItemIndex { get; }
    }
}
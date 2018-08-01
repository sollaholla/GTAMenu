using System;
using GTA;

namespace GTAMenu
{
    public class NativeMenuItemButtonEventArgs : EventArgs
    {
        public NativeMenuItemButtonEventArgs(NativeMenuItemBase menuItem, Control button)
        {
            Item = menuItem;
            Button = button;
        }

        public Control Button { get; }
        public NativeMenuItemBase Item { get; }
    }
}

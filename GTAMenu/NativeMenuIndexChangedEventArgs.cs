namespace GTAMenu
{
    public class NativeMenuIndexChangedEventArgs : NativeMenuItemEventArgs
    {
        public NativeMenuIndexChangedEventArgs(NativeMenuItemBase menuItem, NativeMenuItemBase lastItem,
            int menuItemIndex) :
            base(menuItem, menuItemIndex)
        {
            LastItem = lastItem;
        }

        public NativeMenuItemBase LastItem { get; }
    }
}
namespace GTAMenu
{
    public class NativeMenuItemNavigateIndexEventArgs : NativeMenuItemEventArgs
    {
        public NativeMenuItemNavigateIndexEventArgs(NativeMenuItemBase menuItem, int menuItemIndex, int navIndex) :
            base(menuItem, menuItemIndex)
        {
            NavigationIndex = navIndex;
        }

        public int NavigationIndex { get; }
    }
}
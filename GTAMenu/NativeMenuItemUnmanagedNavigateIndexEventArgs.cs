namespace GTAMenu
{
    public class NativeMenuItemUnmanagedNavigateIndexEventArgs : NativeMenuItemEventArgs
    {
        public NativeMenuItemUnmanagedNavigateIndexEventArgs(NativeMenuItemBase menuItem, int menuItemIndex, int leftRight) : base(menuItem, menuItemIndex)
        {
            LeftRight = leftRight;
        }

        public int LeftRight { get; }
    }
}

namespace GTAMenu
{
    public class NativeMenuItemCheckboxEventArgs : NativeMenuItemEventArgs
    {
        public NativeMenuItemCheckboxEventArgs(NativeMenuItemBase menuItem, int menuItemIndex, bool check) : base(
            menuItem, menuItemIndex)
        {
            Checked = check;
        }

        public bool Checked { get; set; }
    }
}
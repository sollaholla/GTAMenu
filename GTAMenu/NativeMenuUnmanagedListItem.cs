using System.Drawing;

namespace GTAMenu
{
    public class NativeMenuUnmanagedListItem : NativeMenuItemBase
    {
        public NativeMenuUnmanagedListItem(string text, string description) : base(text, description)
        {
            Init();
        }

        public NativeMenuUnmanagedListItem(string text, string description, object value) : base(text, description, value)
        {
            Init();
        }

        public NativeMenuUnmanagedListItem(string text, string description, object value, ShopIcon shopIcon) : base(text, description, value, shopIcon)
        {
            Init();
        }

        public event NativeMenuItemUnmanagedNavigateIndexEvent ChangedIndex;

        private void Init()
        {
            IgnoreClick = true;
        }

        public override void DrawValue(PointF position, bool selected)
        {
            DrawTextValue(Value.ToString(), position, selected);
        }

        public override void DrawTextValue(string text, PointF position, bool selected)
        {
            NativeFunctions.DrawText(text, position - new SizeF(selected && Enabled ? 28 : 14, 40), NativeMenu.DescriptionTextScale, !Enabled ? Color.Gray : selected ? Color.FromArgb(255, 45, 45, 45) : Color.White, 2, GTA.Font.ChaletLondon, false, false);

            if (!selected || !Enabled)
            {
                return;
            }

            NativeFunctions.DrawSprite("commonmenu", "arrowright", position - new SizeF(18, 21), new SizeF(20, 20), 0f, Color.Black);

            NativeFunctions.DrawSprite("commonmenu", "arrowleft", position - new SizeF(18 + NativeFunctions.MeasureStringWidth(text, "jamyfafi", GTA.Font.ChaletLondon, NativeMenu.DescriptionTextScale) + 16, 21), new SizeF(20, 20), 0f, Color.Black);
        }

        public override void OnNavLeftRight(NativeMenu sender, int menuItemIndex, int leftRight)
        {
            ChangedIndex?.Invoke(sender, new NativeMenuItemUnmanagedNavigateIndexEventArgs(this, menuItemIndex, leftRight));
        }
    }
}

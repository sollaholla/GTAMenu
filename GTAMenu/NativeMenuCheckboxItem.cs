using System.Drawing;

namespace GTAMenu
{
    public class NativeMenuCheckboxItem : NativeMenuItemBase
    {
        public NativeMenuCheckboxItem(string text, string description) : base(text, description)
        {
        }

        public NativeMenuCheckboxItem(string text, string description, bool value) : base(text, description, value)
        {
            Checked = value;
        }

        public NativeMenuCheckboxItem(string text, string description, bool value, ShopIcon shopIcon) : base(text,
            description, value, shopIcon)
        {
            Checked = value;
        }

        public bool Checked { get; set; }
        public event NativeMenuItemCheckboxEvent Check;

        public override void DrawValue(PointF position, bool selected)
        {
            if (!Enabled) return;
            var icon = Checked ? ShopIcon.ShopBoxTick : ShopIcon.ShopBoxBlank;
            DrawShopIcon(position - new SizeF(42f, 40f), selected, icon, new SizeF(60, 60));
        }

        public override void OnSelected(NativeMenu sender, NativeMenuItemEventArgs e)
        {
            Checked = !Checked;
            OnCheck(sender, new NativeMenuItemCheckboxEventArgs(this, e.MenuItemIndex, Checked));
        }

        protected virtual void OnCheck(NativeMenu sender, NativeMenuItemCheckboxEventArgs e)
        {
            Check?.Invoke(sender, e);
        }
    }
}
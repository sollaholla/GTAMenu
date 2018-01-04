using System.Drawing;
using Font = GTA.Font;

namespace GTAMenu
{
    public class NativeMenuListItem : NativeMenuItemBase
    {
        public NativeMenuListItem(string text, string description) : base(text, description)
        {
            IgnoreClick = true;
        }

        public NativeMenuListItem(string text, string description, object[] value, int index) : base(text, description, value)
        {
            Init(value, index);
        }

        public NativeMenuListItem(string text, string description, object[] value, int index, ShopIcon shopIcon) : base(text, description, value, shopIcon)
        {
            Init(value, index);
        }

        public int CurrentIndex { get; private set; }

        public string CurrentValue { get; set; }

        public event NativeMenuItemNavigateIndexEvent ChangedIndex;

        private void Init(object[] value, int index)
        {
            CurrentIndex = index;

            CurrentValue = value[CurrentIndex].ToString();

            IgnoreClick = true;
        }

        public override void DrawValue(PointF position, bool selected)
        {
            var value = (dynamic[]) Value;

            if (value == null)
            {
                return;
            }

            DrawTextValue(CurrentValue, position, selected);
        }

        public override void DrawTextValue(string text, PointF position, bool selected)
        {
            NativeFunctions.DrawText(text, position - new SizeF(selected && Enabled ? 28 : 14, 40), NativeMenu.DescriptionTextScale, !Enabled ? Color.Gray : selected ? Color.FromArgb(255, 45, 45, 45) : Color.White, 2, Font.ChaletLondon, false, false);

            if (!selected || !Enabled)
            {
                return;
            }

            NativeFunctions.DrawSprite("commonmenu", "arrowright", position - new SizeF(18, 21), new SizeF(20, 20), 0f, Color.Black);

            NativeFunctions.DrawSprite("commonmenu", "arrowleft", position - new SizeF(18 + NativeFunctions.MeasureStringWidth(text, "jamyfafi", Font.ChaletLondon, NativeMenu.DescriptionTextScale) + 16, 21), new SizeF(20, 20), 0f, Color.Black);
        }

        public override void OnNavLeftRight(NativeMenu sender, int menuItemIndex, int leftRight)
        {
            var value = (object[]) Value;

            if (value == null)
            {
                return;
            }

            CurrentIndex += leftRight;

            if (CurrentIndex < 0)
            {
                CurrentIndex = value.Length - 1;
            }
            else if (CurrentIndex >= value.Length)
            {
                CurrentIndex = 0;
            }

            CurrentValue = value[CurrentIndex].ToString();

            OnChangedIndex(sender, new NativeMenuItemNavigateIndexEventArgs(this, menuItemIndex, CurrentIndex));
        }

        protected virtual void OnChangedIndex(NativeMenu sender, NativeMenuItemNavigateIndexEventArgs e)
        {
            ChangedIndex?.Invoke(sender, e);
        }
    }
}
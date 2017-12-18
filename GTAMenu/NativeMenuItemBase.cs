using System;
using System.Drawing;
using GTA.Native;
using Font = GTA.Font;

namespace GTAMenu
{
    public class NativeMenuItemBase
    {
        public NativeMenuItemBase(string text, string description)
        {
            Text = text;
            Description = description;
            Enabled = true;
        }

        public NativeMenuItemBase(string text, string description, object value) :
            this(text, description)
        {
            Value = value;
        }

        public NativeMenuItemBase(string text, string description, object value, ShopIcon shopIcon) :
            this(text, description, value)
        {
            ShopIcon = shopIcon;
        }

        public object Tag { get; set; }

        public object Value { get; protected set; }

        public string Text { get; }

        public string Description { get; }

        public bool Enabled { get; set; }

        public bool IgnoreClick { get; set; }

        public CursorSprite InteractionCursor { get; set; }

        public ShopIcon ShopIcon { get; }
        public event NativeMenuItemSelectedEvent Selected;

        public void Draw(PointF position, SizeF size, bool selected, bool hover, bool hover2, ref bool overridenCursor)
        {
            var pos = new PointF(position.X + NativeMenu.DescriptionXOffset,
                position.Y + NativeMenu.DescriptionYOffset);
            if (selected || hover)
            {
                NativeFunctions.DrawSprite("commonmenu", "gradient_nav",
                    new PointF(size.Width / 2 + position.X, size.Height / 2 + position.Y), size, 0,
                    selected ? Color.White : NativeMenu.HighlightColor());

                if (selected && hover2 && InteractionCursor != CursorSprite.None)
                {
                    Function.Call(Hash._0x8DB8CFFD58B62552, (int) InteractionCursor); // SET_CURSOR_SPRITE
                    overridenCursor = true;
                }
            }

            var offset = DrawShopIcon(pos, selected, ShopIcon, new SizeF(50, 50));
            NativeFunctions.DrawText(Text, pos + new SizeF(offset, 0f), NativeMenu.DescriptionTextScale,
                Enabled ? selected ? Color.FromArgb(255, 45, 45, 45) : Color.White : Color.Gray, 1, Font.ChaletLondon,
                false, false);
            DrawValue(position + size, selected);
        }

        public float DrawShopIcon(PointF position, bool seleceted, ShopIcon shopIcon, SizeF size)
        {
            if (shopIcon == ShopIcon.None) return 0f;

            GetShopIconFromEnum(shopIcon, out var iconNormal, out var iconHover);
            NativeFunctions.DrawSprite("commonmenu", seleceted ? iconHover : iconNormal, position + new SizeF(15, 18),
                size);
            return 34f;
        }

        public virtual void DrawValue(PointF position, bool selected)
        {
            if (Value == null) return;

            if (Value.GetType() == typeof(ShopIcon))
            {
                var icon = (ShopIcon) Value;
                DrawShopIcon(position - new SizeF(40f, 40f), selected, icon, new SizeF(50, 50));
            }
            else
            {
                var value = Value.ToString();
                DrawTextValue(value, position, selected);
            }
        }

        public virtual void DrawTextValue(string text, PointF position, bool selected)
        {
            NativeFunctions.DrawText(text, position - new SizeF(14, 40), NativeMenu.DescriptionTextScale,
                Enabled ? selected ? Color.FromArgb(255, 45, 45, 45) : Color.White : Color.Gray,
                2, Font.ChaletLondon, false, false);
        }

        private static void GetShopIconFromEnum(ShopIcon icon, out string a, out string b)
        {
            switch (icon)
            {
                case ShopIcon.None:
                    a = string.Empty;
                    b = string.Empty;
                    break;
                case ShopIcon.ShopTrevorIcon:
                    a = "shop_trevor_icon_a";
                    b = "shop_travor_icon_b";
                    break;
                case ShopIcon.ShopTickIcon:
                    a = "shop_tick_icon";
                    b = "shop_tick_icon";
                    break;
                case ShopIcon.ShopTattoosIcon:
                    a = "shop_tattoos_icon_a";
                    b = "shop_tattoos_icon_b";
                    break;
                case ShopIcon.ShopNewStar:
                    a = "shop_new_star";
                    b = "shop_new_star";
                    break;
                case ShopIcon.ShopMichaelIcon:
                    a = "shop_michael_icon_a";
                    b = "shop_michael_icon_b";
                    break;
                case ShopIcon.ShopMaskIcon:
                    a = "shop_mask_icon_a";
                    b = "shop_mask_icon_b";
                    break;
                case ShopIcon.ShopMakeupIcon:
                    a = "shop_makeup_icon_a";
                    b = "shop_makeup_icon_b";
                    break;
                case ShopIcon.ShopLock:
                    a = "shop_lock";
                    b = "shop_lock";
                    break;
                case ShopIcon.ShopHealthIcon:
                    a = "shop_health_icon_a";
                    b = "shop_health_icon_b";
                    break;
                case ShopIcon.ShopGunClubIcon:
                    a = "shop_gunclub_icon_a";
                    b = "shop_gunclub_icon_b";
                    break;
                case ShopIcon.ShopGarageIcon:
                    a = "shop_garage_icon_a";
                    b = "shop_garage_icon_b";
                    break;
                case ShopIcon.ShopGarageBikeIcon:
                    a = "shop_garage_bike_icon_a";
                    b = "shop_garage_bike_icon_b";
                    break;
                case ShopIcon.ShopFranklinIcon:
                    a = "shop_franklin_icon_a";
                    b = "shop_franklin_icon_b";
                    break;
                case ShopIcon.ShopClothingIcon:
                    a = "shop_clothing_icon_a";
                    b = "shop_clothing_icon_b";
                    break;
                case ShopIcon.ShopBarberIcon:
                    a = "shop_barber_icon_a";
                    b = "shop_barber_icon_b";
                    break;
                case ShopIcon.ShopArmorIcon:
                    a = "shop_armour_icon_a";
                    b = "shop_armour_icon_b";
                    break;
                case ShopIcon.ShopAmmoIcon:
                    a = "shop_ammo_icon_a";
                    b = "shop_ammo_icon_b";
                    break;
                case ShopIcon.ShopBoxTick:
                    a = "shop_box_tick";
                    b = "shop_box_tickb";
                    break;
                case ShopIcon.ShopBoxCross:
                    a = "shop_box_cross";
                    b = "shop_box_cross_b";
                    break;
                case ShopIcon.ShopBoxBlank:
                    a = "shop_box_blank";
                    b = "shop_box_blankb";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(icon), icon, null);
            }
        }

        public virtual void OnSelected(NativeMenu sender, NativeMenuItemEventArgs e)
        {
            Selected?.Invoke(sender, e);
        }

        public virtual void OnNavLeftRight(NativeMenu sender, int menuItemIndex, int leftRight)
        {
        }
    }
}
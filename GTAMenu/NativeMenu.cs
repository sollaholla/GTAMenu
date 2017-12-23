using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using Font = GTA.Font;

namespace GTAMenu
{
    public class NativeMenu : IDisposable
    {
        public const float DescriptionTextScale = 0.35f;
        public const float DescriptionXOffset = 8f;
        public const float DescriptionYOffset = 5f;

        private const float ItemCountXOffset = 18;
        private const float MenuItemHeight = 44.2f;
        private const float MenuItemInteractionAreaRatio = 2.3f;
        private const int MenuScrollAreaHeight = 45;
        private const Control NavUp = Control.PhoneUp;
        private const Control NavDown = Control.PhoneDown;
        private const Control NavLeft = Control.PhoneLeft;
        private const Control NavRight = Control.PhoneRight;
        private const Control NavSelect = Control.CreatorAccept;
        private const Control NavCancel = Control.PhoneCancel;

        private string _bannerDict;
        private string _bannerSprite;
        private int _consecutiveScrolls;

        private Scaleform _descriptionScaleform;
        private Scaleform _instructionalButtonsScaleform;

        private bool _disposed;

        private int _inputWait;
        private bool _isUsingScrollBar;
        private int _lastDrawableItemCount;

        private bool _mouseOnScreenEdge;
        private int _scrollIndex;

        private int _selectedIndex;

        private bool _supressAudio;

        private bool _visible;

        public NativeMenu(string title)
        {
            Title = title;
            DescriptionColor = Color.White;
            MenuWidth = 512;
            MenuItems = new List<NativeMenuItemBase>();
            MaxDrawableItems = 7;
            AcceleratedScrolling = true;
        }

        public NativeMenu(string title, MenuBannerType bannerType) : this(title)
        {
            BannerType = bannerType;
        }

        public NativeMenu(string title, string description) : this(title)
        {
            Description = description;
        }

        public NativeMenu(string title, string description, MenuBannerType bannerType) : this(title, description)
        {
            BannerType = bannerType;
        }

        public List<NativeMenuItemBase> MenuItems { get; }

        public MenuBannerType BannerType { get; }

        public FrontEndAudio SoundSet { get; set; }

        public NavigationMode NavigationMode { get; set; }

        public Color DescriptionColor { get; set; }

        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible && !value)
                    OnMenuClosed();

                if (!_visible && value)
                {
                    OnMenuOpened();
                    _inputWait = Game.GameTime + 10;
                }

                _visible = value;
            }
        }

        public bool AcceleratedScrolling { get; set; }

        public string Title { get; }

        public string Description { get; }

        public float MenuWidth { get; set; }

        public bool AllowClickOut { get; set; }

        public float OffsetX { get; set; }

        public float OffsetY { get; set; }

        public int MaxDrawableItems { get; set; }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public event NativeMenuIndexChanged IndexChanged;
        public event NativeMenuItemSelectedEvent ItemSelected;
        public event NativeMenuMenuClosedEvent MenuClosed;
        public event NativeMenuMenuOpenedEvent MenuOpened;

        public void Init()
        {
            _descriptionScaleform = new Scaleform("TEXTFIELD");
            _instructionalButtonsScaleform = new Scaleform("INSTRUCTIONAL_BUTTONS");
            _bannerDict = GetTextureDictForBannerType(BannerType, out _bannerSprite);
        }

        public void Draw()
        {
            if (!Visible)
                return;

            // Manage controls and input functionality.
            ManageControls();

            // Do hud management stuff, and streaming stuff.
            RequestTextureDictionaries();
            HideHud();

            // Check if we can scroll.
            var scrollable = MenuItems.Count > MaxDrawableItems;

            // Get current y offset from top of screen.
            var currentY = OffsetY;

            // Draw banner and description.
            if (DrawBanner(out var h))
                currentY += h;
            if (DrawDescription(currentY, scrollable, _selectedIndex + 1, MenuItems.Count))
                currentY += MenuItemHeight;

            // Draw menu items.
            HandleMenuItems(ref currentY);

            // Draw the scroll bar.
            if (scrollable)
                DrawScrollBar(ref currentY);

            if (MenuItems.Count > 0)
                if (_descriptionScaleform != null)
                    NativeFunctions.DrawTextField(_descriptionScaleform.Handle, MenuItems[_selectedIndex].Description,
                        new PointF(MenuWidth / 2f + OffsetX, currentY + 7), new SizeF(MenuWidth, 0f));

            DrawInstructionalButtons();
            HandleNavInput();
            HandleCameraRotation();
        }

        private void DrawInstructionalButtons()
        {
            if (MenuItems.Count <= 0) return;
            Function.Call(Hash._0x0DF606929C105BE1, _instructionalButtonsScaleform.Handle, 255, 255, 255, 255, 0);
            _instructionalButtonsScaleform.CallFunction("CLEAR_RENDER");
            var count = 0;
            if (MenuItems[_selectedIndex].Enabled)
            {
                _instructionalButtonsScaleform.CallFunction("SET_DATA_SLOT", count, GetControlString(NavSelect), Game.GetGXTEntry("HUD_INPUT2"));
                count++;
            }
            _instructionalButtonsScaleform.CallFunction("SET_DATA_SLOT", count, GetControlString(NavCancel), Game.GetGXTEntry("HUD_INPUT3"));
            _instructionalButtonsScaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
        }

        private static string GetControlString(Control control)
        {
            return Function.Call<string>(Hash._0x0499D7B09FC9B407, 1, (int)control, 0);
        }

        private void HandleCameraRotation()
        {
            NativeFunctions.GetMousePosition(new Size(1920, 1080), out var x, out _);
            if (x >= 1919f)
            {
                _mouseOnScreenEdge = true;
                GameplayCamera.RelativeHeading -= 2.5f;
                Function.Call(Hash._0x8DB8CFFD58B62552, (int) CursorSprite.RightArrow); // SET_CURSOR_SPRITE
            }
            else if (x <= 0f)
            {
                _mouseOnScreenEdge = true;
                GameplayCamera.RelativeHeading += 2.5f;
                Function.Call(Hash._0x8DB8CFFD58B62552, (int) CursorSprite.LeftArrow); // SET_CURSOR_SPRITE
            }
            else
            {
                _mouseOnScreenEdge = false;
            }
        }

        private void HandleNavInput()
        {
            if (Game.IsDisabledControlJustPressed(2, NavCancel) && !IsInputWaiting())
            {
                Visible = false;
                return;
            }

            if (Game.IsDisabledControlJustPressed(2, NavSelect))
            {
                if (MenuItems.Count <= 0) return;
                if (IsInputWaiting()) return;

                var item = MenuItems[_selectedIndex];

                if (item.Enabled)
                    OnItemSelected(new NativeMenuItemEventArgs(item, _selectedIndex));
            }
            else if (
                Game.IsDisabledControlPressed(2, NavDown) ||
                Game.IsDisabledControlPressed(2, GetNavControlForControlType(NavigationMode, NavDown)))
            {
                if (IsInputWaiting()) return;

                _consecutiveScrolls++;

                _inputWait = Game.GameTime + GetScrollTimeMs();

                IncreaseIndex();
            }
            else
            {
                if (Game.IsDisabledControlJustReleased(2, NavDown) ||
                    Game.IsDisabledControlJustReleased(2, GetNavControlForControlType(NavigationMode, NavDown)))
                {
                    _inputWait = Game.GameTime + (AcceleratedScrolling ? 0 : 170);
                }
                else if (Game.IsDisabledControlPressed(2, NavUp) ||
                         Game.IsDisabledControlPressed(2, GetNavControlForControlType(NavigationMode, NavUp)))
                {
                    if (IsInputWaiting()) return;

                    _consecutiveScrolls++;

                    _inputWait = Game.GameTime + GetScrollTimeMs();

                    DecreaseIndex();
                }
                else
                {
                    if (Game.IsDisabledControlJustReleased(2, NavUp) ||
                        Game.IsDisabledControlJustReleased(2, GetNavControlForControlType(NavigationMode, NavUp)))
                    {
                        ResetInputWait();
                    }
                    else
                    {
                        if (Game.IsDisabledControlJustReleased(2, NavSelect))
                        {
                            _inputWait = Game.GameTime;
                        }
                        else
                        {
                            var scrollNormal = Game.GetControlNormal(2, Control.CursorScrollUp) -
                                               Game.GetControlNormal(2, Control.CursorScrollDown);

                            if (Math.Abs(scrollNormal - 1f) < 0.00001f)
                            {
                                if (IsInputWaiting()) return;

                                DecreaseIndex();

                                _inputWait = Game.GameTime + 20;
                            }
                            else if (Math.Abs(scrollNormal + 1f) < 0.00001f)
                            {
                                if (IsInputWaiting()) return;

                                IncreaseIndex();

                                _inputWait = Game.GameTime + 20;
                            }
                            else
                            {
                                if (_isUsingScrollBar && !IsInputWaiting())
                                    _isUsingScrollBar = false;

                                if (_isUsingScrollBar) return;

                                ResetInputWait();

                                _consecutiveScrolls = 0;
                            }
                        }
                    }
                }
            }
        }

        private static Control GetNavControlForControlType(NavigationMode t, Control c)
        {
            switch (c)
            {
                case NavUp:
                    if (t != NavigationMode.Normal)
                        return Control.MoveUpOnly;
                    return c;
                case NavDown:
                    if (t != NavigationMode.Normal)
                        return Control.MoveDownOnly;
                    return c;
                case Control.PhoneLeft:
                    if (t != NavigationMode.Normal)
                        return Control.MoveLeftOnly;
                    return c;
                case Control.PhoneRight:
                    if (t != NavigationMode.Normal)
                        return Control.MoveRightOnly;
                    return c;
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), "Control given was not a navigation control.");
            }
        }

        private void ResetInputWait()
        {
            _inputWait = Game.GameTime;
        }

        private void DecreaseIndex()
        {
            if (MenuItems.Count <= 0) return;

            var lastItem = MenuItems[_selectedIndex];

            _selectedIndex--;

            if (MenuItems.Count - 1 >= MaxDrawableItems)
            {
                if (_selectedIndex < _scrollIndex)
                    _scrollIndex--;

                if (_selectedIndex < 0)
                {
                    _scrollIndex = MenuItems.Count - MaxDrawableItems;
                    _selectedIndex = MenuItems.Count - 1;
                }
            }
            else
            {
                if (_selectedIndex < 0)
                    _selectedIndex = MenuItems.Count - 1;
            }

            OnIndexChanged(
                new NativeMenuIndexChangedEventArgs(MenuItems[_selectedIndex], lastItem, _selectedIndex));
        }

        private void IncreaseIndex()
        {
            if (MenuItems.Count <= 0) return;

            var lastItem = MenuItems[_selectedIndex];

            _selectedIndex = _selectedIndex + 1;

            if (_selectedIndex - _scrollIndex >= MaxDrawableItems)
                _scrollIndex++;

            if (_selectedIndex >= MenuItems.Count)
            {
                _selectedIndex = 0;
                _scrollIndex = 0;
            }

            OnIndexChanged(new NativeMenuIndexChangedEventArgs(MenuItems[_selectedIndex], lastItem, _selectedIndex));
        }

        private bool IsInputWaiting()
        {
            return Game.GameTime < _inputWait;
        }

        private void ManageControls()
        {
            if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                Function.Call(Hash._SHOW_CURSOR_THIS_FRAME);

            Game.DisableAllControlsThisFrame(2);
            Game.EnableControlThisFrame(2, Control.FrontendPause);
            Game.EnableControlThisFrame(2, Control.ReplayStartStopRecording);
            Game.EnableControlThisFrame(2, Control.ReplayStartStopRecordingSecondary);
            Game.EnableControlThisFrame(2, Control.CursorX);
            Game.EnableControlThisFrame(2, Control.CursorY);
            Game.EnableControlThisFrame(2, Control.CursorScrollUp);
            Game.EnableControlThisFrame(2, Control.CursorScrollDown);

            if (NavigationMode == NavigationMode.Movement) return;
            Game.EnableControlThisFrame(2, Control.MoveUpDown);
            Game.EnableControlThisFrame(2, Control.MoveLeftRight);
            Game.EnableControlThisFrame(2, Control.Jump);
            if (Game.CurrentInputMode != InputMode.GamePad) return;
            Game.EnableControlThisFrame(2, Control.LookLeftRight);
            Game.EnableControlThisFrame(2, Control.LookUpDown);
            Game.EnableControlThisFrame(2, Control.Attack);
        }

        private void DrawScrollBar(ref float currentY)
        {
            // Size and offsets.
            var size = new SizeF(MenuWidth, MenuScrollAreaHeight);
            var yOffset = currentY;
            var menuMiddle = MenuWidth / 2f;
            var halfHeight = size.Height / 2f;
            var posX = new PointF(menuMiddle + OffsetX, yOffset + halfHeight + 0.5f);

            // Draw gradient and scroll icon.
            NativeFunctions.DrawRect(posX, size, Color.FromArgb(180, Color.Black));
            NativeFunctions.DrawSprite("commonmenu", "shop_arrows_upanddown", posX, new SizeF(58, 58));
            currentY += MenuScrollAreaHeight + 0.5f;

            // If the input is waiting then return..
            if (IsInputWaiting()) return;

            // Draw button rects.
            if (NativeFunctions.IsMouseInBounds(new PointF(OffsetX, yOffset), size - new SizeF(0, halfHeight)))
            {
                NativeFunctions.DrawRect(posX - new SizeF(0, halfHeight / 2), size - new SizeF(0, halfHeight),
                    HighlightColor());
                if (!Game.IsControlPressed(2, Control.FrontendAccept)) return;
                _consecutiveScrolls++;
                _inputWait = Game.GameTime + GetScrollTimeMs();
                DecreaseIndex();
                _isUsingScrollBar = true;
            }
            else if (NativeFunctions.IsMouseInBounds(new PointF(OffsetX, yOffset + halfHeight),
                size - new SizeF(0, halfHeight)))
            {
                NativeFunctions.DrawRect(posX + new SizeF(0, halfHeight / 2), size - new SizeF(0, halfHeight),
                    HighlightColor());
                if (!Game.IsControlPressed(2, Control.FrontendAccept)) return;
                _consecutiveScrolls++;
                _inputWait = Game.GameTime + GetScrollTimeMs();
                IncreaseIndex();
                _isUsingScrollBar = true;
            }
        }

        private int GetScrollTimeMs()
        {
            return _consecutiveScrolls >= 4 && AcceleratedScrolling ? 75 : AcceleratedScrolling ? 300 : 175;
        }

        private void HandleMenuItems(ref float currentY)
        {
            DrawMenuItemsBackground(currentY, _lastDrawableItemCount);

            var drawCount = 0;
            var overridenCursor = false;
            var isOverAnyItem = false;

            for (var i = _scrollIndex; i < _scrollIndex + MaxDrawableItems; i++)
            {
                if (i >= MenuItems.Count) continue;
                var menuItem = MenuItems[i];
                GetHoverStateOfMenuItem(currentY, drawCount, out var hover, out var interactHover,
                    out var navRightHover);

                if (hover) isOverAnyItem = true;
                var selected = _selectedIndex == i;

                if (menuItem.Enabled)
                    HandleMenuItemInput(i, menuItem, hover, interactHover, navRightHover, selected);

                menuItem.Draw(new PointF(OffsetX, currentY + MenuItemHeight * drawCount),
                    new SizeF(MenuWidth, MenuItemHeight), selected,
                    hover,
                    interactHover,
                    ref overridenCursor);

                drawCount++;
            }

            ClickOutOfMenu(isOverAnyItem);

            _lastDrawableItemCount = drawCount;

            if (!overridenCursor && !_mouseOnScreenEdge)
                Function.Call(Hash._0x8DB8CFFD58B62552, (int) CursorSprite.Normal); // SET_CURSOR_SPRITE
            currentY += GetMenuItemOffsetHeight(drawCount);
        }

        private void GetHoverStateOfMenuItem(float currentYPosition, int drawIndex, out bool hover,
            out bool interactHover, out bool navRightHover)
        {
            hover = NativeFunctions.IsMouseInBounds(new PointF(OffsetX, currentYPosition + MenuItemHeight * drawIndex),
                new SizeF(MenuWidth, MenuItemHeight));
            interactHover = NativeFunctions.IsMouseInBounds(
                new PointF(OffsetX, currentYPosition + MenuItemHeight * drawIndex),
                new SizeF(MenuWidth / MenuItemInteractionAreaRatio, MenuItemHeight));
            navRightHover = NativeFunctions.IsMouseInBounds(
                new PointF(OffsetX + MenuWidth - 50, currentYPosition + MenuItemHeight * drawIndex),
                new SizeF(50, MenuItemHeight));
        }

        private void ClickOutOfMenu(bool isOverAnyItem)
        {
            if (isOverAnyItem) return;
            if (Game.IsDisabledControlPressed(2, Control.CursorAccept) && AllowClickOut)
                Visible = false;
        }

        private void HandleMenuItemInput(int index, NativeMenuItemBase menuItem, bool hover, bool interactHover,
            bool navRightHover, bool selected)
        {
            var canSelect = selected && menuItem.Enabled && !IsInputWaiting();

            // Nav left right.
            if (canSelect && menuItem.IgnoreClick)
                if (Game.IsDisabledControlJustPressed(2, NavLeft))
                {
                    menuItem.OnNavLeftRight(this, index, -1);
                    OnNavLeftRight(this, -1);
                    _inputWait = Game.GameTime + 10;
                }
                else if (Game.IsDisabledControlJustPressed(2, NavRight))
                {
                    menuItem.OnNavLeftRight(this, index, 1);
                    OnNavLeftRight(this, -1);
                    _inputWait = Game.GameTime + 10;
                }

            if (!hover || !Game.IsControlJustPressed(2, Control.CursorAccept)) return;

            if (canSelect)
            {
                if (menuItem.IgnoreClick && !(menuItem.InteractionCursor != CursorSprite.None && interactHover))
                {
                    if (navRightHover)
                    {
                        OnNavLeftRight(this, 1);
                        menuItem.OnNavLeftRight(this, index, 1);
                        _inputWait = Game.GameTime + 10;
                    }
                    else
                    {
                        OnNavLeftRight(this, -1);
                        menuItem.OnNavLeftRight(this, index, -1);
                        _inputWait = Game.GameTime + 10;
                    }
                }
                else
                {
                    OnItemSelected(new NativeMenuItemEventArgs(menuItem, index));
                    _inputWait = Game.GameTime + 10;
                }
            }
            else if (_selectedIndex != index)
            {
                var lastItem = MenuItems[_selectedIndex];
                _selectedIndex = index;
                OnIndexChanged(new NativeMenuIndexChangedEventArgs(menuItem, lastItem, index));
                _inputWait = Game.GameTime + 10;
            }
        }

        private void DrawMenuItemsBackground(float currentY, int drawnItems)
        {
            var height = GetMenuItemOffsetHeight(drawnItems);
            NativeFunctions.DrawSprite("commonmenu", "gradient_bgd",
                new PointF(MenuWidth / 2f + OffsetX, height / 2 + currentY), new SizeF(MenuWidth, height - 2f));
        }

        private static float GetMenuItemOffsetHeight(int drawnItems)
        {
            var height = MenuItemHeight * drawnItems;
            return height;
        }

        private static void HideHud()
        {
            UI.HideHudComponentThisFrame(HudComponent.HelpText);
        }

        private bool DrawDescription(float currentY, bool displayCount, int selectedIndex, int itemCount)
        {
            if (string.IsNullOrEmpty(Description))
                return false;

            // Size.
            var size = new SizeF(MenuWidth, 45);

            // Background.
            NativeFunctions.DrawRect(new PointF(size.Width / 2 + OffsetX, size.Height / 2 + currentY), size,
                Color.Black);

            // Description text.
            NativeFunctions.DrawText(Description,
                new PointF(OffsetX + DescriptionXOffset, currentY + DescriptionYOffset), DescriptionTextScale,
                DescriptionColor, 1, Font.ChaletLondon, false, false);

            // Count text.
            if (displayCount)
                NativeFunctions.DrawText($"{selectedIndex} / {itemCount}",
                    new PointF(size.Width + DescriptionXOffset + OffsetX - ItemCountXOffset,
                        currentY + DescriptionYOffset), DescriptionTextScale, DescriptionColor, 2, Font.ChaletLondon,
                    false, false);

            // We can draw it.
            return true;
        }

        private void RequestTextureDictionaries()
        {
            RequestTextureDict("commonmenu");
            if (!string.IsNullOrEmpty(_bannerDict) && !string.IsNullOrEmpty(_bannerSprite))
                RequestTextureDict(_bannerDict);
        }

        private bool DrawBanner(out float height)
        {
            if (string.IsNullOrEmpty(_bannerSprite) || string.IsNullOrEmpty(_bannerDict))
            {
                height = 0f;
                return false;
            }

            // Size.
            var v = Function.Call<Vector3>(Hash.GET_TEXTURE_RESOLUTION, _bannerDict, _bannerSprite);
            var size = new SizeF(MenuWidth, v.Y);

            // Banner.
            NativeFunctions.DrawSprite(_bannerDict, _bannerSprite,
                new PointF(size.Width / 2 + OffsetX, size.Height / 2 + OffsetY), size);

            // TODO: Banner Text.
            NativeFunctions.DrawText(Title, new PointF(size.Width / 2 + OffsetX, size.Height / 2 / 2), 1f, Color.White,
                0, Font.HouseScript, false, false);

            height = size.Height;
            return true;
        }

        private static void RequestTextureDict(string dict)
        {
            if (NativeFunctions.HasTextureDictionaryLoaded(dict)) return;
            NativeFunctions.RequestTextureDictionary(dict);
            var dateTime = DateTime.Now.AddSeconds(0.5f);
            while (!NativeFunctions.HasTextureDictionaryLoaded(dict) && DateTime.Now < dateTime)
                Script.Yield();
        }

        private static string GetTextureDictForBannerType(MenuBannerType bannerType, out string sprite)
        {
            switch (bannerType)
            {
                case MenuBannerType.InteractionMenu:
                    sprite = "interaction_bgd";
                    return "commonmenu";
                case MenuBannerType.Michael:
                    sprite = "shopui_title_graphics_michael";
                    return sprite;
                case MenuBannerType.Franklin:
                    sprite = "shopui_title_graphics_franklin";
                    return sprite;
                case MenuBannerType.Trevor:
                    sprite = "shopui_title_graphics_trevor";
                    return sprite;
                case MenuBannerType.Barber:
                    sprite = "shopui_title_barber";
                    return sprite;
                case MenuBannerType.Barber2:
                    sprite = "shopui_title_barber2";
                    return sprite;
                case MenuBannerType.Barber3:
                    sprite = "shopui_title_barber3";
                    return sprite;
                case MenuBannerType.Barber4:
                    sprite = "shopui_title_barber4";
                    return sprite;
                case MenuBannerType.CarMod:
                    sprite = "shopui_title_carmod";
                    return sprite;
                case MenuBannerType.CarMod2:
                    sprite = "shopui_title_carmod2";
                    return sprite;
                case MenuBannerType.ClubHouseMod:
                    sprite = "shopui_title_clubhousemod";
                    return sprite;
                case MenuBannerType.ConvenienceStore:
                    sprite = "shopui_title_conveniencestore";
                    return sprite;
                case MenuBannerType.Darts:
                    sprite = "shopui_title_darts";
                    return sprite;
                case MenuBannerType.ExecutiveVehicleUpgrade:
                    sprite = "shopui_title_exec_vechupgrade";
                    return sprite;
                case MenuBannerType.GasStation:
                    sprite = "shopui_title_gasstation";
                    return sprite;
                case MenuBannerType.GolfShop:
                    sprite = "shopui_title_golfshop";
                    return sprite;
                case MenuBannerType.GunRunningGunMod:
                    sprite = "shopui_title_gr_gunmod";
                    return sprite;
                case MenuBannerType.GunClub:
                    sprite = "shopui_title_gunclub";
                    return sprite;
                case MenuBannerType.HighEndFashion:
                    sprite = "shopui_title_highendfashion";
                    return sprite;
                case MenuBannerType.HighEndSalon:
                    sprite = "shopui_title_highendsalon";
                    return sprite;
                case MenuBannerType.ImportExportModGarage:
                    sprite = "shopui_title_ie_modgarage";
                    return sprite;
                case MenuBannerType.LiquorStore:
                    sprite = "shopui_title_liquorstore";
                    return sprite;
                case MenuBannerType.LiquorStore2:
                    sprite = "shopui_title_liquorstore2";
                    return sprite;
                case MenuBannerType.LiquorStore3:
                    sprite = "shopui_title_liquorstore3";
                    return sprite;
                case MenuBannerType.LowEndFashion:
                    sprite = "shopui_title_lowendfashion";
                    return sprite;
                case MenuBannerType.LowEndFashion2:
                    sprite = "shopui_title_lowendfashion2";
                    return sprite;
                case MenuBannerType.MidFashion:
                    sprite = "shopui_title_midfashion";
                    return sprite;
                case MenuBannerType.MovieMasks:
                    sprite = "shopui_title_movie_masks";
                    return sprite;
                case MenuBannerType.SmuglersHangar:
                    sprite = "shopui_title_sm_hangar";
                    return sprite;
                case MenuBannerType.SuperMod:
                    sprite = "shopui_title_supermod";
                    return sprite;
                case MenuBannerType.Tattoos:
                    sprite = "shopui_title_tattoos";
                    return sprite;
                case MenuBannerType.Tattoos2:
                    sprite = "shopui_title_tattoos2";
                    return sprite;
                case MenuBannerType.Tattoos3:
                    sprite = "shopui_title_tattoos3";
                    return sprite;
                case MenuBannerType.Tattoos4:
                    sprite = "shopui_title_tattoos4";
                    return sprite;
                case MenuBannerType.Tattoos5:
                    sprite = "shopui_title_tattoos5";
                    return sprite;
                case MenuBannerType.Tennis:
                    sprite = "shopui_title_tennis";
                    return sprite;
                case MenuBannerType.None:
                    sprite = string.Empty;
                    return sprite;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bannerType), bannerType, null);
            }
        }

        internal static Color HighlightColor()
        {
            return Color.FromArgb(50, Color.White);
        }

        protected virtual void OnIndexChanged(NativeMenuIndexChangedEventArgs e)
        {
            if (!_supressAudio)
                Audio.ReleaseSound(Audio.PlaySoundFrontend("NAV_UP_DOWN", GetSoundSetForFrontEndAudio(SoundSet)));
            IndexChanged?.Invoke(this, e);
            _supressAudio = false;
        }

        protected virtual void OnItemSelected(NativeMenuItemEventArgs e)
        {
            if (!_supressAudio)
                Audio.ReleaseSound(Audio.PlaySoundFrontend("SELECT", GetSoundSetForFrontEndAudio(SoundSet)));
            ItemSelected?.Invoke(this, e);
            e.MenuItem?.OnSelected(this, e);
            _supressAudio = false;
        }

        protected virtual void OnMenuClosed()
        {
            if (!_supressAudio)
                Audio.ReleaseSound(Audio.PlaySoundFrontend("BACK", GetSoundSetForFrontEndAudio(SoundSet)));
            MenuClosed?.Invoke(this, EventArgs.Empty);
            _supressAudio = false;
        }

        protected virtual void OnMenuOpened()
        {
            if (!_supressAudio)
                Audio.ReleaseSound(Audio.PlaySoundFrontend("SELECT", GetSoundSetForFrontEndAudio(SoundSet)));
            MenuOpened?.Invoke(this, EventArgs.Empty);
            _supressAudio = false;
        }

        protected virtual void OnNavLeftRight(NativeMenu sender, int leftRight)
        {
            if (!_supressAudio)
                Audio.ReleaseSound(Audio.PlaySoundFrontend("NAV_LEFT_RIGHT", GetSoundSetForFrontEndAudio(SoundSet)));
            _supressAudio = false;
        }

        private static string GetSoundSetForFrontEndAudio(FrontEndAudio audio)
        {
            switch (audio)
            {
                case FrontEndAudio.FrontEndShop:
                    return "HUD_FRONTEND_CLOTHESSHOP_SOUNDSET";
                case FrontEndAudio.FreeMode:
                    return "HUD_FREEMODE_SOUNDSET";
                case FrontEndAudio.FrontendDefault:
                    return "HUD_FRONTEND_DEFAULT_SOUNDSET";
                default:
                    throw new ArgumentOutOfRangeException(nameof(audio), audio, null);
            }
        }

        public void SupressAudioNextCall()
        {
            _supressAudio = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _descriptionScaleform.Dispose();
                _instructionalButtonsScaleform.Dispose();
            }

            _disposed = true;
        }
    }
}
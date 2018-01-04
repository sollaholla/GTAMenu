using System;
using System.Drawing;
using GTA;
using GTA.Native;
using Font = GTA.Font;

namespace GTAMenu
{
    internal static class NativeFunctions
    {
        private const int MaxStringLength = 99;

        public static float MeasureStringWidth(string text, string label, Font font, float scale)
        {
            var res = NativeFunctions.GetScreenResolution(out _);
            Function.Call(Hash._0x54CE8AC98E120CAB, label); // _BEGIN_TEXT_COMMAND_WIDTH
            Function.Call(Hash._0x6C188BE134E074AA, text); // ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
            Function.Call(Hash.SET_TEXT_FONT, (int)font);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            return Function.Call<float>(Hash._0x85F061DA64ED2F67, 1) * res.Width; // _END_TEXT_COMMAND_GET_WIDTH
        }

        public static bool HasTextureDictionaryLoaded(string dictionary)
        {
            return Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, dictionary);
        }

        public static void RequestTextureDictionary(string dictionary)
        {
            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, dictionary, 0);
        }

        public static bool IsMouseInBounds(PointF topLeft, SizeF boxSize)
        {
            var res = GetScreenResolution(out var a);
            var offset = GetOffsetForAspectRatio(a);
            topLeft += new SizeF(offset * res.Width /*+ offset*/, offset * res.Height /*+ offset*/);
            GetMousePosition(res, out var mouseX, out var mouseY);

            return mouseX >= topLeft.X && mouseX <= topLeft.X + boxSize.Width && mouseY > topLeft.Y &&
                   mouseY < topLeft.Y + boxSize.Height;
        }

        public static void GetMousePosition(Size res, out float mouseX, out float mouseY)
        {
            mouseX = (float) Math.Round(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int) Control.CursorX) *
                                        res.Width);
            mouseY = (float) Math.Round(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int) Control.CursorY) *
                                        res.Height);
        }

        public static void DrawSprite(string dictionary, string texture, PointF position, SizeF size, float heading,
            Color color)
        {
            var resolution = GetScreenResolution(out var a);
            var offset = GetOffsetForAspectRatio(a);

            Function.Call(Hash.DRAW_SPRITE, dictionary, texture, position.X / resolution.Width + offset,
                position.Y / resolution.Height + offset, size.Width / resolution.Width, size.Height / resolution.Height,
                heading, color.R, color.G, color.B, color.A);
        }

        private static float GetOffsetForAspectRatio(float aspect)
        {
            var zone = Function.Call<float>(Hash.GET_SAFE_ZONE_SIZE);
            var safeZone = 1 - zone;
            safeZone *= 0.5f;
            if (aspect <= 1.77777777778f)
                return safeZone;
            var o = 1f - 1.7777777910232544f / aspect;
            o *= 0.5f;
            return o + safeZone;
        }

        public static void DrawSprite(string dictionary, string texture, PointF position, SizeF size, float heading)
        {
            DrawSprite(dictionary, texture, position, size, heading, Color.White);
        }

        public static void DrawSprite(string dictionary, string texture, PointF position, SizeF size)
        {
            DrawSprite(dictionary, texture, position, size, 0, Color.White);
        }

        public static Size GetScreenResolution(out float aspectRatio)
        {
            var ratio = GetAspectRatio();
            aspectRatio = ratio;
            const int height = 1280;
            var newWidth = height * ratio;
            return new Size((int) newWidth, height);
        }

        private static float GetAspectRatio()
        {
            return Function.Call<float>(Hash._0xF1307EF624A80D87, 1); // _GET_ASPECT_RATIO
        }

        public static void DrawRect(PointF position, SizeF size, Color color)
        {
            var resolution = GetScreenResolution(out var a);
            var offset = GetOffsetForAspectRatio(a);
            Function.Call(Hash.DRAW_RECT, position.X / resolution.Width + offset,
                position.Y / resolution.Height + offset,
                size.Width / resolution.Width, size.Height / resolution.Height, color.R, color.G, color.B, color.A);
        }

        public static void DrawText(string text, PointF position, float size, Color color, int justify, Font font,
            bool shadow, bool outline)
        {
            var resolution = GetScreenResolution(out var a);
            var offset = GetOffsetForAspectRatio(a);
            Function.Call(Hash.SET_TEXT_FONT, (int) font);
            Function.Call(Hash.SET_TEXT_SCALE, size, size);
            Function.Call(Hash.SET_TEXT_COLOUR, color.R, color.G, color.B, color.A);
            if (shadow) Function.Call(Hash.SET_TEXT_DROP_SHADOW);
            if (outline) Function.Call(Hash.SET_TEXT_OUTLINE);
            switch (justify)
            {
                case 0:
                    Function.Call(Hash.SET_TEXT_CENTRE, true);
                    break;
                case 1:
                    Function.Call(Hash.SET_TEXT_JUSTIFICATION, justify);
                    break;
                case 2:
                    Function.Call(Hash.SET_TEXT_RIGHT_JUSTIFY, true);
                    Function.Call(Hash.SET_TEXT_WRAP, 0, position.X / resolution.Width + offset);
                    break;
            }
            Function.Call(Hash.SET_TEXT_JUSTIFICATION, justify);
            Function.Call(Hash._0x25FBB336DF1804CB, "jamyfafi"); // BEGIN_TEXT_COMMAND_DISPLAY_TEXT
            Function.Call(Hash._0x6C188BE134E074AA, text); // ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
            Function.Call(Hash._0xCD015E5BB0D96A57, position.X / resolution.Width + offset,
                position.Y / resolution.Height + offset); // END_TEXT_COMMAND_DISPLAY_TEXT
        }

        public static void DrawTextField(int scaleFormHandle, string text, PointF position, SizeF size)
        {
            if (!Function.Call<bool>(Hash.HAS_SCALEFORM_MOVIE_LOADED, scaleFormHandle)) return;
            if (string.IsNullOrEmpty(text)) return;

            var resolution = GetScreenResolution(out var a);
            var offset = GetOffsetForAspectRatio(a);

            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, scaleFormHandle, "SET_TEXT");
            Function.Call(Hash._BEGIN_TEXT_COMPONENT, "CELL_EMAIL_BCON");
            AddLongString(text);
            Function.Call(Hash._0xAE4E8157D9ECF087);
            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, scaleFormHandle, "SET_BACKGROUND_IMAGE");
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_STRING, "CommonMenu");
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_STRING, "Gradient_Bgd");
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 75); // Alpha
            Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

            var xOffset = 0.0005f;
            if (a <= 1.77777777778f)
                xOffset = 0f;

            var x = position.X / resolution.Width + offset + xOffset;
            var y = position.Y / resolution.Height + offset;
            var w = size.Width / resolution.Width + xOffset;

            Function.Call(Hash.DRAW_SCALEFORM_MOVIE, scaleFormHandle, x, y + 0.5f, w, 1f, 255, 255, 255, 0);
            DrawRect(position, new SizeF(size.Width, 2.8f), Color.Black);
        }

        private static void AddLongString(string text)
        {
            for (var i = 0; i < text.Length; i += MaxStringLength)
                Function.Call(Hash._0x6C188BE134E074AA,
                    text.Substring(i,
                        Math.Min(MaxStringLength, text.Length - i))); // ADD_TEXT COMPONENT_SUBSTRING_PLAYER_NAME
        }
    }
}
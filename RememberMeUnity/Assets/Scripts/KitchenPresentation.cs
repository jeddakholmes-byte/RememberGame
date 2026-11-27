using UnityEngine;

namespace RememberMe
{
    public sealed partial class KitchenPrototype
    {
        static readonly Color Parchment = new Color(.97f, .91f, .77f);
        static readonly Color SoftGold = new Color(.96f, .79f, .43f);
        static readonly Color BlueRibbon = new Color(.30f, .60f, .90f);
        static readonly Color QuietTeal = new Color(.25f, .42f, .38f);
        static readonly Color DarkInk = new Color(.12f, .13f, .13f);
        static readonly string[] IngredientNames = {"Flour", "Butter", "Eggs", "Sugar"};
        static readonly string[] DoorNames = {"Clock", "Curtains", "Water"};
        Texture2D circle;

        void OnGUI()
        {
            if (!view) return;
            float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
            if (scale <= 0) return;
            var matrix = GUI.matrix;
            var stage = new Vector2((Screen.width - Width * scale) * .5f, (Screen.height - Height * scale) * .5f);
            var wholeScreen = new Rect(0, 0, Screen.width, Screen.height);

            GUI.matrix = Matrix4x4.TRS(stage, Quaternion.identity, new Vector3(scale, scale, 1));
            Vector2 WorldUI(Vector2 feet)
            {
                Vector2 point = view.WorldToScreenPoint(World(feet));
                return new Vector2(point.x - stage.x, Screen.height - point.y - stage.y) / scale;
            }

            if (bedroomArrival == 0 && !WaitingForBeat && lines == null && choices == null && !mixing && !shaping && chapter < Chapter.Ending)
            {
                DrawGuidance(WorldUI);
                if (currentRoom == Room.Hall && photoSeen) DrawClueReminder();
            }
            if (inspectJars) DrawJarChoice();
            if (showingNote) DrawNote();
            if (visibleCard >= 0) PicturePanel(cardArt[visibleCard], new Rect(366, 229, 420, 259));
            if (shownDoor.HasValue) DrawDoorClue(shownDoor.Value);
            if (showPhoto || showingFamilyPhoto)
                PicturePanel(showPhoto ? photo : livingRoomPhoto, new Rect(254, 110, 644, 418));
            if (handCue > 0 && currentRoom == Room.Kitchen)
            {
                Vector2 cue = WorldUI((grace.Feet + grandma.Feet) * .5f - new Vector2(0, 100));
                Badge(cue, "Together", new Color(SoftGold.r, SoftGold.g, SoftGold.b, Mathf.Clamp01(handCue)));
            }

            GUI.matrix = matrix;
            if (transition > 0) Tint(wholeScreen, new Color(.025f, .035f, .044f, transition / .45f));
            if (memoryFade > 0) Tint(wholeScreen, new Color(.025f, .035f, .044f, memoryFade));
        }

        void DrawGuidance(System.Func<Vector2, Vector2> worldUI)
        {
            foreach (var target in targets)
            {
                string cue = TargetCue(target, out bool required);
                if (cue == null || !required || target.Available?.Invoke() == false) continue;
                Vector2 feet = target == grandmaTarget ? grandma.Feet : target.Feet;
                float above = target == grandmaTarget ? 137 : currentRoom == Room.Hall && feet.y < 500 ? 101 : 61;
                Vector2 actual = worldUI(feet - new Vector2(0, above));
                if (actual.x < 34 || actual.x > Width - 34 || actual.y < 117 || actual.y > Height - 112)
                    continue;
                var matrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(45, actual);
                var halo = new Rect(actual.x - 5, actual.y - 5, 10, 10);
                Tint(halo, new Color(.19f, .13f, .08f, .87f));
                Tint(new Rect(actual.x - 3, actual.y - 3, 6, 6), SoftGold);
                GUI.matrix = matrix;
            }
        }

        string TargetCue(Target target, out bool required)
        {
            required = true;
            float x = target.Feet.x;
            if (currentRoom == Room.Bedroom) return "Keepsake box";
            if (currentRoom == Room.Hall)
            {
                if (target.Feet.y > 600)
                {
                    required = chapter == Chapter.ReturnForGrandma;
                    return "Kitchen";
                }
                required = chapter == Chapter.Escort && x == 576;
                if (chapter == Chapter.ReturnForGrandma) return null;
                var door = x == 317 ? Door.Clock : x == 576 ? Door.Curtains : Door.Water;
                return (inspectedDoors & (1 << (int)door)) != 0 ? DoorNames[(int)door] : "Inspect";
            }
            if (target == grandmaTarget && (chapter == Chapter.MeetGrandma || chapter == Chapter.ReturnForGrandma)) return "Grandma";
            if (chapter == Chapter.Ingredients)
            {
                if (!flourSetOut)
                {
                    if (x == 177 && (ingredients & FlourBit) == 0) return "Flour";
                    if (x == 585 && (ingredients & FlourBit) != 0) return "Set out flour";
                    required = false;
                    // Other ingredients remain usable; only the first task is highlighted.
                    return null;
                }
                if (x == 177 && (ingredients & FlourBit) == 0) return "Flour";
                if (x == 330 && (ingredients & SugarBit) == 0) return "Jars";
                if (x == 761 && (ingredients & EggsBit) == 0) return "Butter & eggs";
                if (x == 585)
                {
                    required = ingredients == AllIngredients || !sugarClueFound;
                    return ingredients == AllIngredients ? "Recipe" : !sugarClueFound ? "Dad’s note" : null;
                }
            }
            if (chapter == Chapter.Cards)
            {
                if (x == 677 && (cards & 1) == 0) return "Picture card";
                if (x == 460 && (cards & 2) == 0) return "Picture card";
                if (x == 761 && (cards & 4) == 0) return "Picture card";
                if (x == 585 && cards == AllCards) return "Recipe";
            }
            if (chapter == Chapter.Baking && x == (baking == 0 ? 677 : 460)) return baking == 0 ? "Mix" : baking == 1 ? "Shape" : "Bake";
            if (chapter == Chapter.Tray && x == 895) return "Blue tray";
            if (target.Feet.y > 600 && (chapter == Chapter.FirstCollapse || chapter == Chapter.FindDoor || chapter == Chapter.Escort))
                return "Doorway";
            required = false;
            if (chapter <= Chapter.Ingredients || chapter == Chapter.ReturnForGrandma || chapter == Chapter.FindDoor)
            {
                if (x == 518) return "Family photo";
                if (x == 975) return "Music box";
            }
            return null;
        }

        void DrawClueReminder()
        {
            var bounds = new Rect(875, 22, 231, 151);
            Panel(bounds);
            GUI.DrawTexture(new Rect(882, 29, 217, 137), livingRoomPhoto, ScaleMode.ScaleToFit);
        }

        void DrawJarChoice()
        {
            var panel = new Rect(312, 162, 528, 313);
            Panel(panel);
            for (int i = 0; i < 2; i++)
            {
                float left = 339 + i * 244;
                var bounds = new Rect(left, 223, 227, 217);
                Tint(bounds, new Color(.24f, .27f, .23f));
                IngredientIcon(new Rect(left + 55, 248, 115, 126), 3, i == 0);
                CenterText(new Rect(left, 383, 227, 42), i == 0 ? "1   Left jar" : "2   Right jar", 23, Parchment);
                if (Pressed(bounds)) { Choose(i); break; }
            }
            CenterText(new Rect(333, 173, 486, 42), sugarClueFound ? "Dad marked the sugar jar with a blue ribbon." : "The labels have worn away.", 22, Parchment);
        }

        void DrawNote()
        {
            var bounds = new Rect(365, 161, 422, 316);
            Panel(bounds);
            Tint(new Rect(380, 176, 392, 286), new Color(.97f, .88f, .68f));
            Label(new Rect(407, 193, 324, 234),
                "Mom,\n\nSugar has the blue ribbon.\nSalt has no ribbon.\nTake your time.\n\nLove, your son.", 24, DarkInk);
            IngredientIcon(new Rect(689, 310, 52, 63), 3, true);
        }

        void DrawRecipe()
        {
            Panel(new Rect(139, 157, 874, sortingCards ? 377 : 228));
            CenterText(new Rect(153, 169, 846, 30), "The family recipe", 26, Parchment);
            for (int i = 0; i < 5; i++)
            {
                var slot = new Rect(160 + 167 * i, 217, 161, 103);
                int step = i - 1;
                bool fixedStep = i == 0 || i == 4;
                bool active = sortingCards && step == recipeOrder.Count;
                Tint(slot, active ? SoftGold : new Color(.46f, .40f, .30f));
                var face = new Rect(slot.x + 3, slot.y + 3, slot.width - 6, slot.height - 6);
                if (fixedStep)
                {
                    Tint(face, Parchment);
                    if (i == 0)
                    {
                        IngredientIcon(new Rect(slot.x + 33, 238, 42, 40), 1, false);
                        IngredientIcon(new Rect(slot.x + 88, 231, 43, 51), 3, true);
                    }
                    else DrawArtCrop(new Rect(slot.x + 40, 228, 81, 60), kitchenArt, new Rect(436, 266, 124, 66));
                    CenterText(new Rect(slot.x, 289, slot.width, 27), i == 0 ? "Cream butter & sugar" : "Bake the cookies", 15, DarkInk);
                }
                else if (step < recipeOrder.Count)
                    GUI.DrawTexture(face, cardArt[recipeOrder[step]], ScaleMode.StretchToFill);
                else
                {
                    Tint(face, new Color(.20f, .24f, .20f));
                    CenterText(face, active ? "+" : "…", 32, Parchment);
                }
                CenterText(new Rect(slot.x, 327, slot.width, 27), (i + 1).ToString(), 20, Parchment);
            }
            if (!sortingCards) return;
            for (int n = 0; n < remainingCardIds.Length; n++)
            {
                float left = (Width - 187f) / 2f + (n - (remainingCardIds.Length - 1) / 2f) * 202f;
                var choice = new Rect(left, 373, 187, 144);
                Tint(choice, QuietTeal);
                GUI.DrawTexture(new Rect(left + 6, 379, 175, 109), cardArt[remainingCardIds[n]]);
                CenterText(new Rect(left, 489, 187, 28), (n + 1) + "   " + CardNames[remainingCardIds[n]], 15, Parchment);
                if (Pressed(choice)) { Choose(n); break; }
            }
        }

        void DrawDoorClue(Door door)
        {
            var bounds = new Rect(318, 151, 516, 374);
            Panel(bounds);
            int doorX = door == Door.Clock ? 317 : door == Door.Curtains ? 576 : 836;
            var hallArt = backdrop.sprite.texture;
            DrawArtCrop(new Rect(345, 169, 462, 315), hallArt, new Rect(doorX - 112, 139, 224, 196));
            var symbol = new Rect(533, 382, 86, 86);
            Tint(symbol, new Color(.08f, .12f, .12f, .93f));
            if (door == Door.Curtains)
            {
                DrawArtCrop(new Rect(542, 390, 68, 68), hallArt, new Rect(554, 288, 44, 34));
                Tint(new Rect(544, 452, 64, 3), BlueRibbon);
            }
            else if (door == Door.Clock)
            {
                Dot(new Rect(545, 393, 62, 62), SoftGold);
                Dot(new Rect(549, 397, 54, 54), Parchment);
                Tint(new Rect(573, 406, 4, 21), DarkInk);
                Tint(new Rect(574, 424, 17, 4), DarkInk);
            }
            else
            {
                Dot(new Rect(557, 421, 38, 31), BlueRibbon);
                for (int i = 0; i < 5; i++)
                    Tint(new Rect(574 - 3 * i, 401 + 6 * i, 4 + 6 * i, 7), BlueRibbon);
            }
            CenterText(new Rect(341, 485, 470, 29), DoorNames[(int)door], 23, Parchment);
        }

        void IngredientIcon(Rect bounds, int item, bool ribbon)
        {
            if (item == 3 && !ribbon)
            {
                DrawArtCrop(bounds, kitchenArt, new Rect(330, 177, 46, 55));
                return;
            }
            var art = ingredientArtwork ? ingredientArtwork : LoadTexture("IngredientArtwork");
            int group = item == 0 ? 0 : item == 3 ? 2 : 1;
            GUI.DrawTextureWithTexCoords(bounds, art, new Rect(group / 3f, 0, 1f / 3f, 1));
        }

        void Dot(Rect bounds, Color color)
        {
            if (!circle)
            {
                const int size = 32;
                circle = new Texture2D(size, size, TextureFormat.RGBA32, false);
                circle.wrapMode = TextureWrapMode.Clamp;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 offset = new Vector2(x + .5f, y + .5f) - Vector2.one * size / 2;
                        circle.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(size / 2f - offset.magnitude)));
                    }
                circle.Apply();
            }
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(bounds, circle);
            GUI.color = previous;
        }

        static void DrawArtCrop(Rect target, Texture2D art, Rect source)
        {
            GUI.DrawTextureWithTexCoords(target, art,
                new Rect(source.x / Width, 1f - source.yMax / Height, source.width / Width, source.height / Height));
        }

        static bool Pressed(Rect bounds)
        {
            var input = Event.current;
            if (input.type != EventType.MouseUp || input.button != 0 || !bounds.Contains(input.mousePosition)) return false;
            input.Use();
            return true;
        }

        static void PicturePanel(Texture2D picture, Rect frame)
        {
            Panel(frame);
            GUI.DrawTexture(new Rect(frame.x + 8, frame.y + 8, frame.width - 16, frame.height - 16), picture, ScaleMode.ScaleToFit);
        }

        static void Badge(Vector2 at, string text, Color color)
        {
            float width = Mathf.Max(74, text.Length * 10 + 21);
            var bounds = new Rect(at.x - width * .5f, at.y - 18, width, 34);
            Tint(bounds, new Color(.07f, .12f, .10f, .94f * color.a));
            Tint(new Rect(bounds.x, bounds.yMax - 2, width, 2), new Color(color.r, color.g, color.b, .68f * color.a));
            CenterText(bounds, text, 19, color);
        }

        static void CenterText(Rect bounds, string value, int size, Color color)
        {
            var style = new GUIStyle(GUI.skin.label) {fontSize = size, alignment = TextAnchor.MiddleCenter};
            style.normal.textColor = color;
            GUI.Label(bounds, value, style);
        }
    }
}

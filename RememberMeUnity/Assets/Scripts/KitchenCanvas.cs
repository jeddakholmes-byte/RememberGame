using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RememberMe
{
    public sealed partial class KitchenPrototype
    {
        static readonly Color FolioEdge = new(.40f, .28f, .16f);
        static readonly Color FolioPaper = new(.96f, .90f, .77f);
        static readonly Color FolioInk = new(.20f, .14f, .09f);
        static readonly Color FolioMuted = new(.38f, .28f, .19f);
        static readonly Color Enamel = new(.16f, .27f, .32f);
        static readonly Color EnamelLip = new(.37f, .50f, .53f);
        static readonly Color Dough = new(.85f, .60f, .34f);
        static readonly Color DoughLight = new(.98f, .79f, .53f);
        static readonly Color Brass = new(.71f, .51f, .27f);

        RectTransform kitchenCanvas, ingredientFolio, cardFolio, craftFolio, mixingBoard, shapingBoard;
        Image waterVeil;
        RectTransform recipeFolio;
        readonly RawImage[] recipePictures = new RawImage[5];
        readonly Button[] placedRecipeCards = new Button[3];
        readonly Text[] recipeCaptions = new Text[5];
        readonly RawImage[] cardPreviews = new RawImage[3];
        readonly Text[] cardPlaces = new Text[3];
        static readonly string[] CardPlaces = { "Bowl", "Oven counter", "Fridge" };
        Text ingredientTotal, craftTitle, craftHint, nextPiece;
        readonly Text[] ingredientMarks = new Text[3];
        static readonly int[] IngredientGroups = { FlourBit, ButterBit | EggsBit, SugarBit };
        static readonly string[] IngredientGroupNames = { "Flour", "Butter & eggs", "Sugar" };
        Texture2D ingredientArtwork;
        SpriteRenderer flourOnCounter;
        readonly Button[] doughSlots = new Button[DoughPieces];
        readonly KitchenOval[] slotRims = new KitchenOval[DoughPieces];
        readonly KitchenOval[] slotDough = new KitchenOval[DoughPieces];
        readonly KitchenOval[] mixSteps = new KitchenOval[MixSteps];
        Button stirButton, doughSource;
        RectTransform spoon, draggedDough;

        void UpdateKitchenCanvas()
        {
            for (int i = 0; i < counterCards.Length; i++)
                counterCards[i].enabled = currentRoom == Room.Kitchen && chapter == Chapter.Cards && (cards & (1 << i)) == 0;
            if (!kitchenCanvas) CreateKitchenCanvas();
            flourOnCounter.enabled = currentRoom == Room.Kitchen && flourSetOut && chapter <= Chapter.Cards;
            Rect panel = hud.OverlayRoot.rect;
            float scale = Mathf.Min(panel.width / Width, panel.height / Height);
            kitchenCanvas.localScale = Vector3.one * scale;
            bool showIngredients = chapter == Chapter.Ingredients && flourSetOut && !WaitingForBeat && lines == null && choices == null;
            bool showCraft = mixing || shaping;
            ingredientFolio.gameObject.SetActive(showIngredients);
            cardFolio.gameObject.SetActive(chapter == Chapter.Cards && lines == null && choices == null);
            for (int i = 0; i < cardPreviews.Length; i++)
            {
                bool found = (cards & (1 << i)) != 0;
                cardPreviews[i].color = new Color(1, 1, 1, found ? 1 : .30f);
                cardPlaces[i].text = (found ? "✓ " : "○ ") + CardPlaces[i];
            }
            craftFolio.gameObject.SetActive(showCraft);
            waterVeil.gameObject.SetActive(waterSpace);
            recipeFolio.gameObject.SetActive(viewRecipe);
            if (viewRecipe)
            {
                for (int i = 0; i < 5; i++)
                {
                    bool fixedStep = i == 0 || i == 4;
                    bool filled = fixedStep || i - 1 < recipeOrder.Count;
                    recipePictures[i].gameObject.SetActive(filled);
                    if (filled) recipePictures[i].texture = cardArt[fixedStep ? (i == 0 ? 1 : 2) : recipeOrder[i - 1]];
                    recipeCaptions[i].text = i == 0 ? "Cream butter & sugar" : i == 4 ? "Bake" :
                        filled ? CardNames[recipeOrder[i - 1]] : "Step " + (i + 1);
                    if (!fixedStep) placedRecipeCards[i - 1].interactable = sortingCards && filled;
                }
            }
            if (showIngredients)
            {
                int count = ((ingredients & FlourBit) != 0 ? 1 : 0) +
                    ((ingredients & (ButterBit | EggsBit)) == (ButterBit | EggsBit) ? 1 : 0) + ((ingredients & SugarBit) != 0 ? 1 : 0);
                ingredientTotal.text = $"Ingredients  ·  {count} / 3";
                for (int i = 0; i < ingredientMarks.Length; i++)
                {
                    bool found = (ingredients & IngredientGroups[i]) == IngredientGroups[i];
                    ingredientMarks[i].text = (found ? "✓  " : "○  ") + IngredientGroupNames[i];
                    ingredientMarks[i].color = found ? FolioInk : FolioMuted;
                }
            }
            if (!showCraft)
            {
                draggedDough.gameObject.SetActive(false);
                return;
            }
            mixingBoard.gameObject.SetActive(mixing);
            shapingBoard.gameObject.SetActive(shaping);
            craftTitle.text = mixing ? "Mix the dough" : "Fill the blue tray";
            craftHint.text = mixing ? "Click Stir or press E." :
                "Drag dough to a slot, click a slot, or press E.";
            if (mixing)
            {
                stirButton.interactable = lines == null && choices == null;
                spoon.localRotation = Quaternion.Euler(0, 0, mixCount * 86f - 22f);
                for (int i = 0; i < mixSteps.Length; i++)
                    mixSteps[i].SetColors(i < mixCount ? Brass : FolioEdge,
                        i < mixCount ? DoughLight : FolioMuted);
            }
            else
            {
                bool doughReady = false;
                for (int i = 0; i < doughSlots.Length; i++)
                {
                    bool placed = (shapedSlots & (1 << i)) != 0;
                    bool ready = lines == null && choices == null && CanPlaceDough(i);
                    doughSlots[i].interactable = ready;
                    doughReady |= ready;
                    slotDough[i].gameObject.SetActive(placed);
                    slotRims[i].SetColors(ready ? DoughLight : EnamelLip, Enamel);
                }
                doughSource.interactable = doughReady;
                nextPiece.text = shapedCount < DoughPieces - 1 ? "Dough" : "Dad’s largest piece";
                if (!doughReady) draggedDough.gameObject.SetActive(false);
            }
        }

        void CreateKitchenCanvas()
        {
            kitchenCanvas = KitchenRect(hud.OverlayRoot, "Kitchen UI", new Rect(0, 0, Width, Height));
            kitchenCanvas.anchorMin = kitchenCanvas.anchorMax = kitchenCanvas.pivot = new Vector2(.5f, .5f);
            kitchenCanvas.anchoredPosition = Vector2.zero;
            // Keep the water effect behind the dialogue and menu canvas children.
            waterVeil = KitchenFill(hud.OverlayRoot, "Water memory veil", new Rect(0, 0, 0, 0), new Color(.025f, .13f, .18f, .42f));
            waterVeil.rectTransform.anchorMin = Vector2.zero;
            waterVeil.rectTransform.anchorMax = Vector2.one;
            waterVeil.rectTransform.offsetMin = waterVeil.rectTransform.offsetMax = Vector2.zero;
            waterVeil.transform.SetAsFirstSibling();

            ingredientArtwork = LoadTexture("IngredientArtwork");
            flourOnCounter = Prop("Flour beside the recipe", InventorySprite(ingredientArtwork,
                new Rect(0, 0, ingredientArtwork.width / 3f, ingredientArtwork.height)), new Vector2(550, 292), 38, 48);
            ingredientFolio = KitchenFrame(kitchenCanvas, "Ingredient folio", new Rect(653, 27, 464, 169), false);
            ingredientTotal = KitchenText(ingredientFolio, "Ingredients", new Rect(17, 10, 430, 24),
                "", 20, FolioInk, TextAnchor.MiddleLeft, true);
            KitchenFill(ingredientFolio, "Guide rule", new Rect(18, 42, 428, 1), Brass);
            for (int i = 0; i < ingredientMarks.Length; i++)
            {
                var illustration = KitchenRect(ingredientFolio, IngredientGroupNames[i] + " artwork",
                    new Rect(44 + i * 143, 44, 94, 94)).gameObject.AddComponent<RawImage>();
                illustration.texture = ingredientArtwork;
                illustration.uvRect = new Rect(i / 3f, 0, 1f / 3f, 1);
                illustration.raycastTarget = false;
                ingredientMarks[i] = KitchenText(ingredientFolio, IngredientGroupNames[i], new Rect(15 + i * 145, 133, 142, 25),
                    "", 18, FolioMuted);
            }

            cardFolio = KitchenFrame(kitchenCanvas, "Picture card collection", new Rect(653, 27, 464, 150), false);
            KitchenText(cardFolio, "Collection title", new Rect(17, 8, 430, 25), "Dad’s picture cards", 21, FolioInk);
            for (int i = 0; i < cardPreviews.Length; i++)
            {
                cardPreviews[i] = KitchenRect(cardFolio, CardPlaces[i] + " picture", new Rect(49 + i * 145, 39, 73, 73))
                    .gameObject.AddComponent<RawImage>();
                cardPreviews[i].texture = cardArt[i];
                cardPreviews[i].raycastTarget = false;
                cardPlaces[i] = KitchenText(cardFolio, CardPlaces[i], new Rect(17 + i * 145, 117, 138, 24), "", 18, FolioInk);
            }

            recipeFolio = KitchenFrame(kitchenCanvas, "Family recipe board", new Rect(116, 165, 920, 338), true);
            KitchenText(recipeFolio, "Recipe title", new Rect(20, 12, 880, 42), "The family recipe", 30, FolioInk);
            KitchenText(recipeFolio, "Undo hint", new Rect(20, 278, 880, 42),
                "Click a placed card to take it back · Backspace undoes the last card", 20, FolioMuted);
            for (int i = 0; i < 5; i++)
            {
                int slot = i - 1;
                var face = KitchenButton(recipeFolio, "Recipe step " + (i + 1), new Rect(22 + i * 177, 67, 164, 164),
                    () => RemoveRecipeCardsFrom(slot));
                if (i > 0 && i < 4) placedRecipeCards[slot] = face;
                else face.interactable = false;
                recipePictures[i] = KitchenRect((RectTransform)face.transform, "Picture", new Rect(3, 3, 158, 158))
                    .gameObject.AddComponent<RawImage>();
                recipePictures[i].raycastTarget = false;
                recipeCaptions[i] = KitchenText(recipeFolio, "Step caption " + i,
                    new Rect(18 + i * 177, 235, 172, 40), "", 18, FolioInk);
            }

            craftFolio = KitchenFrame(kitchenCanvas, "Kitchen work surface", new Rect(259, 156, 634, 446), true);
            craftTitle = KitchenText(craftFolio, "Activity", new Rect(22, 14, 590, 44), "", 34, FolioInk,
                TextAnchor.MiddleCenter, true);
            KitchenFill(craftFolio, "Title rule", new Rect(76, 66, 482, 1), Brass);
            craftHint = KitchenText(craftFolio, "Controls", new Rect(22, 385, 590, 39), "", 23, FolioMuted);

            mixingBoard = KitchenRect(craftFolio, "Mixing board", new Rect(25, 76, 584, 303));
            KitchenText(mixingBoard, "Mixing together", new Rect(0, 4, 584, 29), "Grandma holds the bowl", 23, FolioMuted);
            var bowlPainting = KitchenRect(mixingBoard, "Painted mixing bowl", new Rect(202, 35, 180, 156)).gameObject.AddComponent<RawImage>();
            bowlPainting.texture = cardArt[1];
            bowlPainting.raycastTarget = false;
            spoon = KitchenRect(mixingBoard, "Wooden spoon", new Rect(292, 105, 1, 1));
            for (int i = 0; i < mixSteps.Length; i++)
                mixSteps[i] = KitchenOvalShape(mixingBoard, "Stir progress", new Rect(234 + i * 34, 196, 18, 18),
                    FolioEdge, FolioMuted);
            stirButton = KitchenButton(mixingBoard, "Stir", new Rect(175, 237, 234, 53), MixDough);
            KitchenText((RectTransform)stirButton.transform, "Stir label", new Rect(0, 0, 234, 53),
                "Stir", 24, FolioPaper, TextAnchor.MiddleCenter, true);

            shapingBoard = KitchenRect(craftFolio, "Dough placement board", new Rect(25, 76, 584, 303));
            KitchenText(shapingBoard, "Shaping together", new Rect(0, 4, 584, 32),
                "Grandma steadies the tray", 23, FolioMuted);
            KitchenFill(shapingBoard, "Tray shade", new Rect(100, 52, 441, 170), FolioEdge);
            KitchenFill(shapingBoard, "Tray enamel rim", new Rect(96, 43, 438, 170), EnamelLip);
            KitchenFill(shapingBoard, "Tray enamel", new Rect(104, 51, 422, 154), Enamel);
            var decoration = KitchenRect(shapingBoard, "Painted enamel pattern", new Rect(106, 53, 418, 150))
                .gameObject.AddComponent<RawImage>();
            decoration.texture = kitchenArt;
            // Only sample the blue tray's painted center, without the countertop or adjoining objects.
            decoration.uvRect = new Rect(844f / Width, 1f - 248f / Height, 88f / Width, 60f / Height);
            decoration.color = Color.white;
            decoration.raycastTarget = false;
            for (int i = 0; i < doughSlots.Length; i++)
            {
                int slot = i;
                int size = i == DoughPieces - 1 ? 116 : 90;
                var location = new Rect(174 + i * 137 - size * .5f, 114 - size * .5f, size, size);
                Button button = KitchenButton(shapingBoard, "Tray slot " + (i + 1), location, () => ShapeCookie(slot));
                var clickable = (Image)button.targetGraphic;
                clickable.color = Color.clear;
                button.transition = Selectable.Transition.None;
                doughSlots[i] = button;
                var target = (RectTransform)button.transform;
                slotRims[i] = KitchenOvalShape(target, "Inlay", new Rect(0, 0, size, size), EnamelLip, Enamel, .075f);
                slotDough[i] = KitchenOvalShape(target, "Placed dough", new Rect(8, 8, size - 16, size - 16), Dough, DoughLight);
                KitchenText(shapingBoard, "Slot label " + (i + 1), new Rect(location.center.x - 67, 175, 134, 26),
                    i == 2 ? "Dad’s" : (i + 1).ToString(), 20, FolioPaper);
            }

            doughSource = KitchenButton(shapingBoard, "Pick up dough", new Rect(145, 239, 294, 51), null);
            var sourceRect = (RectTransform)doughSource.transform;
            KitchenOvalShape(sourceRect, "Dough portion", new Rect(16, 9, 43, 33), Dough, DoughLight);
            nextPiece = KitchenText(sourceRect, "Portion label", new Rect(58, 0, 216, 51), "", 23, FolioPaper);
            draggedDough = KitchenOvalShape(kitchenCanvas, "Dragged dough", new Rect(0, 0, 62, 51), Dough, DoughLight).rectTransform;
            draggedDough.pivot = new Vector2(.5f, .5f);
            draggedDough.gameObject.SetActive(false);
            EventTrigger events = doughSource.gameObject.AddComponent<EventTrigger>();
            AddDoughEvent(events, EventTriggerType.BeginDrag, StartDoughDrag);
            AddDoughEvent(events, EventTriggerType.Drag, MoveDoughDrag);
            AddDoughEvent(events, EventTriggerType.EndDrag, DropDough);
        }

        static void AddDoughEvent(EventTrigger target, EventTriggerType kind, System.Action<PointerEventData> action)
        {
            var entry = new EventTrigger.Entry { eventID = kind };
            entry.callback.AddListener(data => action((PointerEventData)data));
            target.triggers.Add(entry);
        }

        void StartDoughDrag(PointerEventData pointer)
        {
            if (!shaping || !doughSource.interactable || pointer.button != PointerEventData.InputButton.Left) return;
            draggedDough.gameObject.SetActive(true);
            MoveDoughDrag(pointer);
        }

        void MoveDoughDrag(PointerEventData pointer)
        {
            if (draggedDough.gameObject.activeSelf &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(kitchenCanvas, pointer.position, pointer.pressEventCamera, out var local))
                draggedDough.anchoredPosition = local + new Vector2(Width * .5f, -Height * .5f);
        }

        void DropDough(PointerEventData pointer)
        {
            if (!draggedDough.gameObject.activeSelf) return;
            draggedDough.gameObject.SetActive(false);
            for (int i = 0; i < doughSlots.Length; i++)
            {
                if (!doughSlots[i].interactable ||
                    !RectTransformUtility.RectangleContainsScreenPoint((RectTransform)doughSlots[i].transform, pointer.position, pointer.pressEventCamera))
                    continue;
                ShapeCookie(i);
                break;
            }
        }

        void VerifyBakingControl(int slot = -1)
        {
            UpdateKitchenCanvas();
            var button = slot < 0 ? stirButton : doughSlots[slot];
            if (!button.gameObject.activeInHierarchy || !button.interactable)
                throw new System.InvalidOperationException("Expected an enabled kitchen baking control.");
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            if (slot == 1)
            {
                var source = (RectTransform)doughSource.transform;
                var destination = (RectTransform)button.transform;
                pointer.position = RectTransformUtility.WorldToScreenPoint(null, source.TransformPoint(source.rect.center));
                ExecuteEvents.Execute(source.gameObject, pointer, ExecuteEvents.beginDragHandler);
                pointer.position = RectTransformUtility.WorldToScreenPoint(null, destination.TransformPoint(destination.rect.center));
                ExecuteEvents.Execute(source.gameObject, pointer, ExecuteEvents.dragHandler);
                ExecuteEvents.Execute(source.gameObject, pointer, ExecuteEvents.endDragHandler);
            }
            else ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        RectTransform KitchenFrame(RectTransform parent, string name, Rect bounds, bool blocksInput)
        {
            var container = KitchenRect(parent, name, bounds);
            KitchenFill(container, "Folio shadow", new Rect(4, 5, bounds.width, bounds.height),
                new Color(.10f, .07f, .05f, .28f));
            var frame = KitchenFill(container, "Folio edge", new Rect(0, 0, bounds.width, bounds.height), FolioEdge);
            frame.raycastTarget = blocksInput;
            KitchenFill(frame.rectTransform, "Paper", new Rect(3, 3, bounds.width - 6, bounds.height - 6), FolioPaper);
            var innerLine = new Color(.58f, .40f, .23f, .42f);
            KitchenFill(frame.rectTransform, "Top border", new Rect(7, 7, bounds.width - 14, 1), innerLine);
            KitchenFill(frame.rectTransform, "Bottom border", new Rect(7, bounds.height - 8, bounds.width - 14, 1), innerLine);
            KitchenFill(frame.rectTransform, "Left border", new Rect(7, 7, 1, bounds.height - 14), innerLine);
            KitchenFill(frame.rectTransform, "Right border", new Rect(bounds.width - 8, 7, 1, bounds.height - 14), innerLine);
            return container;
        }

        static RectTransform KitchenRect(RectTransform parent, string name, Rect bounds)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(bounds.x, -bounds.y);
            rect.sizeDelta = bounds.size;
            return rect;
        }

        static Image KitchenFill(RectTransform parent, string name, Rect bounds, Color color)
        {
            var image = KitchenRect(parent, name, bounds).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        Text KitchenText(RectTransform parent, string name, Rect bounds, string value, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, bool bold = false)
        {
            var text = KitchenRect(parent, name, bounds).gameObject.AddComponent<Text>();
            text.font = hud.StoryTypeface;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = color;
            text.alignment = align;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        static Button KitchenButton(RectTransform parent, string name, Rect bounds, UnityEngine.Events.UnityAction action)
        {
            var image = KitchenFill(parent, name, bounds, FolioEdge);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.colors = new ColorBlock
            {
                normalColor = Color.white, highlightedColor = new Color(1, .90f, .75f), pressedColor = new Color(.84f, .73f, .61f),
                selectedColor = Color.white, disabledColor = new Color(.60f, .58f, .55f), colorMultiplier = 1, fadeDuration = .1f
            };
            if (action != null) button.onClick.AddListener(action);
            return button;
        }

        static KitchenOval KitchenOvalShape(RectTransform parent, string name, Rect bounds, Color edge, Color center, float ring = .18f)
        {
            var mark = KitchenRect(parent, name, bounds).gameObject.AddComponent<KitchenOval>();
            mark.raycastTarget = false;
            mark.Ring = ring;
            mark.SetColors(edge, center);
            return mark;
        }
    }

    internal sealed class KitchenOval : MaskableGraphic
    {
        const int Sides = 40;
        Color edge, center;
        public float Ring { get; set; }

        public void SetColors(Color rim, Color fill)
        {
            if (edge == rim && center == fill) return;
            edge = rim;
            center = fill;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect bounds = GetPixelAdjustedRect();
            Vector2 radius = bounds.size * .5f;
            mesh.AddVert(bounds.center, center, Vector2.zero);
            for (int i = 0; i < Sides; i++)
            {
                float angle = i * Mathf.PI * 2 / Sides;
                Vector2 offset = Vector2.Scale(radius, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
                int point = 1 + 3 * i;
                mesh.AddVert(bounds.center + offset * (1 - Ring), center, Vector2.zero);
                mesh.AddVert(bounds.center + offset, edge, Vector2.zero);
                mesh.AddVert(bounds.center + offset * 1.017f, new Color(edge.r, edge.g, edge.b, 0), Vector2.zero);
                int next = 1 + 3 * ((i + 1) % Sides);
                mesh.AddTriangle(0, point, next);
                mesh.AddTriangle(point, point + 1, next);
                mesh.AddTriangle(point + 1, next + 1, next);
                mesh.AddTriangle(point + 1, point + 2, next + 1);
                mesh.AddTriangle(point + 2, next + 2, next + 1);
            }
        }
    }
}

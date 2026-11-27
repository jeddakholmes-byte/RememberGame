using System;
using System.Collections.Generic;
using UnityEngine;

namespace RememberMe
{
    public sealed partial class KitchenPrototype
    {
        const string ChapterSaveKey = "RememberMe.Kitchen.Save.v1";
        string SaveKey => verifyingRoute ? ChapterSaveKey + ".QA" : ChapterSaveKey;
        [Serializable] sealed class ChapterSave
        {
            public Chapter chapter;
            public Room room;
            public Vector2 feet;
            public int ingredients, cards, baking, mixCount, shapedSlots, anchors, inspectedDoors, wrongDoors;
            public bool photoSeen, sugarClueFound, looped, largestServed, flourSetOut;
        }

        void PauseChapter(string message = null)
        {
            motor.CanMove = false;
            motor.SetInput(Vector2.zero, false, false);
            Offer(message ?? "Paused", new[] { "Resume", "Inventory", "Save Game", "Load Game", "Return to title" }, index => {
                if (index == 1) ShowInventory();
                else if (index == 2) { SaveChapter(); PauseChapter("Game saved."); }
                else if (index == 3) LoadChapter();
                else if (index == 4) UnityEngine.SceneManagement.SceneManager.LoadScene("RememberMeAct1");
            }, false);
        }

        Sprite InventorySprite(Texture2D texture, Rect region)
        {
            var sprite = Sprite.Create(texture, region, new Vector2(.5f, .5f), Ppu);
            roomPropSprites.Add(sprite);
            return sprite;
        }

        void ShowInventory()
        {
            var items = new List<(string Name, Sprite Art)>();
            items.Add(("Keepsake box", keepsake.sprite));
            for (int i = 0; i < IngredientGroups.Length; i++)
                if ((ingredients & IngredientGroups[i]) == IngredientGroups[i])
                    items.Add((IngredientGroupNames[i], InventorySprite(ingredientArtwork,
                        new Rect(ingredientArtwork.width * i / 3f, 0, ingredientArtwork.width / 3f, ingredientArtwork.height))));
            for (int i = 0; i < counterCards.Length; i++)
                if ((cards & (1 << i)) != 0) items.Add((CardNames[i], counterCards[i].sprite));
            if (sugarClueFound)
                items.Add(("Dad’s note", InventorySprite(kitchenArt, new Rect(565f / Width * kitchenArt.width,
                    (1 - 239f / Height) * kitchenArt.height, 70f / Width * kitchenArt.width, 51f / Height * kitchenArt.height))));
            if (chapter >= Chapter.FirstCollapse)
                items.Add(("Cookies & blue tray", InventorySprite(cardArt[2], new Rect(0, 0, cardArt[2].width, cardArt[2].height))));
            Offer("Inventory", new[] { "Back" }, _ => PauseChapter(), false);
            hud.Inventory(items, () => Choose(0));
        }

        void SaveChapter()
        {
            var save = new ChapterSave { chapter = chapter, room = currentRoom, feet = grace.Feet,
                ingredients = ingredients, cards = cards, baking = baking, mixCount = mixCount, shapedSlots = shapedSlots,
                anchors = anchors, inspectedDoors = inspectedDoors, wrongDoors = wrongDoors, photoSeen = photoSeen,
                sugarClueFound = sugarClueFound, looped = looped, largestServed = largestServed,
                flourSetOut = flourSetOut };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        void LoadChapter()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) { PauseChapter("No saved kitchen chapter yet."); return; }
            var save = JsonUtility.FromJson<ChapterSave>(PlayerPrefs.GetString(SaveKey));
            chapter = save.chapter;
            ingredients = save.ingredients; cards = save.cards; baking = save.baking;
            flourSetOut = save.flourSetOut || chapter > Chapter.Ingredients;
            mixCount = save.mixCount; shapedSlots = save.shapedSlots; shapedCount = CountFinds(shapedSlots);
            anchors = save.anchors; inspectedDoors = save.inspectedDoors; wrongDoors = save.wrongDoors;
            photoSeen = save.photoSeen; sugarClueFound = save.sugarClueFound;
            looped = save.looped; largestServed = save.largestServed;
            waterSpace = mixing = shaping = showingFreshCookies = false;
            CloseChoices();
            Enter(save.room, save.feet);
        }
    }
}

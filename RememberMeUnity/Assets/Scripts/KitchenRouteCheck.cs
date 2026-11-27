using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RememberMe
{
    public sealed partial class KitchenPrototype
    {
        bool verifyingRoute;
        bool nativeRoute;
        string captureDirectory;
        Vector2 routeInput;
        bool routeConfirm, routeCancel;

        void StartPlaythroughIfRequested()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out var page) &&
                (page.Query == "?remember-smoke=1" || page.Query == "?kitchen-check=1"))
                StartCoroutine(VerifyChapter());
#else
            string[] arguments = Environment.GetCommandLineArgs();
            nativeRoute = Array.IndexOf(arguments, "-kitchen-check") >= 0 || Array.IndexOf(arguments, "-remember-smoke") >= 0;
            int captureIndex = Array.IndexOf(arguments, "-kitchen-capture");
            if (nativeRoute && captureIndex >= 0 && captureIndex + 1 < arguments.Length)
            {
                captureDirectory = Path.GetFullPath(arguments[captureIndex + 1]);
                Directory.CreateDirectory(captureDirectory);
            }
            if (nativeRoute) StartCoroutine(VerifyChapter());
#endif
        }

        IEnumerator VerifyChapter()
        {
            verifyingRoute = true;
            while (bedroomArrival > 0) yield return null;
            if (lines == null || !lines[0].StartsWith("I'm in my room now."))
                throw new InvalidOperationException("Bedroom arrival dialogue did not play.");
            IEnumerator PressAction()
            {
                routeConfirm = true;
                yield return null;
                if (routeConfirm) throw new InvalidOperationException("The kitchen did not consume the action press.");
            }
            IEnumerator Read()
            {
                while (lines != null || WaitingForBeat) yield return PressAction();
            }
            IEnumerator Select(int index)
            {
                if (choices == null) throw new InvalidOperationException("Expected a decision in " + chapter);
                yield return null;
                hud.HoverForVerification(index);
                hud.VerifyFocus(index);
                yield return PressAction();
                yield return Read();
            }
            IEnumerator Visit(string label, Vector2 destination)
            {
                yield return WalkTo(destination);
                var target = Array.Find(targets, item => item.Label == label);
                if (target == null) throw new InvalidOperationException("Absent target " + label);
                yield return null;
                if (nearby != target) throw new InvalidOperationException("Cannot reach " + label + " from " + grace.Feet + ", found " + nearby?.Label);
                yield return PressAction();
                yield return Read();
            }
            IEnumerator CounterVisit(string label, int x) => Visit(label, new Vector2(x, 370));
            IEnumerator Doorway() => Visit("Kitchen doorway", new Vector2(576, 784));
            IEnumerator ExpectInactive(Vector2 destination)
            {
                yield return WalkTo(destination);
                yield return PressAction();
                if (nearby != null || lines != null || choices != null)
                    throw new InvalidOperationException($"Unexpected interaction at {grace.Feet}: {nearby?.Label}.");
            }
            void Expect(Chapter expected)
            {
                if (chapter != expected) throw new InvalidOperationException("Expected "+expected+", got "+chapter);
            }
            yield return Read();
            if (bedroomChime <= 0) throw new InvalidOperationException("The keepsake did not sound after the arrival dialogue.");
            yield return Capture("01-bedroom");
            foreach (var foot in new[] { new Vector2(960, 345), new Vector2(800, 360), new Vector2(170, 345) })
                if (!Physics2D.OverlapPoint(World(foot), 1 << 8))
                    throw new InvalidOperationException("Bedroom furniture is missing its footprint at " + foot);
            yield return Visit("Open the keepsake box", new Vector2(450, 497));
            yield return Select(1); yield return Select(0); Expect(Chapter.Ingredients);
            if (memoryFade != 0 || Vector2.Distance(grace.Feet, new Vector2(585,455)) > 5)
                throw new InvalidOperationException("Kitchen reunion did not finish its fade and approach.");
            if (flourSetOut || !Objective().Contains("left cupboard"))
                throw new InvalidOperationException("The first kitchen task must introduce the flour cupboard.");
            yield return Doorway();
            if (currentRoom != Room.Kitchen || chapter != Chapter.Ingredients)
                throw new InvalidOperationException("Early kitchen doorway must explain why Grace is staying.");
            yield return ExpectInactive(new Vector2(353, 560));
            yield return CounterVisit("Family photograph",518);
            if (sugarClueFound || showingNote) throw new InvalidOperationException("Dad's note appeared at the family photo.");
            yield return CounterVisit("Music box",975);
            yield return Visit("Talk to Grandma", grandma.Feet + new Vector2(0,34)); Expect(Chapter.Ingredients);
            yield return Capture("02-ingredients");
            yield return Visit("Flour cupboard", new Vector2(205, 390));
            if ((ingredients & FlourBit) == 0) throw new InvalidOperationException("A valid flour approach collected nothing.");
            if (!Objective().Contains("recipe book")) throw new InvalidOperationException("Collected flour has no delivery goal.");
            PauseChapter();
            yield return Select(1);
            hud.VerifyInventory(2);
            yield return Capture("02a-inventory");
            yield return Select(0);
            yield return Select(2);
            yield return Select(0);
            ingredients = 0;
            LoadChapter();
            if (ingredients != FlourBit) throw new InvalidOperationException("Kitchen save/load lost inventory.");
            PlayerPrefs.DeleteKey(SaveKey);
            yield return CounterVisit("White jars",330); yield return Select(1);
            yield return CounterVisit("Recipe book",585); yield return Select(1); Expect(Chapter.Ingredients);
            yield return null;
            if (!flourSetOut || !flourOnCounter.enabled)
                throw new InvalidOperationException("Setting out flour did not acknowledge delivery visibly.");
            SaveChapter();
            flourSetOut = false;
            LoadChapter();
            if (!flourSetOut) throw new InvalidOperationException("Save/load lost the completed flour introduction.");
            // An explorer may collect everything before their first delivery.
            ingredients = AllIngredients;
            flourSetOut = false;
            Recipe();
            yield return Select(1); Expect(Chapter.Ingredients);
            if (!flourSetOut || ingredients != AllIngredients)
                throw new InvalidOperationException("Early collection lost ingredients during the flour introduction.");
            LoadChapter();
            PlayerPrefs.DeleteKey(SaveKey);
            yield return CounterVisit("Recipe book",585);
            if (choices == null) throw new InvalidOperationException("The recipe menu did not open.");
            hud.HoverForVerification(0);
            yield return PressAction();
            yield return Capture("03-dads-note");
            yield return Read();
            if (!sugarClueFound) throw new InvalidOperationException("Dad's note did not stay available as a clue.");
            yield return CounterVisit("White jars",330); yield return Select(0);
            yield return Visit("Refrigerator", new Vector2(790, 391));
            if ((ingredients & (ButterBit | EggsBit)) != (ButterBit | EggsBit))
                throw new InvalidOperationException("The refrigerator did not yield butter and eggs.");
            yield return CounterVisit("Recipe book",585); yield return Select(1); Expect(Chapter.Cards);
            yield return null;
            foreach (var card in counterCards)
                if (!card.enabled) throw new InvalidOperationException("A missing recipe card is invisible in the kitchen.");
            yield return Capture("03a-visible-picture-cards");
            yield return CounterVisit("Refrigerator",761);
            yield return null;
            if (cards != 4 || counterCards[2].enabled || !counterCards[0].enabled || !counterCards[1].enabled)
                throw new InvalidOperationException("Recipe cards must support discovery in any order and hide once collected.");
            yield return CounterVisit("Baking counter",460);
            yield return CounterVisit("Mixing bowl",677);
            yield return CounterVisit("Mixing bowl",677);
            if (cards != AllCards) throw new InvalidOperationException("Repeated cards changed recipe progress.");
            yield return CounterVisit("Recipe book",585);
            yield return Select(0);
            yield return Capture("04-recipe");
            yield return Select(1);
            if (recipeOrder.Count != 1) throw new InvalidOperationException("Recipe placement failed.");
            yield return Select(remainingCardIds.Length);
            if (recipeOrder.Count != 0) throw new InvalidOperationException("Undo did not return the recipe card.");
            yield return Select(1);
            yield return null;
            RemoveRecipeCardsFrom(0);
            yield return null;
            if (recipeOrder.Count != 0) throw new InvalidOperationException("Clicking a placed card did not undo it.");
            yield return Select(0); yield return Select(0);yield return Select(0); Expect(Chapter.Cards);
            yield return CounterVisit("Recipe book",585);
            yield return Select(0);
            yield return Select(1); yield return Select(1);yield return Select(0); Expect(Chapter.Baking);
            yield return CounterVisit("Baking counter",460);
            if (baking != 0) throw new InvalidOperationException("Wrong station advanced baking.");
            yield return CounterVisit("Mixing bowl",677);
            yield return Capture("05-mixing");
            yield return PressAction();
            yield return null;
            VerifyBakingControl();
            yield return null;
            routeCancel = true;
            yield return null;
            if (mixing || mixCount != 2) throw new InvalidOperationException("Closing the mixing board lost its progress.");
            yield return Visit("Mixing bowl", new Vector2(698, 390));
            for (int i = 0; i < 2; i++) { yield return null; VerifyBakingControl(); }
            yield return Read();
            if (baking != 1) throw new InvalidOperationException("Mixing did not produce dough.");
            yield return CounterVisit("Baking counter",460);
            yield return Capture("06-shaping");
            if (CanPlaceDough(2)) throw new InvalidOperationException("The largest slot became available early.");
            yield return null;
            VerifyBakingControl(1);
            if (shapedSlots != 2) throw new InvalidOperationException("The chosen tray slot was not filled.");
            yield return PressAction();
            yield return Read();
            if (shapedSlots != 3 || !CanPlaceDough(2)) throw new InvalidOperationException("The largest cookie was not offered after both regular ones.");
            yield return null;
            VerifyBakingControl(2);
            yield return Read();
            if (shapedSlots != 7) throw new InvalidOperationException("The tray did not keep all three placements.");
            yield return CounterVisit("Baking counter",460); yield return Read();
            Expect(Chapter.Tray);
            yield return CounterVisit("Blue serving tray",895);
            Expect(Chapter.FirstCollapse);
            PauseChapter();
            yield return Select(1);
            hud.VerifyInventory(9);
            yield return Capture("06a-complete-inventory");
            if (cookieTray.enabled) throw new InvalidOperationException("The collected tray is floating on Grace.");
            yield return Select(0);
            yield return Select(0);
            yield return ExpectInactive(new Vector2(576, 726));
            yield return Doorway();
            if (currentRoom != Room.Kitchen) throw new InvalidOperationException("First doorway did not loop to the kitchen.");
            yield return Doorway();
            yield return Visit("Inspect the left door",new Vector2(317,369)); yield return Select(1);
            if (currentRoom != Room.Hall) throw new InvalidOperationException("Door inspection moved Grace without opening it.");
            yield return Visit("Inspect the left door",new Vector2(317,369)); yield return Select(0);
            yield return Doorway();
            yield return Visit("Inspect the right door",new Vector2(836,369));
            hud.HoverForVerification(0);
            yield return PressAction();
            yield return Capture("07a-water-door-dialogue");
            if (!waterSpace || !hud.ShowsDialogue("Grace", "water"))
                throw new InvalidOperationException("The water memory must retain visible, clickable dialogue.");
            hud.ClickDialogueForVerification();
            yield return null;
            yield return Read();
            if (currentRoom != Room.Hall || waterSpace) throw new InvalidOperationException("Water door did not return Grace to the hall.");
            yield return Capture("07b-returned-from-water");
            yield return Visit("Inspect the middle door",new Vector2(576,369));
            yield return Capture("07-curtain-clue");
            yield return Select(0); Expect(Chapter.ReturnForGrandma);
            if (currentRoom != Room.Parlor)
                throw new InvalidOperationException("Finding the living room must leave Grace there until the player returns.");
            PauseChapter();
            yield return Select(2);
            yield return Select(0);
            LoadChapter();
            if (currentRoom != Room.Parlor || Array.Find(targets, target => target.Label == "Back to the hallway") == null)
                throw new InvalidOperationException("The manual return exit must survive save/load.");
            yield return Visit("Back to the hallway", new Vector2(576, 783));
            yield return ExpectInactive(new Vector2(576, 726));
            yield return Visit("Back to the kitchen", new Vector2(596, 783));
            yield return CounterVisit("Music box",975); yield return CounterVisit("Music box",975);
            if (anchors != 2) throw new InvalidOperationException("Repeated memory counted twice.");
            yield return CounterVisit("Family photograph",518);
            if (anchors != 3) throw new InvalidOperationException("Missing memory.");
            yield return Visit("Talk to Grandma",grandma.Feet+new Vector2(0,34));Expect(Chapter.Escort);
            yield return Doorway();
            yield return WaitForEscort(Room.Hall);
            yield return WalkTo(new Vector2(576,369));
            yield return Capture("08-escort");
            yield return Visit("Inspect the middle door",new Vector2(576,369)); yield return Select(0);
            yield return WaitForEscort(Room.Parlor);
            Expect(Chapter.Ending);
            float deadline = Time.realtimeSinceStartup + 16;
            bool capturedPlate = false;
            var graceWalkFrames = new HashSet<Sprite>();
            var grandmaWalkFrames = new HashSet<Sprite>();
            while (chapter != Chapter.Finished && Time.realtimeSinceStartup < deadline)
            {
                if (serving)
                {
                    graceWalkFrames.Add(grace.Renderer.sprite);
                    grandmaWalkFrames.Add(grandma.Renderer.sprite);
                }
                if (largestServed && !capturedPlate)
                {
                    capturedPlate = true;
                    yield return Capture("09-cookie-by-the-window");
                }
                yield return Read();
                yield return null;
            }
            if (!largestServed) throw new InvalidOperationException("Grandma never set out Dad's cookie.");
            if (graceWalkFrames.Count < 2 || grandmaWalkFrames.Count < 2)
                throw new InvalidOperationException("Scripted room entry must animate both Grace and Grandma walking.");
            yield return Read(); Expect(Chapter.Finished);
            verifyingRoute = false;
            Debug.Log("KITCHEN_ROUTE_PASS: walked every puzzle, recovered from wrong choices and doors, escorted Grandma and reached the coat.");
            if (nativeRoute && !Application.isEditor) Application.Quit();
        }

        IEnumerator Capture(string name)
        {
            if (string.IsNullOrEmpty(captureDirectory)) yield break;
            yield return new WaitForSeconds(.5f);
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            hud.PrepareCapture(view);
            yield return null;
            try
            {
                RenderPipeline.SubmitRenderRequest(view, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();
                if (image.GetPixel(target.width / 2, target.height / 2).maxColorComponent < .01f)
                    throw new InvalidOperationException("Empty Kitchen capture: " + name);
                File.WriteAllBytes(Path.Combine(captureDirectory, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                hud.FinishCapture();
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Destroy(image);
            }
        }

        IEnumerator WaitForEscort(Room destination)
        {
            float deadline = Time.realtimeSinceStartup + 9f;
            while (currentRoom != destination && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (currentRoom != destination)
                throw new InvalidOperationException($"Escort never reached {destination}. Grace: {grace.Feet}, Grandma: {grandma.Feet}, " +
                    $"gap: {Vector2.Distance(grace.Feet, grandma.Feet):F1}, waypoints: {trail.Count}, waiting: {whenGrandmaArrives != null}.");
        }

        IEnumerator WalkTo(Vector2 goal)
        {
            const float limit = 12f;
            float deadline = Time.realtimeSinceStartup+limit;
            while (Vector2.Distance(grace.Feet,goal)>5f)
            {
                if (Time.realtimeSinceStartup>deadline)throw new InvalidOperationException("Route blocked in "+chapter+" at "+grace.Feet+" going to "+goal);
                Vector2 waypoint = goal;
                if (!ClearSegment(grace.Feet,goal)) waypoint=FirstWaypoint(grace.Feet,goal);
                float end=Time.time+.12f;
                do {
                    var delta=waypoint-grace.Feet;
                    routeInput=new Vector2(delta.x,-delta.y).normalized*Mathf.Clamp01(delta.magnitude/16f);
                    yield return null;
                } while (Time.time<end && Vector2.Distance(grace.Feet,waypoint)>5f);
            }
            routeInput=Vector2.zero;
            yield return new WaitForFixedUpdate();yield return null;
        }

        static bool ClearSegment(Vector2 from,Vector2 to)
        {
            var delta=World(to-from);
            return !Physics2D.CircleCast(World(from),.118f,delta.normalized,delta.magnitude,(1<<8)|(1<<9));
        }

        static Vector2 FirstWaypoint(Vector2 start,Vector2 goal)
        {
            const int step=20;
            var queue=new Queue<Vector2Int>(); var visited=new HashSet<Vector2Int>();
            var first=new Dictionary<Vector2Int,Vector2>();
            var origin=Vector2Int.RoundToInt(start/step);
            queue.Enqueue(origin);visited.Add(origin);
            while(queue.Count>0)
            {
                var node=queue.Dequeue();Vector2 point=node==origin?start:(Vector2)node*step;
                if(node!=origin && ClearSegment(point,goal))return first[node];
                for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)
                {
                    var next=node+new Vector2Int(x,y);Vector2 at=(Vector2)next*step;
                    if(at.x<110||at.x>1040||at.y<180||at.y>800||visited.Contains(next)||!ClearSegment(point,at))continue;
                    visited.Add(next);queue.Enqueue(next);
                    first[next]=node==origin?at:first[node];
                }
            }
            return start;
        }
    }
}

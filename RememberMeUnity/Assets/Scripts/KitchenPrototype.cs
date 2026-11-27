using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Expression = RememberMe.Act1Game.Expression;

namespace RememberMe
{
    public sealed partial class KitchenPrototype : MonoBehaviour
    {
        const float Width = 1152f;
        const float Height = 864f;
        const float Ppu = 134f;

        enum Room { Kitchen, Hall, Parlor, Bedroom }
        enum Chapter { Bedroom, MeetGrandma, Ingredients, Cards, Baking, Tray, FirstCollapse, FindDoor, ReturnForGrandma, Escort, Ending, Finished }
        enum Door { Clock, Curtains, Water }
        enum Reach { Fixture, Object, Character, Threshold }
        const int FlourBit = 1, ButterBit = 2, EggsBit = 4, SugarBit = 8;
        const int AllIngredients = FlourBit | ButterBit | EggsBit | SugarBit;
        const int AllCards = 7;
        const int MixSteps = 4, DoughPieces = 3;

        [Serializable] sealed class RoomCatalog { public RoomArt[] rooms; }
        [Serializable] sealed class RoomArt { public string art; public float top; public Rect[] furniture; }

        sealed class Actor
        {
            public ActorView View;
            public SpriteRenderer Renderer => View.Renderer;
            public Vector2 Feet;

        }

        sealed class Target
        {
            public readonly Vector2 Feet;
            public readonly string Label;
            public readonly Action Use;
            public readonly Reach Kind;
            public readonly Func<bool> Available;
            public Target(float x, float y, string label, Action use, Reach kind = Reach.Fixture, Func<bool> available = null)
            {
                Feet = new Vector2(x, y);
                Label = label;
                Use = use;
                Kind = kind;
                Available = available;
            }
        }

        RoomCatalog catalog;
        Room currentRoom;
        Chapter chapter;
        Camera view;
        SpriteRenderer backdrop;
        Material roomMaterial;
        [SerializeField] Sprite[] graceIdle, graceWalk, graceDiagonal, graceRun, graceCrouch;
        [SerializeField] Sprite[] grandmaIdle, grandmaWalk;
        GraceMotor motor;
        GameObject obstacles;
        Collider2D grandmaFeet;
        Collider2D keepsakeCollider;
        Rect doorwayPassage;
        AudioSource footsteps;
        AudioClip[] woodSteps;
        int lastStep = -1;
        Actor grace;
        Actor grandma;
        AudioSource music;
        AudioSource sound;
        Sprite[] graceFaces, grandmaFaces;
        Expression mood;
        Texture2D photo, livingRoomPhoto;
        readonly Texture2D[] cardArt = new Texture2D[3];
        readonly SpriteRenderer[] counterCards = new SpriteRenderer[3];
        int visibleCard = -1;
        bool showingFamilyPhoto, waterSpace;
        Door? shownDoor;
        [SerializeField] Font typeface;
        [SerializeField] Sprite recipeTable, shadow;
        Act1HUD hud;
        SpriteRenderer table, tableShadow, bed, keepsake, familyFrame;
        float grandmaPace;
        float grandmaStride;
        Vector2 grandmaHeading = Vector2.down;
        Action whenGrandmaArrives;
        Target grandmaTarget;
        int choiceFrame;
        Vector2 cameraVelocity;
        readonly Queue<Vector2> trail = new Queue<Vector2>();
        Vector2 lastTrailPoint;

        Target[] targets;
        Target nearby;
        string speaker;
        string[] lines;
        Action afterDialogue;
        int line;
        float beatTime;
        Action afterBeat;
        Action<float> duringBeat;
        float beatDuration;
        bool flourSetOut;
        float memoryFade;
        bool WaitingForBeat => afterBeat != null;
        int anchors; // Objects Grace has recognised on the return trip.
        bool showingFreshCookies, largestServed, sugarClueFound;
        float trayFocus;
        bool looped;
        bool showPhoto, showingNote;
        float decay, handCue, clockTime, doorFlicker;
        float endingTime, transition;
        int wrongDoors, inspectedDoors, mixCount, shapedCount, shapedSlots;
        float bedroomArrival, bedroomChime;
        bool mixing, shaping, serving;

        void Awake()
        {
            catalog = JsonUtility.FromJson<RoomCatalog>(Resources.Load<TextAsset>("KitchenChapter/Data/Rooms").text);
            view = new GameObject("Story camera").AddComponent<Camera>();
            view.gameObject.AddComponent<AudioListener>();
            view.orthographic = true;
            view.orthographicSize = Height / Ppu / 2f;
            view.transform.position = new Vector3(Width / Ppu / 2f, -Height / Ppu / 2f, -10f);
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(.041f, .045f, .058f);
            view.rect = new Rect(0, 0, 1, 1);
            backdrop = new GameObject("Room illustration").AddComponent<SpriteRenderer>();
            roomMaterial = new Material(Resources.Load<Shader>("KitchenChapter/Art/MemoryWash"));
            backdrop.sharedMaterial = roomMaterial;
            backdrop.transform.position = new Vector3(0, -Height / Ppu, 0);
            hud = gameObject.AddComponent<Act1HUD>();
            hud.BrowserTypeface = typeface;
            hud.Initialize();
            table = Prop("Family recipe table", recipeTable, new Vector2(355,534), 193,121);
            tableShadow = Prop("Table contact shadow",shadow,new Vector2(355,545),206,34);
            tableShadow.sortingOrder = 1;
            graceFaces = Act1Game.LoadGraceExpressions();
            grandmaFaces = Act1Game.LoadGrandmaExpressions();
            grace = CreateActor("Grace", graceIdle, graceWalk, .88f);
            grace.View.WalkDiagonal = graceDiagonal;
            grace.View.Run = graceRun;
            grace.View.Crouch = graceCrouch;
            motor = grace.View.gameObject.AddComponent<GraceMotor>();
            motor.View = grace.View;
            motor.Body = motor.GetComponent<Rigidbody2D>();
            motor.Body.bodyType = RigidbodyType2D.Kinematic;
            motor.Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            motor.Body.gravityScale = 0;
            motor.Feet = motor.GetComponent<CircleCollider2D>();
            motor.Feet.radius = .115f / motor.transform.localScale.x;
            motor.Feet.offset = Vector2.zero;
            grandma = CreateActor("Grandma", grandmaIdle, grandmaWalk, 1.03f);
            var figure = new GameObject("Grandma figure");
            figure.transform.SetParent(grandma.View.transform,false);
            figure.AddComponent<GrandmaFigure>().Bind(grandma.View);
            grandma.View.gameObject.layer = 9;
            var grandmaCollider = grandma.View.gameObject.AddComponent<CircleCollider2D>();
            grandmaCollider.radius = .12f / grandma.View.transform.localScale.x;
            grandmaCollider.offset = Vector2.zero;
            grandmaFeet = grandmaCollider;
            var grandmaBody = grandma.View.gameObject.AddComponent<Rigidbody2D>();
            grandmaBody.bodyType = RigidbodyType2D.Kinematic;
            grandmaBody.gravityScale = 0;
            var feetShade = Prop("Grandma contact shadow", shadow, Vector2.zero, 47,13);
            feetShade.transform.SetParent(grandma.View.transform, true);
            feetShade.transform.localPosition = new Vector3(0,-.015f,0);
            feetShade.sortingOrder = 2;
            grandmaShadow = feetShade;
            footsteps = gameObject.AddComponent<AudioSource>();
            footsteps.volume = .55f;
            woodSteps = Resources.LoadAll<AudioClip>("Footsteps/Wood");
            motor.Footfall += PlayWoodStep;
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.volume = .32f;
            sound = gameObject.AddComponent<AudioSource>();
            bed = RoomProp("Grace’s bed", "Bed", new Vector2(300, 489), 177, 242);
            keepsake = RoomProp("Keepsake box", "Parcel", new Vector2(411, 484), 59, 46);
            familyFrame = RoomProp("Young Dad beside the blue curtains", "LivingRoomPhoto", new Vector2(530, 154), 77, 46);
            photo = LoadTexture("Photograph");
            livingRoomPhoto = LoadTexture("LivingRoomPhoto");
            for (int i = 0; i < cardArt.Length; i++) cardArt[i] = LoadTexture("Card" + i);
            counterCards[0] = RoomProp("Picture card beside the bowl", "Card0", new Vector2(677, 324), 40, 40);
            counterCards[1] = RoomProp("Picture card on the baking counter", "Card1", new Vector2(460, 324), 40, 40);
            counterCards[2] = RoomProp("Picture card on the refrigerator", "Card2", new Vector2(775, 313), 40, 40);
            kitchenArt = LoadTexture("Kitchen");
            cookieTray = new GameObject("Grandma’s enamel tray").AddComponent<SpriteRenderer>();
            enamel = new Material(Resources.Load<Shader>("KitchenChapter/Art/EnamelTray"));
            var white = Texture2D.whiteTexture;
            trayQuad = Sprite.Create(white, new Rect(0,0,white.width,white.height),new Vector2(.5f,.5f),white.width,0,SpriteMeshType.FullRect);
            cookieTray.sprite = trayQuad;
            cookieTray.sharedMaterial = enamel;
            platedCookie = new GameObject("Dad’s cookie by the window").AddComponent<SpriteRenderer>();
            platedCookie.sprite = trayQuad;
            plateMaterial = new Material(enamel);
            plateMaterial.SetFloat("_SingleCookie", 1);
            platedCookie.sharedMaterial = plateMaterial;
            platedCookie.transform.position = World(new Vector2(754, 291));
            platedCookie.transform.localScale = new Vector3(.18f, .13f, 1);
            platedCookie.sortingOrder = 4;
            Restart();
            StartPlaythroughIfRequested();
        }

        static Texture2D LoadTexture(string name)
        {
            var texture = Resources.Load<Texture2D>("KitchenChapter/Art/" + name);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Actor CreateActor(string name, Sprite[] idle, Sprite[] walk, float height)
        {
            var obj = new GameObject(name);
            var actor = obj.AddComponent<ActorView>();
            actor.Renderer = obj.AddComponent<SpriteRenderer>();
            actor.Idle = idle;
            actor.Walk = walk;
            actor.transform.localScale *= height / idle[0].bounds.size.y;
            actor.Look(Vector2.down);
            return new Actor {View = actor};
        }

        static Vector2 World(Vector2 feet) => new Vector2(feet.x, -feet.y) / Ppu;

        void PlayWoodStep()
        {
            if (woodSteps.Length == 0 || (!motor.CanMove && !serving && chapter != Chapter.Ending)) return;
            int index = lastStep < 0 ? UnityEngine.Random.Range(0, woodSteps.Length) :
                (lastStep + 1 + UnityEngine.Random.Range(0, Mathf.Max(1, woodSteps.Length - 1))) % woodSteps.Length;
            lastStep = index;
            footsteps.pitch = UnityEngine.Random.Range(.94f, 1.06f);
            float level = motor.Crouched ? .23f : motor.Running ? .58f : .42f;
            footsteps.PlayOneShot(woodSteps[index], level * UnityEngine.Random.Range(.87f, 1.13f));
        }

        SpriteRenderer grandmaShadow;
        SpriteRenderer cookieTray, platedCookie;
        Material enamel, plateMaterial;
        Sprite trayQuad;
        Texture2D kitchenArt;
        bool inspectJars, sortingCards, viewRecipe;
        int[] remainingCardIds = Array.Empty<int>();
        readonly List<int> recipeOrder = new List<int>(3);
        readonly List<Sprite> roomPropSprites = new List<Sprite>();
        int exitWaypoint;
        float musicFade;
        AudioClip queuedMusic;
        readonly Vector2[] coatRoute = {new Vector2(508,503), new Vector2(418,380), new Vector2(371,244), new Vector2(319,246)};

        void OnDestroy()
        {
            if (trayQuad) Destroy(trayQuad);
            if (enamel) Destroy(enamel);
            if (plateMaterial) Destroy(plateMaterial);
            if (roomMaterial) Destroy(roomMaterial);
            if (backdrop && backdrop.sprite) Destroy(backdrop.sprite);
            if (circle) Destroy(circle);
            foreach (var sprite in roomPropSprites) Destroy(sprite);
        }


        SpriteRenderer Prop(string name, Sprite sprite, Vector2 feet, float width, float height)
        {
            var prop = new GameObject(name).AddComponent<SpriteRenderer>();
            prop.sprite = sprite;
            prop.transform.position = World(feet);
            prop.transform.localScale = new Vector3(width / (Ppu * sprite.bounds.size.x), height / (Ppu * sprite.bounds.size.y),1);
            prop.sortingOrder = ActorView.Depth(prop.transform.position.y);
            return prop;
        }

        SpriteRenderer RoomProp(string name, string art, Vector2 feet, float width, float height)
        {
            var texture = LoadTexture(art);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, 0), Ppu, 0, SpriteMeshType.FullRect);
            roomPropSprites.Add(sprite);
            return Prop(name, sprite, feet, width, height);
        }

        void Restart()
        {
            afterBeat = null;
            duringBeat = null;
            flourSetOut = false;
            memoryFade = 0;
            hud.Fade(0);
            beatTime = 0;
            chapter = Chapter.Bedroom;
            looped = showingFreshCookies = largestServed = sugarClueFound = false;
            trayFocus = handCue = clockTime = doorFlicker = 0;
            wrongDoors = anchors = ingredients = cards = baking = errors = inspectedDoors = mixCount = shapedCount = shapedSlots = 0;
            photoSeen = showPhoto = showingNote = showingFamilyPhoto = false;
            endingTime = decay = 0;
            exitWaypoint = 0;
            waterSpace = inspectJars = sortingCards = viewRecipe = mixing = shaping = serving = false;
            shownDoor = null;
            whenGrandmaArrives = null;
            visibleCard = -1;
            recipeOrder.Clear();
            choices = null;
            PlayMusic("Rain");
            Enter(Room.Bedroom);
            bedroomArrival = .6f;
            bedroomChime = 0;
            hud.Fade(1f);
        }

        void Enter(Room room, Vector2? entry = null)
        {
            currentRoom = room;
            roomMaterial.SetFloat("_Room", (int)room);
            transition = .45f;
            var art = LoadTexture(catalog.rooms[(int)room].art);
            if (backdrop.sprite != null) Destroy(backdrop.sprite);
            backdrop.sprite = Sprite.Create(art, new Rect(0, 0, art.width, art.height),
                Vector2.zero, Ppu, 0, SpriteMeshType.FullRect);
            backdrop.transform.localScale = new Vector3(Width / art.width, Height / art.height, 1f);
            grace.Feet = entry ?? (room == Room.Kitchen ? new Vector2(585, 587) : new Vector2(576, 627));
            grandma.Feet = room == Room.Kitchen
                ? chapter == Chapter.Escort ? new Vector2(585, 627) : new Vector2(625, 397)
                : room == Room.Hall ? new Vector2(576, 666) : new Vector2(542, 607);
            motor.Place(World(grace.Feet));
            motor.View.Look(Vector2.up);
            grandma.View.transform.position = World(grandma.Feet);
            grandmaHeading = Vector2.down;
            grandma.View.Look(grandmaHeading);
            grandmaPace = 0;
            grandmaFeet.enabled = room == Room.Kitchen && chapter != Chapter.Escort;
            table.enabled = tableShadow.enabled = familyFrame.enabled = room == Room.Kitchen;
            bed.enabled = keepsake.enabled = room == Room.Bedroom;
            cameraVelocity = Vector2.zero;
            BuildObstacles();
            grandma.Renderer.enabled = (room == Room.Parlor && chapter >= Chapter.Ending) ||
                room == Room.Kitchen || (room == Room.Hall && chapter == Chapter.Escort);
            grandmaShadow.enabled = grandma.Renderer.enabled;
            trail.Clear();
            lastTrailPoint = grace.Feet;
            whenGrandmaArrives = null;
            FitView(true);
            targets = room == Room.Kitchen ? new[]
            {
                new Target(895, 339, "Blue serving tray", TakeTray),
                new Target(177, 339, "Flour cupboard", Flour, available: () => chapter >= Chapter.Ingredients),
                new Target(330, 339, "White jars", Jars, available: () => chapter >= Chapter.Ingredients),
                new Target(761, 339, "Refrigerator", Fridge, available: () => chapter >= Chapter.Ingredients),
                new Target(585, 339, "Recipe book", Recipe),
                new Target(677, 339, "Mixing bowl", Bowl),
                new Target(460, 339, "Baking counter", Counter),
                new Target(518, 339, "Family photograph", FamilyPhoto),
                new Target(975, 339, "Music box", MusicBox),
                grandmaTarget = new Target(625, 397, "Talk to Grandma", HelpGrandma, Reach.Character),
                new Target(576, 781, "Kitchen doorway", LeaveKitchen, Reach.Threshold)
            } : room == Room.Hall ? new[]
            {
                new Target(317, 343, "Inspect the left door", () => InspectDoor(Door.Clock)),
                new Target(576, 343, "Inspect the middle door", () => InspectDoor(Door.Curtains)),
                new Target(836, 343, "Inspect the right door", () => InspectDoor(Door.Water)),
                new Target(576, 781, "Back to the kitchen", () => Enter(Room.Kitchen), Reach.Threshold)
            } : room == Room.Bedroom ? new[] {new Target(411, 487, "Open the keepsake box", OpenBox, Reach.Object)} :
                room == Room.Parlor && chapter == Chapter.ReturnForGrandma
                ? new[] {new Target(576, 781, "Back to the hallway", () => Enter(Room.Hall, new Vector2(576, 410)), Reach.Threshold)}
                : Array.Empty<Target>();
            nearby = null;

        }

        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, .05f);
            FitView();
            UpdateMusic(dt);
            if (bedroomArrival > 0)
            {
                bedroomArrival = Mathf.Max(0, bedroomArrival - dt);
                motor.CanMove = false;
                if (bedroomArrival == 0)
                {
                    hud.Fade(0);
                    transition = .45f;
                    Say("Grace", Expression.Quiet, new[] {"I'm in my room now. Mom left Grandma's box beside my bed.",
                        "I keep thinking about the photograph inside."}, () => {
                        bedroomChime = 2.4f;
                        Play("RM_Chime", .26f);
                    });
                }
                return;
            }
            bedroomChime = Mathf.Max(0, bedroomChime - dt);
            transition = Mathf.Max(0, transition - dt);
            handCue = Mathf.Max(0, handCue - dt);
            doorFlicker = Mathf.Max(0, doorFlicker - dt);
            if (chapter < Chapter.Cards) clockTime += dt;
            UpdateRoomMood(dt);
            bool cancel = Input.GetKeyDown(KeyCode.Escape) || routeCancel;
            bool simulatedConfirm = routeConfirm;
            bool confirm = StoryInput.ConfirmPressed || routeConfirm;
            routeCancel = routeConfirm = false;
            if (WaitingForBeat)
            {
                motor.CanMove = false;
                motor.SetInput(Vector2.zero, false, false);
                // Discard clicks made during silence instead of carrying them into the next line.
                hud.TakeContinue();
                beatTime = Mathf.Max(0, beatTime - dt);
                duringBeat?.Invoke(1f - beatTime / beatDuration);
                if (beatTime == 0)
                {
                    var next = afterBeat;
                    afterBeat = null;
                    duringBeat = null;
                    choiceFrame = Time.frameCount;
                    next();
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.I) && choices == null && lines == null && !mixing && !shaping)
            {
                ShowInventory();
                return;
            }
            if (cancel && lines == null)
            {
                CancelInteraction();
                return;
            }
            if (Input.GetKeyDown(KeyCode.R) && chapter == Chapter.Finished)
            {
                Restart();
                return;
            }

            motor.CanMove = choices == null && lines == null && !mixing && !shaping &&
                whenGrandmaArrives == null && chapter < Chapter.Ending;
            if (!motor.CanMove) motor.SetInput(Vector2.zero, false, false);
            if (choices != null)
            {
                if (sortingCards && Input.GetKeyDown(KeyCode.Backspace)) { UndoRecipeCard(); return; }
                for (int n = 0; n < choices.Length; n++)
                    if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + n)) ||
                        Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad1 + n))) { Choose(n); return; }
                hud.HandleMenuInput(confirm);
                return;
            }
            if (lines != null)
            {
                nearby = null;
                if ((StoryInput.Consume(simulatedConfirm) || hud.TakeContinue()) && choiceFrame != Time.frameCount)
                {
                    choiceFrame = Time.frameCount;
                    Advance();
                }
                grandma.View.Pose(Vector2.zero, false, false, 0);
            }
            else if (mixing || shaping)
            {
                if (StoryInput.Consume(confirm) && choiceFrame != Time.frameCount)
                {
                    if (mixing) MixDough();
                    else ShapeCookie();
                }
            }
            else if (whenGrandmaArrives != null)
            {
                FollowGrace(dt);
                if (Vector2.Distance(grace.Feet, grandma.Feet) <= 70f)
                {
                    var arrive = whenGrandmaArrives;
                    whenGrandmaArrives = null;
                    arrive();
                }
            }
            else if (chapter == Chapter.Ending && currentRoom == Room.Parlor) AdvanceEnding(dt);
            else if (chapter != Chapter.Finished)
            {
                MoveGrace();
                FindTarget();
                if (StoryInput.Consume(simulatedConfirm) || hud.TakeInteraction()) ActivateNearby();
                if (chapter == Chapter.Escort && lines == null && choices == null) FollowGrace(dt);
            }
        }

        void CancelInteraction()
        {
            choiceFrame = Time.frameCount;
            if (choices != null) CloseChoices();
            else if (mixing || shaping) mixing = shaping = false;
            else if (whenGrandmaArrives != null) whenGrandmaArrives = null;
            else if (chapter == Chapter.Finished)
                UnityEngine.SceneManagement.SceneManager.LoadScene("RememberMeAct1");
            else PauseChapter();
        }

        void UpdateRoomMood(float dt)
        {
            float goal = 0;
            if (currentRoom == Room.Kitchen)
            {
                if (chapter >= Chapter.Cards && chapter <= Chapter.FirstCollapse) goal = .30f;
                else if (chapter == Chapter.FindDoor) goal = .87f;
                else if (chapter == Chapter.ReturnForGrandma) goal = .93f - CountFinds(anchors) * .16f;
                else if (chapter == Chapter.Escort) goal = .40f;
            }
            else if (currentRoom == Room.Hall)
                goal = chapter == Chapter.Escort ? .32f : .68f;
            else if (currentRoom == Room.Parlor && chapter >= Chapter.Ending)
                goal = Mathf.Clamp01((endingTime - 1f) * .14f);
            decay = Mathf.MoveTowards(decay, goal, dt * .55f);
            roomMaterial.SetFloat("_Decay", decay);
            roomMaterial.SetFloat("_Path", chapter == Chapter.Escort ? 1f : CountFinds(anchors) * .24f);
            roomMaterial.SetFloat("_ClockTime", clockTime);
            roomMaterial.SetFloat("_ClockMissing", chapter >= Chapter.FindDoor ? 1 : 0);
            roomMaterial.SetFloat("_DoorFlicker", Mathf.Clamp01(doorFlicker * 2f));
        }

        void FitView(bool snap = false)
        {
            float ratio = Mathf.Max(.1f, Screen.width / (float)Mathf.Max(1, Screen.height));
            bool kitchen = currentRoom == Room.Kitchen;
            float aspect = Mathf.Clamp(ratio, 1.25f, 1.72f);
            trayFocus = Mathf.MoveTowards(trayFocus, showingFreshCookies ? 1f : 0f, Time.deltaTime / .85f);
            float focus = Mathf.SmoothStep(0f,1f,trayFocus);
            float halfHeight = kitchen ? Mathf.Lerp(273f,142f,focus) : Height / 2;
            float halfWidth = kitchen ? halfHeight * aspect : Width / 2f;
            view.orthographicSize = Mathf.Max(halfHeight, halfWidth / ratio) / Ppu;
            Vector2 center = new Vector2(Width / 2, Height / 2);
            if (kitchen)
            {
                center = new Vector2(Mathf.Clamp(grace.Feet.x,92+halfWidth,1060-halfWidth),
                    Mathf.Clamp(grace.Feet.y - 105, 38+halfHeight, 827-halfHeight));
                // Frame the tray above the dialogue panel and ease back when control returns.
                center = Vector2.Lerp(center, new Vector2(895,243), focus);
            }
            var destination = World(center);
            var cameraPoint = kitchen && !snap
                ? Vector2.SmoothDamp(view.transform.position, destination, ref cameraVelocity, .14f)
                : destination;
            view.transform.position = new Vector3(cameraPoint.x,cameraPoint.y,-10);
        }

        static bool Held(KeyCode a, KeyCode b) => Input.GetKey(a) || Input.GetKey(b);

        void MoveGrace()
        {
            var input = new Vector2(
                (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0),
                (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0));
            if (verifyingRoute) input = routeInput;
            bool escort = chapter == Chapter.Escort;
            motor.WalkSpeed = escort ? 1.15f : 2.45f;
            motor.SetInput(input, !escort && Held(KeyCode.LeftShift, KeyCode.RightShift), !escort && Input.GetKeyDown(KeyCode.C));
            Vector2 position = motor.Body.position * Ppu;
            grace.Feet = new Vector2(position.x, -position.y);
        }

        void BuildObstacles()
        {
            if (obstacles != null) { obstacles.SetActive(false); Destroy(obstacles); }
            obstacles = new GameObject("Walkable room boundaries");
            var data = catalog.rooms[(int)currentRoom];
            doorwayPassage = new Rect(523, 675, 107, 131);
            AddWall(new Rect(0,0,Width,data.top));
            AddWall(new Rect(0,0,127,Height));
            AddWall(new Rect(1025,0,Width-1025,Height));
            AddWall(Rect.MinMaxRect(0, doorwayPassage.yMin, doorwayPassage.xMin, Height));
            AddWall(Rect.MinMaxRect(doorwayPassage.xMax, doorwayPassage.yMin, Width, Height));
            AddWall(Rect.MinMaxRect(doorwayPassage.xMin, doorwayPassage.yMax, doorwayPassage.xMax, Height));
            foreach (Rect rect in data.furniture) AddWall(rect);
            if (currentRoom == Room.Kitchen) AddWall(new Rect(279,507,152,24));
            keepsakeCollider = null;
            if (currentRoom == Room.Bedroom)
            {
                AddWall(new Rect(218, 263, 160, 217));
                keepsakeCollider = AddWall(new Rect(390, 461, 42, 22));
                AddWall(new Rect(925, 215, 110, 136));
                AddWall(new Rect(730, 215, 165, 168));
                AddWall(new Rect(113, 215, 97, 136));
            }
            Physics2D.SyncTransforms();
        }

        Collider2D AddWall(Rect rect)
        {
            var wall = new GameObject("Boundary");
            wall.layer = 8;
            wall.transform.SetParent(obstacles.transform);
            wall.transform.position = World(rect.center);
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = rect.size / Ppu;
            return collider;
        }

        void FollowGrace(float dt)
        {
            if (Vector2.Distance(lastTrailPoint, grace.Feet) >= 9f)
            {
                trail.Enqueue(grace.Feet);
                lastTrailPoint = grace.Feet;
            }
            float gap = Vector2.Distance(grandma.Feet, grace.Feet);
            if (gap >= 45f && (whenGrandmaArrives != null || trail.Count == 0) && ClearFollowPath(grandma.Feet, grace.Feet))
            {
                // Grace is stationary or too far away for the most recent point to hold the pair together.
                trail.Clear();
                trail.Enqueue(grace.Feet);
                lastTrailPoint = grace.Feet;
            }
            if (gap < 37f && ClearFollowPath(grandma.Feet, grace.Feet))
            {
                trail.Clear();
                lastTrailPoint = grace.Feet;
            }
            if (trail.Count == 0)
            {
                StopGrandma();
                return;
            }
            grandmaPace = Mathf.MoveTowards(grandmaPace, Mathf.Clamp((gap - 28f) * 4f, 48f, 184f), 365f * dt);
            float distanceBudget = grandmaPace * dt;
            Vector2 next = grandma.Feet;
            while (trail.Count > 0 && distanceBudget > 0)
            {
                Vector2 point = trail.Peek();
                float distance = Vector2.Distance(next, point);
                if (trail.Count == 1)
                {
                    next = Vector2.MoveTowards(next, point, Mathf.Min(distanceBudget, Mathf.Max(0, distance - 36f)));
                    break;
                }
                next = Vector2.MoveTowards(next, point, distanceBudget);
                distanceBudget -= distance;
                if (next == point) trail.Dequeue();
            }
            MoveGrandma(next);
        }

        static bool ClearFollowPath(Vector2 start, Vector2 end)
        {
            Vector2 delta = World(end - start);
            return !Physics2D.CircleCast(World(start), .112f, delta.normalized, delta.magnitude, 1 << 8);
        }

        void StopGrandma()
        {
            grandmaPace = 0;
            grandma.View.Pose(Vector2.zero, false, false, 0);
        }

        void MoveGrandma(Vector2 next)
        {
            Vector2 delta = World(next - grandma.Feet);
            if (delta.sqrMagnitude > .000001f)
            {
                var hit = Physics2D.CircleCast(World(grandma.Feet), .112f, delta.normalized, delta.magnitude + .012f, 1 << 8);
                if (hit.collider) next = grandma.Feet + new Vector2(delta.x, -delta.y).normalized *
                    Mathf.Max(0, hit.distance - .012f) * Ppu;
            }
            var movement = World(next - grandma.Feet);
            grandma.Feet = next;
            grandma.View.transform.position = World(next);
            var heading = movement.normalized;
            if (heading == Vector2.zero)
            {
                // Subpixel acceleration can be imperceptible for several frames on a fast display.
                grandma.View.Pose(Vector2.zero, false, false, 0);
                return;
            }
            if (Vector2.Dot(heading, grandmaHeading) < .62f)
                grandmaHeading = Mathf.Abs(heading.x) > Mathf.Abs(heading.y)
                    ? Vector2.right * Mathf.Sign(heading.x) : Vector2.up * Mathf.Sign(heading.y);
            grandmaStride += movement.magnitude;
            if (grandmaStride >= .37f) { grandmaStride -= .37f; PlayWoodStep(); }
            grandma.View.Pose(heading, false, false, movement.magnitude / .74f, grandmaHeading);
        }

        void FindTarget()
        {
            nearby = null;
            float best = float.PositiveInfinity;
            foreach (var target in targets)
            {
                if (!TryReach(target, out float distance) || distance >= best) continue;
                best = distance;
                nearby = target;
            }
        }

        bool TryReach(Target target, out float distance)
        {
            distance = float.PositiveInfinity;
            if (target.Available != null && !target.Available()) return false;
            Vector2 approach = target.Feet;
            float footRadius = motor.Feet.bounds.extents.x * Ppu;
            if (target.Kind == Reach.Threshold)
            {
                // Grace's collision radius places her standing limit just short of the end wall.
                if (grace.Feet.y < doorwayPassage.yMax - 53 ||
                    grace.Feet.y > doorwayPassage.yMax - footRadius + 2 ||
                    grace.Feet.x < doorwayPassage.xMin + footRadius - 1 ||
                    grace.Feet.x > doorwayPassage.xMax - footRadius + 1) return false;
            }
            else if (target.Kind == Reach.Fixture)
            {
                // The art lies behind the countertop collider. Measure from a standing position in front of it.
                float floor = catalog.rooms[(int)currentRoom].top;
                approach.y = floor + footRadius + 3;
                float width = currentRoom == Room.Hall ? 72 : 54;
                if (Mathf.Abs(grace.Feet.x - approach.x) > width ||
                    grace.Feet.y < floor || grace.Feet.y - approach.y > 64) return false;
            }
            else if (target.Kind == Reach.Character)
            {
                approach = grandma.Feet;
                if ((approach - grace.Feet).sqrMagnitude > 62 * 62) return false;
            }
            else
            {
                Vector2 closest = keepsakeCollider.ClosestPoint(World(grace.Feet)) * Ppu;
                approach = new Vector2(closest.x, -closest.y);
                if ((approach - grace.Feet).sqrMagnitude > 62 * 62) return false;
            }

            if (target.Kind != Reach.Threshold)
            {
                var blocker = Physics2D.Linecast(World(grace.Feet), World(approach), 1 << 8).collider;
                if (blocker && (target.Kind != Reach.Object || blocker != keepsakeCollider)) return false;
            }
            distance = (approach - grace.Feet).sqrMagnitude;
            return true;
        }

        void ActivateNearby()
        {
            if (nearby == null || choiceFrame == Time.frameCount) return;
            choiceFrame = Time.frameCount;
            motor.SetInput(Vector2.zero, false, false);
            nearby.Use();
        }


        int ingredients, cards, baking, errors;
        bool photoSeen;
        string[] choices;

        Action<int> choose;

        void Offer(string title, string[] options, Action<int> callback, bool compact = true)
        {
            hud.HideDialogue();
            hud.ShowHUD(false);
            hud.Menu(title,title,options,Choose,photoMenu:compact);
            choices = options;
            choose = callback;
            nearby = null;
        }

        void Choose(int index)
        {
            if (choices == null || index < 0 || index >= choices.Length || choiceFrame == Time.frameCount) return;
            choiceFrame = Time.frameCount;
            var callback = choose;
            CloseChoices();
            callback(index);
        }

        void CloseChoices()
        {
            hud.HideMenu();
            inspectJars = sortingCards = viewRecipe = false;
            shownDoor = null;
            choices = null;
            choose = null;
        }

        void OpenBox()
        {
            Offer("Inside the keepsake box", new[] {"Lift the old photograph", "Wind the little music box"}, index => {
                if (index == 1) { Play("RM_Chime", .35f); Say("Grace", Expression.Quiet, new[] {"Just a few notes. Then it stops."}, OpenBox); return; }
                showPhoto = true;
                Say("Grace", Expression.Reflective, new[] {"That's me. And Grandma.", "We used to make cookies together."}, () => {
                    Play("RM_Chime", .26f);
                    Say("Grandma", Expression.Warm, new[] {"Gracie?"}, () =>
                        PauseBeat(.9f, EnterKitchenMemory, amount => memoryFade = amount));
                });
            });
        }

        void EnterKitchenMemory()
        {
            showPhoto = false;
            chapter = Chapter.MeetGrandma;
            Enter(Room.Kitchen);
            PlayMusic("RM_StoryKeepsake");
            grace.View.Look(World(grandma.Feet - grace.Feet));
            PauseBeat(2.9f, () => Say("Grace", Expression.Surprise, new[] {"Grandma?"},
                () => Say("Grandma", Expression.Warm, new[] {"There you are, sweetheart."}, ContinueReunion)),
                amount => memoryFade = 1f - Mathf.Clamp01(amount * 2.9f / .9f));
        }

        void ContinueReunion()
        {
            Say("Grace", Expression.Quiet, new[] {"Can you come home with me?"}, () =>
                Say("Grandma", Expression.Warm, new[] {"After the cookies, dear.", "Your dad always liked these.",
                    "We'll take some to him in the living room."}, () =>
                Say("Grace", Expression.Quiet, new[] {"Then we can go together?"}, () =>
                Say("Grandma", Expression.Warm, new[] {"Of course. Will you help me?"}, () =>
                Say("Grace", Expression.Warm, new[] {"Then I'll help."}, ApproachGrandma)))));
        }

        void ApproachGrandma()
        {
            Vector2 start = grace.Feet;
            Vector2 destination = new Vector2(585, 455);
            PauseBeat(1.6f, HelpGrandma, amount => {
                motor.MoveScripted(World(Vector2.Lerp(start, destination, amount)));
                var feet = motor.Body.position * Ppu;
                grace.Feet = new Vector2(feet.x, -feet.y);
            });
        }

        void FamilyPhoto()
        {
            photoSeen = true;
            showingFamilyPhoto = true;
            if (chapter == Chapter.ReturnForGrandma && (anchors & 1) == 0)
            {
                anchors |= 1;
                Say("Grace", Expression.Reflective, new[] {"Dad in his school coat. The sleeves cover his hands."},
                    () => Say("Grandma", Expression.Warm, new[] {"I left room for him to grow."}, () => showingFamilyPhoto = false));
                return;
            }
            Say("Grace", Expression.Surprise, new[] {"That's Dad! He's so little."}, () =>
                Say("Grandma", Expression.Reflective, new[] {"He was. Always hungry, too.",
                    "He always waited for cookies by the living room window. See the blue curtains?"},
                    () => showingFamilyPhoto = false));
        }

        void MusicBox()
        {
            Play("RM_Chime", .34f);
            if (chapter == Chapter.ReturnForGrandma && (anchors & 2) == 0)
            {
                anchors |= 2;
                Say("Grandma", Expression.Quiet, new[] {"There's a note missing at the end."},
                    () => Say("Grace", Expression.Warm, new[] {"I know. You always used to hum that bit."}));
                return;
            }
            Say("Grace", Expression.Reflective, new[] {"This sounds familiar."}, () =>
                Say("Grandma", Expression.Warm, new[] {"Your dad liked that song. I used to play it at bedtime."}, () =>
                Say("Grace", new[] {"Can you play it again?"}, () =>
                Say("Grandma", Expression.Warm, new[] {"Maybe later, sweetheart. The cookies first."}))));
        }

        void Flour()
        {
            if (chapter == Chapter.MeetGrandma) { AskGrandmaFirst(); return; }
            if ((ingredients & FlourBit) != 0) { Say("Grace", new[] {"I already have the flour."}); return; }
            ingredients |= FlourBit;
            Play("RM_StoryTap", .19f);
            if (chapter == Chapter.Ingredients && !flourSetOut)
                Say("Grace", new[] {"Found the flour."}, () =>
                    Say("Grandma", Expression.Warm, new[] {"Bring it to the recipe book, sweetheart."}));
        }

        void Fridge()
        {
            if (chapter == Chapter.MeetGrandma) { AskGrandmaFirst(); return; }
            if (chapter == Chapter.Cards) { Card(2); return; }
            if ((ingredients & EggsBit) != 0) { Say("Grace", new[] {"I already have the butter and eggs."}); return; }
            ingredients |= ButterBit | EggsBit;
            Play("RM_StoryTap", .19f);
        }

        void Jars()
        {
            if (chapter == Chapter.MeetGrandma) { AskGrandmaFirst(); return; }
            if ((ingredients & SugarBit) != 0) { Say("Grace", new[] {"I already have the ribbon jar."}); return; }
            inspectJars = true;
            Offer("Two white jars. The labels have worn away.", new[] {"Left jar", "Right jar"}, i => {
                if (i == 0)
                {
                    ingredients |= SugarBit;
                    Play("RM_StoryTap", .19f);
                }
                else Say("Grace", new[] {"Wait. This might be salt. I'll put it back."});
            });
        }

        static readonly string[] CardNames = {"Add eggs and flour", "Mix into dough", "Shape the cookies"};

        void Card(int index)
        {
            if ((cards & (1 << index)) != 0)
            {
                Say("Grace", new[] {"I already put this card beside the recipe."});
                return;
            }
            cards |= 1 << index;
            visibleCard = index;
            Play("RM_StoryTap", .21f);
            Say("Grace", new[] {"Dad drew this one. " + CardNames[index] + "."}, () => visibleCard = -1);
        }

        void Bowl()
        {
            if (chapter == Chapter.Cards) Card(0);
            else if (chapter == Chapter.Baking) Bake(false);
            else Say("Grace", new[] {"Grandma's wooden spoon is resting in the bowl."});
        }

        void Counter()
        {
            if (chapter == Chapter.Cards) Card(1);
            else if (chapter == Chapter.Baking) Bake(true);
            else Say("Grace", new[] {"Grandma's oven is here."});
        }

        void AskGrandmaFirst() => Say("Grace", new[] {"I should ask Grandma what she needs."});

        void ReadDadNote()
        {
            showingNote = true;
            sugarClueFound = true;
            Say("Grace", Expression.Reflective, new[] {"Dad wrote this?"}, () => showingNote = false);
        }

        void Recipe()
        {
            if (chapter <= Chapter.Ingredients)
            {
                Offer("The old recipe book", new[] {"Read Dad's note",
                    chapter == Chapter.Ingredients && !flourSetOut && (ingredients & FlourBit) != 0 ?
                        "Set out the flour" : "Set out the ingredients"}, i => {
                    if (i == 0) ReadDadNote();
                    else if (chapter == Chapter.MeetGrandma) AskGrandmaFirst();
                    else if (!flourSetOut && (ingredients & FlourBit) != 0) SetOutFlour();
                    else if (ingredients != AllIngredients) Say("Grace", new[] {"I can bring the ingredients back here once I have them."});
                    else {
                        chapter = Chapter.Cards;
                        PlayMusic("RM_Remember");
                        Say("Grandma", Expression.Warm, new[] {"There it is! Good job, Gracie.", "Your dad used to steal the sugar."},
                            () => Say("Grace", Expression.Surprise, new[] {"Dad?"}, () =>
                            Say("Grandma", Expression.Reflective, new[] {"Just a little. He thought I never noticed.",
                                "Now, let's see. What comes next? This part is hard to read.",
                                "Your dad drew some pictures for me. They must be around here somewhere."},
                            () => Say("Grace", Expression.Concern, new[] {"Grandma? Did the lights just…", "Never mind."}))));
                    }
                });
            }
            else if (chapter == Chapter.Cards && cards == AllCards)
            {
                viewRecipe = true;
                Offer("The family recipe", new[] {"Arrange the picture cards", "Read Dad's note"}, i => {
                    if (i == 1) ReadDadNote();
                    else
                    {
                        SortCards();
                    }
                });
            }
            else
            {
                viewRecipe = true;
                Offer(chapter == Chapter.Cards ? "Three steps are missing" : "The family recipe",
                    new[] {"Close the recipe", "Read Dad's note"}, i => { if (i == 1) ReadDadNote(); });
            }
        }

        void SetOutFlour()
        {
            flourSetOut = true;
            Play("RM_StoryTap", .19f);
            Say("Grace", new[] {"I'll put the flour here."}, () =>
                Say("Grandma", Expression.Warm, new[] {"Thank you, Gracie."}, () => {
                    if (ingredients == AllIngredients)
                        Say("Grace", new[] {"I've got everything else, too.", "We can set it all out by the recipe."});
                    else Say("Grandma", Expression.Warm, new[] {"We'll need butter, eggs and sugar, too.",
                        "Try the fridge for the butter and eggs."});
                }));
        }

        void SortCards()
        {
            var remaining = new List<int> {2,0,1};
            remaining.RemoveAll(recipeOrder.Contains);
            remainingCardIds = remaining.ToArray();
            sortingCards = viewRecipe = true;
            var options = remaining.ConvertAll(i => CardNames[i]);
            if (recipeOrder.Count > 0) options.Add("Undo last card");
            Offer("Choose the picture for step " + (recipeOrder.Count + 2), options.ToArray(), selection => {
                if (selection == remaining.Count) { UndoRecipeCard(); return; }
                recipeOrder.Add(remaining[selection]);
                if (recipeOrder.Count < 3) { SortCards(); return; }
                if (recipeOrder[0] != 0 || recipeOrder[1] != 1)
                {
                    errors++;
                    recipeOrder.Clear();
                    viewRecipe = true;
                    Say("Grace", new[] {errors > 1 ? "First add the flour and eggs. Mix the dough before shaping it." :
                        "Hmm. That doesn't look right. Try again?"}, () => viewRecipe = false);
                    return;
                }
                chapter = Chapter.Baking;
                Say("Grace", Expression.Warm, new[] {"Look, Grandma. Dad drew these. The pictures tell us what to do."}, () =>
                    Say("Grandma", Expression.Warm, new[] {"Oh. These are nice. Then let's try.", "Oh. What was I doing?"}, () =>
                    Say("Grace", Expression.Warm, new[] {"We were mixing. Look. Like this."}, () =>
                    Say("Grandma", Expression.Warm, new[] {"Right. Shall we do it together?"}))));
            });
        }

        void UndoRecipeCard()
        {
            // Choose closes the option menu before invoking its callback, which
            // clears sortingCards. Chapter and placed cards own undo eligibility.
            if (chapter != Chapter.Cards || recipeOrder.Count == 0) return;
            recipeOrder.RemoveAt(recipeOrder.Count - 1);
            SortCards();
        }

        void RemoveRecipeCardsFrom(int index)
        {
            if (!sortingCards || index < 0 || index >= recipeOrder.Count || choiceFrame == Time.frameCount) return;
            choiceFrame = Time.frameCount;
            recipeOrder.RemoveRange(index, recipeOrder.Count - index);
            SortCards();
        }

        string BakingHint()
        {
            if (baking == 0) return "Mix the dough together at the bowl.";
            if (baking == 1) return "Shape the dough at the baking counter.";
            return "Bake the cookies at the baking counter.";
        }

        void Bake(bool atCounter)
        {
            if (atCounter != (baking > 0))
            {
                Say("Grace", new[] { BakingHint() });
                return;
            }
            choiceFrame = Time.frameCount;
            if (baking == 0) mixing = true;
            else if (baking == 1) shaping = true;
            else
            {
                chapter = Chapter.Tray;
                transition = .45f;
                Play("RM_DoorCreak", .21f);
                Play("RM_Chime", .36f);
                showingFreshCookies = true;
                Say("Grace", Expression.Warm, new[] {"They smell good!"}, () =>
                    Say("Grandma", Expression.Warm, new[] {"They do, don't they? We made them together.",
                        "All done. Let's bring these to your dad."}, () => showingFreshCookies = false));
            }
        }

        void MixDough()
        {
            if (!mixing || lines != null || choiceFrame == Time.frameCount) return;
            choiceFrame = Time.frameCount;
            Play("RM_StoryTap", .18f);
            if (++mixCount < MixSteps) return;
            mixing = false;
            baking = 1;
            Say("Grandma", Expression.Warm, new[] {"That's it. I'll keep holding the bowl. Now we can shape the dough."});
        }

        void ShapeCookie()
        {
            for (int slot = 0; slot < DoughPieces; slot++)
                if (CanPlaceDough(slot))
                {
                    ShapeCookie(slot);
                    return;
                }
        }

        bool CanPlaceDough(int slot)
        {
            return shaping && lines == null && slot >= 0 && slot < DoughPieces &&
                (shapedSlots & (1 << slot)) == 0 && (slot != DoughPieces - 1 || shapedCount == DoughPieces - 1);
        }

        void ShapeCookie(int slot)
        {
            if (!CanPlaceDough(slot) || choiceFrame == Time.frameCount) return;
            choiceFrame = Time.frameCount;
            shapedSlots |= 1 << slot;
            Play("RM_StoryTap", .22f);
            shapedCount++;
            if (shapedCount == DoughPieces - 1)
            {
                Say("Grandma", Expression.Warm, new[] {"Wait. Let's make this one bigger."}, () =>
                    Say("Grace", new[] {"Why?"}, () =>
                    Say("Grandma", Expression.Warm, new[] {"Your dad always likes the biggest one."}, () =>
                    Say("Grace", new[] {"Even now?"}, () =>
                    Say("Grandma", Expression.Warm, new[] {"Oh, yes."})))));
            }
            if (shapedCount < DoughPieces) return;
            shaping = false;
            baking = 2;
            Say("Grandma", Expression.Warm, new[] {"There. That should do it."});
        }

        void TakeTray()
        {
            if (chapter < Chapter.Tray) { Say("Grace", new[] { "We'll need this when the cookies are ready." }); return; }
            if (chapter != Chapter.Tray)
            {
                Say("Grace", new[] { "I still have the tray. The cookies are warm." });
                return;
            }
            chapter = Chapter.FirstCollapse;
            Play("RM_StoryTap", .28f);
        }

        void LeaveKitchen()
        {
            if (chapter <= Chapter.Tray)
            {
                Say("Grace", new[] { "I'll finish the cookies with Grandma first." });
                return;
            }
            if (!looped)
            {
                looped = true;
                chapter = Chapter.FindDoor;
                doorFlicker = 1.2f;
                Play("RM_Swell", .54f);
                Say("Grace", Expression.Concern, new[] {"Grandma? Something's wrong."}, () =>
                    Say("Grandma", Expression.Concern, new[] {"Where's the living room?"}, () =>
                    Say("Grace", new[] {"It's right there. Wait here. I'll have a look."}, () =>
                    {
                        Play("RM_DoorCreak", .52f);
                        Enter(Room.Kitchen, new Vector2(485, 616));
                        Say("Grace", Expression.Surprise, new[] {"Huh? Wasn't I just here?"});
                    })));
                return;
            }
            if (chapter == Chapter.Escort) WaitForGrandma(() => Enter(Room.Hall));
            else Enter(Room.Hall);
        }

        void WaitForGrandma(Action ready)
        {
            if (Vector2.Distance(grace.Feet, grandma.Feet) <= 70f) ready();
            else
            {
                whenGrandmaArrives = ready;
                nearby = null;
                motor.SetInput(Vector2.zero, false, false);
            }
        }

        void InspectDoor(Door door)
        {
            inspectedDoors |= 1 << (int)door;
            shownDoor = door;
            string clue;
            if (door == Door.Clock)
            {
                Play("RM_StoryTap", .48f);
                clue = "A clock ticks on the other side.";
            }
            else if (door == Door.Water)
            {
                Play("RM_StoryDrip", .57f);
                clue = "Water drips on the other side.";
            }
            else
                clue = photoSeen ? "Blue curtains, like the ones in Dad’s picture." : "A blue curtain by a warmly lit window.";
            Offer(clue, new[] {"Open the door", "Step back"}, i => {
                if (i != 0) return;
                if (door == Door.Clock) ClockDoor();
                else if (door == Door.Water) SinkDoor();
                else BlueDoor();
            });
        }

        string WrongDoorHint()
        {
            if (wrongDoors < 2) return "I'll try another door.";
            if (!photoSeen) return "The living room… I've seen it before. Maybe the family photo can help.";
            return "Dad was sitting by the blue curtains.";
        }

        void ClockDoor()
        {
            wrongDoors++;
            Play("RM_DoorCreak", .42f);
            Enter(Room.Kitchen);
            Say("Grace", new[] {"Back in the kitchen. " + WrongDoorHint()});
        }

        void SinkDoor()
        {
            Play("RM_StoryDrip", .62f);
            wrongDoors++;
            transition = .45f;
            waterSpace = true;
            Say("Grace", Expression.Tense, new[] {"Is that water? The walls have disappeared."}, () => {
                waterSpace = false;
                transition = .45f;
                Say("Grace", new[] {"There's the hallway again. " + WrongDoorHint()});
            });
        }

        void BlueDoor()
        {
            if (chapter == Chapter.Escort)
            {
                WaitForGrandma(() => {
                    chapter = Chapter.Ending;
                    Enter(Room.Parlor);
                    endingTime = 0f;
                    PlayMusic("RM_StoryKeepsake");
                    StartCoroutine(ServeCookies());
                });
                return;
            }
            if (chapter == Chapter.ReturnForGrandma)
            {
                Say("Grace", new[] { "I’m going back for Grandma." });
                return;
            }
            chapter = Chapter.ReturnForGrandma;
            Play("RM_DoorCreak", .42f);
            Enter(Room.Parlor, new Vector2(576, 601));
            decay = 0;
            roomMaterial.SetFloat("_Decay", 0);
            Say("Grace", new[] {photoSeen ? "Like the blue curtains in Dad’s picture!" : "Blue curtains. There’s an empty chair by the window."},
                () => PauseBeat(.7f, () => {
                    grace.View.Look(Vector2.down);
                    Say("Grace", Expression.Concern, new[] {"Grandma?", "She's still waiting in the kitchen. I'll go back for her."});
                }));
        }

        void HelpGrandma()
        {
            if (chapter == Chapter.MeetGrandma)
            {
                chapter = Chapter.Ingredients;
                Say("Grandma", Expression.Warm, new[] {"Let's start with the flour.",
                    "It's in the cupboard on the left.", "Bring it here, beside the recipe book."});
                return;
            }
            if (chapter == Chapter.Ingredients) { Say("Grandma", new[] {
                !flourSetOut ? (ingredients & FlourBit) == 0 ? "The flour is in the cupboard on the left." :
                    "Bring the flour to the recipe book, sweetheart." : ingredients == AllIngredients ?
                    "Let's set everything out beside the recipe book." : "We still need butter, eggs and sugar. Check the fridge and the jars."}); return; }
            if (chapter == Chapter.Cards || chapter == Chapter.Baking) { Say("Grandma", new[] {"Let's look at the pictures on the counter together."}); return; }
            if (chapter != Chapter.ReturnForGrandma)
            {
                Say("Grandma", new[] { "Make sure the one in the middle doesn’t fall off." });
                return;
            }
            Say("Grandma", Expression.Concern, new[] {"Grace? Where did you go? I can't find the door."},
                () => Say("Grace", Expression.Warm, new[] {"I'm here. I found it. Come with me."},
                () => Say("Grandma", Expression.Quiet, new[] {"Will you stay with me?"},
                () => Say("Grace", Expression.Warm, new[] {"Yes. Take my hand."}, () => {
                    handCue = 2.3f;
                    Say("Grandma", Expression.Warm, new[] {"All right, sweetheart. Let's go."}, () => {
                        chapter = Chapter.Escort;
                        grandmaFeet.enabled = false;
                        trail.Clear();
                        lastTrailPoint = grace.Feet;
                        PlayMusic("RM_Remember");
                    });
                }))));
        }

        System.Collections.IEnumerator ServeCookies()
        {
            serving = true;
            Vector2 place = new Vector2(759, 405);
            Vector2 atTable = new Vector2(796, 409);
            while (Vector2.Distance(grandma.Feet, place) > .5f || Vector2.Distance(grace.Feet, atTable) > .5f)
            {
                float dt = Mathf.Min(Time.deltaTime, .05f);
                MoveGrandma(Vector2.MoveTowards(grandma.Feet, place, 123f * dt));
                Vector2 next = Vector2.MoveTowards(grace.Feet, atTable, 143f * dt);
                motor.MoveScripted(World(next));
                Vector2 actual = motor.Body.position * Ppu;
                grace.Feet = new Vector2(actual.x, -actual.y);
                yield return null;
            }
            StopGrandma();
            grace.View.Look(Vector2.up);
            grandma.View.Look(Vector2.up);
            largestServed = true;
            serving = false;
            Say("Grandma", Expression.Warm, new[] {"There. Your dad always sits right here. He likes the biggest one."}, () =>
                Say("Grace", Expression.Reflective, new[] {"Where is he now?"}, () =>
                Say("Grandma", new[] {"He'll be home from school soon."}, () =>
                Say("Grace", Expression.Surprise, new[] {"From school?"}, () =>
                Say("Grandma", new[] {"Oh. I should get his coat ready. It's cold outside."})))));
        }

        void AdvanceEnding(float dt)
        {
            if (serving) return;
            float previous = endingTime;
            endingTime += dt;
            if (exitWaypoint < coatRoute.Length)
            {
                Vector2 next = Vector2.MoveTowards(grandma.Feet, coatRoute[exitWaypoint], 116f * dt);
                MoveGrandma(next);
                if (next == coatRoute[exitWaypoint]) exitWaypoint++;
            }
            else grandma.View.Look(Vector2.up);
            if (previous < 3.8f && endingTime >= 3.8f) Play("Key", .7f);
            if (endingTime < 4.9f || exitWaypoint < coatRoute.Length) return;
            chapter = Chapter.Finished;
            Say("Grace", Expression.Tense, new[] { "Grandma? Wait!" });
        }

        void Say(string name, string[] words, Action onFinish = null)
            => Say(name, Expression.Neutral, words, onFinish);
        
        void Say(string name, Expression expression, string[] words, Action onFinish = null)
        {
            mood = expression;
            speaker = name;
            lines = words;
            line = 0;
            afterDialogue = onFinish;
            if (name == "Grandma" && grandma.Renderer.enabled)
            {
                grandma.View.Look(World(grace.Feet - grandma.Feet));
                grandmaHeading = grandma.View.Forward;
            }
            PresentLine();
        }

        void PresentLine()
        {
            var faces = speaker == "Grandma" ? grandmaFaces : speaker == "Grace" ? graceFaces : null;
            hud.Dialogue(speaker,lines[line],faces == null ? null : faces[(int)mood]);
            hud.ShowHUD(false);
        }

        void LateUpdate()
        {
            PositionTray();
            bool exploring = bedroomArrival == 0 && !WaitingForBeat && lines == null && choices == null &&
                !mixing && !shaping && chapter != Chapter.Ending;
            hud.ShowHUD(exploring);
            if (exploring)
                hud.Guide(Objective(),"E",nearby == null ? "" :
                    nearby == grandmaTarget && chapter == Chapter.ReturnForGrandma ? "Offer Grandma your hand" :
                    chapter == Chapter.Cards && (nearby.Feet.x == 677 || nearby.Feet.x == 460 || nearby.Feet.x == 761)
                        ? "Inspect the picture card" : nearby.Label);
            UpdateKitchenCanvas();
        }

        void Advance()
        {
            if (++line < lines.Length) { PresentLine(); return; }
            hud.HideDialogue();
            lines = null;
            var callback = afterDialogue;
            afterDialogue = null;
            callback?.Invoke();
        }

        void PauseBeat(float seconds, Action next, Action<float> during = null)
        {
            hud.HideDialogue();
            hud.ShowHUD(false);
            motor.CanMove = false;
            motor.SetInput(Vector2.zero, false, false);
            beatTime = seconds;
            beatDuration = seconds;
            duringBeat = during;
            afterBeat = next;
        }

        void Play(string name, float volume) =>
            sound.PlayOneShot(Resources.Load<AudioClip>("KitchenChapter/Audio/" + name), volume);

        void PlayMusic(string name)
        {
            queuedMusic = Resources.Load<AudioClip>("KitchenChapter/Audio/" + name);
            if (music.clip == queuedMusic && music.isPlaying) { queuedMusic = null; return; }
            musicFade = music.isPlaying ? .38f : 0f;
        }

        void UpdateMusic(float dt)
        {
            if (queuedMusic != null)
            {
                musicFade = Mathf.Max(0,musicFade-dt);
                music.volume = Mathf.Min(music.volume,.32f*musicFade/.38f);
                if (musicFade > 0) return;
                music.clip = queuedMusic;
                queuedMusic = null;
                music.Play();
            }
            music.volume = Mathf.MoveTowards(music.volume,.32f,dt*.46f);
        }

        void PositionTray()
        {
            cookieTray.enabled = currentRoom == Room.Parlor && serving;
            platedCookie.enabled = currentRoom == Room.Parlor && largestServed;
            if (!cookieTray.enabled) return;
            bool grandmaCarries = currentRoom == Room.Parlor && serving;
            bool carried = (chapter > Chapter.Tray && chapter < Chapter.Ending) || grandmaCarries;
            var holder = grandmaCarries ? grandma : grace;
            Vector2 center = carried ? holder.Feet + new Vector2(18, -36) :
                currentRoom == Room.Parlor ? new Vector2(788, 292) : new Vector2(895, 212);
            cookieTray.transform.position = World(center);
            float width = carried ? .27f : currentRoom == Room.Parlor ? .35f : .41f;
            cookieTray.transform.localScale = new Vector3(width, carried ? .13f : .19f, 1);
            cookieTray.sortingOrder = carried ? holder.Renderer.sortingOrder + 1 : 3;
            enamel.SetFloat("_RemoveLargest", largestServed ? 1 : 0);
        }

        static int CountFinds(int bits) => (bits & 1) + ((bits >> 1) & 1) + ((bits >> 2) & 1);

        string Objective()
        {
            if (whenGrandmaArrives != null) return "Wait for Grandma at the doorway.";
            switch (chapter)
            {
                case Chapter.Bedroom: return "Check the sound beside the bed.";
                case Chapter.MeetGrandma: return "Ask Grandma how you can help.";
                case Chapter.Ingredients:
                    if (!flourSetOut) return (ingredients & FlourBit) == 0 ?
                        "Help Grandma: find flour in the left cupboard." : "Bring the flour to the recipe book.";
                    return ingredients == AllIngredients ? "Bring the ingredients to the recipe." : "Gather the ingredients.";
                case Chapter.Cards: return cards == AllCards ? "Put Dad’s cards in order at the recipe." :
                    $"Cards {CountFinds(cards)}/3 · " + ((cards & 1) == 0 ? "Check beside the bowl." :
                    (cards & 2) == 0 ? "Check the oven counter." : "Check the fridge door.");
                case Chapter.Baking: return BakingHint();
                case Chapter.Tray: return "Pick up the blue serving tray.";
                case Chapter.FirstCollapse: return "Bring the cookies to the living room.";
                case Chapter.FindDoor: return currentRoom == Room.Kitchen ? "Try the doorway again." : "Find the living room.";
                case Chapter.ReturnForGrandma:
                    if (currentRoom == Room.Parlor) return "Go back through the doorway for Grandma.";
                    if (currentRoom == Room.Hall) return "Return to the kitchen for Grandma.";
                    return "Offer Grandma your hand.";
                case Chapter.Escort:
                    return currentRoom == Room.Kitchen ? "Lead Grandma to the hallway." : "Take Grandma to the living room.";
                case Chapter.Finished: return "Chapter complete   ·   R to replay   /   Esc for title";
                default: return "By the Blue Curtains";
            }
        }

        static void Tint(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        static void Panel(Rect bounds)
        {
            var previous = GUI.color;
            GUI.color = new Color(.073f, .093f, .092f, .95f);
            GUI.DrawTexture(bounds, Texture2D.whiteTexture);
            GUI.color = new Color(.76f, .60f, .33f);
            GUI.DrawTexture(new Rect(bounds.x, bounds.y, 2f, bounds.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }

        static void Label(Rect bounds, string value, int size, Color color)
        {
            var style = new GUIStyle(GUI.skin.label) {fontSize = size, wordWrap = true};
            style.normal.textColor = color;
            GUI.Label(bounds, value, style);
        }
    }
}

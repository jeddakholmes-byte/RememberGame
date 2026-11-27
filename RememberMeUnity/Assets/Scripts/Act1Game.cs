using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RememberMe
{
    public sealed class Act1Game : MonoBehaviour
    {
        internal enum Expression { Neutral, Warm, Surprise, Concern, Reflective, Tense, Grief, Quiet }

        Sprite[] graceExpressions, grandmaExpressions, momExpressions, dadExpressions;

        public GraceMotor Grace;
        public Camera WorldCamera;
        public Act1HUD HUD;
        public Act1Room[] Rooms;
        public Sprite Cover, Gravestone, ClosedParcel, OpenParcel, Mechanism, WindowView, WalkHomeInRain;
        public Sprite PuddleReflection;
        public Sprite GracePortrait, MomPortrait, DadPortrait, DadSadPortrait, GrandmaPortrait, GrandpaPortrait, UnclePortrait, AuntPortrait;
        public AudioClip TitleMusic, BoxMusic, Hinge, Latch, Gravel, Board, Cloth;
        AudioSource music, ambience, effects, footsteps;
        AudioClip windingClick;
        Act1Room room;
        Act1Story story = new();
        Act1Target nearby;
        bool atTitle, busy, read, menuSave, qa;
        bool gateCallLatched;
        bool butterflyGuiding, cemeteryPan;
        Vector2 butterflyPosition;
        float butterflyArrivedAt;
        int gateCalls, qaMomPans;
        bool qaWideView, qaCloseParcelOverview;
        Vector2 qaDirection;
        bool qaSprint;
        AudioClip[] gravelSteps, woodSteps;
        int lastGravelStep = -1, lastWoodStep = -1;
        readonly System.Random stepRandom = new();
        string saveFile;

        static bool openedChapterLink;

        void Start()
        {
            if (Application.isMobilePlatform)
            {
                int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
                Application.targetFrameRate = Mathf.Clamp(refreshRate, 30, 120);
            }
            else
            {
                // Render on each display refresh, including 120 Hz when available.
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            qa = Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out var page) && page.Query == "?remember-smoke=1";
            if (!openedChapterLink && page != null && (page.Query == "?kitchen=1" || page.Query == "?kitchen-check=1"))
            {
                openedChapterLink = true;
                enabled = false;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Kitchen");
                return;
            }
            saveFile = qa ? "remember-me-qa" : "remember-me-act-one";
            // Keep Unity's default keyboard capture: this player has no HTML text fields.
            // Some editor versions omit WebGLModule from the generated player references.
            if (qa && Act1Story.TryRead(saveFile, out var prior) && prior.Stage == StoryStage.Complete)
                Debug.Log("REMEMBER_WEB_SAVE_RELOADED");
#else
            string[] args = Environment.GetCommandLineArgs();
            if (!openedChapterLink && Array.IndexOf(args, "-kitchen-check") >= 0)
            {
                openedChapterLink = true;
                enabled = false;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Kitchen");
                return;
            }
            qa = Array.IndexOf(args, "-remember-smoke") >= 0;
            saveFile = Path.Combine(Application.persistentDataPath, qa ? "qa-act1.json" : "act1.json");
#endif
            if (qa) Debug.Log("REMEMBER_SMOKE_STARTED");
            HUD.Initialize();
            graceExpressions = LoadGraceExpressions();
            grandmaExpressions = LoadGrandmaExpressions();
            momExpressions = LoadExpressions("Mom", new[] { 0, 1, 5, 2, 4, 6, 3, 7 });
            dadExpressions = LoadExpressions("Dad", new[] { 0, 1, 4, 3, 7, 6, 5, 2 });
            music = Audio(true, .42f);
            ambience = Audio(true, .25f);
            effects = Audio(false, .42f);
            footsteps = Audio(false, .55f);
            gravelSteps = LoadSteps(Gravel, "Stone");
            woodSteps = LoadSteps(Board, "Wood");
            Grace.Footfall += PlayFootfall;
            windingClick = Resources.Load<AudioClip>("MusicBox/Winding");
            HUD.Fade(0);
            Title();
            if (qa) StartCoroutine(Smoke());
        }

        AudioSource Audio(bool loop, float volume)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.loop = loop; source.volume = volume;
            source.playOnAwake = false;
            return source;
        }

        static bool Press(KeyCode key) => Input.GetKeyDown(key);
        static float Axis(KeyCode plus, KeyCode altPlus, KeyCode minus, KeyCode altMinus) =>
            (Input.GetKey(plus) || Input.GetKey(altPlus) ? 1 : 0) -
            (Input.GetKey(minus) || Input.GetKey(altMinus) ? 1 : 0);

        void Update()
        {
            if (HUD.IsModal)
            {
                HUD.HandleMenuInput();
                if (Press(KeyCode.Escape) && !busy && !atTitle) CloseMenu();
                return;
            }
            if (atTitle) return;
            if (busy)
            {
                if (HUD.TakeContinue()) read = true;
                return;
            }
            if (HUD.IsModal)
            {
                if (Press(KeyCode.Escape)) CloseMenu();
                return;
            }
            if (Press(KeyCode.Escape)) { Pause(); return; }
            if (Press(KeyCode.I)) { Inventory(); return; }
            Vector2 direction = new(Axis(KeyCode.D, KeyCode.RightArrow, KeyCode.A, KeyCode.LeftArrow),
                Axis(KeyCode.W, KeyCode.UpArrow, KeyCode.S, KeyCode.DownArrow));
            Grace.SetInput(qa ? qaDirection : direction,
                qa ? qaSprint : Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), Press(KeyCode.C));
            if (room.Id == RoomId.Cemetery)
            {
                var feet = Grace.Body.position;
                bool atGate = Mathf.Abs(feet.x - room.Size.x / 2) < .78f && feet.y < .92f;
                if (!atGate) gateCallLatched = false;
                else if (!gateCallLatched)
                {
                    gateCallLatched = true;
                    Trigger(TargetId.Gate);
                    return;
                }
            }
            nearby = Closest(Grace.Body.position, Grace.View.Forward);
            bool needsCrouch = nearby && NeedsCrouch(nearby.Id);
            string action = !nearby ? "" : needsCrouch ?
                (nearby.Id == TargetId.Puddle ? "Crouch to look into the puddle" : "Crouch to inspect") :
                nearby.Id == TargetId.Parcel && story.ParcelOpened ? "Reopen the parcel" : nearby.Label;
            HUD.Guide(Objective(), needsCrouch ? "C" : "E / Enter", action);
            if (nearby && HUD.TakeInteraction()) Trigger(nearby.Id);
        }

        static AudioClip[] LoadSteps(AudioClip fallback, string surface)
        {
            var clips = Resources.LoadAll<AudioClip>("Footsteps/" + surface);
            return clips.Length > 0 ? clips : new[] { fallback };
        }

        void PlayFootfall()
        {
            if (busy || atTitle || room == null || HUD.IsModal) return;
            bool outdoors = room.Id == RoomId.Cemetery;
            var clips = outdoors ? gravelSteps : woodSteps;
            int previous = outdoors ? lastGravelStep : lastWoodStep;
            int index = stepRandom.Next(previous < 0 ? clips.Length : Mathf.Max(1, clips.Length - 1));
            if (clips.Length > 1 && previous >= 0 && index >= previous) index++;
            if (outdoors) lastGravelStep = index;
            else lastWoodStep = index;

            var clip = clips[index];
            if (!clip) return;
            footsteps.pitch = .94f + (float)stepRandom.NextDouble() * .12f;
            float strength = Grace.Crouched ? .23f : Grace.Running ? .58f : .42f;
            footsteps.PlayOneShot(clip, strength * (.87f + (float)stepRandom.NextDouble() * .26f));
        }

        void LateUpdate()
        {
            if (atTitle || room == null || room.Id == RoomId.Flashback || qaWideView || cemeteryPan) return;
            // The Rigidbody interpolates the rendered position. Read that same position
            // for the camera, so it stops with Grace and does not ease past her.
            WorldCamera.transform.position = CameraPosition(Grace.transform.position);
            var objective = !busy && !HUD.IsModal ? ObjectiveTarget() : null;
            bool guidedByButterfly = UpdateButterflyGuide(objective);
            var highlight = objective && objective.FocusArt ? objective.FocusArt : null;
            HUD.Locate(WorldCamera, objective && !guidedByButterfly ? objective.Approach : (Vector2?)null,
                highlight ? highlight.bounds : (Bounds?)null);
        }

        bool UpdateButterflyGuide(Act1Target target)
        {
            if (!target || room.Id != RoomId.Hall || story.Stage != StoryStage.FindStudy)
            {
                butterflyGuiding = false;
                HUD.ShowButterfly(WorldCamera, null);
                return false;
            }

            // Lead along the open path from the front-room entrance to the back-room door.
            Vector2 destination = target.Approach;
            destination.x += .27f;
            destination.y = Mathf.Clamp(Grace.transform.position.y + 1.1f, room.Entry.y + 1, target.Approach.y + .27f);
            if (!butterflyGuiding)
            {
                butterflyGuiding = true;
                butterflyArrivedAt = Time.time;
                butterflyPosition = destination - new Vector2(.57f, .34f);
            }
            butterflyPosition = Vector2.MoveTowards(butterflyPosition, destination, Time.deltaTime * 4.8f);
            float opacity = Mathf.Lerp(.12f, 1, Mathf.Clamp01((Time.time - butterflyArrivedAt) * 3));
            return HUD.ShowButterfly(WorldCamera, FlutterAt(butterflyPosition), opacity);
        }

        static Vector2 FlutterAt(Vector2 center) =>
            center + new Vector2(Mathf.Sin(Time.time * 2.6f) * .11f, Mathf.Sin(Time.time * 3.1f) * .09f);

        Vector3 CameraPosition(Vector2 feet)
        {
            Vector2 size = room.Size;
            if (room.Id == RoomId.Hall)
                return new Vector3(size.x / 2, size.y / 2, -10);
            float ext = Mathf.Min(size.x / 2, WorldCamera.orthographicSize * WorldCamera.aspect);
            return new Vector3(Mathf.Clamp(feet.x, ext, size.x - ext),
                Mathf.Clamp(feet.y + .28f, WorldCamera.orthographicSize, size.y - WorldCamera.orthographicSize), -10);
        }

        Act1Target Closest(Vector2 feet, Vector2 forward)
        {
            Act1Target best = null;
            float score = float.MaxValue;
            foreach (var target in room.Targets)
            {
                if (target.Id == TargetId.Gate || target.Id == TargetId.LeaveMemory && story.Stage != StoryStage.Complete) continue;
                if (target.Id == TargetId.Ribbon && story.Ribbon || target.Id == TargetId.Book && (story.Book || story.Shelved)) continue;
                if (!target.CanReach(feet, out float space)) continue;
                Vector2 toTarget = target.ReachPoint(feet) - feet;
                float facingBonus = .24f * Mathf.Max(0, Vector2.Dot(forward, toTarget.normalized));
                float priority = space - facingBonus;
                if (priority >= score) continue;
                score = priority;
                best = target;
            }
            return best;
        }

        string Objective() => story.Stage switch
        {
            StoryStage.Farewell => "Find Grandma’s headstone",
            StoryStage.FindDad => "Talk to Dad near the grave",
            StoryStage.FindMom => "Return to Mom by the path",
            StoryStage.OpenParcel => story.ParcelOpened ? "Wind the music box at the bench" : "Open Grandma’s parcel at the bench",
            StoryStage.FindStudy => "Follow the melody to the back room",
            StoryStage.ExploreMemory => room.Id == RoomId.Hall ? "Return to the back room" : "Look through the bay window",
            StoryStage.Complete => room.Id == RoomId.Hall ? "Return to the back room" : "Leave the memory",
            _ => ""
        };

        Act1Target TargetInRoom(TargetId id) => Array.Find(room.Targets, item => item.Id == id);

        Act1Target ObjectiveTarget() => story.Stage switch
        {
            StoryStage.Farewell => TargetInRoom(TargetId.Grave),
            StoryStage.FindDad => TargetInRoom(TargetId.Dad),
            StoryStage.FindMom => TargetInRoom(TargetId.Mom),
            StoryStage.OpenParcel => TargetInRoom(TargetId.Parcel),
            StoryStage.FindStudy => TargetInRoom(TargetId.ToStudy),
            StoryStage.ExploreMemory => TargetInRoom(room.Id == RoomId.Hall ? TargetId.ToStudy : TargetId.Window),
            StoryStage.Complete => TargetInRoom(room.Id == RoomId.Hall ? TargetId.ToStudy : TargetId.LeaveMemory),
            _ => null
        };

        void FaceSpeaker(Act1Target contact)
        {
            if (!contact || !contact.Speaker || !Grace.gameObject.activeSelf) return;
            contact.Speaker.Look(Grace.Body.position - (Vector2)contact.Speaker.transform.position);
            Grace.LookAt(contact.Speaker.transform.position);
        }

        void Song(AudioSource source, AudioClip clip)
        {
            if (source.clip == clip && source.isPlaying) return;
            source.clip = clip;
            if (clip) source.Play(); else source.Stop();
        }

        void Title()
        {
            atTitle = true; busy = false; Grace.CanMove = false;
            HUD.ShowButterfly(WorldCamera, null);
            foreach (var entry in Rooms) entry.gameObject.SetActive(false);
            Grace.gameObject.SetActive(false);
            HUD.HideDialogue(); HUD.ShowHUD(false); HUD.Picture(Cover);
            Song(music, TitleMusic); Song(ambience, null);
            HUD.Menu("Remember Me", "A story about love, memory and goodbye.",
#if UNITY_WEBGL && !UNITY_EDITOR
                new[] {"New Game", "Continue", "Kitchen"}, choice =>
#else
                new[] {"New Game", "Continue", "Kitchen", "Quit"}, choice =>
#endif
                {
                    if (choice == 0) StartCoroutine(Begin());
                    if (choice == 1) Load();
                    if (choice == 2) UnityEngine.SceneManagement.SceneManager.LoadScene("Kitchen");
                    if (choice == 3) Application.Quit();
                }, choice => choice != 1 || Act1Story.TryRead(saveFile, out _), coverMenu: true);
        }

        IEnumerator Begin()
        {
            atTitle = false; busy = true; HUD.HideMenu();
            story = new Act1Story();
            Enter(RoomId.Cemetery);
            HUD.Picture(Gravestone);
            HUD.ChapterCard(true, funeral: true);
            if (qa) yield return Capture("00b-chapter", 200);
            yield return Delay(1.25f);
            const float duration = .86f;
            bool capturedDissolve = false;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
            {
                HUD.PresentationOpacity = Mathf.SmoothStep(1, 0, elapsed / duration);
                if (qa && !capturedDissolve && HUD.PresentationOpacity < .65f && HUD.PresentationOpacity > .12f)
                {
                    capturedDissolve = true;
                    yield return Capture("00c-chapter-dissolve");
                }
                yield return null;
            }
            if (qa && !capturedDissolve) throw new InvalidOperationException("Chapter presentation did not dissolve during gameplay.");
            HUD.PresentationOpacity = 0;
            HUD.ChapterCard(false);
            HUD.Picture(null);
            HUD.PresentationOpacity = 1;
            yield return Say("Grace", "Everyone wore black today.", expression: Expression.Quiet);
            yield return Delay(.45f);
            yield return Say("Grace", "Even Uncle James wore a tie. He never wears ties.", expression: Expression.Quiet);
            FaceSpeaker(TargetInRoom(TargetId.Mom));
            yield return Say("Mom", "See the white flowers past the first crossing, on the right? That’s Grandma’s spot.");
            yield return Say("Mom", "You can go ahead. I’ll stay here by the gate.", expression: Expression.Warm);
            Enable();
        }

        void Enter(RoomId id, bool returnDoor = false, Vector2? savedFeet = null)
        {
            HUD.ShowButterfly(WorldCamera, null);
            butterflyGuiding = false;
            room = Array.Find(Rooms, entry => entry.Id == id);
            foreach (var entry in Rooms) entry.gameObject.SetActive(entry == room);
            story.Room = id;
            gateCallLatched = false;
            Grace.gameObject.SetActive(true);
            Grace.Place(savedFeet ?? (returnDoor ? room.ReturnEntry : room.Entry));
            // Older saves may contain feet inside furniture that subsequently gained
            // a full footprint. Recover onto the floor before enabling movement.
            if (savedFeet.HasValue)
            {
                float radius = Grace.Feet.radius * Grace.Feet.transform.lossyScale.x;
                for (int step = 0; step < 64 && Physics2D.OverlapCircle(Grace.Body.position, radius, 1 << 8); step++)
                    Grace.Place(Grace.Body.position + Vector2.down * .12f);
                if (Physics2D.OverlapCircle(Grace.Body.position, radius, 1 << 8)) Grace.Place(room.Entry);
            }
            if (id == RoomId.Flashback) Grace.LookAt(new Vector2(6, 6.5f));
            if (id == RoomId.Cemetery)
            {
                room.Dad.SetCrying(story.Stage == StoryStage.FindDad);
                room.Dad.Look(TargetInRoom(TargetId.Grave).FocusArt.transform.position - room.Dad.transform.position);
            }
            WorldCamera.orthographicSize = id switch
            {
                RoomId.Flashback => 4.4f,
                RoomId.Hall => 4.3f,
                RoomId.Cemetery => 3.38f,
                _ => 3.22f
            };
            WorldCamera.transform.position = id == RoomId.Flashback ? new Vector3(6, 4.5f, -10) : CameraPosition(Grace.Body.position);
            foreach (var target in room.Targets)
            {
                if (!target.ConditionalArt) continue;
                target.ConditionalArt.enabled = target.Id switch
                {
                    TargetId.Ribbon => !story.Ribbon,
                    TargetId.Book => !story.Book && !story.Shelved, TargetId.Shelf => story.Shelved, _ => true
                };
            }
            bool inMemory = id == RoomId.Hall || id == RoomId.Study;
            Song(music, inMemory && story.Stage == StoryStage.Complete ? null : room.Music);
            ambience.volume = inMemory ? .11f : .25f;
            Song(ambience, room.Ambience);
            Physics2D.SyncTransforms();
        }

        void Enable()
        {
            busy = false;
            HUD.HideDialogue(); HUD.Picture(null); HUD.ShowHUD(true);
            Grace.CanMove = true;
        }

        void Lock()
        {
            busy = true; Grace.CanMove = false;
            HUD.ShowHUD(false);
        }

        void Trigger(TargetId id)
        {
            if (busy || atTitle || NeedsCrouch(id) || id == TargetId.LeaveMemory && story.Stage != StoryStage.Complete) return;
            Lock();
            FaceSpeaker(TargetInRoom(id));
            StartCoroutine(Interact(id));
        }

        bool NeedsCrouch(TargetId id) => (id == TargetId.Ribbon || id == TargetId.Puddle) && !Grace.Crouched;

        internal static Sprite[] LoadGraceExpressions() => LoadExpressions("Grace", new[] { 0, 1, 2, 3, 4, 5, 6, 7 });

        internal static Sprite[] LoadGrandmaExpressions() => LoadExpressions("Grandma", new[] { 0, 1, 4, 2, 3, 5, 6, 7 });

        static Sprite[] LoadExpressions(string person, int[] cells)
        {
            var sheet = Resources.Load<Texture2D>("ExpressionSheets/" + person);
            if (!sheet || sheet.width % 4 != 0 || sheet.height % 2 != 0) return null;
            int width = sheet.width / 4, height = sheet.height / 2;
            return Array.ConvertAll(cells, cell => Sprite.Create(sheet,
                new Rect(cell % 4 * width, (1 - cell / 4) * height, width, height),
                new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect));
        }

        static Sprite Face(Sprite[] expressions, Expression expression, Sprite fallback) =>
            expressions != null ? expressions[(int)expression] : fallback;

        IEnumerator Say(string speaker, string line, string qaPicture = null, Expression expression = Expression.Neutral)
        {
            read = false;
            Sprite face = speaker switch
            {
                "Grace" => Face(graceExpressions, expression, GracePortrait),
                "Mom" or "Mom  ·  From the path" or "Mom  ·  By the path" => Face(momExpressions, expression, MomPortrait),
                "Dad" when room.Id == RoomId.Cemetery && room.Dad.Crying => Face(dadExpressions,
                    expression == Expression.Neutral ? Expression.Grief : expression, DadSadPortrait),
                "Dad" => Face(dadExpressions, expression, DadPortrait),
                "Grandma" => Face(grandmaExpressions, expression, GrandmaPortrait),
                "Grandpa" => GrandpaPortrait,
                "Uncle James" => UnclePortrait,
                "Aunt" => AuntPortrait,
                _ => null
            };
            HUD.Dialogue(speaker, line, face);
            if (qa && (face == null || !HUD.HasPortrait))
                throw new InvalidOperationException("Dialogue portrait missing for " + speaker);
            if (qa && qaPicture != null) yield return Capture(qaPicture);
            if (qa) yield return new WaitForSeconds(.10f);
            else yield return new WaitUntil(() => read);
            HUD.HideDialogue();
        }

        WaitForSeconds Delay(float time) => new WaitForSeconds(qa ? Mathf.Min(.2f, time) : time);

        IEnumerator Interact(TargetId target)
        {
            if (target >= TargetId.WiltedGrave && target <= TargetId.PebbleGrave)
            {
                string[] descriptions = {"The petals are all curled up. Someone tied the stems together.", "I can only read half the name. The rest is under the moss.",
                    "It smells like the leaves Grandma rubbed between her fingers.", "Three stones in a row. One’s just a little crooked."};
                int index = target - TargetId.WiltedGrave;
                yield return Say("Grace", descriptions[index]);
                if (story.VisitOldGrave(index) && story.Stage == StoryStage.Farewell)
                    yield return MomHeadstoneReminder();
                Enable(); yield break;
            }
            switch (target)
            {
                case TargetId.Grave:
                    HUD.Picture(Gravestone);
                    if (story.Stage == StoryStage.Farewell)
                    {
                        yield return Say("Grace", "Evelyn Hart. I can read it without Mom.");
                        yield return Say("Grace", "Is that her? Her hair looks different.", expression: Expression.Surprise);
                        yield return Delay(.65f);
                        yield return Say("Grace", "She knew my name then.", expression: Expression.Quiet);
                        yield return Delay(.85f);
                        HUD.Picture(null);
                        story.Stage = StoryStage.FindDad;
                        room.Dad.SetCrying(true);
                        if (qa) yield return Capture("01f-dad-after-headstone");
                    }
                    else yield return Say("Grace", "She is home now.");
                    break;
                case TargetId.WhiteFlowers:
                    yield return Say("Grace", "Those grew by Grandma’s door. The ones with the little yellow middles.", "01d-flowers-dialogue");
                    yield return Say("Grace", "She said they made her sneeze. I can’t remember when she stopped saying that.", expression: Expression.Concern);
                    break;
                case TargetId.Gate:
                    gateCalls++;
                    if (story.Stage == StoryStage.Farewell)
                        yield return Say("Mom  ·  By the path", "Gracie, go see Grandma’s headstone. Look for the white flowers on the right.");
                    else
                        yield return Say("Mom  ·  By the path", "Gracie! Back this way, love. I’m by the path.");
                    break;
                case TargetId.Window:
                    HUD.Picture(WindowView);
                    if (qa) yield return Capture("04-window");
                    yield return Say("Grace", "The swing’s still wet. Those are the same flowers she planted by the path.");
                    if (room.Id == RoomId.Study && story.Stage == StoryStage.ExploreMemory)
                    {
                        story.Stage = StoryStage.Complete;
                        HUD.Picture(null);
                        Song(music, null);
                        yield return Say("Grace", "I thought I might hear her in here.", expression: Expression.Quiet);
                        yield return Delay(.55f);
                        yield return Say("Grace", "But there's only the rain now. Mom's waiting for me.", expression: Expression.Quiet);
                    }
                    break;
                case TargetId.Dad:
                    if (story.Stage != StoryStage.FindDad)
                        yield return Say("Dad", story.Stage == StoryStage.Farewell ? "There you are. See those flowers? Auntie brought them for Grandma." : "I’m all right to stay here a minute.");
                    else yield return Flashback();
                    break;
                case TargetId.Mom:
                    if (story.Stage == StoryStage.Farewell)
                    {
                        yield return Say("Grace", "Mom? Where’s Grandma now? I mean… where did she go?", expression: Expression.Tense);
                        yield return Say("Mom", "I don’t know, sweetheart.", expression: Expression.Grief);
                        yield return Say("Mom", "Come find me if you want to talk, all right? I’ll be here.", "01e-facing");
                    }
                    else if (story.Stage != StoryStage.FindMom)
                        yield return Say("Mom", "Want me to come with you? It’s okay if you need another minute.");
                    else
                    {
                        HUD.Picture(ClosedParcel);
                        yield return Say("Mom", "I found this when we were going through Grandma’s things.");
                        yield return Say("Grace", "That’s my name. Did she write it?", expression: Expression.Surprise);
                        yield return Say("Mom", "I think so. I’ll put it on the bench, right next to my bag.");
                        story.Stage = StoryStage.OpenParcel;
                    }
                    break;
                case TargetId.Parcel:
                    if (story.Stage != StoryStage.OpenParcel)
                    {
                        yield return Say("Grace", "That’s Mom’s bag. I wonder who the box is for.");
                        break;
                    }
                    if (!story.ParcelOpened)
                    {
                        HUD.Picture(ClosedParcel);
                        yield return Say("Grace", "For Grace. The G looks just like the one on my birthday card.");
                        story.ParcelOpened = true;
                    }
                    HUD.Picture(OpenParcel);
                    int contents = -1;
                    HUD.Menu("Inside the parcel", "A letter, a ribbon, a photograph, a brass key and a wooden music box.",
                        new[] { "Pick Up the Music Box", "Close the Parcel" }, choice => { contents = choice; HUD.HideMenu(); }, photoMenu: true);
                    if (qa)
                    {
                        HUD.VerifyFocus(0);
                        yield return Capture("02c-parcel");
                        if (qaCloseParcelOverview)
                        {
                            HUD.MoveFocusForVerification(MoveDirection.Right);
                            HUD.VerifyFocus(1);
                            yield return Capture("02c-parcel-focus");
                        }
                        HUD.SubmitForVerification();
                    }
                    yield return new WaitUntil(() => contents >= 0);
                    if (contents == 1) break;
                    HUD.Picture(Mechanism);
                    bool wind = false;
                    bool picked = false;
                    HUD.Menu("Grandma’s music box", "The brass key turns on the side.", new[] {"Turn the Key", "Close the Lid"},
                        choice => { wind = choice == 0; picked = true; HUD.HideMenu(); }, photoMenu: true);
                    if (qa)
                    {
                        yield return Capture("02d-music-box");
                        HUD.ChooseForVerification(qaWind ? 0 : 1);
                    }
                    yield return new WaitUntil(() => picked);
                    if (wind)
                    {
                        yield return WindMusicBox();
                        story.Stage = StoryStage.FindStudy;
                        yield return ChangeRoom(RoomId.Hall);
                        yield return Say("Grace", "Grandma's hallway. I remember it like this.", expression: Expression.Quiet);
                        yield return Say("Grace", "A moment ago, I was by the bench...", expression: Expression.Concern);
                        yield return Delay(.5f);
                        yield return Say("Grace", "That song. It's coming from the back room.", expression: Expression.Quiet);
                    }
                    break;
                case TargetId.ToStudy:
                    yield return ChangeRoom(RoomId.Study);
                    if (story.Stage == StoryStage.FindStudy) story.Stage = StoryStage.ExploreMemory;
                    break;
                case TargetId.ToHall:
                    yield return ChangeRoom(RoomId.Hall, true);
                    break;
                case TargetId.LeaveMemory:
                    yield return IntoBedroom();
                    yield break;
                case TargetId.Ribbon:
                    story.Ribbon = true;
                    if (nearby && nearby.Id == target && nearby.ConditionalArt) nearby.ConditionalArt.enabled = false;
                    yield return Say("Grace", "A blue ribbon. Grandma could tie a bow with one hand while she held my hair with the other.");
                    break;
                case TargetId.Puddle:
                    HUD.Picture(PuddleReflection);
                    yield return Say("Grace", "I can see the flowers in the water. Then a drop lands and they’re gone.", "01g-puddle-reflection");
                    break;
                case TargetId.Photo:
                    story.Photo = true;
                    yield return Say("Grace", "That’s the picture from the box. My knees were all muddy that day.");
                    break;
                case TargetId.Uncle:
                    yield return Say("Uncle James", "She kept an extra tie in the hall for me. I never remembered to bring one.");
                    break;
                case TargetId.Aunt:
                    yield return Say("Aunt", "I got those flowers from her garden. Remember how she let you pick them in the rain?");
                    break;
                case TargetId.Grandfather:
                    yield return Say("Grandpa", "I put the kettle on this morning and waited for her to start humming. Funny what you still expect to hear.");
                    break;
                case TargetId.Book:
                    story.Book = true;
                    foreach (var item in room.Targets)
                        if (item.Id == TargetId.Book && item.ConditionalArt) item.ConditionalArt.enabled = false;
                    yield return Say("Grace", "There’s a flower in here. I think this goes on the shelf in the back room.");
                    break;
                case TargetId.Shelf:
                    if (story.Book)
                    {
                        story.Book = false; story.Shelved = true;
                        foreach (var item in room.Targets)
                            if (item.Id == TargetId.Shelf && item.ConditionalArt) item.ConditionalArt.enabled = true;
                        yield return Say("Grace", "Something fell out when I put the book back. ‘One more story, if there’s time.’");
                    }
                    else yield return Say("Grace", story.Shelved ? "The blue book fits right there." : "There’s a space between the books.");
                    break;
                default:
                    yield return Say("Grace", target switch
                    {
                        TargetId.Birdbath => "There used to be sparrows here. Grandma and I tried to count them, but they wouldn’t sit still.",
                        TargetId.Plaque => "My sleeve’s wet where I touched the gate. Grandma brought me through here in the spring.",
                        TargetId.Register => "Grandma signed this page, too. Look, there’s her name again on the next one.",
                        TargetId.Note => "The rain took most of the words. I can still read ‘Still here.’",
                        TargetId.OldCross => "The stone looks very old, but those flowers are fresh.",
                        TargetId.Memorial => "I can’t make out the name. Someone still comes to leave flowers.",
                        TargetId.FamilyPlot => "They have the same last name, like me and Dad.",
                        TargetId.Blanket => "This is the blanket she’d put over my knees. It’s scratchy on this side.",
                        TargetId.Sewing => "It’s the same blue as the ribbon. She kept little bits of thread for when things tore.",
                        TargetId.Letters => "That one’s for Dad. She wrote, ‘The kettle’s on.’",
                        TargetId.Tea => room.Id == RoomId.Study ? "That little cup’s mine. She used to fill it with milk." : "Grandma’s cup. I wasn’t allowed to carry it when it was full.",
                        _ => "I remember this."
                    });
                    break;
            }
            Enable();
        }

        IEnumerator Fade(float from, float to)
        {
            float duration = qa ? .08f : .29f;
            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                HUD.Fade(Mathf.Lerp(from, to, time / duration));
                yield return null;
            }
            HUD.Fade(to);
        }

        IEnumerator MomHeadstoneReminder()
        {
            var mom = TargetInRoom(TargetId.Mom).Speaker;
            Vector2 feet = Grace.Body.position;
            Vector3 from = CameraPosition(Grace.transform.position);
            Vector3 to = CameraPosition(mom.transform.position);
            mom.Look(feet - (Vector2)mom.transform.position);
            cemeteryPan = true;
            yield return PanCamera(from, to);
            yield return Say("Mom  ·  From the path", "Gracie, Grandma’s headstone is by the white flowers. Go and see her spot, love.",
                "01h-fourth-grave-pan");
            if (qa && (Vector3.Distance(from, to) < .5f || Vector3.Distance(WorldCamera.transform.position, to) > .01f ||
                       Vector2.Distance(Grace.Body.position, feet) > .01f))
                throw new InvalidOperationException("The fourth old grave must pan to Mom while Grace stays at the grave.");
            yield return PanCamera(to, from);
            cemeteryPan = false;
            if (qa)
            {
                if (Vector3.Distance(WorldCamera.transform.position, from) > .01f ||
                    Vector2.Distance(Grace.Body.position, feet) > .01f)
                    throw new InvalidOperationException("The camera did not return to Grace after Mom’s reminder.");
                qaMomPans++;
            }
        }

        IEnumerator PanCamera(Vector3 from, Vector3 to)
        {
            float duration = qa ? .14f : .72f;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
            {
                WorldCamera.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / duration));
                yield return null;
            }
            WorldCamera.transform.position = to;
        }

        IEnumerator WindMusicBox()
        {
            float duration = qa ? .24f : 1.75f;
            float originalRain = ambience.volume;
            bool firstClick = false, secondClick = false, playing = false;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
            {
                float progress = elapsed / duration;
                HUD.WindMusicBox(progress);
                ambience.volume = originalRain * (1 - .72f * Mathf.SmoothStep(0, 1, progress));
                if (!firstClick && progress >= .30f)
                {
                    firstClick = true;
                    if (windingClick) effects.PlayOneShot(windingClick, .48f);
                }
                if (!secondClick && progress >= .54f)
                {
                    secondClick = true;
                    if (windingClick) effects.PlayOneShot(windingClick, .42f);
                }
                if (!playing && progress >= .76f)
                {
                    playing = true;
                    Song(music, BoxMusic);
                }
                yield return null;
            }
            HUD.WindMusicBox(1);
            if (!playing) Song(music, BoxMusic);
            yield return Delay(.45f);
            // The room transition swaps the rain loop while the screen is dark.
            ambience.Stop();
            ambience.volume = originalRain;
        }

        IEnumerator ChangeRoom(RoomId id, bool returnDoor = false)
        {
            yield return Fade(0, 1);
            HUD.Picture(null);
            Enter(id, returnDoor);
            yield return Fade(1, 0);
            if (qa && id == RoomId.Flashback)
            {
                Vector2 entry = Grace.Body.position;
                Grace.Place(new Vector2(437f / 96f, (864f - 310f) / 96f));
                if (Grace.Resolve(Vector2.up).y > .30f)
                    throw new InvalidOperationException("The first flashback fireplace admits Grace from the front.");
                Grace.Place(new Vector2(330f / 96f, (864f - 235f) / 96f));
                if (Grace.Resolve(Vector2.right).x > .19f)
                    throw new InvalidOperationException("The first flashback fireplace admits Grace from the side.");
                yield return Capture("02c-flashback-fireplace-side");
                Enter(RoomId.Flashback, savedFeet: new Vector2(437f / 96f, (864f - 235f) / 96f));
                if (Physics2D.OverlapCircle(Grace.Body.position, Grace.Feet.radius * Grace.Feet.transform.lossyScale.x, 1 << 8))
                    throw new InvalidOperationException("An older save left Grace embedded in the flashback hearth.");
                yield return Capture("02d-flashback-old-save-recovery");
                Grace.Place(entry);
                yield return Capture("02-flashback");
                Debug.Log("REMEMBER_FLASHBACK_HEARTH_PASS: first flashback front and side collision verified.");
            }
        }

        IEnumerator WalkActor(ActorView actor, Vector2 end)
        {
            while (Vector2.Distance(actor.transform.position, end) > .004f)
            {
                Vector2 prior = actor.transform.position;
                actor.transform.position = Vector2.MoveTowards(prior, end, (qa ? 9f : 1.4f) * Time.deltaTime);
                actor.Pose((Vector2)actor.transform.position - prior, false, false, Time.deltaTime);
                yield return null;
            }
            actor.Face(Vector2.up); actor.Pose(Vector2.zero, false, false, 0);
        }

        IEnumerator IntoBedroom()
        {
            yield return Say("Grace", "The room slips away. I can hear the cemetery rain again.", expression: Expression.Quiet);
            yield return Fade(0, 1);
            HUD.Picture(WalkHomeInRain);
            ambience.volume = .18f;
            yield return Fade(1, 0);
            if (qa && (!WalkHomeInRain || !HUD.ShowsPicture(WalkHomeInRain)))
                throw new InvalidOperationException("The walk home illustration did not open.");
            yield return Say("Grace", "Later, Mom and I walked home under her umbrella.", "05a-walk-home", Expression.Quiet);
            yield return Fade(0, 1);
            HUD.Picture(null);
            HUD.ChapterCard(true);
            yield return Fade(1, 0);
            if (qa) yield return Capture("05b-act-two-bridge", 200);
            yield return Delay(1.8f);
            yield return Fade(0, 1);
            if (qa)
            {
                if (!Application.CanStreamedLevelBeLoaded("Kitchen"))
                    throw new InvalidOperationException("The Act II bedroom scene is unavailable.");
                Debug.Log("REMEMBER_ACT_II_BRIDGE_PASS: leave the empty memory and open Act II at home.");
#if !UNITY_WEBGL || UNITY_EDITOR
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-with-kitchen") < 0)
                {
                    Application.Quit(0);
                    yield break;
                }
#endif
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("Kitchen");
        }

        IEnumerator Flashback()
        {
            yield return Say("Grace", "Dad?", "02a-grace-talk", Expression.Concern);
            yield return Delay(.8f);
            yield return Say("Grace", "Dad.", expression: Expression.Concern);
            yield return Say("Dad", "Hey, Gracie. Just give me a second.", "02a-dad-portrait");
            yield return Say("Grace", "Are you crying?");
            yield return Delay(.55f);
            yield return Say("Dad", "Yeah. I miss her.", expression: Expression.Grief);
            yield return Delay(.7f);
            yield return Say("Grace", "I remember another evening at Grandma's house.", expression: Expression.Quiet);
            yield return ChangeRoom(RoomId.Flashback);
            room.Door.sprite = room.ClosedDoor;
            room.Grandma.transform.position = new Vector2(5.63f, 5.8f);
            room.Dad.transform.position = new Vector2(7.1f, 5.55f);
            room.Grandma.Look(Vector2.up);
            room.Dad.Look(room.Grandma.transform.position - room.Dad.transform.position);
            yield return Say("Grandma", "I’d better get home. It’s getting late.", expression: Expression.Tense);
            Vector2 threshold = room.Door.transform.position;
            // Leave room for the whole sprite to stand on the floor in front of the door.
            Vector2 grandmaMark = threshold + new Vector2(-.29f, -.71f);
            Vector2 dadMark = threshold + new Vector2(.41f, -.71f);
            yield return WalkActor(room.Grandma, grandmaMark);
            room.Door.sprite = room.OpenDoor;
            doorOpened = true;
            if (Hinge) effects.PlayOneShot(Hinge);
            yield return Say("Dad", "Mom, this is your house. Come and sit with me.", expression: Expression.Concern);
            yield return WalkActor(room.Dad, dadMark);
            room.Dad.Face(Vector2.up);
            if (qa) yield return Capture("02b-door");
            if (Latch) effects.PlayOneShot(Latch);
            room.Door.sprite = room.ClosedDoor;
            Vector2 grandmaFeet = room.Grandma.transform.position;
            Vector2 dadFeet = room.Dad.transform.position;
            doorClosedBeside = Mathf.Abs(grandmaFeet.y - dadFeet.y) < .05f &&
                grandmaFeet.y <= threshold.y - .65f && dadFeet.x - grandmaFeet.x > .64f &&
                dadFeet.x > threshold.x && grandmaFeet.x < threshold.x;
            yield return Say("Grandma", "Grace? Can you open the door for me?", expression: Expression.Surprise);
            room.Dad.Look(room.Grandma.transform.position - room.Dad.transform.position);
            yield return Say("Dad", "Mom, please. We’ve been over this. I can’t do it again.", expression: Expression.Tense);
            Grace.Place(Grace.Body.position + Vector2.down * .24f);
            Grace.LookAt(new Vector2(6, 6.5f));
            yield return Delay(.6f);
            story.Stage = StoryStage.FindMom;
            yield return ChangeRoom(RoomId.Cemetery, true);
            FaceSpeaker(TargetInRoom(TargetId.Dad));
            yield return Say("Dad", "Gracie? You all right?");
            yield return Say("Grace", "Mm-hm.");
            var grave = TargetInRoom(TargetId.Grave);
            Grace.LookAt(grave.FocusArt.transform.position);
            yield return Say("Grace", "He sounded so mad that night. I didn’t know he missed her this much.", expression: Expression.Grief);
            yield return Delay(.8f);
            FaceSpeaker(TargetInRoom(TargetId.Mom));
            yield return Say("Mom", "Grace? When you’re ready, come see me by the bench. I found something of Grandma’s.");
        }

        void Pause()
        {
            Grace.CanMove = false; menuSave = false;
            HUD.Menu("Paused", Objective(), new[] {"Resume", "Inventory", "Save Game", "Load Game", "Return to Title"},
                choice =>
                {
                    if (choice == 0) CloseMenu();
                    if (choice == 1) Inventory();
                    if (choice == 2) Save();
                    if (choice == 3) Load();
                    if (choice == 4) Title();
                }, choice => choice != 3 || Act1Story.TryRead(saveFile, out _));
        }
        void Inventory()
        {
            Grace.CanMove = false;
            var items = new System.Collections.Generic.List<(string Name, Sprite Art)>();
            Sprite Art(TargetId id)
            {
                foreach (var scene in Rooms)
                    foreach (var target in scene.Targets)
                        if (target.Id == id && target.FocusArt) return target.FocusArt.sprite;
                return OpenParcel;
            }
            if (story.Stage == StoryStage.OpenParcel && !story.ParcelOpened) items.Add(("Cardboard parcel", ClosedParcel));
            if (story.ParcelOpened)
            {
                items.Add(("Music box & key", Mechanism));
                items.Add(("Grandma’s letter", OpenParcel));
            }
            if (story.Ribbon) items.Add(("Blue ribbon", Art(TargetId.Ribbon)));
            if (story.Photo) items.Add(("Family photograph", Art(TargetId.Photo)));
            if (story.Book) items.Add(("Blue storybook", Art(TargetId.Book)));
            HUD.Inventory(items, Pause);
        }
        void Save()
        {
            story.Feet = Grace.Body.position;
            story.CemeteryLayout = 2;
            string result;
            try { story.Save(saveFile); result = "Your place and keepsakes were saved."; }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is PlayerPrefsException)
            { result = "Couldn’t write the save file."; }
            menuSave = true;
            HUD.Menu("Save Game", result, new[] {"Back"}, _ => Pause());
        }
        void Load()
        {
            if (!Act1Story.TryRead(saveFile, out var saved))
            {
                HUD.Menu("Load Game", "There is no readable save.", new[] {"Back"}, _ => { if (atTitle) Title(); else Pause(); });
                return;
            }
            story = saved; atTitle = false;
            HUD.HideMenu();
            if (story.Room == RoomId.Study && story.Stage == StoryStage.FindStudy)
                story.Stage = StoryStage.ExploreMemory;
            bool cemeteryMoved = story.Room == RoomId.Cemetery && story.CemeteryLayout < 2;
            Enter(story.Room, cemeteryMoved && story.Stage >= StoryStage.FindMom, cemeteryMoved ? null : story.Feet);
            Enable();
        }
        void CloseMenu()
        {
            if (menuSave) { Pause(); return; }
            HUD.HideMenu(); Grace.CanMove = true;
        }

        bool qaWind, doorOpened, doorClosedBeside;
        IEnumerator TestPuddle()
        {
            var target = TargetInRoom(TargetId.Puddle);
            Grace.Place(target.Approach);
            yield return null;
            if (nearby != target || !HUD.ShowsPrompt("C", "Crouch to look into the puddle"))
                throw new InvalidOperationException("The standing puddle prompt must show the crouch control.");
            Trigger(TargetId.Puddle);
            if (busy || HUD.ShowsPicture(PuddleReflection))
                throw new InvalidOperationException("The puddle opened while Grace was standing.");
            yield return Capture("01g-puddle-approach");

            Grace.SetInput(Vector2.zero, false, true);
            yield return null;
            if (!Grace.Crouched || !HUD.ShowsPrompt("E / Enter", "Look into the puddle"))
                throw new InvalidOperationException("The crouched puddle prompt must show the interaction control.");
            Trigger(TargetId.Puddle);
            if (!busy || !PuddleReflection || !HUD.ShowsPicture(PuddleReflection))
                throw new InvalidOperationException("The puddle illustration did not open while Grace was crouching.");
            yield return new WaitUntil(() => !busy);
            if (HUD.ShowsPicture(PuddleReflection))
                throw new InvalidOperationException("The puddle illustration stayed open after dialogue.");
        }

        IEnumerator TestAction(TargetId target, Vector2 offset = default)
        {
            var entry = Array.Find(room.Targets, item => item.Id == target);
            if (!entry) throw new InvalidOperationException("Smoke target not present: " + target);
            Grace.Place(entry.Approach + offset);
            if (entry.Speaker) Grace.LookAt(entry.Speaker.transform.position);
            if (NeedsCrouch(target)) Grace.SetInput(Vector2.zero, false, true);
            yield return new WaitForFixedUpdate();
            nearby = Closest(Grace.Body.position, Grace.View.Forward);
            if (!nearby || nearby.Id != target) throw new InvalidOperationException("Unreachable smoke interaction: " + target);
            if (target == TargetId.Mom && offset != Vector2.zero) yield return Capture("01e-mom-prompt");
            Trigger(nearby.Id);
            if (entry.Speaker)
            {
                Vector2 toSpeaker = (Vector2)entry.Speaker.transform.position - Grace.Body.position;
                if (Grace.View.Facing != ActorView.Direction(toSpeaker, -1) ||
                    entry.Speaker.Facing != ActorView.Direction(-toSpeaker, -1))
                    throw new InvalidOperationException("Dialogue sprites did not face each other: " + target);
            }
            yield return new WaitUntil(() => !busy);
        }
        IEnumerator Smoke()
        {
            yield return null;
            HUD.VerifyFocus(0);
            yield return Capture("00-title");
            HUD.SubmitForVerification();
            yield return new WaitUntil(() => !busy && !atTitle);
            if (!WorldCamera.orthographic || Rooms.Length != 4) { Application.Quit(2); yield break; }
            yield return Capture("01-cemetery");
            if (room.Size != new Vector2(22, 15) || room.GetComponentsInChildren<ActorView>().Length != 5 ||
                room.GetComponentInChildren<Act1Patrol>())
                throw new InvalidOperationException("The cemetery must contain only Grace’s five family NPCs.");
            Vector3 entryCamera = WorldCamera.transform.position;
            float entryZoom = WorldCamera.orthographicSize;
            qaWideView = true;
            WorldCamera.orthographicSize = room.Size.y / 2;
            WorldCamera.transform.position = new Vector3(room.Size.x / 2, room.Size.y / 2, -10);
            HUD.Locate(WorldCamera, null, null);
            yield return Capture("01b-cemetery-overview");
            WorldCamera.transform.position = entryCamera;
            WorldCamera.orthographicSize = entryZoom;
            qaWideView = false;
            Vector2 walkedFrom = Grace.Body.position;
            qaDirection = Vector2.up;
            yield return MotionFrames(17);
            float walked = Vector2.Distance(walkedFrom, Grace.Body.position);
            if (walked < .4f || Array.IndexOf(Grace.View.Walk, Grace.View.Renderer.sprite) < 0)
                throw new InvalidOperationException($"Walk input moved {walked:F2} units with sprite {Grace.View.Renderer.sprite.name}.");
            yield return Capture("01a-walk");
            qaDirection = Vector2.left;
            yield return null;
            yield return new WaitForFixedUpdate();
            if (Grace.Motion.x >= -.001f || Grace.Motion.y <= .001f)
                throw new InvalidOperationException("Steering must travel through an intermediate heading when the input turns.");
            qaDirection = Vector2.up;
            qaSprint = true;
            yield return MotionFrames();
            if (!Grace.Running || Array.IndexOf(Grace.View.Run, Grace.View.Renderer.sprite) < 0)
                throw new InvalidOperationException("Sprint input did not draw a run frame.");
            yield return Capture("01a-run");
            qaSprint = false;
            Grace.SetInput(qaDirection, false, true);
            yield return MotionFrames();
            if (!Grace.Crouched || Grace.Running || Array.IndexOf(Grace.View.Crouch, Grace.View.Renderer.sprite) < 0)
                throw new InvalidOperationException("Crouch input did not draw a crouch frame.");
            yield return Capture("01a-crouch");
            qaSprint = true;
            yield return MotionFrames();
            if (Grace.Crouched || !Grace.Running)
                throw new InvalidOperationException("Sprint input did not release the crouch.");
            qaDirection = Vector2.zero; qaSprint = false;
            Debug.Log("REMEMBER_MOTION_PASS: separate walk, sprint, and crouch poses; sprint exits crouch.");
            Grace.Place(new Vector2(11f, 1.18f));
            qaDirection = Vector2.down;
            float gateDeadline = Time.time + 1f;
            yield return new WaitUntil(() => HUD.ShowsDialogue("Mom  ·  By the path", "Grandma’s headstone") || Time.time >= gateDeadline);
            qaDirection = Vector2.zero;
            if (!HUD.ShowsDialogue("Mom  ·  By the path", "Grandma’s headstone"))
                throw new InvalidOperationException("Mom did not direct Grace from the gate to Grandma’s headstone.");
            yield return new WaitUntil(() => !busy);
            yield return MotionFrames();
            if (gateCalls != 1) throw new InvalidOperationException("Walking to the gate must trigger exactly one call while inside the region.");
            var mom = TargetInRoom(TargetId.Mom);
            foreach (var direction in new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down })
            {
                Vector2 point = (Vector2)mom.Footprint.bounds.center + direction * .55f;
                if (Closest(point, -direction) != mom) throw new InvalidOperationException($"Mom is inaccessible from {direction}.");
            }
            yield return TestAction(TargetId.Mom, new Vector2(.45f, .64f));
            yield return TestAction(TargetId.Aunt);
            yield return TestAction(TargetId.Uncle);
            yield return TestAction(TargetId.Register);
            yield return TestAction(TargetId.Plaque);
            yield return TestAction(TargetId.Note);
            yield return TestPuddle();
            yield return TestAction(TargetId.Birdbath);
            yield return TestAction(TargetId.Grandfather);
            yield return TestAction(TargetId.FamilyPlot);
            yield return TestAction(TargetId.OldCross);
            yield return TestAction(TargetId.Memorial);
            var grave = TargetInRoom(TargetId.Grave);
            Grace.Place(grave.Approach);
            WorldCamera.transform.position = CameraPosition(Grace.Body.position);
            yield return null;
            yield return Capture("01c-grave-guidance");
            if (!HUD.HasObjectiveArrow) throw new InvalidOperationException("The headstone objective arrow did not appear.");
            yield return TestAction(TargetId.WhiteFlowers);
            yield return TestAction(TargetId.Ribbon);
            if (!story.Ribbon) { Application.Quit(8); yield break; }
            for (int index = 0; index < 4; index++) yield return TestAction((TargetId)((int)TargetId.WiltedGrave + index));
            yield return TestAction(TargetId.WiltedGrave);
            if (story.Graves != 15 || !story.MomCalled || qaMomPans != 1 || story.Stage != StoryStage.Farewell)
                throw new InvalidOperationException("The four unique graves must trigger exactly one reminder before reading Grandma’s headstone.");
            yield return TestAction(TargetId.Grave);
            if (!room.Dad.Crying || Array.IndexOf(room.Dad.Cry, room.Dad.Renderer.sprite) < 0)
                throw new InvalidOperationException("Dad did not switch to the crying sprite after the headstone interaction.");
            story.Graves &= 7;
            story.MomCalled = false;
            yield return TestAction(TargetId.PebbleGrave);
            if (story.Stage != StoryStage.FindDad || qaMomPans != 1)
                throw new InvalidOperationException("Visiting the last old grave after reading Grandma’s headstone must leave Dad’s objective active.");
            yield return TestAction(TargetId.Dad);
            if (!doorOpened || !doorClosedBeside) { Application.Quit(4); yield break; }
            yield return TestAction(TargetId.Mom);
            qaCloseParcelOverview = true;
            yield return TestAction(TargetId.Parcel);
            qaCloseParcelOverview = false;
            yield return TestAction(TargetId.Parcel);
            if (story.Room != RoomId.Cemetery || !story.ParcelOpened) { Application.Quit(5); yield break; }
            qaWind = true;
            yield return TestAction(TargetId.Parcel);
            if (story.Room != RoomId.Hall || story.Stage != StoryStage.FindStudy || room.GetComponentInChildren<ActorView>())
                throw new InvalidOperationException($"Winding the key left Grace in {story.Room} with stage {story.Stage}.");
            yield return Capture("03-hall-entry");
            // Probe the hearth from the front and the thin side strip that used to admit Grace.
            Grace.Place(new Vector2(437f / 96f, (864f - 310f) / 96f));
            if (Grace.Resolve(Vector2.up).y > .30f)
                throw new InvalidOperationException("Grace can enter the painted fireplace from the front.");
            Grace.Place(new Vector2(330f / 96f, (864f - 235f) / 96f));
            if (Grace.Resolve(Vector2.right).x > .19f)
                throw new InvalidOperationException("Grace can enter the painted fireplace from the side.");
            Debug.Log("REMEMBER_HEARTH_PASS: the fireplace blocks both front and side approaches.");
            Grace.Place(new Vector2(2.7f, 4.72f));
            yield return MotionFrames();
            var clear = Grace.Resolve(Vector2.up * .50f).y;
            var stop = Grace.Resolve(Vector2.up).y;
            Debug.Log($"Hall desk collision at {Grace.Body.position}: clear step {clear:F2}, stops at {stop:F2}.");
            if (clear < .47f || stop > .72f ||
                GraceMotor.ClampInput(Vector2.one).magnitude > 1.001f) { Application.Quit(6); yield break; }
            var nearDesk = new Vector2(2.7f, 4.72f + stop);
            Grace.Place(nearDesk);
            Grace.View.Look(Vector2.down);
            Grace.SetInput(Vector2.left, false, false);
            Grace.SetInput(Vector2.zero, false, false);
            yield return MotionFrames();
            if (Grace.View.Facing != 1 || Vector2.Distance(nearDesk, Grace.Body.position) > .005f)
                throw new InvalidOperationException("A direction tap should turn Grace without stepping.");
            qaDirection = Vector2.up;
            yield return MotionFrames();
            if (Grace.View.Facing != 3 || Vector2.Distance(nearDesk, Grace.Body.position) > .025f || Grace.View.Renderer.sprite != Grace.View.Idle[3])
                throw new InvalidOperationException("Grace did not turn toward the desk while her movement was blocked.");
            qaDirection = Vector2.zero;
            yield return Capture("03a-facing-blocked");
            yield return Capture("03-hall");
            var window = TargetInRoom(TargetId.Window);
            if (window.CanReach(window.Approach + Vector2.up * .60f, out _))
                throw new InvalidOperationException("The window can be used through the north wall.");
            yield return TestAction(TargetId.Window);
            yield return TestAction(TargetId.Blanket);
            yield return TestAction(TargetId.Photo);
            yield return TestAction(TargetId.Sewing);
            yield return TestAction(TargetId.Tea);
            yield return TestAction(TargetId.Book);
            yield return TestAction(TargetId.ToStudy);
            var exit = TargetInRoom(TargetId.LeaveMemory);
            if (story.Stage != StoryStage.ExploreMemory || room.GetComponentInChildren<ActorView>() || !exit ||
                Closest(exit.Approach, Vector2.up) == exit)
                throw new InvalidOperationException("The back room must be empty, with the exit hidden until Grace looks outside.");
            yield return Capture("04-empty-study");
            yield return TestAction(TargetId.Shelf);
            yield return TestAction(TargetId.Letters);
            yield return TestAction(TargetId.Tea);
            yield return TestAction(TargetId.Window);
            yield return TestAction(TargetId.ToHall);
            yield return TestAction(TargetId.ToStudy);
            Grace.Place(exit.Approach);
            yield return null;
            if (story.Stage != StoryStage.Complete || !HUD.ShowsPrompt("E / Enter", "Leave the memory"))
                throw new InvalidOperationException("The bay-window memory did not open a path out.");
            Save();
            var savedFeet = Grace.Body.position;
            Title(); Load();
            if (story.Stage != StoryStage.Complete || !story.Shelved || story.Room != RoomId.Study ||
                Vector2.Distance(savedFeet, Grace.Body.position) > .01f) { Application.Quit(7); yield break; }
            Pause();
            HUD.VerifyFocus(0);
            HUD.MoveFocusForVerification(MoveDirection.Down);
            HUD.VerifyFocus(1);
            HUD.HoverForVerification(4);
            HUD.VerifyFocus(4);
            HUD.MoveFocusForVerification(MoveDirection.Up);
            HUD.VerifyFocus(3);
            yield return Capture("05-pause");
            yield return new WaitForSeconds(1);
            Debug.Log("REMEMBER_SMOKE_PASS: cover dissolve, movement and focus, Mom’s gate and fourth-grave directions, crouched puddle, Dad’s flashback, empty rainy memory and save/load.");
            CloseMenu();
            yield return TestAction(TargetId.LeaveMemory);
        }

        static IEnumerator MotionFrames(int count = 12)
        {
            yield return null;
            for (int i = 0; i < count; i++) yield return new WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();
        }
        IEnumerator Capture(string name, int minimumLit = 1500)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            yield return null;
#else
            string location = Path.Combine(Application.dataPath, "..", "..", "..", "QA");
            Directory.CreateDirectory(location);
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            var prior = RenderTexture.active;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            HUD.PrepareCapture(WorldCamera);
            yield return null;
            try
            {
                RenderPipeline.SubmitRenderRequest(WorldCamera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                texture.Apply();
                var pixels = texture.GetPixels32();
                int lit = 0;
                for (int index = 0; index < pixels.Length; index += 32)
                    if (pixels[index].r + pixels[index].g + pixels[index].b > 70) lit++;
                if (lit < minimumLit) throw new InvalidOperationException("Empty capture: " + name);
                File.WriteAllBytes(Path.Combine(location, name + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                HUD.FinishCapture();
                RenderTexture.active = prior;
                RenderTexture.ReleaseTemporary(target);
                Destroy(texture);
            }
#endif
        }
    }
}

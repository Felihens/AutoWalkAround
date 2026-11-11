using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AutoWalkAround
{
    public enum TurnStart { WhenClose, Early }

    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "felih.AutoWalkAround";
        public const string NAME = "Auto Walk Around";
        public const string VERSION = "1.0.4";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<KeyboardShortcut> ToggleKey;
        internal static ConfigEntry<TurnStart> StartTurning;
        internal static ConfigEntry<float> LookAhead;
        internal static ConfigEntry<float> MaxTurn;
        internal static ConfigEntry<bool> PauseWhileStrafing;
        internal static ConfigEntry<bool> AvoidCreatures;
        internal static ConfigEntry<bool> AutoJump;
        internal static ConfigEntry<bool> JumpWhenStuck;
        internal static ConfigEntry<float> MaxJumpHeight;
        private static int _survives = -1;
        // Checked on first use: other plugins may load after this one.
        internal static bool AutoRunSurvivesJump
        {
            get
            {
                if (_survives < 0) _survives = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Pip.ImprovedAutoRunAndJump") ? 1 : 0;
                return _survives == 1;
            }
        }
        internal static ConfigEntry<bool> AutoSprint;
        internal static ConfigEntry<float> SprintStartPercent;
        internal static ConfigEntry<float> SprintStopPercent;
        internal static ConfigEntry<bool> DetailedLog;
        internal static ConfigEntry<bool> IgnorePlants;

        private void Awake()
        {
            Log = Logger;
            Enabled = Config.Bind("1 - General", "Enabled", true,
                "While auto-running, steer around trees, rocks, fences and walls in your way, then carry on in the same direction.");
            ToggleKey = Config.Bind("1 - General", "ToggleKey", new KeyboardShortcut(KeyCode.None),
                "Optional key that turns the steering on and off in game. None = no key.");
            StartTurning = Config.Bind("2 - Steering", "StartTurning", TurnStart.WhenClose,
                "WhenClose: keep running until you're right at the obstacle, then go along it until the auto-run direction is clear again. " +
                "Early: start turning LookAhead metres before the obstacle.");
            LookAhead = Config.Bind("2 - Steering", "LookAhead", 3f,
                new ConfigDescription("How far ahead to look for obstacles, in metres, when StartTurning is Early.", new AcceptableValueRange<float>(1.5f, 8f)));
            MaxTurn = Config.Bind("2 - Steering", "MaxTurn", 90f,
                new ConfigDescription("The widest turn used to get around something, in degrees.", new AcceptableValueRange<float>(30f, 120f)));
            PauseWhileStrafing = Config.Bind("3 - Compatibility", "PauseWhileStrafing", true,
                "Stop steering while you strafe by hand (with Improved Auto-run and Jump, strafing doesn't end auto-run). Turn off if you never strafe during auto-run.");
            AvoidCreatures = Config.Bind("2 - Steering", "AvoidCreatures", false,
                "Also steer around creatures and other players. Off by default because moving creatures make the path wobble.");
            IgnorePlants = Config.Bind("2 - Steering", "IgnorePlants", true,
                "Run straight through bushes, saplings (young trees still growing) and berry or crop plants instead of steering around them.");
            AutoJump = Config.Bind("2 - Steering", "AutoJump", false,
                "Jump over low obstacles (fallen logs, small rocks) instead of going around them. Keep this off unless auto-run survives a jump (Improved Auto-run and Jump does that).");
            JumpWhenStuck = Config.Bind("2 - Steering", "JumpWhenStuck", true,
                "When you get stuck on something low (a fallen log), jump over it. Only used with Improved Auto-run and Jump installed, since a jump ends the game's own auto-run.");
            MaxJumpHeight = Config.Bind("2 - Steering", "MaxJumpHeight", 1.1f,
                new ConfigDescription("The tallest obstacle, in metres, that is jumped over. Anything taller is steered around. " +
                    "A jump is only made when the obstacle is taller than a step, there's room above it and ground behind it, " +
                    "and getting around it would take a wide turn.", new AcceptableValueRange<float>(0.6f, 1.8f)));
            AutoSprint = Config.Bind("4 - Sprint", "AutoSprint", true,
                "While auto-running, sprint whenever your stamina is full, stop when it runs out, wait for it to fill up again, and repeat.");
            SprintStartPercent = Config.Bind("4 - Sprint", "StartAtPercent", 100f,
                new ConfigDescription("Stamina (in % of max) needed to start sprinting again.", new AcceptableValueRange<float>(10f, 100f)));
            SprintStopPercent = Config.Bind("4 - Sprint", "StopAtPercent", 0f,
                new ConfigDescription("Stamina (in % of max) at which sprinting stops. 0 = run until it's empty.", new AcceptableValueRange<float>(0f, 90f)));
            DetailedLog = Config.Bind("9 - Debug", "DetailedLog", false, "Writes details to the BepInEx log.");
            new Harmony(GUID).PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo(NAME + " " + VERSION + " loaded");
        }

        private void Update()
        {
            if (ToggleKey.Value.MainKey == KeyCode.None || !ToggleKey.Value.IsDown()) return;
            if (Chat.instance != null && Chat.instance.HasFocus()) return;
            if (Console.IsVisible() || TextInput.IsVisible()) return;
            Enabled.Value = !Enabled.Value;
            if (Player.m_localPlayer != null)
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Auto-run steering " + (Enabled.Value ? "on" : "off"));
        }

        internal static void Debug(string msg)
        {
            if (DetailedLog.Value) Log.LogInfo(msg);
        }
    }

    // Sprint cycle while auto-running: sprint on full stamina, stop when it runs out, rest until full, repeat.
    // Your sprint key (Shift) switches it by hand: press to stop sprinting (the cycle then waits for you),
    // press again to sprint. Sets the "run" input the game reads, after Improved Auto-run and Jump.
    [HarmonyPatch(typeof(Player), "SetControls")]
    internal static class SprintCycle
    {
        private static bool _sprinting;
        private static bool _wasAuto;
        private static bool _heldOff;       // you switched sprint off: don't start again on your own
        private static bool _keyWasDown;

        [HarmonyPrefix]
        [HarmonyAfter("Pip.ImprovedAutoRunAndJump")]
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(Player __instance, ref bool run, bool ___m_autoRun)
        {
            if (__instance != Player.m_localPlayer) return;
            bool keyDown = RunKeyDown();
            bool pressed = keyDown && !_keyWasDown;
            _keyWasDown = keyDown;

            if (!Plugin.Enabled.Value || !Plugin.AutoSprint.Value || !___m_autoRun
                || __instance.IsAttached() || __instance.IsSwimming() || __instance.IsDead())
            {
                if (_wasAuto) { _wasAuto = false; _sprinting = false; _heldOff = false; }
                return;
            }
            _wasAuto = true;

            float max = __instance.GetMaxStamina();
            if (max <= 0f) return;
            float pct = __instance.GetStamina() / max * 100f;

            if (pressed)
            {
                _sprinting = !_sprinting;
                _heldOff = !_sprinting;
                Plugin.Debug(_sprinting ? "[sprint] your key: sprinting" : "[sprint] your key: stopped, waiting for you");
            }
            else if (!_sprinting && !_heldOff && pct >= Plugin.SprintStartPercent.Value - 0.5f)
            {
                _sprinting = true;
                Plugin.Debug("[sprint] stamina full, sprinting");
            }
            else if (_sprinting && (pct <= Plugin.SprintStopPercent.Value + 0.5f || pct < 1f))
            {
                _sprinting = false;
                Plugin.Debug("[sprint] out of stamina, resting");
            }
            run = _sprinting;
        }

        private static bool RunKeyDown()
        {
            if (Chat.instance != null && Chat.instance.HasFocus()) return false;
            if (Console.IsVisible() || Menu.IsVisible() || TextInput.IsVisible() || InventoryGui.IsVisible()) return false;
            return ZInput.GetButton("Run") || ZInput.GetButton("JoyRun");
        }
    }

    // Runs after the game (and after Improved Auto-run and Jump) has set the auto-run direction,
    // and tilts that direction only while something is in the way.
    [HarmonyPatch(typeof(Player), "SetControls")]
    internal static class Steering
    {
        private static float _angle;        // current tilt from the auto-run heading, degrees (+ = right)
        private static int _side;           // side we are going around on: +1 right, -1 left, 0 none
        private static float _holdUntil;    // keep the tilt at least until this time
        private static float _slowSince = -1f;
        private static float _lastJump;
        private static bool _wasSteering;
        private const float CloseDistance = 0.7f;   // WhenClose: metres in front of you that count as touching
        private static int _lastSide;
        private static float _lastSideUntil;
        private static int _stuckCount;     // stuck again soon after the last time = same obstacle
        private static float _lastStuck = -100f;

        private static int _mask;
        private static int _maskWithCreatures;

        private static int Mask()
        {
            if (_mask == 0)
            {
                _mask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "vehicle", "viewblock");
                _maskWithCreatures = _mask | LayerMask.GetMask("character", "character_net", "character_noenv");
            }
            return Plugin.AvoidCreatures.Value ? _maskWithCreatures : _mask;
        }

        [HarmonyPostfix]
        [HarmonyAfter("Pip.ImprovedAutoRunAndJump")]
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(Player __instance, Vector3 movedir, bool ___m_autoRun, ref Vector3 ___m_moveDir)
        {
            if (__instance != Player.m_localPlayer) return;
            if (!Plugin.Enabled.Value || !___m_autoRun || __instance.IsAttached() || __instance.IsSwimming()
                || __instance.IsDead() || __instance.IsTeleporting())
            {
                Reset();
                return;
            }
            if (Plugin.PauseWhileStrafing.Value && Mathf.Abs(movedir.x) > 0.1f)
            {
                Reset();
                return;
            }

            Vector3 flat = ___m_moveDir; flat.y = 0f;
            float mag = flat.magnitude;
            if (mag < 0.01f) { Reset(); return; }
            Vector3 heading = flat / mag;
            Vector3 pos = __instance.transform.position;
            float now = Time.time;
            bool early = Plugin.StartTurning.Value == TurnStart.Early;
            // WhenClose: only react once the obstacle is about an arm's length away.
            float ahead = early ? Plugin.LookAhead.Value : CloseDistance;

            // Mid-jump: keep going straight over it instead of turning away in the air.
            if (now - _lastJump < 0.9f && !__instance.IsOnGround())
            {
                if (_angle != 0f) ___m_moveDir = Turn(heading, _angle) * mag + Vector3.up * ___m_moveDir.y;
                return;
            }

            if (Plugin.AutoJump.Value && TryJump(__instance, pos, heading, now))
            {
                // jumping along the auto-run line: drop any turn so the jump goes straight over
                _angle = 0f; _side = 0; _wasSteering = false;
                return;
            }

            if (Blocked(pos, heading, ahead) && _angle != 0f && !Blocked(pos, Turn(heading, _angle), ahead))
            {
                // Still going along the obstacle and the current line is clear: keep it,
                // instead of re-picking a sharper or flatter turn every frame (that made it zigzag into long logs).
                _holdUntil = now + 0.35f;
            }
            else if (Blocked(pos, heading, ahead))
            {
                float pick = FindOpening(pos, heading, ahead);
                if (!_wasSteering) Plugin.Debug("[steer] obstacle ahead, turning " + pick.ToString("0") + " degrees");
                _angle = pick;
                _side = pick >= 0f ? 1 : -1;
                _holdUntil = now + 0.35f;
                _wasSteering = true;
            }
            else if (now > _holdUntil && _angle != 0f)
            {
                // Path ahead is clear again: ease back onto the auto-run heading.
                float next = Mathf.MoveTowards(_angle, 0f, 120f * Time.fixedDeltaTime);
                if (!Blocked(pos, Turn(heading, next), early ? ahead * 0.6f : ahead)) _angle = next;
                if (_angle == 0f)
                {
                    if (_wasSteering) Plugin.Debug("[steer] back on course");
                    // Remember the side for a moment: the same log often shows up again right away.
                    if (_side != 0) { _lastSide = _side; _lastSideUntil = now + 3f; }
                    _side = 0;
                    _wasSteering = false;
                }
            }

            Unstick(__instance, now, pos, Turn(heading, _angle));

            if (_angle != 0f) ___m_moveDir = Turn(heading, _angle) * mag + Vector3.up * ___m_moveDir.y;
        }

        // Pick the smallest turn that clears the way, preferring the side we are already going around on.
        private static float FindOpening(Vector3 pos, Vector3 heading, float ahead)
        {
            float max = Plugin.MaxTurn.Value;
            int first = _side != 0 ? _side : (Time.time < _lastSideUntil ? _lastSide : 0);
            for (float a = 15f; a <= max + 0.1f; a += 15f)
            {
                if (first != 0)
                {
                    if (!Blocked(pos, Turn(heading, a * first), ahead)) return a * first;
                    continue;
                }
                bool right = !Blocked(pos, Turn(heading, a), ahead);
                bool left = !Blocked(pos, Turn(heading, -a), ahead);
                if (right && left) return Clearer(pos, heading, a, ahead);
                if (right) return a;
                if (left) return -a;
            }
            if (first != 0)
            {
                // Our side is shut all the way round: try the other one before giving up.
                for (float a = 15f; a <= max + 0.1f; a += 15f)
                    if (!Blocked(pos, Turn(heading, -a * first), ahead)) return -a * first;
            }
            return (first != 0 ? first : 1) * max;
        }

        // Both sides open at the same angle: go the way with more room further out.
        private static float Clearer(Vector3 pos, Vector3 heading, float a, float ahead)
        {
            float r = Room(pos, Turn(heading, a + 15f), Plugin.LookAhead.Value * 1.5f);
            float l = Room(pos, Turn(heading, -a - 15f), Plugin.LookAhead.Value * 1.5f);
            return r >= l ? a : -a;
        }

        // Still stuck (pressed against something, often a long fallen log). Escalate step by step
        // instead of flipping sides at once, which made it go back and forth along the log:
        // 1) jump over it if it's low, 2) turn harder on the same side, 3) only then try the other side.
        private static void Unstick(Player p, float now, Vector3 pos, Vector3 moving)
        {
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb == null) return;
            Vector3 v = rb.linearVelocity; v.y = 0f;
            if (v.magnitude > 0.6f || !p.IsOnGround()) { _slowSince = -1f; return; }
            if (_slowSince < 0f) { _slowSince = now; return; }
            if (now - _slowSince < 0.6f) return;
            _slowSince = -1f;
            _wasSteering = true;

            _stuckCount = now - _lastStuck < 5f ? _stuckCount + 1 : 1;
            _lastStuck = now;

            if (_stuckCount == 1 && Plugin.JumpWhenStuck.Value && Plugin.AutoRunSurvivesJump && now - _lastJump > 1f
                && p.HaveStamina(p.m_jumpStaminaUsage) && IsLow(pos, moving))
            {
                _lastJump = now;
                p.Jump(false);
                Plugin.Debug("[steer] stuck on something low, jumping over it");
                return;
            }

            if (_side == 0)
                _side = Room(pos, Turn(moving, 90f), 6f) >= Room(pos, Turn(moving, -90f), 6f) ? 1 : -1;

            if (_stuckCount <= 2)
            {
                _angle = _side * Mathf.Min(Plugin.MaxTurn.Value + 30f, 120f);
                _holdUntil = now + 0.8f;
                Plugin.Debug("[steer] stuck, turning harder to the " + (_side > 0 ? "right" : "left"));
                return;
            }

            _side = -_side;
            _angle = _side * Mathf.Min(75f, Plugin.MaxTurn.Value);
            _holdUntil = now + 1.2f;
            _stuckCount = 0;
            Plugin.Debug("[steer] still stuck, trying the " + (_side > 0 ? "right" : "left"));
        }

        // Something at knee height with nothing at chest height: a log or small rock you can jump.
        // Measured instead of guessed: how tall it is, and whether there's room over it and ground behind it.
        private static bool IsLow(Vector3 pos, Vector3 dir)
        {
            float dist, height;
            string why;
            return Jumpable(pos, dir, 1.4f, out dist, out height, out why);
        }

        private const float StepHeight = 0.45f;     // the character walks over anything lower without jumping
        private const float CheapDetour = 45f;      // a way around within this turn is better than a jump

        // Auto-jump: only when something on the auto-run line is taller than a step, low enough to clear,
        // has room above and ground behind it, and going around it would take a wide turn (a long fallen log,
        // a low wall) rather than a small sidestep (a lone rock, a stump).
        // Returns true when it jumped.
        private static bool TryJump(Player p, Vector3 pos, Vector3 heading, float now)
        {
            if (now - _lastJump < 1f || !p.IsOnGround() || p.InAttack() || p.IsEncumbered()) return false;
            if (!p.HaveStamina(p.m_jumpStaminaUsage)) return false;
            Rigidbody rb = p.GetComponent<Rigidbody>();
            float speed = 0f;
            if (rb != null) { Vector3 v = rb.linearVelocity; v.y = 0f; speed = v.magnitude; }
            if (speed < 1.5f) return false;                 // standing or pushing against it: Unstick decides
            // take off early enough that the forward speed carries you over, faster = earlier
            float range = Mathf.Clamp(0.5f + speed * 0.15f, 0.8f, 1.8f);

            float dist, height;
            string why;
            if (!Jumpable(pos, heading, range, out dist, out height, out why))
            {
                if (why != null && now - _lastNoJumpLog > 1f) { _lastNoJumpLog = now; Plugin.Debug("[jump] not jumping: " + why); }
                return false;
            }
            if (dist < 0.3f) return false;                   // too close to clear it from here
            float around = DetourAngle(pos, heading, dist);
            if (around <= CheapDetour)
            {
                if (now - _lastNoJumpLog > 1f) { _lastNoJumpLog = now; Plugin.Debug("[jump] not jumping: a " + around.ToString("0") + " degree turn gets around it"); }
                return false;
            }
            _lastJump = now;
            p.Jump(false);
            Plugin.Debug("[jump] jumping over something " + height.ToString("0.00") + " m tall, " + dist.ToString("0.0") + " m ahead");
            return true;
        }
        private static float _lastNoJumpLog;

        // Is there something in front (within range) that is worth and safe to jump?
        // why = reason it isn't (null when there's simply nothing in the way)
        private static bool Jumpable(Vector3 pos, Vector3 dir, float range, out float dist, out float height, out string why)
        {
            dist = 0f; height = 0f; why = null;
            Collider col;
            // something at shin height, above what the character just steps over
            if (!Cast(pos + Vector3.up * (StepHeight + 0.12f), dir, range, 0.12f, out dist, out col)) return false;
            if (IsCreature(col)) { why = "it's a creature"; return false; }

            float maxJump = Plugin.MaxJumpHeight.Value;
            float d2;
            Collider c2;
            // still blocked just above the jump height: too tall (rock face, wall, big boulder)
            if (Cast(pos + Vector3.up * (maxJump + 0.3f), dir, dist + 0.6f, 0.2f, out d2, out c2)) { why = "too tall"; return false; }

            // climb a ladder of thin probes to find where it stops
            height = maxJump;
            for (float h = StepHeight + 0.27f; h <= maxJump + 0.01f; h += 0.15f)
                if (!Cast(pos + Vector3.up * h, dir, dist + 0.5f, 0.1f, out d2, out c2)) { height = h; break; }
            // and refine with a ray down onto its top, just past the front face
            RaycastHit top;
            Vector3 over = pos + dir * (dist + 0.35f);
            over.y = pos.y + maxJump + 0.25f;
            if (Physics.Raycast(over, Vector3.down, out top, maxJump + 0.25f, Mask(), QueryTriggerInteraction.Ignore) && !Passable(top.collider))
                height = Mathf.Max(height - 0.15f, top.point.y - pos.y);
            if (height <= StepHeight) { why = null; return false; }
            if (height > maxJump) { why = "too tall (" + height.ToString("0.00") + " m)"; return false; }

            // room for the body over it (low branches, a roof edge)
            if (Cast(pos + Vector3.up * (height + 0.9f), dir, dist + 1.5f, 0.3f, out d2, out c2)) { why = "no headroom over it"; return false; }

            // ground to land on behind it, not a drop or deep water
            RaycastHit land;
            Vector3 behind = pos + dir * (dist + 1.4f);
            behind.y = pos.y + height + 0.4f;
            if (!Physics.Raycast(behind, Vector3.down, out land, height + 0.4f + 2.5f, Mask(), QueryTriggerInteraction.Ignore))
            { why = "no ground to land on behind it"; return false; }
            if (land.normal.y < WalkableNormalY) { why = "the other side is too steep"; return false; }
            if (ZoneSystem.instance != null && land.point.y < ZoneSystem.instance.m_waterLevel - 0.4f) { why = "water behind it"; return false; }
            return true;
        }

        // Smallest turn (either side) that clears the obstacle; 999 when none within CheapDetour
        private static float DetourAngle(Vector3 pos, Vector3 dir, float dist)
        {
            for (float a = 15f; a <= CheapDetour + 0.1f; a += 15f)
                if (!Blocked(pos, Turn(dir, a), dist + 1.5f) || !Blocked(pos, Turn(dir, -a), dist + 1.5f)) return a;
            return 999f;
        }

        private static bool IsCreature(Collider col)
        {
            if (col == null) return false;
            return col.GetComponentInParent<Character>() != null;
        }

        // Nearest steep, solid surface along dir (creatures always count here, so they're never jumped)
        private static bool Cast(Vector3 from, Vector3 dir, float dist, float radius, out float at, out Collider col)
        {
            at = dist; col = null;
            int n = Physics.SphereCastNonAlloc(from, radius, dir, Hits, dist, MaskWithCreatures(), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                if (Hits[i].distance <= 0f) continue;
                if (Passable(Hits[i].collider)) continue;
                if (Hits[i].normal.y >= WalkableNormalY) continue;
                if (Hits[i].distance < at) { at = Hits[i].distance; col = Hits[i].collider; }
            }
            return col != null;
        }

        private static int MaskWithCreatures()
        {
            Mask();
            return _maskWithCreatures;
        }

        // Probe at knee and chest height, above the height the character just steps over.
        private static bool Blocked(Vector3 pos, Vector3 dir, float dist)
        {
            return Probe(pos + Vector3.up * 0.7f, dir, dist) || Probe(pos + Vector3.up * 1.4f, dir, dist);
        }

        // Only steep surfaces count. A rock or big stone block you are standing on, whose top rises
        // ahead of you, is ground you can walk up, not something to steer around.
        private const float WalkableNormalY = 0.6f;   // about 53 degrees of slope
        private static readonly RaycastHit[] Hits = new RaycastHit[16];

        private static bool Probe(Vector3 from, Vector3 dir, float dist)
        {
            int n = Physics.SphereCastNonAlloc(from, 0.35f, dir, Hits, dist, Mask(), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < n; i++)
            {
                if (Hits[i].distance <= 0f) continue;              // already touching at the start: no usable normal
                if (Passable(Hits[i].collider)) continue;
                if (Hits[i].normal.y < WalkableNormalY) return true;
            }
            return false;
        }

        private static float Room(Vector3 pos, Vector3 dir, float dist)
        {
            int n = Physics.SphereCastNonAlloc(pos + Vector3.up * 1f, 0.35f, dir, Hits, dist, Mask(), QueryTriggerInteraction.Ignore);
            float best = dist;
            for (int i = 0; i < n; i++)
            {
                if (Passable(Hits[i].collider)) continue;
                if (Hits[i].distance < best) best = Hits[i].distance;
            }
            return best;
        }

        // things you can run through anyway: bushes, saplings still growing, berry bushes and crops,
        // and anything on a layer the player doesn't bump into
        private static readonly System.Collections.Generic.Dictionary<int, bool> PassCache = new System.Collections.Generic.Dictionary<int, bool>();

        private static bool Passable(Collider col)
        {
            if (col == null) return true;
            Player p = Player.m_localPlayer;
            if (p != null && Physics.GetIgnoreLayerCollision(p.gameObject.layer, col.gameObject.layer)) return true;
            if (!Plugin.IgnorePlants.Value) return false;
            int id = col.GetInstanceID();
            bool pass;
            if (PassCache.TryGetValue(id, out pass)) return pass;
            pass = false;
            if (col.GetComponentInParent<Plant>() != null || col.GetComponentInParent<Pickable>() != null) pass = true;
            else
            {
                Transform root = col.transform.root;
                string name = (root != null ? root.name : col.name).ToLowerInvariant();
                if (name.Contains("bush") || name.Contains("shrub") || name.Contains("sapling")) pass = true;
            }
            if (PassCache.Count > 4000) PassCache.Clear();
            PassCache[id] = pass;
            if (pass) Plugin.Debug("[steer] ignoring " + (col.transform.root != null ? col.transform.root.name : col.name));
            return pass;
        }

        private static Vector3 Turn(Vector3 dir, float degrees)
        {
            return Quaternion.AngleAxis(degrees, Vector3.up) * dir;
        }

        private static void Reset()
        {
            if (_wasSteering) Plugin.Debug("[steer] stopped");
            _angle = 0f;
            _side = 0;
            _slowSince = -1f;
            _wasSteering = false;
        }
    }
}

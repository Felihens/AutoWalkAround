using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AutoWalkAround
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "felih.AutoWalkAround";
        public const string NAME = "Auto Walk Around";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<KeyboardShortcut> ToggleKey;
        internal static ConfigEntry<float> LookAhead;
        internal static ConfigEntry<float> MaxTurn;
        internal static ConfigEntry<bool> PauseWhileStrafing;
        internal static ConfigEntry<bool> AvoidCreatures;
        internal static ConfigEntry<bool> AutoJump;
        internal static ConfigEntry<bool> AutoSprint;
        internal static ConfigEntry<float> SprintStartPercent;
        internal static ConfigEntry<float> SprintStopPercent;
        internal static ConfigEntry<bool> DetailedLog;

        private void Awake()
        {
            Log = Logger;
            Enabled = Config.Bind("1 - General", "Enabled", true,
                "While auto-running, steer around trees, rocks, fences and walls in your way, then carry on in the same direction.");
            ToggleKey = Config.Bind("1 - General", "ToggleKey", new KeyboardShortcut(KeyCode.None),
                "Optional key that turns the steering on and off in game. None = no key.");
            LookAhead = Config.Bind("2 - Steering", "LookAhead", 3f,
                new ConfigDescription("How far ahead to look for obstacles, in metres.", new AcceptableValueRange<float>(1.5f, 8f)));
            MaxTurn = Config.Bind("2 - Steering", "MaxTurn", 90f,
                new ConfigDescription("The widest turn used to get around something, in degrees.", new AcceptableValueRange<float>(30f, 120f)));
            PauseWhileStrafing = Config.Bind("3 - Compatibility", "PauseWhileStrafing", true,
                "Stop steering while you strafe by hand (with Improved Auto-run and Jump, strafing doesn't end auto-run). Turn off if you never strafe during auto-run.");
            AvoidCreatures = Config.Bind("2 - Steering", "AvoidCreatures", false,
                "Also steer around creatures and other players. Off by default because moving creatures make the path wobble.");
            AutoJump = Config.Bind("2 - Steering", "AutoJump", false,
                "Jump over low obstacles (fallen logs, small rocks) instead of going around them. Keep this off unless auto-run survives a jump (Improved Auto-run and Jump does that).");
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
    // Sets the "run" input the game reads, after Improved Auto-run and Jump has had its say on it.
    [HarmonyPatch(typeof(Player), "SetControls")]
    internal static class SprintCycle
    {
        private static bool _sprinting;
        private static bool _wasAuto;

        [HarmonyPrefix]
        [HarmonyAfter("Pip.ImprovedAutoRunAndJump")]
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(Player __instance, ref bool run, bool ___m_autoRun)
        {
            if (__instance != Player.m_localPlayer) return;
            if (!Plugin.Enabled.Value || !Plugin.AutoSprint.Value || !___m_autoRun
                || __instance.IsAttached() || __instance.IsSwimming() || __instance.IsDead())
            {
                if (_wasAuto) { _wasAuto = false; _sprinting = false; }
                return;
            }
            _wasAuto = true;

            float max = __instance.GetMaxStamina();
            if (max <= 0f) return;
            float pct = __instance.GetStamina() / max * 100f;
            if (!_sprinting && pct >= Plugin.SprintStartPercent.Value - 0.5f)
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
            float ahead = Plugin.LookAhead.Value;

            if (Plugin.AutoJump.Value) TryJump(__instance, pos, heading, now);

            if (Blocked(pos, heading, ahead))
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
                if (!Blocked(pos, Turn(heading, next), ahead * 0.6f)) _angle = next;
                if (_angle == 0f)
                {
                    if (_wasSteering) Plugin.Debug("[steer] back on course");
                    _side = 0;
                    _wasSteering = false;
                }
            }

            Unstick(__instance, now);

            if (_angle != 0f) ___m_moveDir = Turn(heading, _angle) * mag + Vector3.up * ___m_moveDir.y;
        }

        // Pick the smallest turn that clears the way, preferring the side we are already going around on.
        private static float FindOpening(Vector3 pos, Vector3 heading, float ahead)
        {
            float max = Plugin.MaxTurn.Value;
            int first = _side != 0 ? _side : 0;
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
            float r = Room(pos, Turn(heading, a + 15f), ahead * 1.5f);
            float l = Room(pos, Turn(heading, -a - 15f), ahead * 1.5f);
            return r >= l ? a : -a;
        }

        // Still stuck (pressed against something the probes missed, like a low branch or a rock edge):
        // swing to the other side for a moment.
        private static void Unstick(Player p, float now)
        {
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb == null) return;
            Vector3 v = rb.linearVelocity; v.y = 0f;
            if (v.magnitude > 0.6f || !p.IsOnGround()) { _slowSince = -1f; return; }
            if (_slowSince < 0f) { _slowSince = now; return; }
            if (now - _slowSince < 0.6f) return;
            _side = _side == 0 ? 1 : -_side;
            _angle = _side * Mathf.Min(75f, Plugin.MaxTurn.Value);
            _holdUntil = now + 0.8f;
            _slowSince = -1f;
            _wasSteering = true;
            Plugin.Debug("[steer] stuck, trying the " + (_side > 0 ? "right" : "left"));
        }

        private static void TryJump(Player p, Vector3 pos, Vector3 heading, float now)
        {
            if (now - _lastJump < 1f || !p.IsOnGround()) return;
            bool low = Probe(pos + Vector3.up * 0.3f, heading, 1.2f);
            bool high = Probe(pos + Vector3.up * 1.1f, heading, 1.6f);
            if (low && !high)
            {
                _lastJump = now;
                p.Jump(false);
                Plugin.Debug("[steer] jumping over a low obstacle");
            }
        }

        // Probe at knee and chest height, above the height the character just steps over.
        private static bool Blocked(Vector3 pos, Vector3 dir, float dist)
        {
            return Probe(pos + Vector3.up * 0.7f, dir, dist) || Probe(pos + Vector3.up * 1.4f, dir, dist);
        }

        private static bool Probe(Vector3 from, Vector3 dir, float dist)
        {
            RaycastHit hit;
            return Physics.SphereCast(from, 0.35f, dir, out hit, dist, Mask(), QueryTriggerInteraction.Ignore);
        }

        private static float Room(Vector3 pos, Vector3 dir, float dist)
        {
            RaycastHit hit;
            if (Physics.SphereCast(pos + Vector3.up * 1f, 0.35f, dir, out hit, dist, Mask(), QueryTriggerInteraction.Ignore))
                return hit.distance;
            return dist;
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

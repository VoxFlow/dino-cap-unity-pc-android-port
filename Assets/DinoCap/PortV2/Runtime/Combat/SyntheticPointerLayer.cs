using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DinoCap.PortV2
{
    // TASK027N.2.49R REOPEN2 Phase 5 - a real pointer-state abstraction
    // shared by every visible native on-screen control, real mobile touch,
    // real Editor/PC mouse, AND automated synthetic test pointers - all
    // three sources feed the SAME logical pointer state, per this phase's
    // own explicit requirement ("all visible native controls consume this
    // same logical pointer state"). This is deliberately a SEPARATE,
    // scene-agnostic class (not embedded in CityGridController) so it can
    // also drive CombatSandboxV2/a future Jungle Blitz scene without
    // duplicating the press/hold/drag/capture state machine (Phase 6's own
    // "one shared pointer adapter" question).
    //
    // CAPTURE/DRAG SEMANTICS (device-observed original behavior, 2026-10-13): the two move buttons
    // exchange a held finger; every other capture is retained until Up/Cancel and dragging off a
    // button never releases it. See MovePointer for the evidence and what stays UNRESOLVED.
    public sealed class SyntheticPointerLayer
    {
        public enum Phase { Down, Held, Move, Up, Cancel }

        public struct Snapshot
        {
            public int id;
            public Vector2 logicalPosition;
            public Phase phase;
            public string capturedControlId; // null if not over/capturing any registered control this frame
        }

        sealed class TrackedPointer
        {
            public int id;
            public Vector2 logicalPosition;
            public bool isDown;
            public string capturedControlId;
        }

        readonly Dictionary<int, TrackedPointer> tracked = new Dictionary<int, TrackedPointer>();
        readonly List<(string id, Rect logicalRect)> registeredControls = new List<(string, Rect)>();
        readonly List<Snapshot> frameSnapshots = new List<Snapshot>(8);

        // Synthetic/test injection queue - consumed once per Update() call,
        // so a test can inject a full down/move/up sequence across several
        // real frames exactly like a real finger/mouse would produce it.
        readonly Dictionary<int, Vector2> syntheticDownThisFrame = new Dictionary<int, Vector2>();
        readonly Dictionary<int, Vector2> syntheticMoveThisFrame = new Dictionary<int, Vector2>();
        readonly HashSet<int> syntheticUpThisFrame = new HashSet<int>();
        readonly HashSet<int> syntheticCancelThisFrame = new HashSet<int>();
        const int SyntheticPointerIdBase = 1000; // real mouse=0, real touches use Unity's own touch.fingerId (>=0) - synthetic test pointers use a disjoint id range to avoid collision.

        public void RegisterControl(string controlId, Rect logicalRect)
        {
            for(int i=0;i<registeredControls.Count;i++)
                if(registeredControls[i].id==controlId) { registeredControls[i]=(controlId, logicalRect); return; }
            registeredControls.Add((controlId, logicalRect));
        }

        public void ClearControls() => registeredControls.Clear();

        // TASK027N.2.51 SECOND REOPEN: test-only accessor so tests can
        // assert a registered control's own logical rect directly (e.g.
        // against PROVEN native screen-space geometry) without duplicating
        // the registration literals in the test file itself.
        public bool TryGetControlRect(string controlId, out Rect rect)
        {
            for(int i=0;i<registeredControls.Count;i++)
                if(registeredControls[i].id==controlId) { rect=registeredControls[i].logicalRect; return true; }
            rect = default;
            return false;
        }

        // ------------------------------------------------------------
        // Synthetic/test injection - logical coordinates directly (tests
        // already know the logical space, no screen mapping needed).
        // ------------------------------------------------------------
        public int InjectDown(Vector2 logicalPosition, int explicitId = -1)
        {
            int id = explicitId>=0 ? explicitId : SyntheticPointerIdBase + syntheticDownThisFrame.Count + tracked.Count;
            syntheticDownThisFrame[id] = logicalPosition;
            return id;
        }
        public void InjectMove(int id, Vector2 logicalPosition) => syntheticMoveThisFrame[id] = logicalPosition;
        public void InjectUp(int id) => syntheticUpThisFrame.Add(id);
        public void InjectCancel(int id) => syntheticCancelThisFrame.Add(id);

        // ------------------------------------------------------------
        // Main per-frame update - call once per Update(). Polls real mouse
        // + real touch (screen-space, mapped through the given game-view
        // rect into logicalWidth x logicalHeight space - a click outside
        // the rect, i.e. in a pillarbox/letterbox black bar, produces NO
        // pointer event at all, per this phase's own explicit requirement)
        // and merges in any synthetic events injected this frame.
        // ------------------------------------------------------------
        public IReadOnlyList<Snapshot> Update(Rect gameViewScreenRect, float logicalWidth, float logicalHeight, bool pollRealInput)
        {
            frameSnapshots.Clear();

            if(pollRealInput)
            {
                PollRealMouse(gameViewScreenRect, logicalWidth, logicalHeight);
                PollRealTouch(gameViewScreenRect, logicalWidth, logicalHeight);
            }

            // Apply synthetic events (already in logical space).
            foreach(var kv in syntheticDownThisFrame)
                BeginPointer(kv.Key, kv.Value);
            foreach(var kv in syntheticMoveThisFrame)
                MovePointer(kv.Key, kv.Value);
            foreach(int id in syntheticUpThisFrame)
                EndPointer(id, Phase.Up);
            foreach(int id in syntheticCancelThisFrame)
                EndPointer(id, Phase.Cancel);

            syntheticDownThisFrame.Clear();
            syntheticMoveThisFrame.Clear();
            syntheticUpThisFrame.Clear();
            syntheticCancelThisFrame.Clear();

            // A finger/button that is down but produced no event this frame is HELD. The native engine receives
            // touchesMoved only for touches that actually moved (EventEAGLView 0x188680), so a resting finger sends
            // nothing; consumers that must react to real moves (the shop's cancel-on-leave, move-button transfer)
            // therefore never see a fabricated per-frame Move from a resting finger. Held keeps IsControlPressed true.
            foreach(var kv in tracked)
            {
                bool hadEvent=false;
                for(int i=0;i<frameSnapshots.Count;i++) if(frameSnapshots[i].id==kv.Key){ hadEvent=true; break; }
                if(!hadEvent)
                    frameSnapshots.Add(new Snapshot{ id=kv.Key, logicalPosition=kv.Value.logicalPosition, phase=Phase.Held, capturedControlId=kv.Value.capturedControlId });
            }

            return frameSnapshots;
        }

        // Releases every currently-held pointer as Cancel - used for focus-
        // loss safety (mirrors the already-established Phase O pattern for
        // movement keys, generalized to the pointer layer itself).
        public void CancelAll()
        {
            var ids = new List<int>(tracked.Keys);
            foreach(int id in ids)
                EndPointer(id, Phase.Cancel);
        }

        public bool IsControlPressed(string controlId)
        {
            foreach(var s in frameSnapshots)
                if(s.capturedControlId==controlId && (s.phase==Phase.Down || s.phase==Phase.Held || s.phase==Phase.Move))
                    return true;
            return false;
        }

        // Click-on-release controls (the native switch-weapon button, slot dword_214F40 in sub_56E94): the press only
        // arms the control; the action fires when the finger/button is released while STILL inside the control's rect.
        // Releasing outside (after dragging off) or a Cancel does nothing.
        public bool WasControlReleasedInsideThisFrame(string controlId)
        {
            if(!TryGetControlRect(controlId, out var rect)) return false;
            foreach(var s in frameSnapshots)
                if(s.capturedControlId==controlId && s.phase==Phase.Up && rect.Contains(s.logicalPosition))
                    return true;
            return false;
        }

        public bool WasControlPressedThisFrame(string controlId)
        {
            foreach(var s in frameSnapshots)
                if(s.capturedControlId==controlId && s.phase==Phase.Down)
                    return true;
            return false;
        }

        void PollRealMouse(Rect gameViewScreenRect, float logicalWidth, float logicalHeight)
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if(mouse==null) return;
            Vector2 screenPosGUI = ToGuiSpace(mouse.position.ReadValue());
            const int mouseId = 0;
            if(mouse.leftButton.wasPressedThisFrame)
            {
                if(GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logical))
                    BeginPointer(mouseId, logical);
                // A press that lands outside the game view (a black bar
                // click) intentionally produces NO pointer event at all.
            }
            else if(mouse.leftButton.isPressed)
            {
                // Move only when the pointer really moved (native: touchesMoved fires only for moving touches); a resting
                // button produces a Held snapshot in Update(). Dragged outside the game view: no event, capture is kept.
                if(GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logical)
                   && tracked.TryGetValue(mouseId, out var mouseTracked) && mouseTracked.logicalPosition!=logical)
                    MovePointer(mouseId, logical);
            }
            else if(mouse.leftButton.wasReleasedThisFrame)
            {
                EndPointer(mouseId, Phase.Up);
            }
#else
            const int mouseId = 0;
            Vector2 screenPosGUI = ToGuiSpace(Input.mousePosition);
            if(Input.GetMouseButtonDown(0))
            {
                if(GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logical))
                    BeginPointer(mouseId, logical);
            }
            else if(Input.GetMouseButton(0))
            {
                if(GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logical)
                   && tracked.TryGetValue(mouseId, out var mouseTracked) && mouseTracked.logicalPosition!=logical)
                    MovePointer(mouseId, logical);
            }
            else if(Input.GetMouseButtonUp(0))
            {
                EndPointer(mouseId, Phase.Up);
            }
#endif
        }

        void PollRealTouch(Rect gameViewScreenRect, float logicalWidth, float logicalHeight)
        {
            // Real device touch - Input.touches remains the correct legacy
            // source regardless of ENABLE_INPUT_SYSTEM (the legacy input
            // backend stays active under activeInputHandler=2 "Both",
            // already independently confirmed this parent task, Phase O).
            int count = Input.touchCount;
            for(int i=0;i<count;i++)
            {
                Touch t = Input.GetTouch(i);
                Vector2 screenPosGUI = ToGuiSpace(t.position);
                int id = t.fingerId;

                switch(t.phase)
                {
                    case UnityEngine.TouchPhase.Began:
                        if(GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logicalDown))
                            BeginPointer(id, logicalDown);
                        break;
                    case UnityEngine.TouchPhase.Moved:
                        // Only a touch that actually moved is a move (native touchesMoved). Stationary = no event (Held).
                        if(GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logicalMove))
                            MovePointer(id, logicalMove);
                        break;
                    case UnityEngine.TouchPhase.Stationary:
                        break;
                    case UnityEngine.TouchPhase.Ended:
                        // Native tests the END point (touchesEnded location) for click-on-release controls.
                        if(tracked.TryGetValue(id, out var endTracked)
                           && GameplayPresentationCamera.ScreenToLogical(screenPosGUI, gameViewScreenRect, logicalWidth, logicalHeight, out var logicalEnd))
                            endTracked.logicalPosition = logicalEnd;
                        EndPointer(id, Phase.Up);
                        break;
                    case UnityEngine.TouchPhase.Canceled:
                        EndPointer(id, Phase.Cancel);
                        break;
                }
            }
        }

        static Vector2 ToGuiSpace(Vector2 screenPointBottomLeftOrigin)
        {
            // Input.mousePosition / touch.position / the new Input System's
            // Mouse.position are all bottom-left-origin, Y-up (standard
            // Unity screen space). GUI/Event coordinates (and therefore
            // this class's own gameViewScreenRect, which is built to match
            // GUI space for direct use by IMGUI on-screen controls) are
            // top-left-origin, Y-down. Flip once, here, so every caller
            // downstream works in one consistent convention.
            return new Vector2(screenPointBottomLeftOrigin.x, Screen.height - screenPointBottomLeftOrigin.y);
        }

        void BeginPointer(int id, Vector2 logicalPosition)
        {
            var p = new TrackedPointer { id=id, logicalPosition=logicalPosition, isDown=true };
            p.capturedControlId = HitTest(logicalPosition);
            tracked[id] = p;
            frameSnapshots.Add(new Snapshot{ id=id, logicalPosition=logicalPosition, phase=Phase.Down, capturedControlId=p.capturedControlId });
        }

        void MovePointer(int id, Vector2 logicalPosition)
        {
            if(!tracked.TryGetValue(id, out var p))
            {
                // A move/drag with no prior Down (e.g. a synthetic test
                // injected Move without Down) is treated as a fresh capture
                // at the current position - safe, permissive default for
                // test ergonomics; real input sources always send Down first.
                BeginPointer(id, logicalPosition);
                return;
            }
            p.logicalPosition = logicalPosition;
            // NATIVE-PROVEN (ARM, sub_56E94 = mainSceneiPad touch virtual, vtable 0x1FDAD8 slot +36; gameplay state [this+0xB8]==4)
            // and matching original-device observation (iPad 4, 2026-10-13). The native handler keeps one touch-id slot per
            // button (dword_214F30..40). Phase 1 (move): inside move-rect A with the id held in slot B -> the id moves to slot A;
            // inside move-rect B with the id held in slot A -> moves to slot B; ANYWHERE ELSE (including off every button) the
            // handler exits without changing any slot or flag, and it never inspects the fire/melee/other slots on a move.
            // Hence: only the two move buttons exchange a finger, dragging off keeps the control held, nothing else transfers.
            // Phase 2 (end) releases whichever slot holds the id. See Reports/DC1_Build_Establishment/native_touch_investigation_checkpoint.md.
            if(IsMoveButton(p.capturedControlId))
            {
                string hit = HitTest(logicalPosition);
                if(IsMoveButton(hit)) p.capturedControlId = hit;
            }
            frameSnapshots.Add(new Snapshot{ id=id, logicalPosition=logicalPosition, phase=Phase.Move, capturedControlId=p.capturedControlId });
        }

        void EndPointer(int id, Phase endPhase)
        {
            if(!tracked.TryGetValue(id, out var p))
                return;
            frameSnapshots.Add(new Snapshot{ id=id, logicalPosition=p.logicalPosition, phase=endPhase, capturedControlId=p.capturedControlId });
            tracked.Remove(id);
        }

        static bool IsMoveButton(string id) => id=="moveLeft" || id=="moveRight";

        string HitTest(Vector2 logicalPosition)
        {
            // Last-registered-wins on overlap (registration order = draw
            // order in every caller this reopening wires up) - no two of
            // City Grid's own on-screen controls overlap in practice.
            for(int i=registeredControls.Count-1;i>=0;i--)
                if(registeredControls[i].logicalRect.Contains(logicalPosition))
                    return registeredControls[i].id;
            return null;
        }
    }
}

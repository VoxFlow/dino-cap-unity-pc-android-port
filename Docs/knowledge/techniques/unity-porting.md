# Porting recovered behaviour to Unity

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

- Separate **source-derived semantics** from **platform-adapter mechanics**. A Unity component is acceptable only as a vessel for a recovered rule; it must not add timing, damage, range or state transitions.
- Classify every difference: source-faithful, intentional bug correction, modern visual enhancement, platform adapter, debug only; keep the original contract written next to each.
- Pause, time and input are common traps: the original paused by a scene time scale of 0, while a Unity flag only froze what was wired to it. Search for every clock (delayed destroy, wall-clock deadlines), not just the objects you thought of.
- Touch: IMGUI delivers one pointer as a mouse event, so multi-touch controls need the pointer layer, not `OnGUI` events.
- Keep original quirks unless the owner decides otherwise. See [fidelity and divergences](../../fidelity/shipped-quirks-and-divergences.md).


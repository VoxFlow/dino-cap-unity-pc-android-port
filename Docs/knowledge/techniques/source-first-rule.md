# Rule: the original's bytes and instructions come first

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

**Symptom:** a confident claim about the original turns out wrong after a device test.

**Rule:** rank evidence: original bytes and raw assets, then processor instructions and data flow, then callers and lifecycle, then parsed data, then decompiler output, then similar code, then inference. Label every claim
(PROVEN, LIKELY, POSSIBLE, UNRESOLVED, FALSIFIED) and keep the label tied to the exact claim.

**Why:** decompiler output misread vector instructions (a "fixed direction" that was really homing) and a float constant (a spawn margin of 195 that was exactly 200).
Verify signedness, width, float/int conversion and branch operators on the instructions.

**Also:** a function body does not prove when it runs; prove the caller, trigger and gating state. "Not found" is not "does not exist".
See [failure ledger](../../quality/failure-ledger.md), [lessons](../../quality/lessons-learned.md).


# Testing and validation: what was checked, and what the checks cannot show

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The game has a large automated test suite, and the owner compared it with a real iPad. The tests
> prove the rules and numbers behave as designed; they cannot prove it *looks* or *feels* right. Only comparing with the
> original device does that, which is why several "all tests pass" results were later overturned.

## Layers of checking

| Layer | Proves | Does not prove |
|---|---|---|
| Original-program analysis (read-only) | What the original code and data do | That the Unity port implements it |
| Automated Unity tests (headless) | Logic and state: numbers, timers, flags, save/restore | Visual correctness, display pacing, feel |
| Rendered captures from Unity | World geometry of the layers that camera draws | On-screen controls, overlay UI, original-device equality |
| Owner testing on iPad vs PC/Android | Whether it looks and behaves like the original | Anything not tried |
| Independent AI audit of the write-up | Whether text matches the records | That the game itself is correct |

## Test numbers (as recorded)

- Last full suite before the latest fixes: **1194 tests, 1188 passing** (figures taken from a project checkpoint). The
  recorded causes are a parked pair of K4 raptor tests plus three environment errors (file-replace and lock errors that
  pass when rerun alone). That accounts for five outcomes, not six, and the original run file was not located, so the
  exact breakdown of the six non-passing results (failed, skipped or inconclusive) is not fully accounted for here.
- Earlier broad runs: 922 of 922 (Jungle restoration), 968 and 1004 of 1004 (follow-ups).
- Later fixes were checked with targeted tests only. The single full run on the release-candidate source (15 Oct) gave **1215 tests: 1211 passed, 1 skipped (a manual render-capture test), 3 failed** - the parked K4 raptor pair and one setup error (file-replace) that passes when its class is rerun alone (40 of 40). The earlier 1194/1188 figures above predate the latest fixes.

## Rules the project adopted

1. Run only tests that touch the change while fixing; run the full suite once at a stable point.
2. A failing test is rerun alone before it is called a regression.
3. Never write "the visual defect is fixed because the tests pass".
4. A test must assert independently recovered source truth, never a value that was adjusted to match the code.
5. No unbounded waits in tests; every wait has a timeout. (Screen capture inside the headless Unity run is known to
   hang in this environment, so it is not used.)

## What could not be validated

Original-device acceptance for the flamethrower and laser rows, physical multi-monitor behaviour, key-by-key use inside
Jungle, and the final public release files. These are listed under open items in
[fidelity and divergences](../fidelity/shipped-quirks-and-divergences.md).

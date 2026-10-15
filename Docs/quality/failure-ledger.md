# Failure ledger: what went wrong, how it was found, and the fix

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** A blameless list of the material mistakes, grouped by kind. It says who or what was involved only where the record
> shows it. The "Basis" column says whether a row is a recorded incident, an inferred cause, or only a later rule associated with it; the
> protection column is the rule or practice that followed, not proof that it was caused by this incident. The numbers refer to the rows of the
> [corrected claims](../history/corrected-claims.md) table.

| Class | Examples (corrected-claims row) | Basis | How it was detected | Fix and protection added |
|---|---|---|---|---|
| Misreading of low-level code (decompiler or vector instructions) | 1 dinosaurs "fixed direction"; 22 spawn margin 195 | recorded (float literal check); cause inferred | Device observation; later float-literal check | Rule: instructions beat decompiler output; registry marks the claim FALSIFIED |
| Superseded task conclusions | 2, 3, 4, 5, 6 (Tasks 002, 012, 016) | recorded (supersession notices) | Later closure tasks and exact ledgers | Notice at the top of the old report, successor named |
| Test that passed while the visual was wrong | 7, 9 (placement; "sidewalk" height) | recorded; cause inferred | Comparison with original video | Visual-source falsification rule; no calibration as source truth |
| Calibrated constant treated as source | 9 (82 and −181) | recorded (disarmed-heuristics list) | Owner and the no-heuristics rule | Constant replaced by the derived value; entry in the disarmed-heuristics list |
| Premature "done" | 10 (City closure with 0 of 32 accepted) | recorded (0 of 32 acceptance) | Owner device acceptance | Parent-task rule: completion needs every row closed or proven blocked |
| Invented mechanism not in the original | 11, 12, 13, 16, 19, 20 (queues, cooldown, timers, fixed range) | recorded per row; cause inferred | Owner device tests plus branch-level code reading | Native-faithful-fix rule; held-flag model; registry contracts |
| Wrong label or table reading | 14, 15, 17, 18, 21 | recorded per row | Re-check against code or device | Contract corrected, old claim kept as history |
| Documentation errors (this write-up) | spawn margin copied from a falsified entry; port save format; level clear trigger | recorded (audit findings) | Independent Codex audit | Audit-and-response cycle; claim logs keep the wrong entry marked WRONG |
| Acceptance based on report images | shop table edge | recorded (owner re-check) | Owner re-check on a build | Marked leftover; images not acceptance evidence |
| Pause not covering every time source | blood burst timer; spawn, hurt and immunity deadlines | recorded (owner report, code audit) | Owner report, then Codex code audit | Scene clock; lifetime counted on unpaused time |
| Overwritten history | 185 report outputs overwritten in one pass | recorded (build report disclosure) | Build-establishment disclosure | Disclosed; old hashes not claimed to authenticate them |
| Tool-delivery failure | an archive delivered without its tool files (Task027M.7.1/7.2) | recorded (delivery report) | Verification of delivered contents | Immutable nested archive |
| Documentation overstatement | a tools table implied the F3 trace tool had been used on a device; it was source only, never built or run. Not a delivery failure | recorded (owner question, correction) | Owner asked | Wording corrected |

**Rules existed but a mistake still happened:** the particle-freeze rule was written but timers were missed. The lesson is that a
rule lists what was thought of, and an independent search for the same pattern in the code (all clocks, not just particles) is needed.

Actor attribution is intentionally omitted unless the record is direct; see the [timeline](../history/timeline-and-agents.md).

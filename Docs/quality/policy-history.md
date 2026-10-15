# How the working rules evolved, and why

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The project's rulebook was not written in advance; most rules were added in response to problems. The "Purpose / inferred rationale" column is the
> author's reading of the policy text and the corrected-claims record (the purpose or inferred rationale), not a quoted incident record, except where
> a source is named.

| Rule (policy section) | Purpose / inferred rationale | Effect |
|---|---|---|
| Source-truth order: original bytes and processor instructions first, decompiler output later (start of project) | Early readings from decompiler output were wrong | Every claim carries a confidence label |
| Read-only inspection of the original program | Protect the original | The policy forbids changing the database unless explicitly authorised; inspected tasks report no changes. A complete mutation history was not audited |
| Visual evidence and headless limits (N2.42) | Characters were accepted as placed correctly because a number matched, yet looked wrong against the original video | Numeric proof is not visual proof; captures compared with original frames |
| No calibration as source truth (N2.43) | A value was tuned until it looked right | Constants must come from the original; a list of rejected heuristics is kept |
| Readiness gate (N2.48) | "Ready to test" was declared before a real patched build existed | Readiness needs proven numbers, a patched build, and a passing targeted test |
| Persistent source-contract registry (N2.47R) | Findings were being re-derived and contradicted across tasks | One registry of proven contracts, read before every task |
| Parent-task completion (N2.51 reopens) | A whole-mode task was closed with none of its checklist accepted | A big task stays open until each row is closed or proven blocked; session end is not task end |
| Native-faithful fixes only, and say when the original differs from the port (N2.50) | Fixes that only looked right, and observed differences patched silently | Reproduce the original mechanism; report every contradiction of an earlier claim |
| Time-efficient test workflow (N2.51 workflow) | Repeated full test runs were slow | Targeted tests while fixing; one full run at the end |
| Larger investigation budget (section 6) | Raised at the owner's request | About 25 steps, or 40 for a single trace |
| Side questions without stopping work | Turns were ending only to ask minor questions | Ask while continuing independent work |

Lesson: a long rulebook has a cost, because every session has to read it. The registry and checkpoints carry the facts; the policy
file should carry only rules that change behaviour.

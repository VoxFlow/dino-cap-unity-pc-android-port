# AI agents: failure and recovery

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

Recorded failure kinds and what helped (see the [failure ledger](../../quality/failure-ledger.md) for cases):

- Confident wrong claims copied from a registry entry's text instead of its status: read the status first; keep wrong entries marked WRONG rather than deleting them.
- Tuning a constant until a screenshot looks right: forbid it, and require the value to come from the original.
- Premature "done": a task is done only when each checklist row is closed or proven blocked.
- Overwriting earlier outputs when regenerating: write new revisions, keep old hashes, disclose any loss.
- A rule exists but the mistake recurs: add an independent search for the pattern, not another sentence in the policy.
- An independent second agent auditing the write-up found errors the author missed; ask for findings only, not rewrites.


# Working with AI agents on a project like this

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The project switched between two AI coding agents. It worked because everything important was
> written down in small, reusable files, so a new agent never had to remember a conversation. If you want to repeat this
> with less effort and cost, give the agent less to read and a narrower question each time.

## What changed hands, and how

The sequence below is the owner's account; see the [timeline](../history/timeline-and-agents.md) for which parts are
backed by direct records (the Codex eight-defect hand-off with its N2.51 continuation, and the later build hand-back).

Claude Code did the foundations and most of the reverse engineering. For a stretch the owner switched to Codex, which
continued the City Grind convergence, the Jungle Blitz restoration and the runtime-contradiction fixes, and which also
audited some of Claude's earlier claims. Claude then returned for builds and the later fixes and drafted this write-up,
with Codex auditing it. Where hand-offs are recorded, the new agent started from written hand-off material, not from the other's memory.

## What made the hand-offs work

- **A policy file** with the rules (source-truth order, confidence labels, no guessed constants, how to classify
  differences). It grew to over 1,500 lines, which is itself a cost: every new session has to read it.
- **A contract registry** (227 entries at this snapshot) instead of rereading about 13,800 report files. A new agent asks "what is already
  proven about X?" and reads one entry.
- **Checkpoint files** with the exact resume step, so stopping mid-task loses nothing.
- **An audit by a different agent**, which found real errors in this write-up that the author had missed.

## How to prompt more efficiently

1. **One causal question per task.** "How does pause work?" beats "check the whole game".
2. **Give exact starting points:** the registry id, the report folder, the file and the test names.
3. **Say what not to redo.** "Reuse the proven contract; only reopen it if new evidence contradicts it."
4. **Targeted tests while fixing; one full run at the end.** Full runs are slow and cost the most.
5. **Keep the policy file short.** Move history into the registry and keep the file to rules that change behaviour.
6. **Make the agent record contradictions as it finds them** and mark superseded claims immediately, not at the end.
7. **Ask the agent to say what its check cannot prove** (for example, headless tests do not prove visuals).
8. **Let a second agent audit anything that will be published.** Ask for findings only, not rewrites.

## Pitfalls seen

Agents can sound sure while wrong (a falsified value was copied from a registry entry's text instead of its status);
can tune a constant until a screenshot looks right; can declare a task done early; and can overwrite earlier outputs
when regenerating. The remedies are the rules above plus a human checking on a real device.

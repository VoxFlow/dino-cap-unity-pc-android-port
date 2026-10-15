# AI agents: prompting and context

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

- One causal question per task; give the registry id, report folder, file and test names to start from.
- Keep the policy file short; move facts into a registry and checkpoints. A fresh agent should read one entry, not 13,000 files.
- Say what not to redo ("reuse the proven contract unless new evidence contradicts it").
- Targeted tests while fixing, one full run at the end; long runs in the background.
- Ask the agent to state what its check cannot prove, and to record contradictions the moment it finds them.
- Hand off with written checkpoints and an exact resume step.
See [working with AI agents](../../quality/working-with-ai-agents.md).


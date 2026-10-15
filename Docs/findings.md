# Our findings: how this game was taken apart

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](start/disclaimer.md).*

We didn't remake Dino Cap from memory. We opened the original iPad game, read how it actually works, and rebuilt it to match, then checked it against a real iPad. This is the trail,
including the parts we got wrong and fixed.

**Start with whatever you're curious about**

- *How does the game work under the hood?* [Overview](how-it-works/01-overview.md): sprites and animation files, the screen, sound, combat controls, dinosaurs, saves, pause, menus, Jungle Blitz.
- *What's different from the original, and why?* [Quirks we kept and changes we made](fidelity/shipped-quirks-and-divergences.md), with side-by-side pictures.
- *What did we get wrong?* [Corrected claims](history/corrected-claims.md) and the [failure ledger](quality/failure-ledger.md).
- *How was it built?* [Timeline](history/timeline-and-agents.md), [every investigation](history/task-index.md), [lessons](quality/lessons-learned.md) and [working with AI agents](quality/working-with-ai-agents.md).
- *Show me the code-level evidence.* [Technical index](technical/functions/ADDRESS_INDEX.md): binary identity, function addresses and offsets, instruction excerpts, call graph, original-to-Unity map.
- *Can I use the ideas for another game?* [Reusable notes](knowledge/README.md).

Builds and checks: [validation matrix](release/build-validation-matrix.md).

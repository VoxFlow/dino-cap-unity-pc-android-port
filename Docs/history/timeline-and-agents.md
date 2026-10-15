# Timeline: phases of the project and which agent did what

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The work moved through phases, from reading the save file to recovering how the weapons and
> dinosaurs behave, then to making it feel right on a real iPad. Some device-acceptance rows are still open. Two AI coding agents (Claude Code and OpenAI Codex)
> worked on it at different times under the same written rules, and a human owner tested on a real device and decided
> what to keep. This page says who did what and why the hand-offs happened.

## Roles

- **Owner:** set the goals and rules, supplied the original iPad app and video, tested on an iPad and on Android and PC,
  and made every policy decision (for example, keep an original bug, or add a modern option).
- **Claude Code:** the local reverse-engineering operator and first author of the reports and registry.
- **Codex:** took over implementation and acceptance work for a stretch, and independently checked prior claims.
- **Supervising reviewer:** a separate chat assistant used for planning tasks and auditing evidence.

The agents worked under a shared, evolving project policy file. Violations and corrections are documented rather than
hidden, and the policy grew when a mistake showed a missing rule (see the quality chapters).

## Phases

*Provenance of the "Main agent" column: phase ownership is the owner's attribution, supported directly only for (a) the Codex
eight-defect convergence assignment and its N2.51 continuation (hand-off prompt and the lifecycle/visual/menu report) and
(b) the later build hand-back (build-establishment report). Authorship of the other phases follows the owner's account and
the report authorship notes; it has not been verified passage by passage in the agent session logs. Authorship and
motive are separate claims.*

| Phase | Work | Main agent |
|---|---|---|
| 1. Foundations (Tasks 001–009) | Save file, progression, worlds, shop loop, stored values | Claude |
| 2. Formats and renderer (Tasks 010–020) | Animation files, event markers, atlas, byte-exact extractor, reference renderer | Claude |
| 3. Combat and effects (Tasks 021–027M) | Weapons, hands and muzzles, gore, burn and laser effects, audio voice limits, engine tick model | Claude |
| 4. Dinosaurs and rage (027N.1–2.31) | All dinosaur types, blood systems, bite commit, rage face | Claude |
| 5. City Grind as a whole (N2.32–N2.51) | Spawning, scene rebuild, placement from source, registry workflow, external-reference input adapter | Claude began the series; Codex later continued N2.51 (including the lifecycle/visual/menu part) |
| 6. Hand-off and convergence | A written hand-off prompt assigned the eight-defect City convergence to Codex | Codex |
| 7. Runtime acceptance (N2.52–N2.55) | Menu/City causal closures, Jungle Blitz restoration, corrections to camera and runtime contradictions (scope taken from the task artifacts) | Codex |
| 8. Builds and fixes | Windows and Android builds (first records exist in the build-establishment report), then fixes after device testing: pause as time scale, melee/fire arbitration, rage face, door, audio-cap review | Claude took over the later fixes |
| 9. Documentation | Rewriting the record for readers, with a Codex audit | Claude drafts, Codex audits |

## Why the work changed hands

The owner switched agents. The following is the owner's account plus what the written records show; per-passage
verification against the agent session logs is still pending, so treat motives as owner testimony:

- A long project fills an agent's working memory. Written checkpoints, a contract registry and a policy file let a
  fresh agent resume without the old conversation.
- Some mistakes were made by the first agent against the written policy: placement values that were adjusted to look
  right, and a manual ground-level tweak in the City scene. These were rejected under the "no calibration as source
  truth" rule and redone from the original transform chain. Using a second agent gave an independent check of earlier
  claims; Codex corrected, for example, the camera device labels.
- Handing over by written document, not by remembered conversation, is what made the switch possible; this is the
  main practical lesson of the project (see "Working with AI agents").

## What the owner decided (a few examples)

| Decision | Classification |
|---|---|
| Keep the camera one-frame blip when turning away from an edge (original bug) | Source-faithful quirk, noted in the write-up |
| Keep the bazooka/house-animation overlap | Source-faithful quirk |
| Keep the wide-screen camera catch-up speed scaling | Platform adapter |
| Enable Internal 2X as an option | Modern visual enhancement |
| Keep the dinosaur death sound inside the shared 28-sound cap | Source-faithful |
| Debug tools exist in every build but are off by default | Debug only |
| Publishing or distributing a release needs the owner's approval; local test builds already exist | Process |

## Evidence pointers

Reports: the full list is in the [task index](task-index.md); registry entries cited there. The hand-off documents and
agent session records are kept in the private archive; only conclusions are quoted here.

# Lessons learned

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** Most mistakes came from believing something because it looked convincing. These are the rules
> that came out of them.

1. **A number that matches is not a meaning that is right.** A character placed at a "correct" number stood in the
   wrong place on the street. Compare landmarks against original footage, and derive the value from the original's own
   transform chain, never from adjusting until it looks right.
2. **A function's body does not say when it runs.** Prove the caller, the trigger and the state that gates it before
   claiming when something happens.
3. **"Not found" is not "does not exist".** Words like only, never, always, every, exactly need exhaustive evidence.
4. **Do not trust decompiler output for signs, widths and vector maths.** The processor instructions win. Two early
   errors (dinosaurs "walking in a fixed direction" and a spawn margin of 195) were misreads of this kind.
5. **Serialised is not persistent.** A saved value can be overwritten before gameplay uses it; trace write, save, read,
   rebuild and the real consumer.
6. **Similar code is not identical code.** Phone and iPad versions look alike but are separate; the verified one is
   PROVEN, the twin is LIKELY until checked.
7. **Keep odd original behaviour.** Quirks (a camera blip, an overlap, dropped sounds) are recorded, not "fixed", unless
   the owner explicitly decides otherwise.
8. **A real device overrides an earlier PROVEN label.** Record the contradiction, mark the old claim falsified or
   superseded, keep the history, and list the other systems that may share the mistake.
9. **Do not call a task done while its checklist is empty.** An early "done" for City Grind had 0 of 32 items accepted by
   the owner and was reopened.
10. **Write the history honestly.** Old reports get a notice pointing to the current answer; they are not deleted. Some
    outputs were overwritten during a later pass, and that loss is disclosed.
11. **Images in a report are not acceptance.** A report capture suggested the shop table edge was clean; the owner's own
    check on a build showed it was not.

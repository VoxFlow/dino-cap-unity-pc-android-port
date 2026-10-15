# 1. Overview: what was built and how

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** Dino Cap 1 (iOS, version 1.9.8) was studied instead of copied from memory. The original
> program and its data files are the authority; the Unity game is rebuilt to behave the same way, and every place where
> it intentionally differs is labelled.

## 1.1 The method

1. **Take the original apart read-only.** The shipped binary was inspected in a disassembler without changing it.
   Game data files were unpacked and decoded.
2. **Write down what is proven.** Each recovered rule is stored as a *contract* in a registry with a confidence label
   (PROVEN, LIKELY, POSSIBLE, UNRESOLVED, FALSIFIED) and pointers to the evidence.
3. **Build in Unity from the contract**, not from impressions. Numbers come from the original code or data, never from
   adjusting a value until it looks right.
4. **Test twice.** Automated tests check logic and state; the owner then compares against an original iPad.
5. **Correct and record.** When a device comparison disagrees with an earlier claim, the claim is marked
   falsified or superseded, kept as history, and the registry records its successor.

## 1.2 Source-truth order

Original binary and data bytes → processor instructions and data flow → who calls what → parsed data files → decompiler
output → similar code elsewhere → inference. When the decompiler and the instructions disagree, the instructions win.

## 1.3 Labels for every difference

| Label | Meaning |
|---|---|
| Source-faithful | Reproduces the shipped behaviour |
| Intentional shipped-bug correction | The original is proven buggy and the owner chose to fix it |
| Modern visual enhancement | Presentation improvement not claimed as original (for example Internal 2X) |
| Platform adapter | Glue for Unity/PC/Android that expresses a recovered rule (for example mouse as a finger) |
| Debug only | Test tools, off by default |

## 1.4 What the port contains

City Grind (the street campaign with shop and upgrades), Jungle Blitz (survival), the main menu and options, a debug
Combat Workbench, a Windows build and an Android build. Chapters 2–10 explain the parts; the fidelity and history
sections explain what is proven and how mistakes were found.

## 1.5 Where to read next

- Playing: [Download guide](../start/download-guide.md), [Controls for PC](../start/controls-pc.md).
- Words you may not know: [Glossary](../start/glossary.md).

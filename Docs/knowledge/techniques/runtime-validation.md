# Validating at runtime

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

- Headless tests prove logic and state. They do not prove appearance, pacing or feel. Never write "fixed" from a passing test alone.
- A numeric match is not a visual match; compare landmarks against original footage. A report image is not acceptance; check on a real build.
- Tests must assert independently recovered truth, never a value tuned to the code. No unbounded waits; every wait has a timeout.
- Run targeted tests while fixing and one full suite at a stable point; rerun a failure alone before calling it a regression.
- Keep raw result files, and sanitize them before sharing. See [testing and validation](../../quality/testing-and-validation.md), [build matrix](../../release/build-validation-matrix.md).


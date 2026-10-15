# Troubleshooting

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](disclaimer.md).*

- **I clicked "Download ZIP" and there is no game.** That is the source code. Get the game from the repository's
  **Releases** section ([Download guide](download-guide.md)).
- **Windows says it protected my PC.** The build is not code-signed, so SmartScreen may warn. Compare the SHA-256 on the
  release page with your download before running.
- **The game does not start from the archive.** Extract the whole archive first and keep `Dino Cap.exe` next to its data
  folder.
- **Android will not install it.** Allow installing from your browser or file manager once; it needs Android 7.1 or
  newer.
- **The picture looks soft or blocky.** Check the output filter (Smooth vs Original) and Internal 1X/2X in the options;
  see [how the picture reaches the screen](../how-it-works/03-how-the-picture-reaches-your-screen.md).
- **Shots sometimes go silent while holding a machine gun.** That is original behaviour: the game can play only 28
  sounds at once ([sound](../how-it-works/04-sound.md)).
- **Pressing the melee key during a swing does nothing.** Intended: only a press made *during* the swing and still held
  when it ends repeats it ([combat controls](../how-it-works/05-combat-controls.md)).
- **Tearing in the Unity editor.** Turn on V-Sync in the editor's Game view; that fixed it. The owner did not see it in
  tested builds; cross-build absence was not separately measured.

If it is none of these, open an issue and include your device, OS version and what you pressed.

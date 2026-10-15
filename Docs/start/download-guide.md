# Download guide

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](disclaimer.md).*

> **In simple terms.** Download the game from the **Releases** section of this project's **GitHub page** (the
> "Releases" link on the right-hand side of the repository, or the *Releases* tab). Do **not** use the green *Code*
> button or "Download ZIP": that gives you the project's source files, not the game. Pick the file for your device
> from the latest release, unpack it (Windows) or install it (Android), and play.

 The Releases page is at https://github.com/VoxFlow/dino-cap-unity-pc-android-port/releases (it works once the repository is public).

## Windows PC (64-bit)

1. On GitHub open **Releases**, choose the newest release, and under *Assets* download the Windows archive `Dino-Cap-Windows-x64.zip` ([https://github.com/VoxFlow/dino-cap-unity-pc-android-port/releases](https://github.com/VoxFlow/dino-cap-unity-pc-android-port/releases)) (it contains `Dino Cap.exe` and its data folder).
2. Extract the **whole archive** to a normal folder (for example your Documents). Do not run the game from inside the
   archive, and do not move `Dino Cap.exe` away from the files extracted next to it.
3. Double-click `Dino Cap.exe`.
4. An unsigned or unrecognised build may make Windows SmartScreen show a warning. Check the SHA-256 checksum listed on
   the release page against your download before choosing "Run anyway". (The Windows executable is not code-signed: checked on the player executable of this build.)
5. Play with mouse and keyboard (see [Controls for PC](controls-pc.md)). A controller is not needed.

Fullscreen: press **F11** or **Alt + Enter** at any time.

## Android phone or tablet

1. On GitHub open **Releases**, choose the newest release, and under *Assets* download `Dino-Cap-Android.apk` ([https://github.com/VoxFlow/dino-cap-unity-pc-android-port/releases](https://github.com/VoxFlow/dino-cap-unity-pc-android-port/releases)) on the device (or copy it over from a PC).
2. Open the file. Android will ask permission to install apps from this source; allow it for that one install.
3. Needs Android 7.1 (API 25) or newer. The package is built with IL2CPP as one universal APK for both 32-bit (ARMv7) and 64-bit (ARM64)
   devices.
4. Play with the on-screen touch controls, as on the original iPad/iPhone game.

## Verify the download (optional but recommended)

Each release lists SHA-256 checksums. On Windows PowerShell:

```powershell
Get-FileHash .\Dino-Cap-Android.apk -Algorithm SHA256
```

The printed value must equal the one on the release page.

## Something wrong?

See [Troubleshooting](troubleshooting.md) (written with the quality chapters) or open an issue on the repository.

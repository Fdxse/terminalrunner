# Terminal Runner
Just to keep things moving along.... :)

A C# / .NET 10 console app that prints endless, realistic looking Linux terminal
activity: green text on a black background, while keeping your screen awake.

## What it shows

It starts with a fake boot (kernel log, systemd `[  OK  ]` lines, login and
message of the day), then loops forever through random "commands" typed at a
bash prompt:

- `apt update` / `apt-get install` with downloads, unpacking and setup
- Linux kernel build (`make -j16 bzImage modules`) with the odd compiler warning
- Rust `cargo build --release`
- `git clone` with live progress counters, followed by `git log`
- `docker build` with BuildKit style steps
- `tail -f` on an nginx access log
- `dmesg` kernel messages
- `ping`, `hexdump -C`, `pytest`
- `df -h`, `free -h`, `uptime`, `systemctl status`, `docker ps`

Now and then it runs `clear` or presses Enter on an empty prompt, like a real person.

## Keeping the screen awake

On Windows the app calls `SetThreadExecutionState` (the same API video players
use), so the display does not turn off and the PC does not go to sleep while it
runs. It also taps the unused **F15** key once a minute, which resets the idle
timer so screen savers and "lock after X minutes" do not start either. Turn that
off with `--no-keypress` if you only want the display kept on.

Everything is restored when the app exits.

## Download the exe

GitHub Actions builds a ready-to-run Windows exe (about 36 MB, no .NET install needed):

- **Latest build:** open the [Actions tab](../../actions/workflows/build.yml), pick the newest
  green run on `main` and download the `TerminalRunner-win-x64` artifact (a zip with the exe).
- **Releases:** push a version tag and the exe is attached to a GitHub Release:

  ```bash
  git tag v1.0.0
  git push origin v1.0.0
  ```

  It then shows up under [Releases](../../releases).

Windows SmartScreen may warn the first time since the exe is not code-signed.
Choose "More info" and then "Run anyway".

## Run it

```bash
dotnet run --project src/TerminalRunner
```

Or open `TerminalRunner.slnx` in Visual Studio / VS Code and press F5.

To build a single exe you can copy anywhere:

```bash
dotnet publish src/TerminalRunner -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

The exe ends up in `src/TerminalRunner/bin/Release/net10.0/win-x64/publish/`.

## Windowed or fullscreen

It runs in a normal window by default. To switch:

- Press **F** (or **F11**) while it runs to toggle fullscreen and windowed.
- Or start in fullscreen: `TerminalRunner --fullscreen`

Fullscreen works by sending Alt+Enter, which both Windows Terminal and the
classic console window use for fullscreen. In Windows Terminal, F11 and
Alt+Enter also work directly.

## Options

| Option | What it does |
| --- | --- |
| `-f`, `--fullscreen` | Start in fullscreen |
| `-s`, `--speed <n>` | Speed multiplier from 0.1 to 10 (default 1) |
| `--seed <n>` | Fixed random seed, gives the same output every run |
| `--no-boot` | Skip the boot sequence |
| `--no-keypress` | Do not send the F15 keep-alive key press |
| `-h`, `--help` | Show help |

## Keys while running

| Key | Action |
| --- | --- |
| F / F11 | Toggle fullscreen |
| Space / P | Pause and resume |
| + / - | Faster / slower |
| Q / Esc / Ctrl+C | Quit |

## Project layout

```
src/TerminalRunner/
  Program.cs            Entry point and setup
  AppOptions.cs         Command line parsing
  Platform/             Sleep prevention and fullscreen (Win32 P/Invoke)
  Terminal/             Console output, typing effect, prompt
  Content/              Fake data: package names, kernel messages, etc.
  Scenes/               One class per fake command (apt, make, git, ...)
```

No external NuGet packages are used. To add a new kind of output, create a
class that inherits `Scene` and add it to the list in `SceneDirector`.

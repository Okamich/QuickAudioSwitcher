# QuickAudioSwitcher 🔊🎛

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20%7C%2010-0078D4.svg)](https://microsoft.com/windows)
[![Architecture](https://img.shields.io/badge/Architecture-x64-brightgreen.svg)]()

A lightweight, zero-latency Windows audio output switcher and hardware studio deck designed for instant playback toggling between speakers, headphones, and multi-monitor setups (including identical HDMI/DisplayPort displays).

[Русское описание ниже / Russian description below](#-на-русском)

---

## ⚡ Key Highlights

* **Identical Monitor Disambiguation**: Easily solve the "two identical monitors" problem (e.g. twin Xiaomi or Samsung displays) by assigning custom friendly aliases (`Left Monitor`, `Right Monitor`) and using the built-in **▶ Test** chime button to physically hear where the audio plays.
* **Instant Switching without UI Freeze**: Non-blocking asynchronous CoreAudio switching (`IPolicyConfig` COM interop) decouples the Nvidia/AMD HDMI audio clock renegotiation handshake from the UI thread.
* **Global Hotkeys & Side Mouse Buttons**: Cycle through favorite endpoints using keyboard shortcuts (e.g. `Ctrl + Alt + A`) or mouse buttons (**Mouse 4**, **Mouse 5**, **Middle Click**).
* **Multi-Monitor DPI-Aware OSD**: Displays a sleek, non-activating, click-through HUD pill centered on the screen where your mouse cursor is currently located.
* **Dark & Light Studio Themes**: Instant live theme toggling between **Dark Studio Hardware** and **Light Studio**.
* **Bilingual UI**: Seamless support for both **English** and **Russian**.
* **Minimal Footprint**: Single standalone executable (< 1 MB), ~25 MB RAM, 0% CPU idle.

---

## 📸 Interface Preview

* **Tray Flyout**: Click the tray icon to open a sleek channel-strip audio deck with volume fader and mute toggle.
* **On-Screen Display (OSD)**: Displays current output and volume level upon switching without stealing window focus.
* **Settings Window**: Configure rotation devices, record shortcuts, and choose language and theme.

---

## 🚀 Download & Installation

You can download the latest precompiled single-file release:
1. **Framework-Dependent (`QuickAudioSwitcher.exe` ~775 KB)**: Requires .NET 8 Desktop Runtime (pre-installed on modern Windows 11 systems). Starts instantly and uses minimal disk space.
2. **Self-Contained (`QuickAudioSwitcher.exe` ~71 MB)**: Fully standalone with bundled .NET 8 runtime. Runs on any Windows 10/11 x64 machine with zero external dependencies.

---

## 🛠️ Building from Source

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Visual Studio 2022 (v17.8+) or VS Code with C# Dev Kit
* Windows 10 / 11 (x64)

### Build Commands
```bash
# Clone the repository
git clone https://github.com/Okamich/QuickAudioSwitcher.git
cd QuickAudioSwitcher

# Build solution
dotnet build QuickAudioSwitcher.sln -c Release

# Publish single-file executable
dotnet publish QuickAudioSwitcher/QuickAudioSwitcher.csproj -c Release -r win-x64 --no-self-contained -p:PublishSingleFile=true -o publish/
```

---

## 🏗️ Architecture & Technology Stack

* **UI Layer**: WPF (.NET 8) with custom XAML design system (`Styles.xaml`), vector icons, and `DynamicResource` live theme switching.
* **System Tray**: High-DPI Windows Forms `NotifyIcon` paired with a custom dark/light `ToolStripProfessionalRenderer` (`DarkStudioMenuRenderer`).
* **Audio Core**: Windows CoreAudio API via COM interop (`IMMDeviceEnumerator`, `IMMNotificationClient`, `IAudioEndpointVolume`, and undocumented `IPolicyConfig` COM interface).
* **Audio Tester**: WASAPI Shared Mode IEEE 32-bit float procedural tone synthesizer matching endpoint native mix formats (48 kHz / 44.1 kHz).
* **Global Hooks**: Win32 `RegisterHotKey` combined with a low-level mouse hook (`WH_MOUSE_LL`) for mouse side buttons.

---

## 🇷🇺 На русском

**QuickAudioSwitcher** — компактная и быстрая системная утилита для переключения аудиовыходов в Windows 11 и 10.

### Основные возможности:
1. **Решение проблемы двух одинаковых мониторов**:
   * Позволяет задать понятные псевдонимы (например, «Монитор Левый», «Монитор Правый», «Колонки»).
   * Кнопка **«▶ Тест»** воспроизводит тестовый перезвон именно в выбранное устройство, позволяя мгновенно понять, какой монитор где находится.
2. **Переключение без зависаний**:
   * Асинхронное переключение аудиоустройства через `IPolicyConfig` не блокирует интерфейс во время драйверного HDMI-хэндшейка Nvidia/AMD.
3. **Горячие клавиши и боковые кнопки мыши**:
   * Поддержка клавиатурных комбинаций (по умолчанию `Ctrl + Alt + A`) и мыши (**Mouse 4**, **Mouse 5**, **Колесико**).
4. **Экранный индикатор (OSD HUD)**:
   * Полупрозрачная плашка с названием активного выхода и уровнем громкости.
   * Автоматически появляется на том мониторе, где находится курсор мыши, и не перехватывает фокус у полноэкранных игр и программ.
5. **Темы оформления и локализация**:
   * Переключение между **Тёмной** и **Светлой** студийными темами.
   * Полная поддержка **Русского** и **Английского** языков.
6. **Автозапуск с Windows**:
   * Опция тихого автозапуска в системный трей возле часов.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

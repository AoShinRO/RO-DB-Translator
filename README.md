# 🌀 RO-DB-Translator
> A high-performance Windows desktop utility focused on automated translation, sanitization, and parsing of structured Ragnarok Online databases (`rAthena YML`) leveraging the DivinePride API.

![Windows](https://img.shields.io/badge/OS-Windows_WPF-blue?style=for-the-badge&logo=windows)
![.NET](https://img.shields.io/badge/.NET_Framework-C%23-purple?style=for-the-badge&logo=dotnet)
![Ragnarok](https://img.shields.io/badge/Community-rAthena_Emulation-orange?style=for-the-badge)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/AoShinRO/RO-DB-Translator)

**RO-DB-Translator** automates the localization and migration workflow for Ragnarok Online private server administrators. Instead of manually updating thousands of lines across complex database files, this tool parses the YML tree, extracts entity IDs at runtime, queries the official DivinePride authoritative database, and reconstructs the file while strictly preserving the original structural integrity.

---

## ⚙️ Core Features

* **Native YML Syntax Parsing:** Automatically isolates the `Body:` block of database files (`item_db.yml` or `mob_db.yml`), bypassing metadata headers.
* **Dynamic Entity Detection:** Auto-detects the operational target schema (Monsters vs. Items) based on the input file's prefix.
* **Dynamic Charset Filter (Anti-Garbage):** An internal validation algorithm that detects and drops CJK/Korean/broken encoding callbacks using an ANSI range inspector.
* **String Cleaning & Sanitization:** Automatically strips unwanted database tags, bracket brackets (`[ID]`), and control characters for clean in-game text.
* **Hard-Coded Client Buffer Limits (Crash Prevention):** Automatically truncates strings respecting historical `ragexe.exe` client memory buffers to avoid sprite/string crashes.

---

## 🧠 Technical Architecture & Internal Mechanics

The application core is designed as a streamlined, high-throughput monolithic engine focused on asynchronous stream processing (`async/await`) to eliminate allocation overhead and keep the WPF UI responsive during heavy text file operations.

### Processing Pipeline
1. **Section Isolation:** The utility streams the text file, skips metadata, and jumps straight into the `Body:` node.
2. **Regex Stream Match:** Each item block is captured, tokenized, and isolated using strict regular expressions.
3. **ANSI Quarantine Check:** Before committing the API response text into the database output, a character range verification takes place:

> 🛡️ **Internal ANSI Validation Logic:**
> If any incoming character fails the range check (`char > 255` / U+00FF), it is dropped to prevent text encoding corruption. Characters known for causing broken rendering in localized game clients (such as `¿`, `®`, `³`, `×`) trigger a fallback to the `#FailedTranslation` flag.

### Client-Side Buffer Constraints (Memory Bounds)
The processing pipeline injects a strict substring limiter (`Cap23`) explicitly adjusted to prevent client-side memory-handling crashes:

| Database Target | Maximum Character Limit | Technical Rationale |
| :--- | :--- | :--- |
| `mob_db.yml` | **23 characters** | Prevents buffer overflows when rendering name text overlays above the monster HP bar. |
| `item_db.yml` | **39 characters** | Ensures safe layouts inside the inventory window, storage UI, and trade sockets. |

---

## 🛠️ Configuration & API Integration

An official developer API Key is required to request data from DivinePride.

1. Go to [DivinePride](https://www.divine-pride.net/) and register/retrieve your `apiKey`.
2. Input your key into the application interface. The utility persistently saves your credentials on application exit via standard Windows `Properties.Settings`.

---

## 🚀 Build & Deployment

### Prerequisites
* Windows 10 / 11
* .NET Framework 4.8+
* Visual Studio 2022

## 💾 Download & Quick Start

### 🚀 Pre-Compiled Release
If you don't want to compile the source code manually, you can download the ready-to-use executable directly from the **[GitHub Releases](https://github.com/AoShinRO/RO-DB-Translator/releases)** section.

### Compilation Steps:
1. Clone the repository:
```bash
   git clone https://github.com/AoShinRO/RO-DB-Translator.git
```
2. Open the solution file (`.sln`) inside Visual Studio.
3. Set your active build configuration to `Release`.
4. Build the solution (`Ctrl + Shift + B`).
5. Find the compiled binaries ready inside the `\bin\Release\` folder.

---

## ⚖️ Disclaimer / Legal Notice

This project is an open-source automation utility developed strictly for educational purposes, security research, and private server environment development (rAthena/Hercules emulation testing). It is not affiliated, associated, authorized, endorsed by, or in any way officially connected with Gravity Co., Ragnarok Online, rAthena, or the DivinePride administration. Users must adhere to DivinePride API rate limits to prevent temporary IP bans.

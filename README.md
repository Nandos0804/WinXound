# WinXound

WinXound is a free and open source Front-End GUI Editor for CSound, CSoundAV, CSoundAC, with Python and Lua support, developed by Stefano Bonetti. It runs on Microsoft Windows, Apple OsX and Linux.

---

> [!IMPORTANT]
> **Maintenance Note:** This repository is a community-maintained clone of the original WinXound project developed by **Stefano Bonetti**. The goal of this fork is to preserve the source code and provide updates for compatibility with modern operating systems (Windows 10/11, macOS, and current Linux distributions).

---

## Build Guides

- Windows build guide: [Windows/README.md](Windows/README.md)
- Linux build guide: [Linux/README.md](Linux/README.md)

## About

WinXound is a lightweight, powerful editor designed for Csound users. It provides a user-friendly interface for editing and compiling audio code across multiple platforms.

### Key Features

* **Multi-Language Editing:** Support for Csound (csd, orc, sco), Python (py), and Lua (lua) with Syntax Highlighting.
* **Compilers:** Run Csound, CsoundAV, CsoundAC, Python, and Lua compilers directly from the editor.
* **Integrated Tools:** * User-friendly Csound analysis GUI.
  * Integrated Csound manual help.
  * Opcodes autocompletion menu.
  * Csd explorer (File structure for Tags and Instruments).
* **Editor Comfort:** * Rectangular text selection and bookmarks.
  * Split-view (horizontal or vertical).
  * Customizable syntax highlighter colors.
  * Internal audio player for rendered files.

## Technical Specifications

WinXound was originally developed using different native frameworks for each platform to ensure maximum performance:

| Platform | Language / Framework | IDE Used |
| :--- | :--- | :--- |
| **Windows** | C# / .NET | Visual Studio |
| **macOS** | Objective-C / Cocoa | Xcode |
| **Linux** | C++ / Gtkmm | Anjuta |

**Note:** The TextEditor component is based on the **Scintilla** text control by Neil Hodgson.

## Original Requirements (v3.4.1)

* **Windows:** Version 7, 8, 10, or 11 (32/64 bit).
* **macOS:** OS X 10.5 or higher.
* **Csound:** Requires Csound 5 or 6 installed on the system path.
* **Optional:** Python interpreter (for Python support) and CsoundAV.

## License & Disclaimer

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Credits & Attribution

**Original Developer:** Stefano Bonetti

**Special Thanks:** * **macOS Debugging:** Giuseppe Silvi.

* **Contributors & Beta Testers:** Roberto Doati, Gabriel Maldonado, Mark Jamerson, Andreas Bergsland, Oeyvind Brandtsegg, Francesco Biasiol, Giorgio Klauer, Paolo Girol, Francesco Porta, Eric Dexter, Menno Knevel, Joseph Alford, Panos Katergiathis, James Mobberley, Fabio Macelloni, Maurizio Goina, Andrés Cabrera, Peiman Khosravi, Rory Walsh, Luis Jure, and Giovanni Doro.

# DL-Turbo-Free ⚡️

DL-Turbo-Free is a high-performance WPF application designed to streamline dubbing and video production workflows. It seamlessly converts subtitle files (`.srt`, `.ass`) into beautifully formatted Word documents (`.docx`), optimizing script management for voice actors and directors.

## ✨ Key Features

* **Multi-Format Support:** Easily import and parse standard `.srt` and `.ass` subtitle files.
* **Modernized UI:** Navigate through a sleek, dark-themed interface with an intuitive card-based layout.
* **Advanced Actor Management:** * Automatically parse and fill actor roles from multiple subtitle formats.
    * **Custom Separators:** Define custom separators with runtime regex generation, complete with live preview and validation.
* **Smart Script Merging & Export:** Generate ready-to-print `.docx` scripts with built-in statistics, row alignment, and syntax highlighting.
* **Actor Splitter:** Isolate and export individual `.srt` files per actor for granular control during the dubbing process.

## 🚀 Recent Updates & Under-the-Hood Improvements

* **Asynchronous Processing:** Conversion operations are now fully asynchronous, keeping the UI highly responsive during heavy script processing.
* **Performance Boosts:** Implemented aggressive Regex caching for significantly faster subtitle parsing.
* **Enhanced Stability:** Fixed UI-related bugs (such as `InvalidCastException` in dropdown selections) and resolved row index mismatches in the DocX statistics table.

## 🛠️ Tech Stack

* **.NET 10** (WPF)
* **Regex Source Generators** (for high-performance text parsing)
* **System.Text.Json**
* **Xceed DocX** (for robust Word document generation)

## 📦 Installation & Usage

1. Clone the repository: `git clone https://github.com/KillerTonix/DL-Turbo-Free.git`
2. Open the solution in Visual Studio.
3. Build and run the project (Requires .NET 10 SDK).
4. Load your `.srt` or `.ass` files, configure your actor separators, and export your `.docx` script!

## Screnshot
<img width="797" height="642" alt="image" src="https://github.com/user-attachments/assets/733cdd56-ca14-423b-8070-932ed6c6ced4" />


## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to check the [issues page](https://github.com/KillerTonix/DL-Turbo-Free/issues).

# DL Turbo Free

A high-performance WPF application built with **.NET 10** that converts subtitle files (`.srt`, `.ass`) into formatted Word documents (`.docx`) for dubbing and video production workflows.

## 🚀 Features

### Core Functionality
* **Multi-Format Support:** fast parsing of SubRip (`.srt`) and Advanced Substation Alpha (`.ass`) files.
* **Smart Script Merging:** Automatically merges subtitle lines based on time gaps (e.g., gaps < 2s are merged into one line) to create readable dubbing scripts.
* **Word Export:** Generates professional `.docx` tables with columns for Time, Actor, and Text.
* **Actor Management:**
    * Parses multiple actor formats: `[Name] Text`, `Name: Text`, `Name/`, `(Name)`.
    * **Auto-fill Undefined:** Automatically propagates actor names to subsequent lines if they are undefined.
    * **Highlighting:** specific actors can be highlighted in the output document with custom colors (Cyan, Green, Yellow, etc.).
* **Statistics:** Generates a summary table at the end of the document counting lines per actor.

### Tools
* **Actor Splitter:** Export separate `.srt` files for each actor (useful for individual voice actor recording sessions).
* **Memory Optimization:** Uses .NET 10 Source Generators and UTF-8 byte arrays for low-memory JSON processing.

## 🛠️ Tech Stack

* **Framework:** .NET 10 (C# 14) / WPF
* **Architecture:** Modern Service-based architecture with `record` types and immutable data models.
* **Parsing:** Regex Source Generators (`[GeneratedRegex]`) for high-performance text extraction.
* **Serialization:** `System.Text.Json` with Source Generators (`JsonSerializerContext`).
* **Word Generation:** [Xceed DocX](https://github.com/xceedsoftware/DocX) library (Fluent API).

## 📦 Installation

1.  Ensure you have the **.NET 10 Runtime** installed.
2.  Download the latest release from the [Releases Page](#).
3.  Run `SrtToDocx.exe`.

## 📖 Usage

1.  **Import:** Click to load an `.srt` or `.ass` file.
2.  **Settings:**
    * Select the **Highlight Actor** from the dropdown.
    * Choose a **Highlight Color**.
3.  **Export:**
    * **To Docx:** Generates the script table.
    * **To Separate Srt:** Creates a folder with individual subtitle files for every actor found.

## 📄 License
Distributed under the MIT License. See `LICENSE` for more information.

![DL Turbo Free](https://github.com/user-attachments/assets/09b9c8c2-04f6-41fe-91d1-70014e3ba73c)

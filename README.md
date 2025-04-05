# Typing Behavior Recognition

A C#/.NET 9 project for identifying users based on **typing behavior** — specifically, the unique rhythm and dynamics of their keystrokes. The application collects metadata about key events (not text) and uses machine learning to recognize individuals.

---

## 📌 Features

- ⌨️ Collects key press/release timings (keyDown, keyUp, durations, intervals)
- 📁 Generates anonymized CSV datasets without logging typed content
- 🧠 Machine Learning module for behavioral biometric recognition
- 🛡️ Privacy-first: no content logging, fully anonymous biometric profiling
- 🧩 Built using the MVVM architectural pattern for clean separation of concerns

---

## 🧱 Technologies Used

- **.NET 9**
- **C#** with **WPF / WinUI** (MVVM pattern)
- **ML.NET** or optional integration with Python models (e.g. via ONNX or IPC)
- **ReactiveUI** / **CommunityToolkit.Mvvm** (recommended for MVVM support)
- CSV I/O with `CsvHelper` or built-in APIs

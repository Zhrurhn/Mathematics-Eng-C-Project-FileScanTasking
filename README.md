# Sequential Task Management Project - File Scan Tasking

## Overview
The **Sequential Task Management Project** is a Windows desktop application that helps users efficiently manage file scanning tasks. By integrating with the VirusTotal API, it allows for easy queuing, scanning, and reporting of file statuses. Designed for both academic and practical use, this project highlights structured task execution and simple but powerful features.

---

## Features
- **Task Sequencing**:
  - Files are processed sequentially, ensuring logical task execution.
  - Clear progress updates via a progress bar.
- **File Analysis**:
  - Compute file hashes (SHA256) for verification.
  - Retrieve VirusTotal reports for scanned files.
- **Reporting**:
  - Generate and save scan results in JSON and CSV formats.
  - Automatically organize reports into designated directories.
- **Notification System**:
  - Display progress and error messages.
  - Generate email reports for completed scans.
- **Error Management**:
  - Handle API or network errors gracefully.
  - Inform the user of any missing or invalid files.

---

## Installation

### Requirements
- **.NET 6.0 or higher**
- Visual Studio 2022 or higher
- VirusTotal API key

### Setup Instructions
1. Clone this repository:
   ```bash
   git clone https://github.com/your-repo/FileScanTasking.git
   ```
2. Open the solution file `SequentialTaskManager.sln` in Visual Studio.
3. Add your VirusTotal API key to `appsettings.json`:
   ```json
   {
       "VirusTotalApiKey": "your-api-key-here"
   }
   ```
4. Build the solution:
   - Go to `Build` > `Build Solution` or press `Ctrl + Shift + B`.
5. Run the application:
   - Press `F5` or click the `Start` button in Visual Studio.

---

## Usage

### Adding Files
1. Click the **Add File** button to open a file dialog.
2. Select one or more files to add to the scanning queue.
3. The file list will display the file names and their masked hash values.

### Running Scans
1. Click the **Start Scan** button to process all queued files sequentially.
2. The application will:
   - Compute file hashes.
   - Query VirusTotal for file reports.
   - Save results as JSON and CSV in the `Results` directory.

### Viewing Reports
- Reports are stored in:
  - JSON format: `Results/Log_Json`
  - CSV format: `Results/Csv`

### Email Reports
- A summary email is sent after each scan if email is configured.

---

## Project Structure

```plaintext
SequentialTaskManager/
├── MainWindow.xaml          # UI definition
├── MainWindow.xaml.cs       # UI logic and event handling
├── TaskManager.cs           # Queue management for files
├── VirusTotalService.cs     # API calls to VirusTotal
├── FileScan.cs              # File metadata and hash calculations
├── appsettings.json         # Configuration file (API key storage)
├── Results/                 # Generated reports (JSON and CSV)
│   ├── Log_Json/            # JSON reports
│   └── Csv/                 # CSV reports
```

---

## Example Commands

### Adding Files
- Use the **Add File** button in the application interface to select files.

### Running Scans
- Click the **Start Scan** button to process all files in the queue.

### Viewing Reports
- Access JSON and CSV reports in the `Results` directory using any compatible viewer.

---

## Contributing
We welcome contributions to enhance this project. To contribute:
1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature-branch-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add a new feature"
   ```
4. Push to your fork and submit a pull request.

---

## Future Enhancements
- Add support for parallel file processing.
- Implement a dashboard for detailed report analysis.
- Enhance UI for drag-and-drop file uploads.
- Integrate additional DLP features for broader file security.

---




# 🎒 OKKT - Class Trip Planner

A comprehensive and intuitive mobile application designed to simplify class trip planning, cost calculations, and financial management for teachers and students.

![Tech Stack](https://skillicons.dev/icons?i=visualstudio,dotnet,cs,github)

---

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Technical Details](#technical-details)
- [Customization](#customization)
- [Contributing](#contributing)

---

## Features

### 🎯 Core Functionality
- **Trip Planning:** Create detailed trip plans with destinations, dates, and participant information
- **Cost Management:** Add multiple cost items with support for regular and discounted pricing
- **Financial Analysis:** Calculate per-person costs and monthly savings requirements
- **Smart Suggestions:** Get recommendations when costs exceed available funds

### 📊 Advanced Analytics
- **Visual Charts:** Interactive pie charts showing coverage and cost distribution
- **Individual Analysis:** Student-by-student financial capability assessment
- **Cost Breakdown:** Detailed visualization of expense categories
- **Coverage Reports:** Clear indicators of who can afford the trip

### 💾 Data Management
- **Local Storage:** Save trip data locally in JSON format
- **Photo Documentation:** Attach receipts and photos for expense tracking
- **PDF Export:** Generate comprehensive trip reports in PDF format
- **Trip Library:** Browse and manage all saved trip plans

### 🎨 User Experience
- **Dark Theme:** Easy-on-the-eyes dark interface
- **Responsive Design:** Optimized for mobile devices
- **Intuitive Navigation:** Tab-based interface for seamless workflow
- **Visual Feedback:** Animations and transitions for better user engagement

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code
- Android SDK (for Android development)

### Installation
#### Clone the Repository
Clone the OKKT25 repository from GitHub and navigate to the project directory.

#### Restore Dependencies
Restore the necessary dependencies for the project.

#### Build the Application
Build the application for development.

#### Run on Android
Run the application on an Android device or emulator.

#### Building for Different Platforms
- **Android:** Build the application for Android in release mode.
- **Windows:** Build the application for Windows in release mode.

---

## Usage

### Creating a New Trip
1. Open the app and navigate to "New Trip"
2. Fill in trip details:
   - Trip name and destination
   - Start and end dates
   - Number of participants
3. Add cost items with optional discounts
4. Set pocket money preferences (group average or per-person)
5. Calculate and review results

### Managing Saved Trips
1. Go to the "Planned Trips" tab
2. View all saved trip summaries
3. Tap any trip to see detailed information
4. Add photos or export as PDF

### Financial Planning
The app automatically calculates:
- Total trip cost
- Cost per participant
- Monthly savings required
- Individual coverage analysis

Get suggestions for cost reduction if needed.

---

## Project Structure
- **Models/**: Contains data models for cost items, trip data, and trip summaries
- **Views/**: Includes XAML files for the main page, past trips, trip details, and image viewer
- **Resources/**: Stores application styling and assets

---

## Technical Details
### Architecture
- **Framework:** .NET MAUI (Multi-platform App UI)
- **Language:** C#
- **UI:** XAML with MVVM pattern
- **Storage:** Local JSON files
- **Charts:** Custom MAUI Graphics drawing

### Key Components
#### Cost Calculation
Calculates the total cost based on regular and discounted pricing for cost items.

#### PDF Export
- Uses PdfSharpCore for document generation
- Includes trip details, costs, and attached photos
- Automatically saves to device downloads folder

#### Photo Management
- Capture new photos or select from gallery
- Store locally with trip data
- View with zoom and pan capabilities

---

## Customization
### Theming
The app uses a consistent dark theme with orange accents. Customize colors in:
- Resources/Styles/Colors.xaml
- Resources/Styles/Styles.xaml

---

## Contributing
1. Fork the project
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

Built with ❤️ using .NET MAUI for PENdroid!

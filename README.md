# ABB-WEBSOCKET-CONSOLE

🤖 RWS Robot Web Services Client for ABB Robots

A comprehensive .NET client library for ABB Robot Web Services (RWS) API.

ABB RobotWare 6.16.

📋 Overview

A feature-rich .NET client for interacting with ABB industrial robots through the Robot Web Services (RWS) REST API. This library provides easy-to-use methods for controlling and monitoring ABB robot controllers, supporting real-time operations and event-driven programming.

✨ Features

🔧 Controller Management
- ✅ Authentication & Session Management - Secure login/logout functionality
- ✅ System Information - Retrieve controller name, type, RobotWare version
- ✅ Options & Configurations - Access system options and features

🔌 I/O System Operations
- 📊 Digital I/O - Read/write DI/DO signals
- 📈 Analog I/O - Read/write AI/AO signals
- 🔍 Signal Monitoring - Real-time state and value tracking
- 📋 Batch Operations - Get all signals with pagination support

⚡ RAPID Programming Interface
- ▶️ Program Control - Start/stop/reset RAPID programs
- 📊 Task Management - Monitor task states (T_ROB1, etc.)
- 🔤 Variable Access - Read/write RAPID variables
- 🔄 Execution States - Get program execution status

📁 File System Operations
- 📂 File Listing - Browse robot controller file system
- 📄 File Metadata - Access file sizes, dates, permissions
- 🗂️ Directory Navigation - Navigate through HOME, system folders

📡 Real-time Event System
- 🌐 WebSocket Support - Real-time event subscriptions
- 🔔 Event Notifications - Subscribe to I/O changes
- 🎯 Custom Subscriptions - Monitor specific resources
- 🔄 Background Processing - Asynchronous event handling



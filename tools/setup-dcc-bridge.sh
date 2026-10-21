#!/bin/bash
# DCC Bridge Setup Script for NAAD Project (Linux/macOS)
# Installs and configures dcc-bridge for Blender, Maya, and 3ds Max

echo "=== DCC Bridge Setup for NAAD ==="

# Check if Python is installed
echo "[1/4] Checking Python installation..."
if ! command -v python3 &> /dev/null; then
    echo "Python3 not found. Please install Python 3.8+ first."
    exit 1
fi
python3 --version

# Install dcc-bridge
echo "[2/4] Installing dcc-bridge..."
pip3 install dcc-bridge

if [ $? -ne 0 ]; then
    echo "Failed to install dcc-bridge"
    exit 1
fi
echo "dcc-bridge installed successfully"

# Configure Blender
echo "[3/4] Configuring Blender..."
dcc setup blender
echo "Blender configured"

# Configure Maya (optional - uncomment if needed)
# echo "Configuring Maya 2024..."
# dcc setup maya --version 2024
# echo "Maya 2024 configured"

# Configure 3ds Max (optional - uncomment if needed)
# echo "Configuring 3ds Max..."
# dcc setup 3dsmax
# echo "3ds Max configured"

# Verify installation
echo "[4/4] Verifying installation..."
dcc status

echo "=== Setup Complete ==="
echo "To use dcc-bridge:"
echo "  - Open your DCC application (Blender, Maya, etc.)"
echo "  - Run: dcc status"
echo "  - Run: dcc run code \"print('hello')\""
echo "  - Run: dcc run file /path/to/script.py"

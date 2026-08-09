#!/bin/bash
set -e

# 1. MainView.cs
sed -i 's/handledEventsToo: true/handledEventsToo: false/g' src/ui/Features/Main/MainView.cs

# 2. AudioVisualizer.cs (PointerPressed)
sed -i 's/e.Handled = true;//g' src/ui/Controls/AudioVisualizerControl/AudioVisualizer.cs

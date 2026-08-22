import sys

def main():
    file = 'src/ui/Features/Main/MainViewModel.cs'
    with open(file, 'r') as f:
        content = f.read()

    # Need to remove the fallback to _mediaInfo?.Dimension.Width/Height in UpdatePosFromEvent and DrawVisualPosOverlay
    pass

if __name__ == "__main__":
    main()

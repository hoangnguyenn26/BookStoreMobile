import os
import re
from difflib import get_close_matches

# Paths
VIEWS_DIR = "Views"
ICONS_DIR = os.path.join("Resources", "Icons")

# Regex to find icon/image references in XAML
icon_patterns = [
    r'Icon="([^"]+)"',
    r"Icon='([^']+)'",
    r'Source="([^"]+)"',
    r"Source='([^']+)'"
]

# Gather all icon/image references from XAML files
def find_icon_references():
    icon_refs = set()
    for root, _, files in os.walk(VIEWS_DIR):
        for file in files:
            if file.endswith(".xaml"):
                with open(os.path.join(root, file), encoding="utf-8") as f:
                    content = f.read()
                    for pattern in icon_patterns:
                        matches = re.findall(pattern, content)
                        for match in matches:
                            if match.endswith(".png"):
                                icon_refs.add(match)
    return icon_refs

# List all icon files in the Icons directory
def list_icon_files():
    return set(f for f in os.listdir(ICONS_DIR) if f.endswith(".png"))

def main():
    icon_refs = find_icon_references()
    icon_files = list_icon_files()

    print("=== Icon References in XAML ===")
    for ref in sorted(icon_refs):
        print(ref)
    print("\n=== Icon Files in Resources/Icons ===")
    for icon in sorted(icon_files):
        print(icon)

    print("\n=== Matching & Suggestions ===")
    for ref in sorted(icon_refs):
        if ref in icon_files:
            print(f"[OK] {ref} found.")
        else:
            # Try to find a close match
            close = get_close_matches(ref, icon_files, n=1, cutoff=0.6)
            if close:
                print(f"[SUGGEST] {ref} not found. Consider renaming {close[0]} -> {ref}")
                # Uncomment below to actually rename (be careful!)
                # os.rename(os.path.join(ICONS_DIR, close[0]), os.path.join(ICONS_DIR, ref))
            else:
                print(f"[MISSING] {ref} not found and no close match.")

if __name__ == "__main__":
    main()

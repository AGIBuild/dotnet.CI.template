#!/usr/bin/env bash
set -euo pipefail

# Initializes this template with your project name via interactive wizard.
# Usage:
#   ./init.sh                                        # interactive
#   ./init.sh --name Acme.Payments --yes              # non-interactive

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
OLD_SAMPLE="Dotnet.CI.Template.Sample"
OLD_SLN="Dotnet.CI.Template"

NEW_NAME=""
AUTHOR=""
RESET_GIT=false
FORCE=false

# ── Parse arguments ──────────────────────────────────────────────────

while [[ $# -gt 0 ]]; do
  case "$1" in
    --name)     NEW_NAME="$2"; shift 2 ;;
    --author)   AUTHOR="$2"; shift 2 ;;
    --reset-git) RESET_GIT=true; shift ;;
    --yes)      FORCE=true; shift ;;
    *)          echo "Unknown option: $1"; exit 1 ;;
  esac
done

validate_name() {
  [[ "$1" =~ ^[A-Za-z][A-Za-z0-9.]*$ ]]
}

# ── Interactive wizard ───────────────────────────────────────────────

if [ "$FORCE" = false ]; then
  echo ""
  echo "  dotnet.CI.template - Project Setup"
  echo "  ==================================="
  echo ""
  echo "  This wizard will customize the template for your project."
  echo ""

  # Project name
  if [ -z "$NEW_NAME" ]; then
    while true; do
      printf "  ? Project name (e.g. Acme.Payments): "
      read -r NEW_NAME
      if validate_name "$NEW_NAME"; then
        break
      fi
      echo "    Must start with a letter and contain only letters, digits, and dots."
      NEW_NAME=""
    done
  fi
  echo "    Project name: $NEW_NAME"

  # Author
  if [ -z "$AUTHOR" ]; then
    printf "  ? Author / organization (leave blank to skip): "
    read -r AUTHOR
  fi
  if [ -n "$AUTHOR" ]; then
    echo "    Author: $AUTHOR"
  fi

  # Reset git
  if [ "$RESET_GIT" = false ]; then
    printf "  ? Reset git history to a fresh commit? [y/N]: "
    read -r reset_answer
    case "$reset_answer" in
      [Yy]*) RESET_GIT=true ;;
    esac
  fi

  # Preview
  affected_files=$(find "$SCRIPT_DIR" -type f \
    -not -path '*/.git/*' \
    -not -path '*/node_modules/*' \
    -not -path '*/artifacts/*' \
    -not -path '*/.vitepress/dist/*' \
    -not -path '*/bin/*' \
    -not -path '*/obj/*' \
    -not -name 'init.sh' \
    -not -name 'init.ps1' \
    -print0 | xargs -0 grep -rl "$OLD_SAMPLE\|$OLD_SLN" 2>/dev/null | wc -l | tr -d ' ')
  affected_dirs=$(find "$SCRIPT_DIR" -type d -name "*$OLD_SAMPLE*" \
    -not -path '*/.git/*' 2>/dev/null | wc -l | tr -d ' ')

  current_version="(unknown)"
  props_file="$SCRIPT_DIR/Directory.Build.props"
  if [ -f "$props_file" ]; then
    cv=$(grep -oP '(?<=<VersionPrefix>)[^<]+' "$props_file" 2>/dev/null || \
         grep -o '<VersionPrefix>[^<]*</VersionPrefix>' "$props_file" | sed 's/<[^>]*>//g')
    [ -n "$cv" ] && current_version="$cv"
  fi

  echo ""
  echo "  ──────────────────────────────────────"
  echo ""
  echo "  The following changes will be applied:"
  echo ""
  echo "    Rename   $OLD_SAMPLE -> $NEW_NAME"
  echo "    Rename   $OLD_SLN.slnx -> $NEW_NAME.slnx"
  [ -n "$AUTHOR" ] && echo "    Update   Authors -> $AUTHOR"
  echo "    Reset    VersionPrefix $current_version -> 0.1.0"
  if [ "$RESET_GIT" = true ]; then
    echo "    Git      reset to fresh commit"
  else
    echo "    Git      preserved"
  fi
  echo "    Affect   $affected_files files, $affected_dirs directories"
  echo ""
  echo "  WARNING: This operation is IRREVERSIBLE."
  echo ""

  printf "  ? Proceed? [y/N]: "
  read -r confirm
  case "$confirm" in
    [Yy]*) ;;
    *)
      echo ""
      echo "  Cancelled."
      exit 0
      ;;
  esac
  echo ""
else
  if [ -z "$NEW_NAME" ]; then
    echo "Error: --name is required when using --yes."
    exit 1
  fi
  if ! validate_name "$NEW_NAME"; then
    echo "Error: Project name must start with a letter and contain only letters, digits, and dots."
    exit 1
  fi
fi

# ── Execute ──────────────────────────────────────────────────────────

echo "[1/6] Replacing file contents..."
find "$SCRIPT_DIR" -type f \
  -not -path '*/.git/*' \
  -not -path '*/node_modules/*' \
  -not -path '*/artifacts/*' \
  -not -path '*/.vitepress/dist/*' \
  -not -path '*/bin/*' \
  -not -path '*/obj/*' \
  -not -name 'init.sh' \
  -not -name 'init.ps1' \
  -print0 | while IFS= read -r -d '' file; do
    if file "$file" | grep -q text; then
      if grep -ql "$OLD_SAMPLE" "$file" 2>/dev/null; then
        sed -i '' "s|$OLD_SAMPLE|$NEW_NAME|g" "$file" 2>/dev/null || \
        sed -i "s|$OLD_SAMPLE|$NEW_NAME|g" "$file"
      fi
      if grep -ql "$OLD_SLN" "$file" 2>/dev/null; then
        sed -i '' "s|$OLD_SLN|$NEW_NAME|g" "$file" 2>/dev/null || \
        sed -i "s|$OLD_SLN|$NEW_NAME|g" "$file"
      fi
    fi
done

# Author update
if [ -n "$AUTHOR" ]; then
  echo "[2/6] Updating author..."
  find "$SCRIPT_DIR" -type f -name '*.csproj' \
    -not -path '*/build/*' \
    -not -path '*/_build/*' | while IFS= read -r csproj; do
      if grep -q '<Authors>' "$csproj" 2>/dev/null; then
        sed -i '' "s|<Authors>[^<]*</Authors>|<Authors>$AUTHOR</Authors>|g" "$csproj" 2>/dev/null || \
        sed -i "s|<Authors>[^<]*</Authors>|<Authors>$AUTHOR</Authors>|g" "$csproj"
      fi
  done
else
  echo "[2/6] Skipping author update..."
fi

# Rename directories (deepest first)
echo "[3/6] Renaming directories..."
find "$SCRIPT_DIR" -depth -type d -name "*$OLD_SAMPLE*" \
  -not -path '*/.git/*' \
  -not -path '*/node_modules/*' | while IFS= read -r dir; do
    new_dir="$(dirname "$dir")/$(basename "$dir" | sed "s|$OLD_SAMPLE|$NEW_NAME|g")"
    mv "$dir" "$new_dir"
    echo "  $dir -> $new_dir"
done

# Rename files
echo "[4/6] Renaming files..."
find "$SCRIPT_DIR" -type f -name "*$OLD_SAMPLE*" \
  -not -path '*/.git/*' \
  -not -path '*/node_modules/*' | while IFS= read -r file; do
    new_file="$(dirname "$file")/$(basename "$file" | sed "s|$OLD_SAMPLE|$NEW_NAME|g")"
    mv "$file" "$new_file"
    echo "  $file -> $new_file"
done

if [ -f "$SCRIPT_DIR/$OLD_SLN.slnx" ]; then
  mv "$SCRIPT_DIR/$OLD_SLN.slnx" "$SCRIPT_DIR/$NEW_NAME.slnx"
  echo "  $OLD_SLN.slnx -> $NEW_NAME.slnx"
fi

# Reset version
echo "[5/6] Resetting version to 0.1.0..."
PROPS_FILE="$SCRIPT_DIR/Directory.Build.props"
if [ -f "$PROPS_FILE" ]; then
  sed -i '' "s|<VersionPrefix>[^<]*</VersionPrefix>|<VersionPrefix>0.1.0</VersionPrefix>|g" "$PROPS_FILE" 2>/dev/null || \
  sed -i "s|<VersionPrefix>[^<]*</VersionPrefix>|<VersionPrefix>0.1.0</VersionPrefix>|g" "$PROPS_FILE"
fi

# Update build paths
echo "[6/6] Updating build configuration..."
PARAMS_FILE="$SCRIPT_DIR/build/BuildTask.Parameters.cs"
if [ -f "$PARAMS_FILE" ]; then
  sed -i '' "s|$OLD_SLN\.slnx|$NEW_NAME.slnx|g" "$PARAMS_FILE" 2>/dev/null || \
  sed -i "s|$OLD_SLN\.slnx|$NEW_NAME.slnx|g" "$PARAMS_FILE"
fi

# Optional: reset git history
if [ "$RESET_GIT" = true ]; then
  echo ""
  echo "Resetting git history..."
  rm -rf "$SCRIPT_DIR/.git"
  git -C "$SCRIPT_DIR" init
  git -C "$SCRIPT_DIR" add .
  git -C "$SCRIPT_DIR" commit -m "Initial commit from dotnet.CI.template"
fi

# Clean up init scripts
rm -f "$SCRIPT_DIR/init.sh" "$SCRIPT_DIR/init.ps1"

echo ""
echo "Done! Your project '$NEW_NAME' is ready."
echo ""
echo "Next steps:"
echo "  1. Update GitHub URL in docs/.vitepress/config.ts"
echo "  2. Run: dotnet restore --force-evaluate"
echo "  3. Run: dotnet build"

#!/usr/bin/env bash
set -euo pipefail

# Thin wrapper that collects interactive input, then delegates to NUKE Init target.
# Usage:
#   ./init.sh                                        # interactive
#   ./init.sh --name Acme.Payments --yes              # non-interactive

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

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
  echo "  ChengYuan - Project Setup"
  echo "  ========================="
  echo ""
  echo "  This wizard will customize the template for your project."
  echo ""

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

  if [ -z "$AUTHOR" ]; then
    printf "  ? Author / organization (leave blank to skip): "
    read -r AUTHOR
  fi
  if [ -n "$AUTHOR" ]; then
    echo "    Author: $AUTHOR"
  fi

  if [ "$RESET_GIT" = false ]; then
    printf "  ? Reset git history to a fresh commit? [y/N]: "
    read -r reset_answer
    case "$reset_answer" in
      [Yy]*) RESET_GIT=true ;;
    esac
  fi

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

# ── Delegate to NUKE Init target ─────────────────────────────────────

chmod +x "$SCRIPT_DIR/build.sh"

ARGS=(Init --ProjectName "$NEW_NAME")
if [ -n "$AUTHOR" ]; then
  ARGS+=(--Author "$AUTHOR")
fi
if [ "$RESET_GIT" = true ]; then
  ARGS+=(--ResetGit)
fi

"$SCRIPT_DIR/build.sh" "${ARGS[@]}"

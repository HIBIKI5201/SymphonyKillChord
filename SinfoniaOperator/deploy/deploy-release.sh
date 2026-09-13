#!/usr/bin/env bash

set -euo pipefail

readonly PACKAGE_PATH="${1:?package path is required}"
readonly RELEASE_ID="${2:?release id is required}"
readonly DEPLOY_ROOT="${3:-/opt/sinfonia-specsearch}"
readonly SERVICE_NAME="${4:-sinfonia-specsearch.service}"
readonly RELEASES_DIRECTORY="$DEPLOY_ROOT/releases"
readonly RELEASE_DIRECTORY="$RELEASES_DIRECTORY/$RELEASE_ID"
readonly STAGING_DIRECTORY="$RELEASE_DIRECTORY.staging"
readonly CURRENT_PATH="$DEPLOY_ROOT/publish"
readonly MAX_RELEASE_COUNT=3

if [[ ! "$RELEASE_ID" =~ ^[0-9a-f]{40}-[0-9]+-[0-9]+$ ]]; then
  echo "[Deploy] release id must contain a Git commit SHA, run id, and attempt." >&2
  exit 1
fi

if [[ "$DEPLOY_ROOT" != /opt/* || "$DEPLOY_ROOT" == /opt ]]; then
  echo "[Deploy] deploy root must be a directory directly below /opt." >&2
  exit 1
fi

if [[ ! -f "$PACKAGE_PATH" ]]; then
  echo "[Deploy] package not found: $PACKAGE_PATH" >&2
  exit 1
fi

mkdir -p "$RELEASES_DIRECTORY"
rm -rf -- "$STAGING_DIRECTORY"
mkdir -p "$STAGING_DIRECTORY"
tar --extract --gzip --file "$PACKAGE_PATH" --directory "$STAGING_DIRECTORY"

if [[ ! -f "$STAGING_DIRECTORY/SinfoniaOperator" || ! -f "$STAGING_DIRECTORY/SinfoniaOperator.dll" ]]; then
  echo "[Deploy] published application is incomplete." >&2
  rm -rf -- "$STAGING_DIRECTORY"
  exit 1
fi
chmod 755 "$STAGING_DIRECTORY/SinfoniaOperator"

machine_architecture="$(uname -m)"
binary_description="$(file --brief "$STAGING_DIRECTORY/SinfoniaOperator")"
case "$machine_architecture" in
  x86_64)
    expected_binary_architecture="x86-64"
    ;;
  aarch64|arm64)
    expected_binary_architecture="ARM aarch64"
    ;;
  *)
    echo "[Deploy] unsupported server architecture: $machine_architecture" >&2
    rm -rf -- "$STAGING_DIRECTORY"
    exit 1
    ;;
esac

if [[ "$binary_description" != *"$expected_binary_architecture"* ]]; then
  echo "[Deploy] binary architecture does not match server architecture: $binary_description" >&2
  rm -rf -- "$STAGING_DIRECTORY"
  exit 1
fi

rm -rf -- "$RELEASE_DIRECTORY"
mv "$STAGING_DIRECTORY" "$RELEASE_DIRECTORY"

previous_target=""
if [[ -L "$CURRENT_PATH" ]]; then
  previous_target="$(readlink -f "$CURRENT_PATH")"
elif [[ -d "$CURRENT_PATH" ]]; then
  previous_target="$RELEASES_DIRECTORY/legacy-$(date -u +%Y%m%d%H%M%S)"
  mv "$CURRENT_PATH" "$previous_target"
elif [[ -e "$CURRENT_PATH" ]]; then
  echo "[Deploy] current publish path is neither a directory nor a symlink." >&2
  exit 1
fi

activate_release() {
  local target="$1"
  local temporary_link="$DEPLOY_ROOT/.publish.next"

  rm -f -- "$temporary_link"
  ln -s "$target" "$temporary_link"
  mv -Tf "$temporary_link" "$CURRENT_PATH"
}

rollback() {
  echo "[Deploy] health check failed; rolling back." >&2
  sudo systemctl stop "$SERVICE_NAME" || true
  if [[ -n "$previous_target" && -d "$previous_target" ]]; then
    activate_release "$previous_target"
    sudo systemctl start "$SERVICE_NAME"
  fi
}

activate_release "$RELEASE_DIRECTORY"
deployment_start="$(date -u '+%Y-%m-%d %H:%M:%S')"
sudo systemctl restart "$SERVICE_NAME"

is_ready=false
for _ in {1..90}; do
  if ! sudo systemctl is-active --quiet "$SERVICE_NAME"; then
    rollback
    sudo journalctl --unit "$SERVICE_NAME" --lines 50 --no-pager >&2
    exit 1
  fi

  registration_logs="$(sudo journalctl --unit "$SERVICE_NAME" --since "$deployment_start" --no-pager)"
  if grep --fixed-strings --quiet "/branchesを" <<< "$registration_logs"; then
    is_ready=true
    break
  fi

  sleep 2
done

if [[ "$is_ready" != true ]]; then
  echo "[Deploy] /branches command registration was not confirmed." >&2
  rollback
  printf '%s\n' "$registration_logs" >&2
  exit 1
fi

rm -f -- "$PACKAGE_PATH"

touch "$RELEASE_DIRECTORY"
mapfile -t release_directories < <(
  find "$RELEASES_DIRECTORY" -mindepth 1 -maxdepth 1 -type d -printf '%T@ %p\n' \
    | sort --numeric-sort --reverse \
    | cut --delimiter=' ' --fields=2-
)
for ((index = MAX_RELEASE_COUNT; index < ${#release_directories[@]}; index++)); do
  stale_release="${release_directories[$index]}"
  if [[ "$stale_release" == "$RELEASES_DIRECTORY/"* ]]; then
    rm -rf -- "$stale_release"
  fi
done

echo "[Deploy] release $RELEASE_ID is active."
sudo systemctl status "$SERVICE_NAME" --no-pager

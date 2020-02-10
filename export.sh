#!/bin/bash

set -euo pipefail

export DOCKER_CMD="docker run --rm -ti -v $(pwd):/code godot:latest"

echo "::[ Exporting to Windows ...]"
$DOCKER_CMD --export "Windows Desktop" ./exports/SC-Space-Shooter-Win64.exe
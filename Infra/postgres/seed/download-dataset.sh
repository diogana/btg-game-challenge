#!/usr/bin/env bash
set -euo pipefail
seed_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
archive_path="$(mktemp)"
trap 'rm -f "$archive_path"' EXIT
curl --fail --location --retry 2 https://www.kaggle.com/api/v1/datasets/download/evgeny1928/playstation-games-info --output "$archive_path"
python3 - "$archive_path" "$seed_dir/output.json" <<'PYTHON'
import json, sys, zipfile
from pathlib import Path
with zipfile.ZipFile(sys.argv[1]) as archive:
    raw = archive.read('output.json')
    data = json.loads(raw)
    if not isinstance(data, dict):
        raise ValueError('Expected object keyed by catalog URL')
    Path(sys.argv[2]).write_bytes(raw)
    print(f'Dataset ready: {len(data)} entries')
PYTHON

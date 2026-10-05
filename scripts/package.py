from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

root = Path(__file__).resolve().parents[1]
target = root.parent / "btg-software-engineering-challenge.zip"
excluded = {".git", ".vs", ".idea", ".vscode", "bin", "obj", "node_modules", "dist", ".angular", ".terraform", "TestResults", "__pycache__"}
with ZipFile(target, "w", ZIP_DEFLATED) as archive:
    for path in sorted(root.rglob("*")):
        relative = path.relative_to(root)
        if not path.is_file() or excluded.intersection(relative.parts):
            continue
        if path.name in {".env", "output.json", "dataset.zip", "crash.log"} or ".tfstate" in path.name or path.suffix in {".tfplan", ".tfvars", ".pyc"}:
            continue
        if "cypress" in relative.parts and ("videos" in relative.parts or "screenshots" in relative.parts):
            continue
        archive.write(path, Path(root.name) / relative)
with ZipFile(target) as archive:
    assert archive.testzip() is None
    assert root.name + "/README.md" in archive.namelist()
    print(f"Validated ZIP: {len(archive.namelist())} files, {target.stat().st_size} bytes")
print(target)

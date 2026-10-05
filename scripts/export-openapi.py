import argparse
import json
import urllib.request
from pathlib import Path
import yaml
from openapi_spec_validator import validate_spec

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument("--check", action="store_true", help="Compare running APIs with committed contracts")
parser.add_argument("--offline", action="store_true", help="Validate committed YAML only")
args = parser.parse_args()
for family, port in [("Auth", 5002), ("Core", 5001), ("Bff", 5000)]:
    path = root / f"src/{family}/GameLending.{family}.Api/openapi.yaml"
    if args.offline:
        contract = yaml.safe_load(path.read_text())
    else:
        with urllib.request.urlopen(f"http://localhost:{port}/swagger/v1/swagger.json", timeout=30) as response:
            contract = json.load(response)
        if args.check:
            if contract != yaml.safe_load(path.read_text()):
                raise SystemExit(f"Contract drift: {path}")
        else:
            path.write_text(yaml.safe_dump(contract, allow_unicode=True, sort_keys=False))
    validate_spec(contract)
    assert contract["openapi"].startswith("3.1.")
    assert contract["components"]["securitySchemes"]["Bearer"]["scheme"] == "bearer"
    print(f"{family}: valid OpenAPI {contract['openapi']}, {len(contract['paths'])} paths")

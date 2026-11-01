"""Resumable audit bookkeeping; conformance is never inferred from pending rows."""
from pathlib import Path
from datetime import datetime, timezone
import hashlib
import json
import re

BASE = Path(__file__).resolve().parent
ROOT = BASE.parents[2]

def read(name):
    return json.loads((BASE / name).read_text(encoding="utf-8-sig"))

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()

def save(name, value):
    path = BASE / name
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    temporary.replace(path)

def now():
    return datetime.now(timezone.utc).isoformat()

def seed():
    inventory = read("AUDIT.inventory.json")
    structure = read("AUDIT.structure.json")
    if structure.get("compilation_errors"):
        raise RuntimeError("Semantic compilation has unresolved errors")
    manifest = read("consistency.json")
    progress = read("TODO.progress.json")
    old = read("AUDIT.coverage.json")
    if isinstance(old, dict) and "criteria" in old:
        return
    files = inventory["files"]
    lib = [f["path"] for f in files if f["role"] == "library" and f["path"].endswith(".cs") and not f["generated"]]
    tests = [f["path"] for f in files if f["role"] == "tests" and f["path"].endswith(".cs")]
    config = [f["path"] for f in files if f["role"] == "configuration and guides" or f["path"].endswith(".csproj")]
    test_inputs = [f["path"] for f in files if f["role"] == "tests"]
    criteria = {}
    for number in range(1, 51):
        criterion = f"C{number:02}"
        paths = lib
        if number <= 8 or number in (37, 45, 49):
            paths = lib + tests
        if 32 <= number <= 35:
            paths = test_inputs
        if number in (6, 36, 42, 43, 44):
            paths = paths + config
        if number == 38:
            paths = config
        if 39 <= number <= 41:
            paths = [f["path"] for f in files if f["style_eligible"]]
        criteria[criterion] = {
            "target": "CONVENTIONS.md#" + criterion.lower(),
            "comparison_unit": "Maintained file and its structurally enumerated declarations/boundaries; applicability and semantic checks recorded explicitly",
            "records": [{"path": p, "disposition": "Unreviewed", "reason": "Awaiting criterion-specific inspection", "evidence": [], "tasks": []} for p in sorted(set(paths))]
        }
    coverage = {
        "schema_version": 2,
        "conventions_revision": manifest["conventions_revision"],
        "conventions_digest": manifest["conventions_digest"],
        "inventory_sha256": digest(BASE / "AUDIT.inventory.json"),
        "historical_c47": old,
        "criteria": criteria,
        "checks": {}
    }
    save("AUDIT.coverage.json", coverage)
    progress["work_units"] = [{"id": "full-inventory", "state": "complete", "eligible_set": [f["path"] for f in files], "processed": [f["path"] for f in files], "remaining": [], "evidence": ["AUDIT.inventory.json", "AUDIT.structure.json", "AUDIT.compilation.json"], "remaining_work": ""}]
    for criterion, unit in criteria.items():
        paths = [r["path"] for r in unit["records"]]
        progress["work_units"].append({"id": criterion + "/solution", "criterion": criterion, "state": "pending", "eligible_set": paths, "processed": [], "remaining": paths, "evidence": ["AUDIT.coverage.json#/criteria/" + criterion], "remaining_work": "Inspect criterion-specific declarations, settings and semantic boundaries; search/parse hits alone do not establish compliance"})
    progress["scope"]["stage"] = "audit"
    progress["status"] = "in_progress"
    progress["inputs"]["inventory_sha256"] = digest(BASE / "AUDIT.inventory.json")
    progress["inputs"]["structure"] = "AUDIT.structure.json"
    progress["inputs"]["structure_sha256"] = digest(BASE / "AUDIT.structure.json")
    progress["next_step"] = "Revalidate historical C47 scope, review full-library asynchronous boundaries, then complete remaining criteria against the pinned targets."
    progress["checkpoint"] += 1
    progress["updated_at"] = now()
    save("TODO.progress.json", progress)
    manifest["files"]["audit_inspector"] = ["AUDIT.inspect.csproj", "AUDIT.inspect.cs", "AUDIT.compilation.json", "AUDIT.refresh.py"]
    save("consistency.json", manifest)
    publish()

def checkpoint(criterion):
    coverage = read("AUDIT.coverage.json")
    progress = read("TODO.progress.json")
    rows = coverage["criteria"][criterion]["records"]
    unit = next(u for u in progress["work_units"] if u.get("criterion") == criterion)
    unit["processed"] = [r["path"] for r in rows if r["disposition"] != "Unreviewed"]
    unit["remaining"] = [r["path"] for r in rows if r["disposition"] == "Unreviewed"]
    unit["state"] = "in_progress" if unit["remaining"] else "complete"
    unit["remaining_work"] = "Review remaining exact paths and their semantic boundaries" if unit["remaining"] else ""
    progress["checkpoint"] += 1
    progress["updated_at"] = now()
    progress["next_step"] = f"Continue {criterion}: {len(unit['remaining'])} file units remain, then proceed through pending criteria."
    save("TODO.progress.json", progress)
    publish()

def publish():
    coverage = read("AUDIT.coverage.json")
    progress = read("TODO.progress.json")
    path = BASE / "COVERAGE.md"
    old = path.read_text(encoding="utf-8-sig")
    marker = "<!-- current-audit-end -->"
    if marker in old:
        historical = old.split(marker, 1)[1]
    else:
        historical = "\n\n## Historical C47 audit and implementation record\n\nThe following original matrix/reasons are retained for provenance; they are superseded by current per-criterion dispositions above as those units are revalidated.\n\n" + old[old.index("**Freshness:**"):]
    lines = ["# Solution consistency audit coverage", "", "Current full audit: **in progress**, pinned to convention revision **61**. All nine policy decisions were approved and documented. The exact current eligible sets and individual dispositions are in [AUDIT.coverage.json](AUDIT.coverage.json); declaration/call evidence is in [AUDIT.structure.json](AUDIT.structure.json).", "", "Inventory: 1,090 maintained inputs, including 783 library C# files (one generated designer) and 258 test C# files. Both modern and legacy syntax variants parsed without errors; library net10 symbol compilation also has zero errors. Parsing, symbol binding and inventory alone do not count as semantic audit completion.", "", "| Criterion | Compliant | Deviation | Exception | Unreviewed | N/A | Eligible files |", "| --- | ---: | ---: | ---: | ---: | ---: | ---: |"]
    for criterion, unit in sorted(coverage["criteria"].items()):
        counts = {d: sum(r["disposition"] == d for r in unit["records"]) for d in ["Compliant", "Deviation", "Justified exception", "Unreviewed", "Not applicable"]}
        lines.append(f"| [{criterion}](CONVENTIONS.md#{criterion.lower()}) | {counts['Compliant']} | {counts['Deviation']} | {counts['Justified exception']} | {counts['Unreviewed']} | {counts['Not applicable']} | {len(unit['records'])} |")
    complete = sum(u.get("criterion") is not None and u["state"] == "complete" for u in progress["work_units"])
    lines += ["", f"Completed criteria: **{complete}/50**. File units overlap across criteria and must not be summed as distinct files. Unreviewed units are explicit and prevent a complete-audit status. The eight original implementation/policy tasks and nine adopted-policy decisions retain their history in [TODO.md](TODO.md).", "", marker]
    path.write_text("\n".join(lines) + historical, encoding="utf-8")
    for output in progress["outputs"]:
        target = BASE / output["path"]
        if target.exists():
            output["sha256"] = digest(target)
    for name in ["AUDIT.inventory.json", "AUDIT.structure.json", "AUDIT.compilation.json", "AUDIT.refresh.py", "AUDIT.inspect.cs", "AUDIT.inspect.csproj", "DOCUMENTING.adopt.ps1"]:
        if not any(o["path"] == name for o in progress["outputs"]):
            progress["outputs"].append({"path": name, "sha256": digest(BASE / name)})
    save("TODO.progress.json", progress)

if __name__ == "__main__":
    seed()
    print(json.dumps({"status": "in_progress", "criteria": len(read('AUDIT.coverage.json')['criteria'])}))

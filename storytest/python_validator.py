#!/usr/bin/env python3
"""
Story Test Framework - Python Source Validator

Validates Python projects by scanning .py files and flagging Story Test style
violations analogous to the C#/.NET validator. Focuses on pragmatic rules that
map well to Python codebases without requiring third‑party deps.

Rules implemented (mapped to StoryViolationType):
- INCOMPLETE_IMPLEMENTATION:
  • Functions/methods that are stubs (pass/ellipsis) or raise NotImplementedError
  • TODO/FIXME comments in code
- PREMATURE_CELEBRATION:
  • Functions that only return a constant literal regardless of inputs
- UNUSED_CODE (heuristic):
  • Classes with only pass and no members (likely placeholders)
- OTHER:
  • Hollow Enums (Enum with <=1 meaningful members or only placeholder names)

Note: This is intentionally lightweight and safe to run anywhere. It does not
perform import execution or whole‑program analysis.
"""
from __future__ import annotations

import ast
import io
import sys
import tokenize
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, List, Optional, Sequence, Tuple

from .validator import StoryViolation, StoryViolationType

_PLACEHOLDER_NAMES = {"none", "default", "todo", "temp", "placeholder", "unknown"}
_DEFAULT_EXCLUDES = {
    ".git",
    "__pycache__",
    "venv",
    ".venv",
    "env",
    "ENV",
    "dist",
    "build",
    ".eggs",
    "site-packages",
}


@dataclass
class _Context:
    file_path: Path
    module: str


def _iter_python_files(root: Path, includes: Optional[Sequence[str]] = None, excludes: Optional[Sequence[str]] = None) -> Iterable[Path]:
    root = root.resolve()
    inc = list(includes or ["**/*.py"])
    exc = set(excludes or []) | _DEFAULT_EXCLUDES

    seen: set[Path] = set()
    for pattern in inc:
        for p in root.glob(pattern):
            if not p.is_file() or p.suffix != ".py":
                continue
            # Exclude directories in path parts
            if any(part in exc for part in p.parts):
                continue
            if p not in seen:
                seen.add(p)
                yield p


def _read_text(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        return path.read_text(encoding=sys.getdefaultencoding(), errors="ignore")


def _has_todo_or_fixme_tokens(src: str) -> List[Tuple[int, str]]:
    hits: List[Tuple[int, str]] = []
    try:
        for tok in tokenize.generate_tokens(io.StringIO(src).readline):
            if tok.type == tokenize.COMMENT:
                text = tok.string.lower()
                if "todo" in text or "fixme" in text:
                    hits.append((tok.start[0], tok.string.strip()))
    except tokenize.TokenError:
        pass
    return hits


def _is_pass_or_ellipsis_only(body: List[ast.stmt]) -> bool:
    if not body:
        return True
    for stmt in body:
        if isinstance(stmt, ast.Pass):
            continue
        if isinstance(stmt, ast.Expr) and isinstance(getattr(stmt, "value", None), ast.Constant) and getattr(stmt.value, "value", None) is Ellipsis:
            # expression: ...
            continue
        # Allow a sole docstring then pass
        if isinstance(stmt, ast.Expr) and isinstance(getattr(stmt, "value", None), ast.Constant) and isinstance(stmt.value.value, str):
            # Docstring; keep checking others
            continue
        return False
    return True


def _returns_constant_only(node: ast.AST) -> Optional[ast.Return]:
    """Return the return node if the function body is a single constant return."""
    if not isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
        return None
    # Skip if body includes more than one non-docstring statement
    body = [s for s in node.body if not (isinstance(s, ast.Expr) and isinstance(getattr(s, "value", None), ast.Constant) and isinstance(s.value.value, str))]
    if len(body) != 1 or not isinstance(body[0], ast.Return):
        return None
    ret = body[0]
    if isinstance(getattr(ret, "value", None), ast.Constant):
        return ret
    return None


def _detect_enum_hollow(cls: ast.ClassDef) -> bool:
    # Check if inherits Enum/IntEnum names (without importing runtime)
    bases = {getattr(b, "id", None) or getattr(getattr(b, "attr", None), "lower", lambda: None)() for b in cls.bases}
    base_names = {getattr(b, "id", None) or getattr(b, "attr", None) for b in cls.bases}
    normalized = {str(n).lower() for n in base_names if n}
    if not ("enum" in normalized or "intenum" in normalized):
        return False

    members: List[str] = []
    for stmt in cls.body:
        if isinstance(stmt, ast.Assign):
            for t in stmt.targets:
                if isinstance(t, ast.Name):
                    members.append(t.id)
    # Remove dunder and placeholders
    members = [m for m in members if not (m.startswith("__") and m.endswith("__"))]
    meaningful = [m for m in members if m.lower() not in _PLACEHOLDER_NAMES]
    return len(meaningful) <= 1


def _module_name_for(path: Path, root: Path) -> str:
    try:
        rel = path.relative_to(root)
        parts = list(rel.with_suffix("").parts)
        if parts and parts[-1] == "__init__":
            parts = parts[:-1]
        return ".".join(parts) or rel.stem
    except Exception:
        return path.stem


def validate_python_path(
    path: str | Path,
    *,
    includes: Optional[Sequence[str]] = None,
    excludes: Optional[Sequence[str]] = None,
    verbose: bool = False,
) -> List[StoryViolation]:
    """Validate a directory or a file containing Python code.

    Returns a list of StoryViolation entries analogous to the .NET validator.
    """
    p = Path(path)
    root = p if p.is_dir() else p.parent
    violations: List[StoryViolation] = []

    files: Iterable[Path]
    if p.is_file() and p.suffix == ".py":
        files = [p]
    else:
        files = _iter_python_files(root=p, includes=includes, excludes=excludes)

    for file in files:
        src = _read_text(file)
        # Comment scanning for TODO/FIXME
        for line_no, text in _has_todo_or_fixme_tokens(src):
            violations.append(
                StoryViolation(
                    type_name=_module_name_for(file, root),
                    member="<module>",
                    violation=f"Comment contains TODO/FIXME: {text}",
                    violation_type=StoryViolationType.INCOMPLETE_IMPLEMENTATION,
                    file_path=str(file),
                    line_number=line_no,
                )
            )

        # AST-based checks
        try:
            tree = ast.parse(src, filename=str(file))
        except SyntaxError as ex:
            # Skip files with syntax errors (could be for different python versions)
            continue

        module_name = _module_name_for(file, root)

        for node in ast.walk(tree):
            # Function/Method checks
            if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
                # NotImplementedError raise
                for stmt in node.body:
                    if isinstance(stmt, ast.Raise):
                        exc = getattr(stmt, "exc", None)
                        # raise NotImplementedError or raise NotImplementedError()
                        name = None
                        if isinstance(exc, ast.Name):
                            name = exc.id
                        elif isinstance(exc, ast.Call) and isinstance(exc.func, ast.Name):
                            name = exc.func.id
                        if name and name == "NotImplementedError":
                            violations.append(
                                StoryViolation(
                                    type_name=module_name,
                                    member=node.name,
                                    violation="Function raises NotImplementedError",
                                    violation_type=StoryViolationType.INCOMPLETE_IMPLEMENTATION,
                                    file_path=str(file),
                                    line_number=getattr(stmt, "lineno", 0),
                                )
                            )

                # Pass/Ellipsis only
                if _is_pass_or_ellipsis_only(node.body):
                    violations.append(
                        StoryViolation(
                            type_name=module_name,
                            member=node.name,
                            violation="Function is a stub (pass/ellipsis only)",
                            violation_type=StoryViolationType.INCOMPLETE_IMPLEMENTATION,
                            file_path=str(file),
                            line_number=getattr(node, "lineno", 0),
                        )
                    )

                # Constant-only return
                ret = _returns_constant_only(node)
                if ret is not None:
                    violations.append(
                        StoryViolation(
                            type_name=module_name,
                            member=node.name,
                            violation="Function returns a constant literal (suspiciously simple)",
                            violation_type=StoryViolationType.PREMATURE_CELEBRATION,
                            file_path=str(file),
                            line_number=getattr(ret, "lineno", 0),
                        )
                    )

            # Class checks
            if isinstance(node, ast.ClassDef):
                # Empty placeholder class
                non_doc = [s for s in node.body if not (isinstance(s, ast.Expr) and isinstance(getattr(s, "value", None), ast.Constant) and isinstance(s.value.value, str))]
                if _is_pass_or_ellipsis_only(non_doc):
                    violations.append(
                        StoryViolation(
                            type_name=module_name,
                            member=node.name,
                            violation="Class is a placeholder (no members)",
                            violation_type=StoryViolationType.UNUSED_CODE,
                            file_path=str(file),
                            line_number=getattr(node, "lineno", 0),
                        )
                    )

                # Hollow Enums
                if _detect_enum_hollow(node):
                    violations.append(
                        StoryViolation(
                            type_name=module_name,
                            member=node.name,
                            violation="Enum has minimal/placeholder members (hollow enum)",
                            violation_type=StoryViolationType.OTHER,
                            file_path=str(file),
                            line_number=getattr(node, "lineno", 0),
                        )
                    )

    return violations


__all__ = [
    "validate_python_path",
]
#!/usr/bin/env python3
"""Best-effort safety checks for the stand-alone public showcase. No dependencies."""
from __future__ import annotations
from pathlib import Path
import struct
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
DENY_SUFFIXES = {'.cs','.csproj','.sln','.xaml','.dll','.exe','.pdb','.db','.sqlite','.sqlite3','.pem','.pfx','.p12','.key','.bak','.7z','.zip','.env'}
DENY_NAMES = {'id_rsa','id_ed25519','credentials.json','secrets.json','.gitconfig','appsettings.production.json'}
SKIP_DIRS = {'.git','__pycache__','.venv','.idea','node_modules'}
PATTERNS = [
    ('private key', re.compile(r'-----BEGIN (?:RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----')),
    ('GitHub credential', re.compile(r'(?:ghp_|gho_|ghu_|ghs_|ghr_)[A-Za-z0-9_]{20,}')),
    ('GitHub fine-grained token', re.compile(r'github_pat_[A-Za-z0-9_]{20,}')),
    ('AWS access-key identifier', re.compile(r'\b(?:AKIA|ASIA)[A-Z0-9]{16}\b')),
    ('obvious credential assignment', re.compile(r'(?im)^\s*(?:api_key|client_secret|password|access_token)\s*[:=]\s*["\x27]?[A-Za-z0-9/_+=-]{16,}["\x27]?\s*$')),
]
LINK = re.compile(r'!?\[[^\]]*\]\(([^)]+)\)')
errors=[]
filecount=0
for f in sorted(ROOT.rglob('*')):
    if any(p in SKIP_DIRS for p in f.relative_to(ROOT).parts):
        continue
    if not f.is_file():
        continue
    filecount+=1
    if f.suffix.lower() in DENY_SUFFIXES or f.name.lower() in DENY_NAMES or f.name.startswith('.env'):
        errors.append(f'{f.relative_to(ROOT)}: disallowed file type or name')
        continue
    if f.stat().st_size > 500_000:
        errors.append(f'{f.relative_to(ROOT)}: unexpectedly large file')
        continue
    # The showcase permits only sanitized, bounded PNG screenshots in one public asset directory.
    if f.suffix.lower() == '.png':
        rel=f.relative_to(ROOT)
        if len(rel.parts) != 3 or rel.parts[:2] != ('assets', 'screenshots'):
            errors.append(f'{rel}: PNG images are restricted to assets/screenshots/')
        else:
            data=f.read_bytes()
            if (not data.startswith(b'\x89PNG\r\n\x1a\n')
                    or len(data) < 33
                    or data[12:16] != b'IHDR'):
                errors.append(f'{rel}: invalid PNG signature or image header')
            else:
                width, height = struct.unpack('>II', data[16:24])
                if not (320 <= width <= 4096 and 240 <= height <= 4096):
                    errors.append(f'{rel}: unexpected screenshot dimensions')
        continue
    try:
        s=f.read_text(encoding='utf-8')
    except UnicodeError:
        errors.append(f'{f.relative_to(ROOT)}: binary or invalid UTF-8'); continue
    for title,pattern in PATTERNS:
        if pattern.search(s):
            errors.append(f'{f.relative_to(ROOT)}: possible {title}')
    if f.suffix.lower()=='.md':
        for raw in LINK.findall(s):
            path=raw.split('#',1)[0].split('?',1)[0]
            if not path or re.match(r'^[a-z][a-z\d+.-]*:',path,re.I):
                continue
            dest=(f.parent/path).resolve()
            if not dest.is_relative_to(ROOT.resolve()) or not dest.exists():
                errors.append(f'{f.relative_to(ROOT)}: broken/unsafe link: {raw}')

if errors:
    print('PUBLIC CONTENT CHECK FAILED:')
    for e in errors: print(' - '+e)
    sys.exit(1)
print(f'PUBLIC CONTENT CHECK PASSED: {filecount} files inspected; no flagged types, obvious credentials, or broken relative Markdown links.')
print('Manual IP/privacy/history review is still required before publication.')

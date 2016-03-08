#!/usr/bin/env python

from setuptools import setup
from pip.req import parse_requirements

install_requires = parse_requirements("requirements.txt")
install_requires = [str(ir.req) for ir in install_requires]

sctk = {
    "name": "togo",
    "description": "Togo space planning optimizer",
    "author":"Giles Hall",
    "packages": ["togo"],
    "version": "0.1",
    "package_dir": {"togo": "src"},
    "scripts": ["scripts/togo_runner.py", "scripts/togo_service.py", "scripts/run_venv.sh"],
    "install_requires": install_requires,
}

if __name__ == "__main__":
    setup(**sctk)

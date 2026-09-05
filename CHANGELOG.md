# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.2] - 2026-09-05

### Changed

- The repository is public, so install instructions use the HTTPS URL again.
  The SSH remote is documented as the fallback for a private repository.

## [0.1.1] - 2026-09-05

### Changed

- Install instructions now use the SSH remote. The repository is private, and
  Unity runs `git` with terminal prompts disabled, so an HTTPS URL fails with
  `could not read Username for 'https://github.com'`.

## [0.1.0] - 2026-09-05

### Added

- Initial package layout: `Runtime`, `Editor`, `Tests`, `Samples~`, `Documentation~`.
- `Tools → Liang Tools → About` window reporting the resolved package version.

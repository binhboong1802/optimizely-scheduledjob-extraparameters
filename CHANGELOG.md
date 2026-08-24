# Changelog

All notable changes to **OptiScheduledJob.ExtraParameters** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

The package ships as two release lines: **1.x** targets Optimizely CMS 12, **2.x** targets
Optimizely CMS 13. See [README.md](README.md) for installation instructions for each.

## [1.0.1] - 2026-08-24 (Optimizely CMS 12)

### Fixed

- Jobs decorated with `[ScheduledPlugInWithExtraParameters]` were never registered in
  `tblScheduledItem` on a cold start (no existing row for the job's GUID) and did not appear under
  Admin → Scheduled Jobs at all ([#1](https://github.com/binhboong1802/optimizely-scheduledjob-extraparameters/issues/1)).
  CMS 12's plug-in descriptor repository keys descriptors by the exact attribute type, so the
  scheduler's lookup for `ScheduledPlugInAttribute` never surfaced the subclassed attribute.
  `AddScheduledJobExtraParameters()` now replaces the scheduler's job locator
  (`IScheduledJobLocator`) with one that also queries the subclass attribute's key; EPiServer's own
  scan/sync logic then registers and updates these jobs normally. This also fixes attribute metadata
  changes (`DisplayName`, interval, …) never being synced to the database on restart for such jobs.
  Note: `AddScheduledJobExtraParameters()` must be called after `AddCms()` (as the README already
  shows) for the replacement to take effect. The CMS 13 / 2.x line is unaffected — CMS 13's job
  discovery is inheritance-aware.

## [2.0.0] - 2026-08-14 (Optimizely CMS 13)

Port of the CMS 12 line to CMS 13. The public API is unchanged — namespaces, type names, the
`AddScheduledJobExtraParameters()` extension and the Dynamic Data Store layout all carry over, so
existing saved parameter values are read back as-is after an upgrade.

### Added

- New project `src/OptiScheduledJob.ExtraParameters.Cms13`, maintained alongside the CMS 12 project
  and added to the solution.

### Changed

- Targets `net10.0` only (CMS 13 requires .NET 10); dropped `net6.0`, `net8.0` and `net9.0`.
- `EPiServer.CMS` dependency moved to `[13.0.0,14.0)`.
- `[ScheduledPlugInWithExtraParameters]` now derives from `EPiServer.Scheduler.ScheduledJobAttribute`
  instead of the obsolete `EPiServer.PlugIn.ScheduledPlugInAttribute`. `DisplayName`, `Description`
  and `GUID` are unchanged, so consumer usage of the attribute needs no edits.
- Replaced `Newtonsoft.Json` with `System.Text.Json` in
  `ScheduledJobExtraParametersDataService` — CMS 13 dropped Newtonsoft framework-wide. Reads are
  case-insensitive and writes keep CLR property names, so blobs written by 1.x round-trip unchanged.
- The injected admin script now derives the shell route base from the current page instead of the
  hardcoded `/episerver/` prefix, following CMS 13's move of module URL segments to `/Optimizely/`.
- Packaging keys off a new `$(ModuleName)` property so the module zip keeps the plain
  `OptiScheduledJob.ExtraParameters` name despite the `.Cms13` project file name; the
  `CopyZipFiles.targets` shim is packed for `net10.0` only.

### Verified against

`EPiServer.CMS` 13.1.0 on a CMS 13 Alloy site (.NET 10) — job registration through the subclassed
`ScheduledJobAttribute`, the protected-module zip under `modules/_protected/`, the injected admin UI
form and the save round-trip through the Dynamic Data Store.
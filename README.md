# nc-sba-opo

3D multiplayer co-op brick breaker built with Unity.

## Project Goal

- Build a playable 3D brick breaker prototype first.
- Expand it into a 2-player co-op experience.
- Stabilize networking before adding larger content scope.

## Current Structure

- Unity project root: `nc-sba-opo/`
- Main asset structure guide: [`Assets/PROJECT_STRUCTURE.md`](Assets/PROJECT_STRUCTURE.md)
- Planning document: [`docs/PLAN.md`](docs/PLAN.md)

## Development Flow

1. Build the single-player core loop first.
2. Add lobby and multiplayer session flow.
3. Synchronize gameplay state with host-authoritative networking.
4. Add co-op mechanics, content, and polish.

## Milestones

- `M1`: Prototype
- `M2`: Single-player Core
- `M3`: Multiplayer Connection
- `M4`: Gameplay Sync
- `M5`: Co-op Mechanics
- `M6`: Polish

Detailed milestone notes live in [`docs/PLAN.md`](docs/PLAN.md).

## GitHub Workflow

- Use `Issues` for implementation tasks.
- Use `Projects` with `Todo`, `Doing`, and `Done`.
- Keep only one issue in `Doing` at a time.
- Attach every issue to a milestone when possible.

## Branch Strategy

- `main`: stable branch
- `feat/*`: feature work
- `fix/*`: bug fixes
- `chore/*`: maintenance

## Commit Style

- `feat: add paddle movement controller`
- `fix: correct ball reflection on side walls`
- `chore: update Unity gitignore rules`
- `docs: add milestone planning document`

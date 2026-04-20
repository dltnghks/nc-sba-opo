# Development Plan

## Overview

This project is a solo-developed 3D multiplayer co-op brick breaker game in Unity.

The safest production order is:

1. Prototype the core brick breaker loop locally.
2. Finish a solid single-player version.
3. Add multiplayer room and session flow.
4. Synchronize gameplay state.
5. Add co-op mechanics and content.
6. Polish, balance, and stabilize.

## Milestones

### M1 Prototype

- Paddle movement
- Ball launch and movement
- Ball reflection rules
- Brick destruction
- Clear and fail conditions

### M2 Single-player Core

- Stage data structure
- Score and life systems
- Base HUD
- Basic audio feedback
- Restart and scene flow

### M3 Multiplayer Connection

- Networking package integration
- Room create/join flow
- Lobby scene
- Player spawn
- Ready state and game start sync

### M4 Gameplay Sync

- Host-authoritative game state manager
- Synchronize paddle and player input state
- Synchronize ball state
- Synchronize brick state and round results
- Add multiplayer desync test checklist

### M5 Co-op Mechanics

- Shared objective rules
- At least one co-op-specific brick gimmick
- Team-based power-up design
- Co-op round result flow

### M6 Polish

- Feedback and VFX
- Sound mix improvement
- Difficulty balancing
- Bug fixing
- Build validation

## Project Board Rules

- Columns: `Todo`, `Doing`, `Done`
- Keep only one task in `Doing`
- Split work into tasks that can finish in half a day to two days
- Link each task to a milestone when possible

## First Recommended Issues

- Set up bootstrap and gameplay scenes
- Implement paddle movement
- Implement ball reflection logic
- Create brick health and destruction flow
- Build game state manager
- Add result UI
- Choose and integrate networking stack

# RangeTrainer — Project Instructions

This file holds the **standing rules** for working on RangeTrainer. It complements
(does not duplicate) the global rules in `~/CLAUDE.md` (Plan-before-editing,
Simplify-before-commit, Tests-before-commit, Daily-documentation, Spec-refinement).

The full method is documented in **`doc/sdlc.md`** — read it before working here.

## Source of truth

`doc/` is authoritative. It answers **why / what** (stable):
- `doc/user-stories.md` — functional requirements (FR1–47), epics, stories.
- `doc/architecture.md`, `doc/tech-stack.md` — the system's structure.
- `doc/sdlc.md` — how we build (this method, the DoD, the guardrails).

Issues answer **which unit of work, now**. `spec/spec-*.md` answers **how, in
detail** (created only when a feature needs refinement). **Reference, never copy**
between them.

## Executing a story

1. Read the **issue** → acceptance criteria, scope, and the `doc/` reference.
2. Read the referenced `doc/` element → the *why*.
3. Read the **Definition of Done** in `doc/sdlc.md`.
4. Stay strictly **within the issue's scope** (respect the "out of scope" section).
5. Before closing, verify every DoD item — including **promoting any durable
   decision back into `doc/`** (otherwise `doc/` rots).

## Invariants

- **Every story references `doc/`** — a feature references `user-stories.md`; an
  enabler references `architecture.md` / `tech-stack.md` / `sdlc.md`.
- **Reference, don't copy** — no artifact recopies another's content.
- **Respect scope and WIP** — the maintainer's review is the bottleneck and the
  guardrail for what CI cannot check; do not sprawl beyond what was pulled.

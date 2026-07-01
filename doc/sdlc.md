# SDLC — How We Build RangeTrainer

This document describes the **method** used to build RangeTrainer: how work is
described, pulled, executed, verified, and shipped. It is a durable reference and
a deliverable in its own right — RangeTrainer is as much a **method showcase** (an
AI-assisted, LEAN-oriented SDLC on GitHub, deployed to Scaleway) as it is a poker
range-training app.

It is intentionally lightweight and **evolves by kaizen**: we start with the
minimum that works and improve it each iteration, rather than designing the whole
process up front.

---

## 1. Guiding principle — altitude = rate of change

Every piece of information lives at the altitude that matches **how fast it
changes**. Mismatching the two is the root cause of documentation rot.

| Rate of change | Home |
|---|---|
| Slow (vision, epics, requirements, architecture, this method) | `doc/` |
| Per unit of work (what to build, now) | a **GitHub Issue** |
| During a single PR (the detailed how) | `spec/spec-*.md` (on demand) |

> Concrete proof: the hosting choice (a *volatile* decision) was once written into
> the *durable* architecture doc and immediately went stale. Volatile detail does
> **not** belong in `doc/`, even when it "concerns architecture" — it belongs close
> to the code (an issue, a spec, or a code comment).

---

## 2. Sources of truth

Three artifacts describe work. Each **references** the next; none **copies** it.
That single rule is what keeps them from drifting.

| | `doc/` | **GitHub Issue** | `spec/spec-*.md` |
|---|---|---|---|
| Answers | Why / What (stable) | Which unit of work, now | How, in detail |
| Altitude | Vision, epics, requirements, architecture, method | One pullable story | One feature in progress |
| Lifetime | Whole project | From `Todo` to `Done` | The duration of a PR |
| Contains | Intent, constraints, FR1–47, architecture, this SDLC | Link to the referenced `doc/`, acceptance criteria, scope | Refinement + living plan `[ ]/[~]/[x]` |
| Does **not** contain | Any volatile detail | A copy of the requirement | A re-justification of the "why" |
| Written by | The maintainer (rarely) | Maintainer/AI when pulling the work | AI during execution |

- `doc/` answers **why / what** (stable).
- The **issue** answers **which unit of work, now** (transient).
- `spec/` answers **how, in detail** (disposable, created only when a feature
  genuinely needs refinement — not one per issue).

---

## 3. Work items — two kinds of story

Not every story maps to a user-facing requirement. There are two kinds, and both
still reference `doc/` — just different faces of it.

| | **Feature story** | **Enabler story** |
|---|---|---|
| Delivers | user value | a technical foundation |
| Example | Register, train a range | scaffold, CI gate, Scaleway deploy, guardrails |
| References in `doc/` | `user-stories.md` (the FR, the user-why) | `architecture.md` / `tech-stack.md` / `sdlc.md` (the system-why) |

So `doc/` has a **functional face** (`user-stories.md`) and a **structural/method
face** (`architecture.md`, `tech-stack.md`, `sdlc.md`). The invariant holds for
both: **every story references `doc/`.**

### Epic 0 — Walking Skeleton & Pipeline

The technical skeleton (scaffold, CI gate, deploy, guardrails) is a set of
**enabler** stories grouped under **Epic 0**, roughly mirroring the rungs of the
[pipeline kata](#8-toolchain--ci-gates). `architecture.md §2` describes the target
scaffold; `sdlc.md` (this document) describes the pipeline and method. Epics 1–7
are the app; Epic 0 is the method made concrete.

---

## 4. Lifecycle of a story

```
doc/ (FR or structural element)   ── stable reference, unchanged
        │  pull
        ▼
GitHub Issue  (link to doc/ + acceptance criteria + scope)   ── the pull token on the board
        │  execute
        ▼
spec/spec-*.md  (refinement + living plan)   ── only if the feature needs it
        │  implement
        ▼
Pull Request  (Closes #issue)   ── through the CI gate + review
        │  merge
        ▼
Board → Done   +   ✦ durable decisions promoted back into doc/
```

The last step is the one everyone forgets: **if a durable decision emerged during
the PR, it is promoted back into `doc/`** — otherwise `doc/` rots, exactly as the
hosting choice did. This promotion is part of the Definition of Done (§5) and is
enforced as a guardrail (§6).

---

## 5. Definition of Done (DoD)

**Acceptance criteria (AC)** are story-specific and live *in the issue*. The
**DoD** is the universal bar every story clears; it lives here, once, and issues
**reference** it rather than copying it.

A story is **Done** when:

1. Acceptance criteria are met.
2. Tests are written and green (the `ci` gate).
3. Patch coverage is satisfied (once past the skeleton — see §8).
4. All required checks pass (security suite, etc. — see §8).
5. `spec/` is updated if one existed.
6. ✦ Durable decisions have been promoted back into `doc/`.

### Machine-gated vs human-gated

The kata's lesson is *"the guarantee is the gate, never trust the agent."* But not
all of the DoD can be a machine gate — and being honest about that is the point:

- **Machine-gated** (CI verifies): 2, 3, 4.
- **Human-gated** (maintainer review verifies): 1, 5, 6. No CI can reliably tell
  whether the acceptance criteria are *genuinely* met or whether the `doc/` update
  is *meaningful* (touching a file to fool a check is trivially gameable).

The maintainer's review **is** the guardrail for the non-mechanizable part of the
DoD. This is exactly why review is the bottleneck, and why WIP is limited to what
the maintainer can actually shepherd through (§7).

---

## 6. Guardrails — four layers

Each rule has a place where it is enforced.

| Layer | Where | Enforces |
|---|---|---|
| **Entry** | issue template (`.github/ISSUE_TEMPLATE/story.yml`) | issue is well-formed: `doc/` reference, acceptance criteria, scope |
| **Standing** | project `CLAUDE.md` | permanent rules the AI reads every session: reference `doc/`, stay in scope, promote durable decisions |
| **Mechanical exit** | `ci` gate + required checks | tests, patch coverage, security |
| **Human exit** | maintainer PR review | acceptance criteria genuinely met, `doc/` promotion meaningful |

An issue that is not well-formed (no `doc/` reference, no acceptance criteria)
should **not be pulled**. The AI agent is governed by the same four layers as any
human contributor — whatever opens a PR passes the same gates.

---

## 7. Board & WIP

The board (**GitHub Projects v2**) is the central piece of the SDLC: it is the
method made visible. Work you cannot see is work you cannot improve.

- **Columns (minimal to start):** `Todo → In progress → In review → Done`.
- **Hierarchy:** Epic → Story via **sub-issues**; a story splits into Task
  sub-issues only when it is too big for a single PR. We do not pre-decompose —
  planning-ahead is itself WIP.
- **WIP limit:** a soft column limit on `In progress`. In a solo (or small) setup
  there are no hand-offs between people, but there **is** a real bottleneck: the
  **maintainer's review**. An AI agent can open many PRs in parallel; the human who
  reviews and merges is near single-threaded. Limiting WIP aligns what is *pulled*
  with what can actually be *shepherded through the gate* (Theory of Constraints:
  subordinate everything to the bottleneck).

Team-only ceremony (velocity points, per-person swimlanes) is deliberately omitted:
it coordinates people we do not have and adds waste.

---

## 8. Toolchain & CI gates

The pipeline mechanics are proven in a separate learning repo,
**ACL20-pipeline-kata** (SC³ method — smallest end-to-end slice, one new mechanism
per "rung", break the gate on purpose to understand it, one ADR line per rung).
RangeTrainer **replays the kata on the real app, with Claude Code as the agent.**

**Kept from the kata (wholesale):**
- GitHub Actions CI with **path filters** and an **aggregate `ci` gate** (one
  required check that aggregates the rest).
- **nbgv** versioning by git-height → image tag.
- **Ruleset-as-code** on `main`: the `ci` check is required and AI-agnostic.
- **GitHub Deployment** objects for traceable deploys.
- **Contract tests** (PactNet).

**Added on top (LEAN best practices):**
- **Dependabot** — small, continuous dependency bumps instead of one painful batch.
- **Concurrency** (cancel-in-progress) — stop wasting runners on superseded commits.
- **Merge queue** — safe serialized integration into `main`.
- **Security suite** (all free on public repos): **CodeQL**, **Copilot Autofix**,
  **secret scanning (+ push protection)**, **dependency review**.
- **Patch coverage** as the coverage gate — new code in a PR must be covered;
  **total coverage stays informational** (a thermometer, never a target, to avoid
  gaming). Patch coverage starts **informational** and flips to **blocking once the
  skeleton is merged**, so boilerplate is never counted against us.

**Deliberately dropped:**
- NuGet cache, PR auto-merge, DORA metrics (DevLake/Grafana) — not worth the
  complexity for this project right now.

---

## 9. Deferred decisions

Decided at the **last responsible moment**, not up front. Listed here so the
deferral is explicit, not forgotten:

- **Conventional commits** enforced on the **PR title** (squash-merge makes the PR
  title the commit on `main`). Add when wiring CI.
- **Community health files** (`SECURITY.md`, `LICENSE`) — additive, no coupling.
  `SECURITY.md` matters most (public repo + Scaleway keys); without a `LICENSE` the
  public repo defaults to all-rights-reserved.
- **Board elaboration** — custom fields and automations, added by kaizen as needed.
  The minimal board is the default and needs no decision.
- **Scaleway deployment shape** — Kapsule (managed K8s) vs Serverless Containers for
  a single-container Aspire app. Resolved at deployment time. Supersedes the stale
  Azure/Railway wording still in `tech-stack.md` / `architecture.md`.

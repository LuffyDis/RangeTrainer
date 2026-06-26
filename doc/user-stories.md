# RangeTrainer — User Stories

> Adapted from the PokerTrainer BMAD planning artifacts.
> **Key change vs. source:** authentication is **standard** (login + registration). The
> anonymous-account / "trial without signup" model has been removed. Every feature
> requires an authenticated user, and all data is user-scoped.

## Overview

RangeTrainer is a poker range training PWA that turns passive range knowledge into
active table recall. Players define preflop situations using a horizontal action bar,
build weighted ranges on a 13×13 grid with multi-range color overlay, and drill them
through FSRS-powered spaced repetition that targets boundary hands.

This document is the complete epic and story breakdown.

---

## Requirements Inventory

### Functional Requirements

> FR numbers are kept identical to the source for traceability with `architecture.md`.
> `FR42` (trial experience without signup) is **removed** in RangeTrainer.

**Situation Management**
- FR1: User can create a new situation by selecting position, action sequence, hero action, and raise sizing via a horizontal action bar
- FR2: User can set a default game profile (format, table size, stakes, stack depth, ante) that pre-fills new situations
- FR3: System can auto-detect situation type (RFI, 3-bet, defend, call vs raise, 4-bet) from the action sequence
- FR4: User can view a list of all their situations with summary information
- FR5: User can filter situations by position, situation type, and user-defined tags
- FR6: User can edit an existing situation's metadata
- FR7: User can delete a situation and all its associated ranges
- FR8: User can add, remove, and rename tags on a situation

**Range Building**
- FR9: User can create a named, color-coded range within a situation
- FR10: User can paint combos on a 13×13 grid by clicking individual cells or dragging across multiple cells
- FR11: User can assign a weight (0-100% frequency) to any combo in a range
- FR12: User can view multiple ranges overlaid on the same grid with distinct colors
- FR13: User can switch the active range to focus on it (others faded)
- FR14: User can see conflict dots on combos that differ across ranges in the same situation
- FR15: User can hover/tap a conflict dot to see per-range breakdown (name, color, weight)
- FR16: User can clone an existing range to create a new independent copy
- FR17: User can delete a range from a situation
- FR18: User can rename a range or change its color

**Range Import/Export**
- FR19: User can import a range from standard text notation (e.g., JJ+, ATs+, A5s-A2s, KQo:0.75)
- FR20: System can parse standard range text notation and map it to grid combos with weights
- FR21: System can display clear error messages for unrecognized import notation with suggested corrections
- FR22: User can export a range as standard text notation
- FR23: User can export a range as an image (grid screenshot)
- FR24: User can copy a range to clipboard
- FR25: Exported ranges include situation metadata (position, action sequence, situation type)

**Study & Drilling**
- FR26: User can start a drill session on one or more selected ranges
- FR27: User can start a drill session from the home screen with system-suggested ranges based on mastery data
- FR28: System can select drill questions with 60% targeting boundary hands and 40% targeting clear includes/excludes
- FR29: User can answer "Pick the Range" drill questions (Is [hand] in your [Range Name]? Yes/No + weight)
- FR30: User can play "Spot the Difference" drill (identify deliberate errors in a displayed range grid)
- FR31: User can play "Puzzle Rush" drill (streak mode — one wrong answer ends the run, personal bests tracked)
- FR32: User can play "Reverse Drill" (view a range grid, identify the situation and range name)
- FR33: System can display immediate feedback after each answer showing the correct answer with full range grid context and the queried hand highlighted
- FR34: User can stop a drill session at any time and see a session summary
- FR35: System can display session summary with accuracy percentage, missed combos, and mastery score changes

**Mastery & Spaced Repetition**
- FR36: System can maintain a per-combo mastery score for each range in each situation
- FR37: System can schedule combo reviews using the FSRS spaced repetition algorithm
- FR38: System can prioritize combos with below-average mastery for more frequent drilling
- FR39: User can view mastery scores for their ranges and situations
- FR40: System can update mastery scores in real time after each drill answer

**User Identity & Profiles**
- FR41: User can register an account and authenticate (standard email + password login)
- ~~FR42: User can use the app without signing up for an initial trial experience~~ — **REMOVED** (no anonymous/trial mode in RangeTrainer)
- FR43: User can create and switch between multiple game profiles (e.g., 6-max cash, MTT)

**Offline & PWA**
- FR44: User can install the app as a PWA on their device's home screen
- FR45: User can access drill functionality offline using cached data
- FR46: System can sync data automatically when internet connection is restored
- FR47: User can access previously loaded situations and ranges while offline

### Non-Functional Requirements

**Performance**
- NFR1: Grid paint interaction response time < 16ms (60fps)
- NFR2: Drill question presentation < 200ms from answer to next question
- NFR3: Situation creation API response < 500ms
- NFR4: PWA cached page load < 1 second
- NFR5: First visit page load (4G) < 3 seconds
- NFR6: WASM bundle size < 5MB compressed

**Security**
- NFR7: JWT-based authentication, HTTPS only
- NFR8: Password storage hashed with ASP.NET Identity (bcrypt/PBKDF2)
- NFR9: Users can only access their own situations, ranges, and mastery data
- NFR10: **All endpoints require authentication** (no trial exception) — only the registration and login endpoints are anonymous

**Reliability**
- NFR11: Zero data loss for combo weights and mastery scores
- NFR12: App remains fully functional for drilling when offline
- NFR13: All offline changes sync without data loss when reconnected

**Accessibility**
- NFR14: Grid and drill controls fully keyboard-navigable
- NFR15: Range colors accompanied by text labels; avoid pure red/green pairs
- NFR16: Minimum 44×44px touch targets on mobile for grid cells and drill buttons

### FR Coverage Map

| FR | Epic | Description |
|----|------|-------------|
| FR41 | Epic 1 | Register + authenticate (standard login) |
| FR43 | Epic 1 | Multiple game profiles |
| FR1–FR8 | Epic 2 | Situation engine |
| FR9–FR18 | Epic 3 | Range builder |
| FR19–FR25 | Epic 4 | Range library (import/export) |
| FR26–FR29, FR33–FR35 | Epic 5 | Core drilling |
| FR30–FR32, FR36–FR40 | Epic 6 | Advanced drills & mastery |
| FR44–FR47 | Epic 7 | Offline & PWA |

> Project scaffold & infrastructure (the old Story 1.1) is **not** a user story — it lives
> in `architecture.md` under "Implementation Sequence" / the scaffold commands.

---

## Epic List

### Epic 1: Account & Game Profiles
Users can register, log in with email and password, and manage game profiles. Every
feature behind the app requires an authenticated user.
**FRs covered:** FR41, FR43

### Epic 2: Situation Engine
Users can create preflop situations in seconds using the horizontal action bar, manage
game profiles for fast defaults, and browse their situation library.
**FRs covered:** FR1–FR8

### Epic 3: Range Builder
Users can create, paint, and manage multiple color-coded ranges per situation with
weighted combos on the 13×13 grid. Multi-range overlay with conflict detection.
**FRs covered:** FR9–FR18

### Epic 4: Range Library (Import/Export/Filter)
Users can import solver ranges, export their ranges, filter and organize their library
with tags. Full range portability.
**FRs covered:** FR19–FR25

### Epic 5: Study System — Core Drilling
Users can drill their ranges with Pick the Range drill, smart boundary-hand selection,
immediate feedback with full grid context, and session summaries.
**FRs covered:** FR26, FR27, FR28, FR29, FR33, FR34, FR35

### Epic 6: Advanced Drill Modes & Mastery
Users can play Spot the Difference, Puzzle Rush, and Reverse Drill. FSRS spaced
repetition tracks per-combo mastery and adapts question selection.
**FRs covered:** FR30, FR31, FR32, FR36, FR37, FR38, FR39, FR40

### Epic 7: Offline & PWA
Users can install the app as a PWA, drill offline on mobile, and all data syncs
automatically when back online.
**FRs covered:** FR44, FR45, FR46, FR47

---

## Epic 1: Account & Game Profiles

Users can register, log in, and manage game profiles. Authentication is standard
(email + password); there is no anonymous or trial path.

### Story 1.1: User Registration

As a **new visitor**,
I want to register an account with my email and password,
So that I can access the app and have my data saved securely to my account.

**Acceptance Criteria:**

**Given** a visitor on the registration page
**When** they submit a valid email and password via POST /api/auth/register
**Then** a User is created in the [identity] schema
**And** the password is hashed with ASP.NET Identity defaults (bcrypt/PBKDF2)
**And** a JWT token with UserId and email claims is returned and stored in localStorage
**And** all subsequent API requests include the JWT in the Authorization header

**Given** an email that is already registered
**When** the visitor tries to register with it
**Then** a 409 Conflict error is returned with "Email already in use"

**Given** an invalid email format or a password that does not meet policy
**When** the visitor submits registration
**Then** a 422 error is returned listing the validation failures

### Story 1.2: User Login

As a **registered user**,
I want to log in with my email and password,
So that I can access my account on any device.

**Acceptance Criteria:**

**Given** a registered user with valid credentials
**When** they submit email and password via POST /api/auth/login
**Then** a JWT token is returned with UserId and email claims
**And** the token is stored in Blazor localStorage
**And** the user is redirected to the home screen

**Given** invalid credentials
**When** they submit login
**Then** a 401 Unauthorized error is returned with "Invalid email or password"
**And** the error message does not reveal whether the email exists

**Given** an unauthenticated user
**When** they try to reach any page other than login/register
**Then** they are redirected to the login page

### Story 1.3: Game Profile Management

As a **user**,
I want to create and switch between game profiles (6-max cash, MTT, etc.),
So that new situations are pre-filled with my recurring settings.

**Acceptance Criteria:**

**Given** an authenticated user
**When** they create a game profile with format, table size, stakes, stack depth, and ante
**Then** the profile is saved and set as active

**Given** a user with multiple game profiles
**When** they switch the active profile
**Then** new situations use the active profile's defaults

**Given** a user creates a new situation
**When** an active game profile exists
**Then** format, table size, stakes, stack depth, and ante are pre-filled from the profile
**And** only position, action sequence, hero action, and raise sizing need manual input

---

## Epic 2: Situation Engine

Users can create preflop situations in seconds using the horizontal action bar, manage
defaults, and browse their situation library.

### Story 2.1: Situation Domain Model & Create Situation

As a **user**,
I want to create a preflop situation by defining position, action sequence, hero action, and raise sizing,
So that I can anchor my ranges to a specific, reproducible poker scenario.

**Acceptance Criteria:**

**Given** an authenticated user
**When** they submit a CreateSituation command with position, action sequence, hero action, and raise sizing
**Then** a Situation aggregate is created with auto-derived SituationType (RFI, 3-bet, defend, etc.)
**And** the active game profile pre-fills format, table size, stakes, stack depth, and ante
**And** the Situation passes CheckInvariants() validation
**And** a SituationCreatedEvent is published via Wolverine outbox
**And** the situation is persisted in the [range] schema, scoped to the user
**And** the API returns 201 Created with the SituationId

**Given** an invalid action sequence (e.g., fold then raise)
**When** the user submits the command
**Then** a 422 error is returned with all validation errors

### Story 2.2: Horizontal Action Bar (Blazor Component)

As a **user**,
I want to define a situation using a horizontal action bar showing positions left to right,
So that I can set up any preflop scenario in 3 taps instead of filling forms.

**Acceptance Criteria:**

**Given** the user is on the Create Situation page
**When** the ActionBar component renders
**Then** positions are displayed left to right based on active game profile's table size
**And** the user taps a position to set it as hero position
**And** available actions appear for each position in the sequence
**And** raise sizing input appears when a raise action is selected

**Given** the user completes the action bar
**When** they confirm the situation
**Then** the CreateSituation command is sent to the API
**And** the situation type is auto-detected and displayed

**Given** the user has an active game profile
**When** the Create Situation page loads
**Then** format, table size, stakes, stack depth, and ante are pre-filled

### Story 2.3: Situation List & Browse

As a **user**,
I want to see all my situations in a browsable list with summary information,
So that I can quickly find and access any situation I've created.

**Acceptance Criteria:**

**Given** an authenticated user with existing situations
**When** they navigate to the situation list page
**Then** situations are displayed with: situation type, hero position, action sequence display, range count, tags, and creation date
**And** situations are paginated (20 per page)
**And** situations are sorted by most recently created

**Given** a user with no situations
**When** they view the list page
**Then** an empty state is shown with a prominent "Create Your First Situation" button

### Story 2.4: Filter Situations

As a **user**,
I want to filter my situation library by position, situation type, and tags,
So that I can quickly find specific spots to study or edit.

**Acceptance Criteria:**

**Given** a user with multiple situations
**When** they apply a position filter (e.g., "CO")
**Then** only situations where hero position is CO are shown

**Given** multiple filters applied (e.g., position=CO, type=3-bet)
**When** the filters are combined
**Then** only situations matching ALL filters are shown (AND logic)

**Given** a user applies a tag filter (e.g., "#weak-spot")
**When** the filter is active
**Then** only situations with that tag are shown

**Given** filters are applied
**When** the user clears all filters
**Then** the full situation list is restored

### Story 2.5: Edit & Delete Situations

As a **user**,
I want to edit a situation's metadata or delete a situation entirely,
So that I can keep my library clean and accurate.

**Acceptance Criteria:**

**Given** an existing situation
**When** the user edits its metadata (stakes, stack depth, ante, raise sizing)
**Then** the changes are saved and the situation type is re-derived if action sequence changed
**And** all associated ranges remain intact

**Given** an existing situation with ranges
**When** the user deletes the situation
**Then** the situation and ALL its ranges and combos are deleted
**And** a confirmation dialog is shown before deletion
**And** related mastery data in the Study module is cleaned up via RangeDeletedEvent

### Story 2.6: Tag Management

As a **user**,
I want to add, remove, and rename tags on my situations,
So that I can organize my library with personal labels like #weak-spot or #tournament-only.

**Acceptance Criteria:**

**Given** an existing situation
**When** the user adds a tag (free-form text)
**Then** the tag is saved and appears on the situation card in the list

**Given** a situation with existing tags
**When** the user removes a tag
**Then** the tag is removed from that situation only

**Given** a user types a tag name
**When** existing tags match the input
**Then** auto-complete suggestions appear from previously used tags

---

## Epic 3: Range Builder

Users can create, paint, and manage multiple color-coded ranges per situation with
weighted combos on the 13×13 grid. Multi-range overlay with conflict detection.

### Story 3.1: Range Domain Model & Create Range

As a **user**,
I want to create a named, color-coded range within a situation,
So that I can start building my range for a specific poker scenario.

**Acceptance Criteria:**

**Given** an existing situation
**When** the user creates a new range with a name and color
**Then** a Range entity is created with 169 combos initialized at weight 0
**And** the range belongs to the situation (SituationId foreign key)
**And** the Situation aggregate validates via CheckInvariants() (max 10 ranges, unique names, unique colors)
**And** a RangeCreatedEvent is published via Wolverine outbox
**And** the API returns 201 Created with the RangeId

**Given** a situation already has 10 ranges
**When** the user tries to create another range
**Then** a 422 error is returned with "Situation.MaxRangesReached"

**Given** a duplicate range name within the same situation
**When** the user tries to create it
**Then** a 409 error is returned with "Situation.DuplicateRangeName"

### Story 3.2: SVG Range Grid Component

As a **user**,
I want to see a 13×13 grid representing all 169 hand categories,
So that I can visualize and interact with my poker ranges.

**Acceptance Criteria:**

**Given** a range is selected for editing
**When** the RangeGrid component renders
**Then** a 13×13 SVG grid is displayed with pocket pairs on the diagonal, suited hands above, offsuit hands below
**And** each cell shows the hand notation (e.g., AKs, JJ, T9o)
**And** cells are color-coded based on the active range's color and combo weight (opacity = weight)
**And** empty combos (weight 0) show no color fill
**And** the grid is responsive: full size on desktop, touch-optimized on mobile (minimum 44×44px per cell)
**And** the grid renders within 16ms frame budget (NFR1)

### Story 3.3: Paint Combos (Click & Drag)

As a **user**,
I want to paint combos on the grid by clicking or dragging,
So that I can quickly build a range by selecting hands visually.

**Acceptance Criteria:**

**Given** a range is active for editing
**When** the user clicks a single cell
**Then** the combo is set to weight 1.0 (100%) and the cell fills with the range color

**Given** a combo is already at 100%
**When** the user clicks it again
**Then** a weight input appears (ComboWeightInput component) allowing 0-100% entry

**Given** the user presses and drags across multiple cells
**When** they release
**Then** all dragged cells are set to weight 1.0 (batch selection)
**And** the grid updates in real time during the drag (< 16ms per frame)

**Given** the user is on a mobile device
**When** they long-press a cell
**Then** the weight input appears (replaces right-click on desktop)

### Story 3.4: Combo Weight Management

As a **user**,
I want to assign specific weights (0-100%) to individual combos,
So that I can represent mixed strategies and solver frequencies.

**Acceptance Criteria:**

**Given** a combo in the active range
**When** the user sets a weight via the ComboWeightInput
**Then** the weight is saved as a value between 0.0 and 1.0
**And** the cell opacity reflects the weight (0.5 = 50% opacity)
**And** the UpdateComboWeights command is sent to the API

**Given** a weight outside 0-1 range
**When** the user submits it
**Then** the ComboWeight value object returns Result.Failure with "ComboWeight.OutOfRange"

**Given** the user wants to set multiple combos to the same weight
**When** they drag-select cells and enter a weight
**Then** all selected cells receive the same weight in a single bulk operation

### Story 3.5: Multi-Range Overlay

As a **user**,
I want to see multiple ranges overlaid on the same grid with distinct colors,
So that I can compare my ranges for the same situation at a glance.

**Acceptance Criteria:**

**Given** a situation with multiple ranges
**When** the user views the range editor
**Then** all ranges are displayed on the grid simultaneously with their assigned colors
**And** cells with combos in multiple ranges show blended/layered colors

**Given** multiple ranges are visible
**When** the user clicks a range name in the range list
**Then** that range becomes the "active" range
**And** the active range is shown in full color, others are faded (reduced opacity)
**And** only the active range is editable

### Story 3.6: Conflict Detection

As a **user**,
I want to see conflict indicators on combos that differ across ranges,
So that I can identify where my ranges disagree and investigate discrepancies.

**Acceptance Criteria:**

**Given** a situation with multiple ranges
**When** a combo has different weights across ranges
**Then** a conflict dot is displayed on that cell

**Given** a conflict dot is visible
**When** the user hovers (desktop) or taps (mobile) the conflict dot
**Then** a tooltip shows the per-range breakdown: range name, color, and weight for each range

**Given** all ranges agree on a combo's weight
**When** the grid renders
**Then** no conflict dot is shown for that cell

### Story 3.7: Clone, Delete & Rename Range

As a **user**,
I want to clone an existing range, delete ranges I no longer need, and rename or recolor ranges,
So that I can efficiently manage my range collection within a situation.

**Acceptance Criteria:**

**Given** an existing range
**When** the user clones it with a new name and color
**Then** a new independent Range is created with all combo weights copied from the source
**And** the Situation aggregate validates (max 10, unique name, unique color)
**And** the clone has no dependency on the source

**Given** an existing range
**When** the user deletes it
**Then** the range and all its combos are removed
**And** a RangeDeletedEvent is published

**Given** an existing range
**When** the user renames it or changes its color
**Then** the changes are saved
**And** the Situation aggregate validates uniqueness

---

## Epic 4: Range Library (Import/Export/Filter)

Users can import solver ranges, export their ranges, filter and organize their library
with tags. Full range portability.

### Story 4.1: Import Range from Text Notation

As a **user**,
I want to import a range by pasting standard text notation,
So that I can bring my solver outputs and coach ranges into the app in seconds.

**Acceptance Criteria:**

**Given** an existing situation with a range
**When** the user opens the import dialog and pastes text notation (e.g., `JJ+, ATs+, A5s-A2s, KQs, 87s+, AQo:0.75`)
**Then** the RangeTextParser parses the notation and maps combos to the 13×13 grid with weights
**And** the grid previews the imported combos before confirming
**And** on confirm, the combo weights are saved to the range

**Given** notation with weight syntax (e.g., `AQo:0.75`)
**When** the parser processes it
**Then** the combo is set to weight 0.75

**Given** notation without explicit weight (e.g., `JJ+`)
**When** the parser processes it
**Then** the combo is set to weight 1.0 (100%)

### Story 4.2: Import Error Handling

As a **user**,
I want clear error messages when my import text has issues,
So that I can fix the notation and successfully import my ranges.

**Acceptance Criteria:**

**Given** import text with unrecognized notation (e.g., `KJhh`)
**When** the parser encounters it
**Then** a clear error is displayed: "Could not parse: 'KJhh' — did you mean 'KJo' or suit-specific 'KhJh'?"
**And** successfully parsed combos are still shown on the preview grid
**And** the user can correct errors and re-parse

**Given** import text with mixed valid and invalid entries
**When** the parser processes it
**Then** all valid entries are parsed and previewed
**And** all invalid entries are listed with specific error messages and suggestions
**And** the user can choose to import partial results or fix and retry

### Story 4.3: Export Range as Text Notation

As a **user**,
I want to export my range as standard text notation,
So that I can share it with other tools, coaches, or study partners.

**Acceptance Criteria:**

**Given** an existing range with combos
**When** the user exports as text notation
**Then** the range is converted to standard notation (e.g., `JJ+, ATs+, A5s-A2s, KQo:0.75`)
**And** combos at 100% weight omit the weight suffix
**And** combos with fractional weights include the `:weight` suffix
**And** the output is grouped logically (pairs, suited, offsuit)

**Given** an exported range
**When** the export is generated
**Then** situation metadata is included as a header comment: position, action sequence, situation type

### Story 4.4: Export as Image & Copy to Clipboard

As a **user**,
I want to export my range as an image or copy it to clipboard,
So that I can share it visually on forums, Discord, or with friends.

**Acceptance Criteria:**

**Given** an existing range displayed on the grid
**When** the user exports as image
**Then** a PNG screenshot of the SVG grid is generated
**And** the image includes the range name, situation description, and color legend
**And** the image is downloaded to the user's device

**Given** an existing range
**When** the user clicks "Copy to Clipboard"
**Then** the range text notation is copied to the system clipboard
**And** a confirmation toast is shown: "Range copied to clipboard"

**Given** an exported image or text
**When** the export includes metadata
**Then** the situation context (position, action sequence, situation type) is visible in the export

---

## Epic 5: Study System — Core Drilling

Users can drill their ranges with Pick the Range drill, smart boundary-hand selection,
immediate feedback with full grid context, and session summaries.

### Story 5.1: Study Module Foundation & Integration Query

As a **developer**,
I want the Study module set up with its DbContext, domain entities, and integration query to Range Context,
So that the drill system has the data access foundation it needs.

**Acceptance Criteria:**

**Given** the Study module project exists
**When** the module is initialized
**Then** StudyDbContext is configured with schema `[study]`
**And** DrillSession aggregate root and MasteryCard entity are created
**And** the integration query pattern is wired: `GetRangeForDrillIntegrationQuery` in SharedKernel, handler in RangeContext's IntegrationHandlers/, `IRangeQueryService` interface in Study module, `RangeQueryService` implementation using IMessageBus
**And** StudyModule.cs registers all services and endpoints
**And** EF Core migration creates the study schema tables

**Given** the Study module needs range data for a drill
**When** `IRangeQueryService.GetRangeForDrillAsync()` is called
**Then** the integration query flows through Wolverine to the RangeContext handler
**And** combo data with weights is returned

### Story 5.2: Start Drill Session

As a **user**,
I want to start a drill session on a selected range,
So that I can begin actively training my range recall.

**Acceptance Criteria:**

**Given** an existing situation with at least one range
**When** the user selects a range and taps "Drill"
**Then** a DrillSession is created with the selected range and situation
**And** the drill page loads with the first question
**And** the session tracks start time and question count

**Given** the user is on the home screen
**When** the system has mastery data from previous sessions
**Then** drill suggestions are displayed: "BB Defend vs CO — 67% mastery. 12 combos due for review."
**And** the user can tap a suggestion to start a session directly

**Given** no ranges exist yet
**When** the user tries to start a drill
**Then** a message is shown: "Create a situation and range first to start drilling"

### Story 5.3: Pick the Range Drill & Question Selection

As a **user**,
I want to answer "Is this hand in your range?" questions with smart targeting of boundary hands,
So that I practice the exact hands where I make mistakes.

**Acceptance Criteria:**

**Given** an active drill session
**When** the QuestionSelector picks the next question
**Then** 60% of questions target boundary hands (combos at the edge of the range)
**And** 40% of questions target clear includes (weight > 0.8) or clear excludes (weight = 0)

**Given** a question is presented
**When** the user sees "Is [ATs] in your [CO RFI] range?"
**Then** the user can answer Yes or No
**And** if the combo has a weight > 0, the correct answer is Yes
**And** if the combo has weight = 0, the correct answer is No

**Given** a combo with a fractional weight (e.g., 0.75)
**When** the user answers Yes
**Then** a follow-up asks: "At what frequency?" with a slider or input
**And** the answer is evaluated against the actual weight (tolerance ± 10%)

### Story 5.4: Drill Feedback with Grid Context

As a **user**,
I want to see the full range grid with the queried hand highlighted after each answer,
So that every answer becomes a micro-learning moment.

**Acceptance Criteria:**

**Given** the user submits an answer
**When** the feedback is displayed
**Then** the result is shown immediately (correct = green checkmark, incorrect = red X)
**And** the full 13×13 range grid is displayed in read-only mode
**And** the queried hand cell is highlighted with a distinct border/glow
**And** the correct weight is shown for the queried combo
**And** the next question loads within 200ms (NFR2)

**Given** an incorrect answer
**When** feedback is shown
**Then** the correct answer is clearly indicated

### Story 5.5: Session Summary & Stop

As a **user**,
I want to stop my drill session at any time and see a summary of my performance,
So that I know how I did and what to focus on next.

**Acceptance Criteria:**

**Given** an active drill session
**When** the user taps "Stop" or "End Session"
**Then** the session ends and a summary is displayed

**Given** a completed session
**When** the summary is shown
**Then** it displays: total questions answered, accuracy percentage, list of missed combos with correct answers, and mastery score changes per combo
**And** missed boundary hands are highlighted separately

**Given** a session with zero questions answered
**When** the user stops immediately
**Then** no session is recorded and the user returns to the previous screen

---

## Epic 6: Advanced Drill Modes & Mastery

Users can play Spot the Difference, Puzzle Rush, and Reverse Drill. FSRS spaced
repetition tracks per-combo mastery and adapts question selection.

### Story 6.1: FSRS Integration & Mastery Scoring

As a **user**,
I want the system to track my mastery per combo and schedule reviews using spaced repetition,
So that I focus on the hands I actually struggle with instead of reviewing everything equally.

**Acceptance Criteria:**

**Given** a user answers a drill question
**When** the answer is submitted
**Then** the MasteryCard for that combo is updated via FSRS.Core
**And** FSRS calculates the next review date based on answer correctness
**And** the mastery score is updated in real time
**And** the updated mastery data is persisted in the [study] schema

**Given** a new range is created (RangeCreatedEvent received)
**When** the Study module handles the event
**Then** MasteryCards are initialized for all included combos (weight > 0) with FSRS default state

**Given** combos with below-average mastery
**When** the QuestionSelector picks the next question
**Then** below-average combos are prioritized for more frequent drilling
**And** the 60/40 boundary split still applies, but within each bucket, weaker combos come first

### Story 6.2: View Mastery Scores

As a **user**,
I want to see my mastery scores for ranges and situations,
So that I know which spots I've mastered and which need more work.

**Acceptance Criteria:**

**Given** an authenticated user with drill history
**When** they view a situation's detail page
**Then** mastery information is displayed per range: overall mastery percentage, combos mastered vs due for review

**Given** a range with mastery data
**When** the user views the range grid
**Then** cells are tinted based on mastery level (green = mastered, yellow = learning, red = struggling)
**And** this mastery overlay can be toggled on/off

**Given** the situation list page
**When** situations are displayed
**Then** each situation card shows an overall mastery indicator

### Story 6.3: Spot the Difference Drill

As a **user**,
I want to play a drill that shows my range with deliberate errors for me to identify,
So that I test my holistic range knowledge, not just individual hands.

**Acceptance Criteria:**

**Given** the user selects "Spot the Difference" drill mode
**When** the drill starts
**Then** the system generates a modified version of the user's range with 5-8 deliberate errors
**And** the modified grid is displayed

**Given** the modified grid is shown
**When** the user taps cells they believe are errors
**Then** selected cells are marked as "flagged"
**And** the user can submit when they've found all errors they can identify

**Given** the user submits their answer
**When** feedback is shown
**Then** correctly identified errors are highlighted in green
**And** missed errors are highlighted in red
**And** false positives are shown in yellow
**And** a score is calculated: correct identifications / total errors
**And** mastery scores are updated for all error combos

### Story 6.4: Puzzle Rush Drill

As a **user**,
I want to play a streak-based drill where one wrong answer ends the run,
So that I can challenge myself and track personal bests.

**Acceptance Criteria:**

**Given** the user selects "Puzzle Rush" drill mode
**When** the drill starts
**Then** questions are presented using the Pick the Range format
**And** a streak counter is prominently displayed

**Given** an active Puzzle Rush
**When** the user answers correctly
**Then** the streak counter increments
**And** the next question appears immediately (< 200ms)

**Given** an active Puzzle Rush
**When** the user answers incorrectly
**Then** the run ends immediately
**And** the final streak is displayed
**And** if it's a personal best, it's highlighted and saved

**Given** Puzzle Rush difficulty settings
**When** the user selects difficulty
**Then** "Easy" excludes boundary hands
**And** "Normal" uses the standard 60/40 boundary split
**And** "Hard" targets 80% boundary hands + requires weight accuracy

### Story 6.5: Reverse Drill

As a **user**,
I want to see a range grid and identify which situation and range it belongs to,
So that I develop visual pattern recognition for my ranges.

**Acceptance Criteria:**

**Given** the user selects "Reverse Drill" mode
**When** the drill starts
**Then** a range grid is displayed (read-only, showing all combos with weights)
**And** the user must identify the situation and the range name

**Given** the grid is displayed
**When** the user selects their answer from a list of options
**Then** the options include the correct situation/range plus 3-4 plausible distractors

**Given** the user submits their answer
**When** feedback is shown
**Then** the correct situation and range name are revealed
**And** mastery scores are updated for the identified range

**Given** the user has fewer than 4 situations with ranges
**When** they try Reverse Drill
**Then** a message explains: "You need at least 4 situations with ranges to play Reverse Drill"

---

## Epic 7: Offline & PWA

Users can install the app as a PWA, drill offline on mobile, and all data syncs
automatically when back online.

### Story 7.1: PWA Installation & Service Worker

As a **user**,
I want to install RangeTrainer on my phone's home screen like a native app,
So that I can access it instantly without opening a browser.

**Acceptance Criteria:**

**Given** a user visits the app in a supported browser
**When** they have completed their first drill session
**Then** an "Add to Home Screen" prompt is shown

**Given** the user installs the PWA
**When** they tap the home screen icon
**Then** the app launches in standalone mode (no browser chrome)
**And** the app loads in < 1 second from cache (NFR4)

**Given** the service worker is installed
**When** the app is loaded on subsequent visits
**Then** static assets are served from cache
**And** the service worker updates in the background when a new version is available
**And** the WASM bundle stays under 5MB compressed (NFR6)

### Story 7.2: IndexedDB Full Library Cache

As a **user**,
I want all my situations, ranges, and mastery data cached locally,
So that I can access everything even without internet.

**Acceptance Criteria:**

**Given** an authenticated user
**When** the app is online
**Then** the full library is synced to IndexedDB: all situations, ranges with combos, tags, and FSRS mastery card data
**And** the sync happens in the background without blocking the UI

**Given** data changes on the server
**When** the app is online
**Then** IndexedDB is updated incrementally (not full re-sync)

**Given** the user opens the app offline
**When** they browse situations and ranges
**Then** all data is served from IndexedDB
**And** the UI is indistinguishable from online mode

### Story 7.3: Offline Drill Capability

As a **user**,
I want to drill my ranges while offline,
So that I can study anywhere without needing internet.

**Acceptance Criteria:**

**Given** the app is offline and IndexedDB has cached data
**When** the user starts a drill session
**Then** the drill functions fully: questions are generated, answers are evaluated, feedback with grid is shown
**And** FSRS scheduling runs client-side using cached mastery card data
**And** question selection (60/40 boundary) works client-side

**Given** drill answers are submitted offline
**When** answers are recorded
**Then** they are stored in an offline drill result queue in IndexedDB
**And** a subtle indicator shows "offline — results will sync when online"

**Given** all 4 drill modes
**When** the app is offline
**Then** all drill modes function correctly using cached data

### Story 7.4: Automatic Sync on Reconnect

As a **user**,
I want my offline drill results to sync automatically when I'm back online,
So that my mastery data stays accurate without manual action.

**Acceptance Criteria:**

**Given** the app was used offline and has queued drill results
**When** internet connection is restored
**Then** the SyncService automatically pushes queued drill results to the server
**And** the server processes results sequentially and recalculates FSRS mastery scores
**And** the updated mastery data is synced back to IndexedDB

**Given** sync is in progress
**When** results are being pushed
**Then** a subtle sync indicator is shown
**And** the user can continue using the app normally during sync

**Given** sync fails
**When** the failure occurs
**Then** the queued results are preserved in IndexedDB
**And** sync is retried automatically on next connectivity change
**And** no drill results are ever lost (NFR11, NFR13)

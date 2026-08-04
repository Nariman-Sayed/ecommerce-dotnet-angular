# Definition of Done and Contribution Convention

## Branch naming

feature/<phase-name>, test/<scope>, docs/<scope>

## PR requirements

One PR per phase, or per sub-phase when a phase is split into smaller
parts. My PR description explains what changed, how I tested it, and
references the relevant video numbers from the course when applicable.
It also references any requirement or security finding it resolves.

## A phase is done when

The code is merged after mentor review, as assigned in docs/roadmap.md.
I have updated the requirements, ERD, and API contract files if the
phase changed any business rule, entity, or endpoint. No new secrets
were committed to source control. I manually tested the change against
the acceptance criteria described in the phase's PR.
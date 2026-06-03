# Day 8 — Completion Checklist

## Code

- [ ] <!-- populated during STEP 6 from the approved summary -->

## Build & Local Verification

- [ ] `dotnet build` succeeds — 0 errors, 0 warnings
- [ ] `dotnet run` starts without errors

## Infra & Config

- [ ] `Infra/Day-008/appsettings-template.md` reviewed (no new settings, or new settings documented)

## Deploy & Azure Verification

- [ ] Deploy via `/deploy` slash command
- [ ] `GET /health` returns 200 from Azure
- [ ] `POST /api/ai/chat` returns 200 from Azure

## Documentation

- [ ] `docs/notes/Day-008/architect-thinking.md` written
- [ ] `docs/notes/Day-008/posture-check.md` filled (end of day, before commit)
- [ ] Git commit: `feat(day-008): batch api cost controls`

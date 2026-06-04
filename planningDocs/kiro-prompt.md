Analyze the entire contents of the Planning-Docs/ folder and every document in docs/. Read every single file. Understand the business vision, every phase from phase-1 through the final phase, every feature, every workflow, every entity, every relationship, every business rule, every API endpoint, every page, every component, and every integration point described.

From this analysis, generate a complete set of Kiro steering files in .kiro/steering/ that serve as the permanent brain of this project. These files must contain EVERYTHING needed to implement the entire application from scratch without the user ever needing to re-explain anything.

## What to generate in .kiro/steering/:

1. **business-vision.md** — The full product vision, target users, value proposition, and success criteria
2. **domain-model.md** — Every entity, enum, status, relationship, and field with exact names and types
3. **business-rules.md** — Every validation rule, constraint, invariant, workflow condition, and edge case
4. **feature-inventory.md** — Every feature across all phases: every page, every button, every link, every modal, every form, every table, every filter, every export, every notification
5. **api-contracts.md** — Every API endpoint: route, method, request/response DTOs, authorization, validation
6. **database-design.md** — Every table, column, type, constraint, index, relationship, stored procedure, view, and seed data
7. **implementation-phases.md** — Phase-by-phase breakdown with EXACT features per phase, dependencies, and completion criteria. Nothing left ambiguous.
8. **architecture-decisions.md** — Every technology choice, pattern, library, and structural convention
9. **commands-reference.md** — Every CLI command needed to scaffold, generate, build, migrate, seed, test, and run the entire application end-to-end (dotnet, ng, npm, ef, docker — all of them, in order)
10. **conventions.md** — Naming patterns, folder structures, coding standards, file organization for both backend and frontend
11. **workflows.md** — Every user workflow end-to-end: what the user clicks, what the system does, what happens on success, what happens on failure
12. **ui-pages.md** — Every page/route in the frontend: what components it contains, what data it displays, what actions are available, what API calls it makes

## Rules for these steering files:

- Use `inclusion: auto` in the front-matter of every file
- Use `#[[file:<relative_path>]]` references to link back to original source documents
- Write in imperative language: "The system SHALL...", "This page MUST display...", "This endpoint RETURNS..."
- Preserve EXACT field names, entity names, status values, and types from the source documents
- Do NOT summarize loosely — extract PRECISE specifications
- Do NOT skip anything because it seems minor — every button, every link, every column, every validation message matters
- Each file should be comprehensive but under 400 lines — split into parts if needed (e.g., feature-inventory-part1.md, feature-inventory-part2.md)

## After generating the steering files:

Do NOT stop. Do NOT ask "shall I continue?" Do NOT present options.

Proceed immediately to implement the ENTIRE application, phase by phase, in order:

- Every backend project, layer, class, interface, handler, validator, mapper, migration, seed
- Every stored procedure, view, function in the database
- Every API endpoint fully wired with authentication, authorization, validation, error handling
- Every Angular component, service, model, route, guard, interceptor, pipe, directive
- Every page fully built with real UI — forms, tables, filters, pagination, modals, toasts, loading states
- Every integration between frontend and backend fully connected
- All Docker configuration, environment configs, CI/CD pipelines

## Standards:

- Production-grade enterprise application — no placeholder code, no TODO comments, no "implement later"
- Full error handling, logging, audit trails, soft deletes, concurrency handling
- Proper authentication and role-based authorization on every endpoint and every route
- Responsive UI with accessibility compliance
- Complete validation on both client and server side
- All CQRS commands and queries fully implemented with MediatR
- All Entity Framework configurations, migrations, and seed data

## When to stop and ask for input:

ONLY when the entire application is fully coded, builds successfully, migrations are ready, and the application is ready to launch in Visual Studio for testing. That is the ONLY time you pause and say "Ready for testing."

No shortcuts. No skipped features. No partial implementations. Everything described in the planning docs gets built.


ENTIRE APPLICATION MUST RUN FROM THE LOCAL LAPTOP, LOCAL SQL, NO DOCKER, NO CLOUD, ALL MOCK AND SHOULD WORK LOCALLY.
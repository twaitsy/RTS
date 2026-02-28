# RTS

## Definition ID lifecycle (for content authors)

Definition IDs are now treated as stable keys once they are created.

- On first asset validation, an empty `id` is auto-filled from the asset name and finalized.
- After finalization, Unity `OnValidate` no longer auto-fills missing IDs or accepts ad-hoc edits.
- If a finalized ID is cleared, Unity logs a warning and leaves it empty so the issue is explicit.
- If a finalized ID is manually edited in the Inspector, Unity warns and restores the finalized ID value.
- To intentionally rename an ID, use **Tools → Data → Definition ID Migration**.

### Intentional ID rename workflow

1. Open **Tools → Data → Definition ID Migration**.
2. Select the target definition asset.
3. Enter the new ID (must match: lowercase alphanumeric segments separated by `.`, `_`, or `-`).
4. Run **Validate + Migrate**.

The tool blocks migration when:

- The new ID is empty.
- The new ID format is invalid.
- The new ID is already used by another definition.

When migration succeeds, the tool updates the target definition ID and all matching serialized `*id` string references across ScriptableObject assets in one operation.

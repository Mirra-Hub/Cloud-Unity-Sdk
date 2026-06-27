# MirraCloud Showcase — official Unity example

A polished, runnable example that demonstrates **every MirraCloud SDK service** with a real,
visual UI (not raw JSON). You sign in from a dedicated auth screen, land on a grid of services,
and open any service to see its data rendered properly — tables, cards, avatars, reward chips,
progress bars, live countdowns, and interactive tools.

Built entirely with **UI Toolkit** (runtime UXML/USS), wired through the same **VContainer**
bootstrap the rest of the SDK uses.

---

## Run it

1. Open the scene **`Example/Scenes/MC_Showcase.unity`**.
2. Make sure `Assets/Plugins/MirraCloud/Resources/Configuration.asset` points at your backend
   (`ProjectId`, `BranchId`, `Url`). The example ships pointed at a local dev host.
3. Press **Play**.

You start on the **auth screen**: pick a provider (Guest / Device / Email, or an external
provider via in-app WebView/OpenID). On success you move to the **services screen** — a grid of
all SDK modules. Tap any card to open its detail view.

> Dev tip: the `ShowcaseInstaller` component on `ShowcaseRoot` has a `_devForceServices` toggle
> that skips the auth gate and drops you straight on the services grid (handy for visual QA).

---

## How it's put together

```
Example/
  Scenes/MC_Showcase.unity        # ShowcaseUI (UIDocument) + ShowcaseRoot (installer) + EventSystem
  UI/
    Showcase.uxml / Showcase.uss  # design-system tokens + every sc- component style
    BridgePanelSettings.asset      # ConstantPixelSize panel (crisp text)
  Scripts/Showcase/
    App/      ShowcaseApp, ShowcaseInstaller, ShowcaseModules, Nav, Popup, Toasts
    Views/    AuthView, ServicesView, ServiceView (base) + one view per service
    Components/  Avatar, Card, Chip/RewardChip/CountdownChip, StatTile, ListRow,
                 DataTable, ProgressBar, SectionHeader, Skeleton/EmptyState/ErrorState
    Infra/    ViewBind, RemoteImageLoader, Fmt
```

**Bootstrap & DI.** `ShowcaseInstaller : LifetimeScope` auto-parents to the VContainer Root scope
(which provides the `IMirraCloudSdk` singleton), grabs the scene's `UIDocument`, and runs
`ShowcaseApp` (an `IStartable`). `ShowcaseApp` builds the nav/overlay/toast hosts, gates on auth,
and routes provider buttons to the SDK.

**Auth.** `AuthView` offers Guest / Device / Email and external providers. External providers use
**OpenID over an in-app WebView** (`LoginOpenIdAsync(providerId, new OpenIdLoginOptions { UseInAppWebView = true })`)
— no native plugins required. (WebView is unavailable on WebGL/in-Editor.)

**Per-service views.** `ShowcaseApp.OpenModule` resolves a view by module id. Every service has a
hand-built `ServiceView` subclass (back button + accent title + scrollable content column). They
bind data through one small helper:

```csharp
ViewBind.Load(sdk.SomeService.SomeReadAsync(), slot, data => RenderIt(data),
    isEmpty: d => d == null || d.Length == 0,
    emptyView: () => EmptyState.Build("glyph", "Nothing here"));
```

`ViewBind` drives the uniform `AsyncOperation<RestApiResult<T>>` contract through
**Loading → Data / Empty / Error** automatically (the SDK never throws on HTTP — failures are
values), so each view only writes the happy-path render.

---

## What each service shows

| Service | View |
| --- | --- |
| Player Account | Hero card (avatar, nickname/@handle, trait chips, lifetime stat tiles, segments) + sub-profiles |
| Leaderboard | Tab per board, config chips + ranked table (medal ranks, avatars, scores) |
| Economy | Wallet tiles + energy meters (fill bar + recharge/cooldown) + item grid |
| Friends | Counts strip + friends (presence) / incoming / outgoing requests |
| Assets Storage | Summary stats + by-type breakdown + folders list + assets table |
| Chats | Lookup by channel/group id → channel header, members, recent messages |
| Tournaments | Tab per tournament, leagues with rewards-for-places, standings, your rewards |
| Challenges | Card per challenge with live progress bar, status, reward tiers, countdown |
| Daily Rewards | Streak/progress header + day-by-day reward track + streak bonuses + milestones |
| Groups | My-groups list → group card + members table (owner highlighted) |
| Remote Config | Per-group typed key/value table |
| Localization | Lookup by collection → language selector + key→translation table |
| Segments | Player membership chips + all-segments status table |
| Entities | Config snapshot → per-config dynamic field table + components |
| Cloud Save | Player key/value records (type, value, access masks, version) |
| Purchases | Store catalog (price/currency/rewards) + order history |
| Promo Codes | Redeem tool (with status gate) + redemption history |
| Profanity Filter | Check tool → verdict, masked output, matched fragments |
| Cloud Code | Invoke a function by key → dynamic JSON result |
| Analytics | Fire tools (custom event / session / playtime) |
| WebView | Open-a-URL tool (gated on `IsReady`) + live event log |
| Deployment | Local config card + resolve-branch-for-version tool |

---

## Notes

- **Read-only by intent.** Detail screens demonstrate reads (and a few non-destructive tools like
  promo redeem, profanity check, cloud-code invoke). Money/buy flows and bulk mutations are left
  out on purpose.
- **Font.** UI Toolkit's default runtime font renders as solid boxes here, so `Showcase.uss` pins
  an explicit `-unity-font` (LiberationSans). Keep that if you fork the styles.
- **Avatars.** `RemoteImageLoader` fetches avatar URLs with an initials+color fallback, so a
  missing/broken image never shows a broken box.

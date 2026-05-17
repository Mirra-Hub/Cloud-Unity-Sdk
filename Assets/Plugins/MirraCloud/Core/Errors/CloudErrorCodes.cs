namespace MirraCloud.Core.Errors
{
    /// <summary>
    /// Catalogue of every Cloud backend error code the SDK is aware of.
    /// </summary>
    /// <remarks>
    /// <para>
    /// MUST stay in sync with:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     Backend: <c>Hub Cloud Backend/CloudShared/Errors/Common/CommonErrorCodes.cs</c>
    ///     and module-specific <c>Code</c> constants in
    ///     <c>&lt;Module&gt;/Contracts/Errors/</c>.
    ///   </item>
    ///   <item>
    ///     Frontend: <c>Frontend/src/cloud/shared/errors/CloudErrorCodes.ts</c>.
    ///   </item>
    /// </list>
    /// <para>
    /// Manual synchronisation by design — codegen is deliberately out of
    /// scope. When you add a code on the backend, update this file and the
    /// frontend constant in the same PR.
    /// </para>
    /// </remarks>
    public static class CloudErrorCodes
    {
        // ─── Common ─────────────────────────────────────────────────────
        public const string CommonNotFound = "common.not_found";
        public const string CommonForbidden = "common.forbidden";
        public const string CommonUnauthorized = "common.unauthorized";
        public const string CommonConflict = "common.conflict";
        public const string CommonValidation = "common.validation";
        public const string CommonConcurrency = "common.concurrency";
        public const string CommonRateLimited = "common.rate_limited";

        // ─── Internal (server / contract violations) ────────────────────
        public const string InternalUnhandled = "internal.unhandled";
        public const string InternalUntyped = "internal.untyped_error";

        // ─── Module-specific codes ──────────────────────────────────────
        // Group by module, keep the prefix consistent with the backend
        // <module>.<kind> convention.

        // Challenges
        public const string ChallengesParticipationRequired = "challenges.participation_required";
        public const string ChallengesAlreadyFinished = "challenges.already_finished";
        public const string ChallengesPlayerNotFound = "challenges.player_not_found";
        public const string ChallengesClaimNotAllowed = "challenges.claim_not_allowed";
        public const string ChallengesClaimNotReady = "challenges.claim_not_ready";
        public const string ChallengesEmptyPlayerName = "challenges.empty_player_name";

        // Leaderboards
        public const string LeaderboardsParticipationRequired = "leaderboards.participation_required";

        // Tournaments
        public const string TournamentsParticipationRequired = "tournaments.participation_required";
    }
}

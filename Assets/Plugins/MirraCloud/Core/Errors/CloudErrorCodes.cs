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

        // ─── AssetsStorage ──────────────────────────────────────────────
        public const string AssetsStorageAssetNameConflict = "assets_storage.asset_name_conflict";
        public const string AssetsStorageAssetNotFound = "assets_storage.asset_not_found";
        public const string AssetsStorageAssetPathInvalid = "assets_storage.asset_path_invalid";
        public const string AssetsStorageAssetProjectOrBranchMismatch = "assets_storage.asset_project_or_branch_mismatch";
        public const string AssetsStorageAssetsConfigNotFound = "assets_storage.assets_config_not_found";
        public const string AssetsStorageAssetsConfigProjectMismatch = "assets_storage.assets_config_project_mismatch";
        public const string AssetsStorageBranchHeadCommitMissing = "assets_storage.branch_head_commit_missing";
        public const string AssetsStorageCacheKeyConflict = "assets_storage.cache_key_conflict";
        public const string AssetsStorageCacheKeyNotFound = "assets_storage.cache_key_not_found";
        public const string AssetsStorageFileNameControlCharacters = "assets_storage.file_name_control_characters";
        public const string AssetsStorageFileNameEmpty = "assets_storage.file_name_empty";
        public const string AssetsStorageFileNameForbiddenCharacters = "assets_storage.file_name_forbidden_characters";
        public const string AssetsStorageFileNameForbiddenSequence = "assets_storage.file_name_forbidden_sequence";
        public const string AssetsStorageFileNameTooLong = "assets_storage.file_name_too_long";
        public const string AssetsStorageFolderNameConflict = "assets_storage.folder_name_conflict";
        public const string AssetsStorageFolderNotFound = "assets_storage.folder_not_found";
        public const string AssetsStorageFolderProjectOrBranchMismatch = "assets_storage.folder_project_or_branch_mismatch";
        public const string AssetsStorageInsufficientStorage = "assets_storage.insufficient_storage";
        public const string AssetsStorageInvalidFolderId = "assets_storage.invalid_folder_id";
        public const string AssetsStorageQuotaLookupFailed = "assets_storage.quota_lookup_failed";
        public const string AssetsStorageTargetFolderProjectMismatch = "assets_storage.target_folder_project_mismatch";

        // ─── Challenges ─────────────────────────────────────────────────
        public const string ChallengesAlreadyFinished = "challenges.already_finished";
        public const string ChallengesClaimNotAllowed = "challenges.claim_not_allowed";
        public const string ChallengesClaimNotReady = "challenges.claim_not_ready";
        public const string ChallengesEmptyPlayerName = "challenges.empty_player_name";
        public const string ChallengesParticipationRequired = "challenges.participation_required";
        public const string ChallengesPlayerNotFound = "challenges.player_not_found";

        // ─── Deployment ─────────────────────────────────────────────────
        public const string DeploymentBranchCloneFailed = "deployment.branch_clone_failed";
        public const string DeploymentBranchConflict = "deployment.branch_conflict";
        public const string DeploymentBranchHandlerNotRegistered = "deployment.branch_handler_not_registered";
        public const string DeploymentBranchHasUncommittedChanges = "deployment.branch_has_uncommitted_changes";
        public const string DeploymentBranchInitialCannotDelete = "deployment.branch_initial_cannot_delete";
        public const string DeploymentBranchInitializationFailed = "deployment.branch_initialization_failed";
        public const string DeploymentBranchMissingHeadOrDraft = "deployment.branch_missing_head_or_draft";
        public const string DeploymentBranchNameTaken = "deployment.branch_name_taken";
        public const string DeploymentBranchNoDraftChanges = "deployment.branch_no_draft_changes";
        public const string DeploymentBranchNoHeadCommit = "deployment.branch_no_head_commit";
        public const string DeploymentBranchNotFound = "deployment.branch_not_found";
        public const string DeploymentBranchProjectMismatch = "deployment.branch_project_mismatch";
        public const string DeploymentBranchReferencedByRouting = "deployment.branch_referenced_by_routing";
        public const string DeploymentClientVersionRequired = "deployment.client_version_required";
        public const string DeploymentCommitDraftNotFound = "deployment.commit_draft_not_found";
        public const string DeploymentCommitHeadNotFound = "deployment.commit_head_not_found";
        public const string DeploymentCommitIsDraft = "deployment.commit_is_draft";
        public const string DeploymentCommitNotFound = "deployment.commit_not_found";
        public const string DeploymentCommitNothingToCommit = "deployment.commit_nothing_to_commit";
        public const string DeploymentCommitParentNotFound = "deployment.commit_parent_not_found";
        public const string DeploymentCommitProjectMismatch = "deployment.commit_project_mismatch";
        public const string DeploymentConfigKeyNotRegistered = "deployment.config_key_not_registered";
        public const string DeploymentEnvironmentRequired = "deployment.environment_required";
        public const string DeploymentInvalidBranchId = "deployment.invalid_branch_id";
        public const string DeploymentInvalidClientVersion = "deployment.invalid_client_version";
        public const string DeploymentMergeBranchesDifferentProject = "deployment.merge_branches_different_project";
        public const string DeploymentMergeBranchesHaveDrafts = "deployment.merge_branches_have_drafts";
        public const string DeploymentMergeBranchesMissingHead = "deployment.merge_branches_missing_head";
        public const string DeploymentMergeHeadCommitNotFound = "deployment.merge_head_commit_not_found";
        public const string DeploymentMergeServiceFailure = "deployment.merge_service_failure";
        public const string DeploymentMergeUnresolvedConflicts = "deployment.merge_unresolved_conflicts";
        public const string DeploymentResolveNoActiveBranch = "deployment.resolve_no_active_branch";
        public const string DeploymentRuntimeRoutingDefaultTargetMissing = "deployment.runtime_routing_default_target_missing";
        public const string DeploymentRuntimeRoutingNotConfigured = "deployment.runtime_routing_not_configured";
        public const string DeploymentServiceConfigInvariantViolation = "deployment.service_config_invariant_violation";
        public const string DeploymentTargetBranchProjectMismatch = "deployment.target_branch_project_mismatch";

        // ─── Economy ────────────────────────────────────────────────────
        public const string EconomyBaseTemplateNotFound = "economy.base_template_not_found";
        public const string EconomyBranchEnvironmentNotSet = "economy.branch_environment_not_set";
        public const string EconomyConfigInstanceNotFound = "economy.config_instance_not_found";
        public const string EconomyConfigurationInvalid = "economy.configuration_invalid";
        public const string EconomyContractInvalidArgument = "economy.contract_invalid_argument";
        public const string EconomyCurrencyOutOfRange = "economy.currency_out_of_range";
        public const string EconomyCurrencyPrecisionMismatch = "economy.currency_precision_mismatch";
        public const string EconomyDuplicateResourceKey = "economy.duplicate_resource_key";
        public const string EconomyEnergyConfigNotFound = "economy.energy_config_not_found";
        public const string EconomyEnergyInvalidDuration = "economy.energy_invalid_duration";
        public const string EconomyInvalidAmount = "economy.invalid_amount";
        public const string EconomyInvalidStableId = "economy.invalid_stable_id";
        public const string EconomyInventoryConfigNotFound = "economy.inventory_config_not_found";
        public const string EconomyInventoryDuplicateKey = "economy.inventory_duplicate_key";
        public const string EconomyInventoryInvalidKey = "economy.inventory_invalid_key";
        public const string EconomyInventoryMaxSlotsInvalid = "economy.inventory_max_slots_invalid";
        public const string EconomyInventoryNameRequired = "economy.inventory_name_required";
        public const string EconomyInventoryOverflowCircular = "economy.inventory_overflow_circular";
        public const string EconomyInventoryOverflowTargetMissing = "economy.inventory_overflow_target_missing";
        public const string EconomyInventoryOverflowTargetNotFound = "economy.inventory_overflow_target_not_found";
        public const string EconomyInventoryOverflowTargetSelf = "economy.inventory_overflow_target_self";
        public const string EconomyItemConsumeNotConfigured = "economy.item_consume_not_configured";
        public const string EconomyItemNoSlots = "economy.item_no_slots";
        public const string EconomyItemSlotNotFound = "economy.item_slot_not_found";
        public const string EconomyNotEnoughCurrency = "economy.not_enough_currency";
        public const string EconomyNotEnoughEnergy = "economy.not_enough_energy";
        public const string EconomyNotEnoughItems = "economy.not_enough_items";
        public const string EconomyResourceKeyRequired = "economy.resource_key_required";
        public const string EconomyResourceKindMismatch = "economy.resource_kind_mismatch";
        public const string EconomyResourceNotFound = "economy.resource_not_found";
        public const string EconomyTemplateInheritanceCycle = "economy.template_inheritance_cycle";
        public const string EconomyTemplateNotFound = "economy.template_not_found";
        public const string EconomyUnknownKind = "economy.unknown_kind";

        // ─── Events ─────────────────────────────────────────────────────
        public const string EventsActiveEventsCapReached = "events.active_events_cap_reached";
        public const string EventsConfigInstanceNotFound = "events.config_instance_not_found";
        public const string EventsEventBranchMismatch = "events.event_branch_mismatch";
        public const string EventsEventNotFound = "events.event_not_found";
        public const string EventsInvalidInstanceStableId = "events.invalid_instance_stable_id";
        public const string EventsOverrideBranchMismatch = "events.override_branch_mismatch";
        public const string EventsOverrideHandlerNotRegistered = "events.override_handler_not_registered";
        public const string EventsOverrideNotFound = "events.override_not_found";
        public const string EventsOverridePayloadInvalid = "events.override_payload_invalid";
        public const string EventsScheduleInvalidRange = "events.schedule_invalid_range";
        public const string EventsScheduleRecurrenceRequired = "events.schedule_recurrence_required";
        public const string EventsScheduleRequired = "events.schedule_required";

        // ─── Friends ────────────────────────────────────────────────────
        public const string FriendsInvalidPlayerId = "friends.invalid_player_id";
        public const string FriendsSideBlocked = "friends.side_blocked";

        // ─── GameAnalytics ──────────────────────────────────────────────
        public const string GameAnalyticsConfigNotFound = "game_analytics.config_not_found";
        public const string GameAnalyticsCreatedDateHeaderRequired = "game_analytics.created_date_header_required";
        public const string GameAnalyticsDashboardNotFound = "game_analytics.dashboard_not_found";
        public const string GameAnalyticsDashboardTemplateNotFound = "game_analytics.dashboard_template_not_found";
        public const string GameAnalyticsEventBatchEmpty = "game_analytics.event_batch_empty";
        public const string GameAnalyticsEventBatchTooLarge = "game_analytics.event_batch_too_large";
        public const string GameAnalyticsEventDoesNotAcceptParameters = "game_analytics.event_does_not_accept_parameters";
        public const string GameAnalyticsEventFilterBetweenRequiresTwoValues = "game_analytics.event_filter_between_requires_two_values";
        public const string GameAnalyticsEventFilterContainsRequiresString = "game_analytics.event_filter_contains_requires_string";
        public const string GameAnalyticsEventFilterInRequiresAtLeastOneValue = "game_analytics.event_filter_in_requires_at_least_one_value";
        public const string GameAnalyticsEventFilterInvalidParameterId = "game_analytics.event_filter_invalid_parameter_id";
        public const string GameAnalyticsEventFilterParameterIdentifierRequired = "game_analytics.event_filter_parameter_identifier_required";
        public const string GameAnalyticsEventFilterParameterNotInEvent = "game_analytics.event_filter_parameter_not_in_event";
        public const string GameAnalyticsEventFilterRequiresExactlyOneValue = "game_analytics.event_filter_requires_exactly_one_value";
        public const string GameAnalyticsEventHasNoFilterableParameters = "game_analytics.event_has_no_filterable_parameters";
        public const string GameAnalyticsEventHasNoParameters = "game_analytics.event_has_no_parameters";
        public const string GameAnalyticsEventIdOrNameRequired = "game_analytics.event_id_or_name_required";
        public const string GameAnalyticsEventIdsRequired = "game_analytics.event_ids_required";
        public const string GameAnalyticsEventNameRequired = "game_analytics.event_name_required";
        public const string GameAnalyticsEventNotFound = "game_analytics.event_not_found";
        public const string GameAnalyticsEventParameterNotFound = "game_analytics.event_parameter_not_found";
        public const string GameAnalyticsEventParametersMissing = "game_analytics.event_parameters_missing";
        public const string GameAnalyticsEventsNotFound = "game_analytics.events_not_found";
        public const string GameAnalyticsFunnelConditionsLimit = "game_analytics.funnel_conditions_limit";
        public const string GameAnalyticsFunnelNotFound = "game_analytics.funnel_not_found";
        public const string GameAnalyticsFunnelQueryFailed = "game_analytics.funnel_query_failed";
        public const string GameAnalyticsFunnelStepNotFound = "game_analytics.funnel_step_not_found";
        public const string GameAnalyticsFunnelStepsLimit = "game_analytics.funnel_steps_limit";
        public const string GameAnalyticsInvalidDashboardName = "game_analytics.invalid_dashboard_name";
        public const string GameAnalyticsInvalidDateRange = "game_analytics.invalid_date_range";
        public const string GameAnalyticsInvalidEventId = "game_analytics.invalid_event_id";
        public const string GameAnalyticsInvalidEventName = "game_analytics.invalid_event_name";
        public const string GameAnalyticsInvalidEventParameterValue = "game_analytics.invalid_event_parameter_value";
        public const string GameAnalyticsInvalidFunnelIdForWidget = "game_analytics.invalid_funnel_id_for_widget";
        public const string GameAnalyticsInvalidFunnelName = "game_analytics.invalid_funnel_name";
        public const string GameAnalyticsInvalidFunnelStepNumber = "game_analytics.invalid_funnel_step_number";
        public const string GameAnalyticsInvalidParameterDescription = "game_analytics.invalid_parameter_description";
        public const string GameAnalyticsInvalidParameterId = "game_analytics.invalid_parameter_id";
        public const string GameAnalyticsInvalidParameterName = "game_analytics.invalid_parameter_name";
        public const string GameAnalyticsInvalidProjectId = "game_analytics.invalid_project_id";
        public const string GameAnalyticsInvalidWidgetId = "game_analytics.invalid_widget_id";
        public const string GameAnalyticsInvalidWidgetName = "game_analytics.invalid_widget_name";
        public const string GameAnalyticsMetricKeyDuplicate = "game_analytics.metric_key_duplicate";
        public const string GameAnalyticsMetricKeyRequired = "game_analytics.metric_key_required";
        public const string GameAnalyticsMetricRequired = "game_analytics.metric_required";
        public const string GameAnalyticsMetricTemplateNotFound = "game_analytics.metric_template_not_found";
        public const string GameAnalyticsMetricValueRequired = "game_analytics.metric_value_required";
        public const string GameAnalyticsParameterNotInEvent = "game_analytics.parameter_not_in_event";
        public const string GameAnalyticsPlayerIdHeaderRequired = "game_analytics.player_id_header_required";
        public const string GameAnalyticsQuantileRange = "game_analytics.quantile_range";
        public const string GameAnalyticsQueryBuilderNotFound = "game_analytics.query_builder_not_found";
        public const string GameAnalyticsQueryFailed = "game_analytics.query_failed";
        public const string GameAnalyticsSerializationFailed = "game_analytics.serialization_failed";
        public const string GameAnalyticsSessionIdHeaderRequired = "game_analytics.session_id_header_required";
        public const string GameAnalyticsSystemEventNotDeletable = "game_analytics.system_event_not_deletable";
        public const string GameAnalyticsSystemEventNotRenamable = "game_analytics.system_event_not_renamable";
        public const string GameAnalyticsSystemFilterColumnRequired = "game_analytics.system_filter_column_required";
        public const string GameAnalyticsSystemFilterInvalidInteger = "game_analytics.system_filter_invalid_integer";
        public const string GameAnalyticsSystemFilterUnknownColumn = "game_analytics.system_filter_unknown_column";
        public const string GameAnalyticsUnknownEventParameter = "game_analytics.unknown_event_parameter";
        public const string GameAnalyticsWidgetFunnelRequired = "game_analytics.widget_funnel_required";
        public const string GameAnalyticsWidgetMetricRequired = "game_analytics.widget_metric_required";
        public const string GameAnalyticsWidgetMissing = "game_analytics.widget_missing";
        public const string GameAnalyticsWidgetNotFound = "game_analytics.widget_not_found";

        // ─── GameLocalization ──────────────────────────────────────────
        public const string GameLocalizationBranchNotEditable = "game_localization.branch_not_editable";
        public const string GameLocalizationCollectionNameInvalid = "game_localization.collection_name_invalid";
        public const string GameLocalizationCollectionNotFound = "game_localization.collection_not_found";
        public const string GameLocalizationCollectionNotInBranch = "game_localization.collection_not_in_branch";
        public const string GameLocalizationDefaultGroupInitializationFailed = "game_localization.default_group_initialization_failed";
        public const string GameLocalizationDefaultGroupNotDeletable = "game_localization.default_group_not_deletable";
        public const string GameLocalizationGroupNameInvalid = "game_localization.group_name_invalid";
        public const string GameLocalizationGroupNotFound = "game_localization.group_not_found";
        public const string GameLocalizationGroupNotInBranch = "game_localization.group_not_in_branch";
        public const string GameLocalizationKeyNameInvalid = "game_localization.key_name_invalid";
        public const string GameLocalizationLanguageNotEnabled = "game_localization.language_not_enabled";
        public const string GameLocalizationLanguageValueNotFound = "game_localization.language_value_not_found";
        public const string GameLocalizationLastLanguageRemoval = "game_localization.last_language_removal";
        public const string GameLocalizationLocalizationCollectionMismatch = "game_localization.localization_collection_mismatch";
        public const string GameLocalizationLocalizationKeyNotFound = "game_localization.localization_key_not_found";
        public const string GameLocalizationLocalizationKeyNotInBranch = "game_localization.localization_key_not_in_branch";
        public const string GameLocalizationLocalizationTextInvalid = "game_localization.localization_text_invalid";
        public const string GameLocalizationSettingsInitializationFailed = "game_localization.settings_initialization_failed";
        public const string GameLocalizationSettingsNotFound = "game_localization.settings_not_found";
        public const string GameLocalizationTranslationEmptyTargets = "game_localization.translation_empty_targets";
        public const string GameLocalizationTranslationFailed = "game_localization.translation_failed";

        // ─── Leaderboards ──────────────────────────────────────────────
        public const string LeaderboardsParticipationRequired = "leaderboards.participation_required";

        // ─── Platforms ──────────────────────────────────────────────────
        public const string PlatformsConsentStoreUnavailable = "platforms.consent_store_unavailable";
        public const string PlatformsInvalidPlatformName = "platforms.invalid_platform_name";
        public const string PlatformsLegalDocumentAlreadyPublished = "platforms.legal_document_already_published";
        public const string PlatformsLegalDocumentCustomKeyRequired = "platforms.legal_document_custom_key_required";
        public const string PlatformsLegalDocumentDuplicate = "platforms.legal_document_duplicate";
        public const string PlatformsLegalDocumentLocaleRequired = "platforms.legal_document_locale_required";
        public const string PlatformsLegalDocumentNotPublished = "platforms.legal_document_not_published";
        public const string PlatformsLegalDocumentPublished = "platforms.legal_document_published";
        public const string PlatformsLegalDocumentTitleRequired = "platforms.legal_document_title_required";
        public const string PlatformsLegalDocumentWrongPlatform = "platforms.legal_document_wrong_platform";
        public const string PlatformsLegalInfoCompanyNameRequired = "platforms.legal_info_company_name_required";
        public const string PlatformsLegalInfoContactEmailRequired = "platforms.legal_info_contact_email_required";
        public const string PlatformsLegalInfoNotConfigured = "platforms.legal_info_not_configured";
        public const string PlatformsMarketplaceSettingsTypeMismatch = "platforms.marketplace_settings_type_mismatch";
        public const string PlatformsMarketplaceTypeMismatch = "platforms.marketplace_type_mismatch";
        public const string PlatformsPlatformInUse = "platforms.platform_in_use";
        public const string PlatformsPlatformNameDuplicate = "platforms.platform_name_duplicate";

        // ─── PlayerAccounts ─────────────────────────────────────────────
        public const string PlayerAccountsAccountDoesNotExist = "player_accounts.account_does_not_exist";
        public const string PlayerAccountsAccountNotFound = "player_accounts.account_not_found";
        public const string PlayerAccountsAuthBelongsToAnotherAccount = "player_accounts.auth_belongs_to_another_account";
        public const string PlayerAccountsAuthIssuerPipelineEmpty = "player_accounts.auth_issuer_pipeline_empty";
        public const string PlayerAccountsAuthRecordNotFound = "player_accounts.auth_record_not_found";
        public const string PlayerAccountsAuthRedirectUrlMissing = "player_accounts.auth_redirect_url_missing";
        public const string PlayerAccountsBranchEnvironmentMissing = "player_accounts.branch_environment_missing";
        public const string PlayerAccountsBranchProjectMismatch = "player_accounts.branch_project_mismatch";
        public const string PlayerAccountsCacheKeyExists = "player_accounts.cache_key_exists";
        public const string PlayerAccountsCacheKeyMissing = "player_accounts.cache_key_missing";
        public const string PlayerAccountsCacheTtlInvalid = "player_accounts.cache_ttl_invalid";
        public const string PlayerAccountsDeviceIdRequired = "player_accounts.device_id_required";
        public const string PlayerAccountsEmailRequired = "player_accounts.email_required";
        public const string PlayerAccountsExternalAuthCertificateInvalid = "player_accounts.external_auth_certificate_invalid";
        public const string PlayerAccountsExternalAuthExchangeFailed = "player_accounts.external_auth_exchange_failed";
        public const string PlayerAccountsExternalAuthInvalidIdToken = "player_accounts.external_auth_invalid_id_token";
        public const string PlayerAccountsExternalAuthInvalidPayload = "player_accounts.external_auth_invalid_payload";
        public const string PlayerAccountsExternalAuthInvalidSignature = "player_accounts.external_auth_invalid_signature";
        public const string PlayerAccountsExternalAuthMisconfigured = "player_accounts.external_auth_misconfigured";
        public const string PlayerAccountsExternalAvatarDomainNotAllowed = "player_accounts.external_avatar_domain_not_allowed";
        public const string PlayerAccountsExternalAvatarMalformed = "player_accounts.external_avatar_malformed";
        public const string PlayerAccountsExternalAvatarPatternInvalid = "player_accounts.external_avatar_pattern_invalid";
        public const string PlayerAccountsExternalUserIdRequired = "player_accounts.external_user_id_required";
        public const string PlayerAccountsGuestIdRequired = "player_accounts.guest_id_required";
        public const string PlayerAccountsImageFetchFailed = "player_accounts.image_fetch_failed";
        public const string PlayerAccountsImageNotFound = "player_accounts.image_not_found";
        public const string PlayerAccountsImageUploadFailed = "player_accounts.image_upload_failed";
        public const string PlayerAccountsInvalidCredentials = "player_accounts.invalid_credentials";
        public const string PlayerAccountsInvalidProjectId = "player_accounts.invalid_project_id";
        public const string PlayerAccountsLastAuthMethod = "player_accounts.last_auth_method";
        public const string PlayerAccountsLoginRequired = "player_accounts.login_required";
        public const string PlayerAccountsMarketplaceSettingsMissing = "player_accounts.marketplace_settings_missing";
        public const string PlayerAccountsMarketplaceUnsupported = "player_accounts.marketplace_unsupported";
        public const string PlayerAccountsNicknamePatternInvalid = "player_accounts.nickname_pattern_invalid";
        public const string PlayerAccountsNicknamePatternMismatch = "player_accounts.nickname_pattern_mismatch";
        public const string PlayerAccountsNicknameProfanity = "player_accounts.nickname_profanity";
        public const string PlayerAccountsNicknameRequired = "player_accounts.nickname_required";
        public const string PlayerAccountsNicknameTaken = "player_accounts.nickname_taken";
        public const string PlayerAccountsPlatformDisabled = "player_accounts.platform_disabled";
        public const string PlayerAccountsPlatformIdInvalid = "player_accounts.platform_id_invalid";
        public const string PlayerAccountsPlatformNotFound = "player_accounts.platform_not_found";
        public const string PlayerAccountsProfileNotFound = "player_accounts.profile_not_found";
        public const string PlayerAccountsProviderDocumentTypeMismatch = "player_accounts.provider_document_type_mismatch";
        public const string PlayerAccountsProviderDuplicate = "player_accounts.provider_duplicate";
        public const string PlayerAccountsProviderHandlerMissing = "player_accounts.provider_handler_missing";
        public const string PlayerAccountsProviderMisconfigured = "player_accounts.provider_misconfigured";
        public const string PlayerAccountsProviderNotEnabled = "player_accounts.provider_not_enabled";
        public const string PlayerAccountsProviderNotFound = "player_accounts.provider_not_found";
        public const string PlayerAccountsRandomCountOutOfRange = "player_accounts.random_count_out_of_range";
        public const string PlayerAccountsRefreshTokenRequired = "player_accounts.refresh_token_required";
        public const string PlayerAccountsRepositoryFailure = "player_accounts.repository_failure";
        public const string PlayerAccountsSessionExpired = "player_accounts.session_expired";
        public const string PlayerAccountsSessionMismatch = "player_accounts.session_mismatch";
        public const string PlayerAccountsSessionNotFound = "player_accounts.session_not_found";
        public const string PlayerAccountsSessionProjectMismatch = "player_accounts.session_project_mismatch";
        public const string PlayerAccountsSettingsUniqueRelock = "player_accounts.settings_unique_relock";
        public const string PlayerAccountsSettingsValidation = "player_accounts.settings_validation";
        public const string PlayerAccountsSuccessUrlScheme = "player_accounts.success_url_scheme";
        public const string PlayerAccountsTestAccountCredentialsRequired = "player_accounts.test_account_credentials_required";
        public const string PlayerAccountsTestAccountLimitReached = "player_accounts.test_account_limit_reached";
        public const string PlayerAccountsUnsupportedProviderType = "player_accounts.unsupported_provider_type";
        public const string PlayerAccountsUserIdRequired = "player_accounts.user_id_required";

        // ─── ProjectStatistics ─────────────────────────────────────────
        public const string ProjectStatisticsInvalidOrganizationId = "project_statistics.invalid_organization_id";
        public const string ProjectStatisticsInvalidPeriod = "project_statistics.invalid_period";
        public const string ProjectStatisticsMessageDeserializationFailed = "project_statistics.message_deserialization_failed";
        public const string ProjectStatisticsMessageEmpty = "project_statistics.message_empty";
        public const string ProjectStatisticsMetricsForDateNotFound = "project_statistics.metrics_for_date_not_found";
        public const string ProjectStatisticsMetricsRoutesMissing = "project_statistics.metrics_routes_missing";
        public const string ProjectStatisticsMetricsUpdateFailed = "project_statistics.metrics_update_failed";
        public const string ProjectStatisticsOrganizationProjectsNotFound = "project_statistics.organization_projects_not_found";
        public const string ProjectStatisticsProjectIdRequired = "project_statistics.project_id_required";
        public const string ProjectStatisticsRouteNotFound = "project_statistics.route_not_found";

        // ─── RulesConstructor ──────────────────────────────────────────
        public const string RulesConstructorBranchIdRequired = "rules_constructor.branch_id_required";
        public const string RulesConstructorInvalidRuleId = "rules_constructor.invalid_rule_id";
        public const string RulesConstructorNodeChildRequired = "rules_constructor.node_child_required";
        public const string RulesConstructorRuleNotFound = "rules_constructor.rule_not_found";
        public const string RulesConstructorRuleNotModified = "rules_constructor.rule_not_modified";
        public const string RulesConstructorTreeDepthExceeded = "rules_constructor.tree_depth_exceeded";
        public const string RulesConstructorTreeNodeInvalid = "rules_constructor.tree_node_invalid";

        // ─── Tariffs ───────────────────────────────────────────────────
        public const string TariffsAccountIdRequired = "tariffs.account_id_required";
        public const string TariffsDefaultPlanProtected = "tariffs.default_plan_protected";
        public const string TariffsInvalidDeltaBytes = "tariffs.invalid_delta_bytes";
        public const string TariffsInvalidOrganizationId = "tariffs.invalid_organization_id";
        public const string TariffsInvalidPlanId = "tariffs.invalid_plan_id";
        public const string TariffsInvalidProjectId = "tariffs.invalid_project_id";
        public const string TariffsInvalidUsageBytes = "tariffs.invalid_usage_bytes";
        public const string TariffsOrganizationIdRequired = "tariffs.organization_id_required";
        public const string TariffsPlanKeyConflict = "tariffs.plan_key_conflict";
        public const string TariffsPlanKeyRequired = "tariffs.plan_key_required";
        public const string TariffsPlanNotFoundById = "tariffs.plan_not_found_by_id";
        public const string TariffsPlanNotFoundByKey = "tariffs.plan_not_found_by_key";
        public const string TariffsPlanParametersInvalid = "tariffs.plan_parameters_invalid";
        public const string TariffsProjectIdRequired = "tariffs.project_id_required";
        public const string TariffsProjectNotFound = "tariffs.project_not_found";
        public const string TariffsProjectsNotFoundForOrganization = "tariffs.projects_not_found_for_organization";
        public const string TariffsTariffCreateFailed = "tariffs.tariff_create_failed";
        public const string TariffsTariffNotFoundForOrganization = "tariffs.tariff_not_found_for_organization";

        // ─── Tournaments ───────────────────────────────────────────────
        public const string TournamentsCohortSizeInvalid = "tournaments.cohort_size_invalid";
        public const string TournamentsConfigDeleteFailed = "tournaments.config_delete_failed";
        public const string TournamentsConfigNotFound = "tournaments.config_not_found";
        public const string TournamentsConfigUpdateFailed = "tournaments.config_update_failed";
        public const string TournamentsEmptyPlayerName = "tournaments.empty_player_name";
        public const string TournamentsEmptyTables = "tournaments.empty_tables";
        public const string TournamentsInvalidFriendId = "tournaments.invalid_friend_id";
        public const string TournamentsLeagueMetaNotFound = "tournaments.league_meta_not_found";
        public const string TournamentsMissingCountry = "tournaments.missing_country";
        public const string TournamentsMissingTableId = "tournaments.missing_table_id";
        public const string TournamentsParticipationRequired = "tournaments.participation_required";
        public const string TournamentsPersistenceFailed = "tournaments.persistence_failed";
        public const string TournamentsPlayerCohortMissing = "tournaments.player_cohort_missing";
        public const string TournamentsPlayerEntryNotFound = "tournaments.player_entry_not_found";
        public const string TournamentsPlayerNotInCohort = "tournaments.player_not_in_cohort";
    }
}

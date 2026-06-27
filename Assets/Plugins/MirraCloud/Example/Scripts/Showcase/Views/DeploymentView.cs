using System;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.Deployment.Dto;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Deployment detail: the local configuration (project / configured branch / URL) as a card, plus a
    /// resolve tool that maps a client/build version to the branch the server routes it to.
    /// </summary>
    public sealed class DeploymentView : ServiceView
    {
        private TextField _version;
        private VisualElement _resultSlot;

        public DeploymentView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
            : base(meta, onBack, sdk, images)
        {
        }

        protected override void Populate()
        {
            Content.Add(BuildConfigCard());

            Content.Add(new SectionHeader("Resolve branch for version"));
            var bar = new VisualElement();
            bar.AddToClassList("sc-chat-lookup");
            _version = new TextField { label = "Client version", value = Application.version };
            _version.AddToClassList("sc-field");
            _version.AddToClassList("sc-chat-lookup__input");
            bar.Add(_version);
            var resolve = new Button(Resolve) { text = "Resolve" };
            resolve.AddToClassList("sc-btn");
            resolve.AddToClassList("sc-btn--primary");
            bar.Add(resolve);
            Content.Add(bar);

            var hint = new Label("Maps a build/client version to the branch the server will route it to.");
            hint.AddToClassList("sc-chat-hint");
            Content.Add(hint);

            _resultSlot = AddSlot();
            Replace(_resultSlot, EmptyState.Build("->", "Resolve a version to see its branch"));
        }

        private VisualElement BuildConfigCard()
        {
            var card = new Card(Meta.Accent);
            card.style.marginBottom = 14;
            card.WithTitle("Current deployment", Meta.Accent);

            string project = "—", branch = "(default)", url = null;
            try
            {
                var cfg = MirraCloud.Configuration.Load();
                if (cfg != null)
                {
                    project = string.IsNullOrEmpty(cfg.ProjectId) ? "—" : cfg.ProjectId;
                    branch = string.IsNullOrEmpty(cfg.BranchId) ? "(default)" : cfg.BranchId;
                    url = cfg.Url;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Showcase] config load failed: " + e.Message);
            }

            var stats = new VisualElement();
            stats.AddToClassList("sc-stat-grid");
            stats.Add(new StatTile("Project", "#").Set(Fmt.Truncate(project, 18)));
            stats.Add(new StatTile("Branch", "->").Set(branch));
            card.Body.Add(stats);

            if (!string.IsNullOrEmpty(url))
            {
                var chips = new VisualElement();
                chips.AddToClassList("sc-chip-row");
                chips.style.marginTop = 10;
                chips.Add(new Chip(url, ChipTone.Info));
                card.Body.Add(chips);
            }

            return card;
        }

        private void Resolve()
        {
            var version = (_version.value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(version))
            {
                Replace(_resultSlot, EmptyState.Build("->", "Enter a version, e.g. 1.0.0"));
                return;
            }
            ViewBind.Load(Sdk.Deployment.ResolveBranchAsync(version), _resultSlot, RenderResolved);
        }

        private VisualElement RenderResolved(ResolveBranchResponseDto dto)
        {
            var card = new Card(new Color(0.18f, 0.81f, 0.63f));
            card.WithTitle("Resolved branch", new Color(0.18f, 0.81f, 0.63f));
            card.Body.Add(Row("Branch name", dto.branchName, ChipTone.Accent));
            card.Body.Add(Row("Branch id", Fmt.Truncate(dto.branchId, 24), ChipTone.Neutral));
            card.Body.Add(Row("Build version", dto.buildVersion, ChipTone.Info));
            return card;
        }

        private static VisualElement Row(string title, string value, ChipTone tone)
        {
            var row = new ListRow();
            row.SetTitle(title);
            row.SetTrailing(new Chip(string.IsNullOrEmpty(value) ? "—" : value, tone));
            return row;
        }
    }
}

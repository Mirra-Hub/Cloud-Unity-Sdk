using System;
using MirraCloud.Core;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Shared chrome for a polished per-service detail screen: a back button + accent-tinted title,
    /// then a vertically scrollable content column. Concrete views fill <see cref="Content"/> from
    /// their <see cref="Populate"/> override (invoked once after the base chrome is built).
    /// </summary>
    public abstract class ServiceView : VisualElement
    {
        protected readonly ServiceMeta Meta;
        protected readonly IMirraCloudSdk Sdk;
        protected readonly RemoteImageLoader Images;

        /// <summary>Scrollable column the concrete view appends sections into.</summary>
        protected readonly ScrollView Content;

        protected ServiceView(ServiceMeta meta, Action onBack, IMirraCloudSdk sdk, RemoteImageLoader images)
        {
            Meta = meta;
            Sdk = sdk;
            Images = images;

            AddToClassList("sc-detail");

            var header = new VisualElement();
            header.AddToClassList("sc-detail__header");
            var back = new Button(() => onBack?.Invoke()) { text = "‹" };
            back.AddToClassList("sc-back-btn");
            var title = new Label(meta.Title);
            title.AddToClassList("sc-detail__title");
            title.style.color = meta.Accent;
            header.Add(back);
            header.Add(title);
            Add(header);

            Content = new ScrollView(ScrollViewMode.Vertical);
            Content.AddToClassList("sc-svc-content");
            Add(Content);

            Populate();
        }

        protected abstract void Populate();

        /// <summary>Appends an empty bound slot (used as a target for <see cref="ViewBind"/>).</summary>
        protected VisualElement AddSlot(float marginBottom = 14f)
        {
            var slot = new VisualElement();
            slot.style.marginBottom = marginBottom;
            Content.Add(slot);
            return slot;
        }

        /// <summary>Swap a slot's contents (mirrors ViewBind's private replace, for manual fan-out loads).</summary>
        protected static void Replace(VisualElement slot, VisualElement content)
        {
            if (slot == null)
            {
                return;
            }
            slot.Clear();
            if (content != null)
            {
                slot.Add(content);
            }
        }
    }
}

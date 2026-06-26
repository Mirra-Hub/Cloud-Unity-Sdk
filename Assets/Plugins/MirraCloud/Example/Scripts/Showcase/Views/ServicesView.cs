using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>Profile data shown in the services-screen header.</summary>
    public sealed class ProfileHeader
    {
        public string Nickname;
        public string Username;
        public string AvatarUrl;
    }

    /// <summary>
    /// The post-login home: a profile header (avatar + nickname + @handle + logout) over a grid
    /// of SDK module cards. Tapping a card raises <see cref="ModuleOpened"/>.
    /// </summary>
    public sealed class ServicesView : VisualElement
    {
        public event Action<ServiceMeta> ModuleOpened;
        public event Action LogoutRequested;

        public ServicesView(ProfileHeader profile, RemoteImageLoader images)
        {
            AddToClassList("sc-services");

            var bar = new VisualElement();
            bar.AddToClassList("sc-svc-topbar");

            var avatar = new Avatar(40);
            string nick = profile != null && !string.IsNullOrEmpty(profile.Nickname) ? profile.Nickname : "Player";
            string user = profile != null && !string.IsNullOrEmpty(profile.Username) ? profile.Username : "guest";
            avatar.BindUrl(images, profile != null ? profile.AvatarUrl : null, nick);
            bar.Add(avatar);

            var texts = new VisualElement();
            texts.AddToClassList("sc-svc-topbar__texts");
            var name = new Label(nick);
            name.AddToClassList("sc-svc-topbar__name");
            var handle = new Label("@" + user);
            handle.AddToClassList("sc-svc-topbar__handle");
            texts.Add(name);
            texts.Add(handle);
            bar.Add(texts);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1f;
            bar.Add(spacer);

            var logout = new Button(() => LogoutRequested?.Invoke()) { text = "Logout" };
            logout.AddToClassList("sc-btn");
            bar.Add(logout);
            Add(bar);

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("sc-svc-scroll");
            var grid = new VisualElement();
            grid.AddToClassList("sc-svc-grid");
            foreach (var m in ShowcaseModules.All)
            {
                grid.Add(BuildCard(m));
            }
            scroll.Add(grid);
            Add(scroll);
        }

        private VisualElement BuildCard(ServiceMeta m)
        {
            var card = new VisualElement();
            card.AddToClassList("sc-svc-card");

            var icon = new VisualElement();
            icon.AddToClassList("sc-svc-card__icon");
            icon.style.backgroundColor = new Color(m.Accent.r, m.Accent.g, m.Accent.b, 0.16f);
            var glyph = new Label(m.Glyph);
            glyph.AddToClassList("sc-svc-card__glyph");
            glyph.style.color = m.Accent;
            icon.Add(glyph);

            var title = new Label(m.Title);
            title.AddToClassList("sc-svc-card__title");
            title.style.color = m.Accent;

            card.Add(icon);
            card.Add(title);
            card.RegisterCallback<ClickEvent>(_ => ModuleOpened?.Invoke(m));
            return card;
        }
    }
}

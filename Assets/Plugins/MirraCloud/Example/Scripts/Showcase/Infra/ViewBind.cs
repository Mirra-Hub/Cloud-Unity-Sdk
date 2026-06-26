using System;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>
    /// Binds the SDK's uniform <c>AsyncOperation&lt;RestApiResult&lt;T&gt;&gt;</c> to a UI slot,
    /// driving it through Loading → {Data | Empty | Error}. The SDK never throws on HTTP (failures
    /// are values), so this branches on <see cref="RestApiResult.IsSuccess"/> and always renders
    /// from <c>r.Data</c> (never a stale cache).
    /// </summary>
    public static class ViewBind
    {
        public static async void Load<T>(
            AsyncOperation<RestApiResult<T>> op,
            VisualElement slot,
            Func<T, VisualElement> render,
            Func<T, bool> isEmpty = null,
            Func<VisualElement> emptyView = null)
        {
            if (slot == null)
            {
                return;
            }

            Skeleton.Into(slot);

            if (op == null)
            {
                Replace(slot, ErrorState.Build(null));
                return;
            }

            await op.Task();
            var r = op.Result;

            if (r == null || !r.IsSuccess)
            {
                Replace(slot, ErrorState.Build(r?.Error));
                return;
            }

            if (isEmpty != null && isEmpty(r.Data))
            {
                Replace(slot, emptyView != null ? emptyView() : EmptyState.Default());
                return;
            }

            VisualElement content;
            try
            {
                content = render(r.Data);
            }
            catch (Exception e)
            {
                content = ErrorState.Message(e.Message);
            }
            Replace(slot, content);
        }

        private static void Replace(VisualElement slot, VisualElement content)
        {
            slot.Clear();
            if (content != null)
            {
                slot.Add(content);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MirraCloud.Example.Sandbox
{
    /// <summary>
    /// How a single SDK method should be surfaced in the sandbox UI.
    /// </summary>
    public enum ControlKind
    {
        Action,             // fire-and-forget side effect, no meaningful payload
        Query,              // no input, returns data
        ParametrizedQuery,  // needs typed input (added in later milestones)
        Subscription        // a C# event surfaced in a live log (later milestones)
    }

    /// <summary>
    /// Normalized result of any SDK call: ready for the output area + history.
    /// </summary>
    public sealed class OpResult
    {
        public string Label;   // which control produced this
        public bool Ok;
        public string Status;  // one-line meta: OK/FAIL · HTTP · ms · retries
        public string Body;    // pretty-printed JSON body or error detail
        public string Method;  // GET/POST/...
        public string Route;
        public long? Http;
        public long DurationMs;
    }

    /// <summary>
    /// One invokable control on a module screen. The <see cref="Invoke"/> closure
    /// is hand-written per method so we keep full compile-time type safety while the
    /// generic renderer owns all the layout/binding/output boilerplate.
    /// </summary>
    public sealed class ControlDescriptor
    {
        public string Label;
        public ControlKind Kind = ControlKind.Query;
        public bool Destructive;
        public Func<Task<OpResult>> Invoke;
    }

    /// <summary>
    /// A module = one card on the home grid and one detail screen.
    /// </summary>
    public sealed class ModuleDescriptor
    {
        public string Id;
        public string Title;
        public string Glyph = "{ }";
        public Color Accent = new Color(0.60f, 0.63f, 0.65f);
        public Func<string> Info; // optional read-only key:value text block
        public List<ControlDescriptor> Controls = new List<ControlDescriptor>();
    }
}

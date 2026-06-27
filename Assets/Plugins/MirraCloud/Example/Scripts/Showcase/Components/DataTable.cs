using System;
using System.Collections;
using UnityEngine.UIElements;

namespace MirraCloud.Example.Showcase
{
    /// <summary>Column descriptor for <see cref="DataTable"/>. Cell builds a VisualElement from a row object.</summary>
    public sealed class DataColumn
    {
        public string Header;
        public Func<object, VisualElement> Cell;
        public float Grow = 1f;       // proportional width when FixedWidth == false
        public bool FixedWidth;
        public float Px;              // fixed pixel width when FixedWidth == true
        public string Align;          // null/"left" | "right" | "center"
    }

    /// <summary>
    /// A ranked/columnar table (sticky header + scrollable rows). Plain ScrollView-based — robust
    /// for showcase-sized lists; swap to a virtualized ListView later if rows get large.
    /// </summary>
    public sealed class DataTable : VisualElement
    {
        private readonly DataColumn[] _cols;
        private readonly ScrollView _body;

        public DataTable(DataColumn[] cols)
        {
            _cols = cols ?? Array.Empty<DataColumn>();
            AddToClassList("sc-table");

            var header = new VisualElement();
            header.AddToClassList("sc-table__header");
            foreach (var c in _cols)
            {
                var h = new Label(c.Header ?? string.Empty);
                h.AddToClassList("sc-table__hcell");
                ApplyColumn(h, c);
                header.Add(h);
            }
            Add(header);

            _body = new ScrollView(ScrollViewMode.Vertical);
            _body.AddToClassList("sc-table__body");
            Add(_body);
        }

        public DataTable Bind(IEnumerable rows, Func<object, bool> highlight = null)
        {
            _body.Clear();
            if (rows == null)
            {
                return this;
            }
            foreach (var row in rows)
            {
                var tr = new VisualElement();
                tr.AddToClassList("sc-table__row");
                if (highlight != null && highlight(row))
                {
                    tr.AddToClassList("sc-table__row--hi");
                }
                foreach (var c in _cols)
                {
                    var cell = new VisualElement();
                    cell.AddToClassList("sc-table__cell");
                    ApplyColumn(cell, c);
                    var inner = c.Cell != null ? c.Cell(row) : new Label(string.Empty);
                    if (inner != null)
                    {
                        cell.Add(inner);
                    }
                    tr.Add(cell);
                }
                _body.Add(tr);
            }
            return this;
        }

        private static void ApplyColumn(VisualElement el, DataColumn c)
        {
            if (c.FixedWidth)
            {
                el.style.width = c.Px;
                el.style.flexGrow = 0f;
                el.style.flexShrink = 0f;
            }
            else
            {
                el.style.flexGrow = c.Grow <= 0f ? 1f : c.Grow;
                el.style.flexBasis = new Length(0f);
                el.style.flexShrink = 1f;
            }

            if (c.Align == "right")
            {
                el.AddToClassList("sc-cell-right");
            }
            else if (c.Align == "center")
            {
                el.AddToClassList("sc-cell-center");
            }
        }
    }
}

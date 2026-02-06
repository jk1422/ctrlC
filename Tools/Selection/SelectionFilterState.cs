using static ctrlC.Tools.Selection.SelectionTool;

public sealed class SelectionFilterState
{
    public bool Roads = true;
    public bool Buildings = true;
    public bool Trees = true;
    public bool Props = true;
    public bool Areas = true;

    public bool All => Roads && Buildings && Trees && Props && Areas;

    public void SetAll() => Roads = Buildings = Trees = Props = Areas = !All;

    public void Toggle(SelectableFilters f)
    {
        switch (f)
        {
            case SelectableFilters.Road: Roads = !Roads; break;
            case SelectableFilters.Building: Buildings = !Buildings; break;
            case SelectableFilters.Tree: Trees = !Trees; break;
            case SelectableFilters.Prop: Props = !Props; break;
            case SelectableFilters.Area: Areas = !Areas; break;
        }
    }

    public SelectableFilters ToMask()
    {
        SelectableFilters m = SelectableFilters.None;
        if (Roads) m |= SelectableFilters.Road;
        if (Buildings) m |= SelectableFilters.Building;
        if (Trees) m |= SelectableFilters.Tree;
        if (Props) m |= SelectableFilters.Prop;
        if (Areas) m |= SelectableFilters.Area;
        return m;
    }
}
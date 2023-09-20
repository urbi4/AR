public class NavItem
{

    public NavItem(string finalIdentifier, string label, string description)
    {
        this.FinalIdentifier = finalIdentifier;
        this.Label = label;
        this.Description = description;
    }

    public string FinalIdentifier { get; set; }
    public string Label { get; set; }
    public string Description { get; set; }
}
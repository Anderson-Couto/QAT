namespace QAT.Core.Models.Google;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class ElementValue
{
    public string text { get; set; }
    public int value { get; set; }
}

public class Element
{
    public ElementValue distance { get; set; }
    public ElementValue duration { get; set; }
    public ElementValue duration_in_traffic { get; set; }
    public string status { get; set; }
}

public class Row
{
    public List<Element> elements { get; set; }
}

public class GoogleDistanceMatrix
{
    public List<string> destination_addresses { get; set; }
    public List<string> origin_addresses { get; set; }
    public List<Row> rows { get; set; }
    public string status { get; set; }
}




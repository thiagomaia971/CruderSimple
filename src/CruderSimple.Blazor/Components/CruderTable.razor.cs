using Blazorise;
using CruderSimple.Core.Extensions;
using Microsoft.AspNetCore.Components;

namespace CruderSimple.Blazor.Components;

public partial class CruderTable : ComponentBase
{
    [Parameter] public string Title {get;set;}
    [Parameter] public IEnumerable<object> Data {get;set;} = Enumerable.Empty<object>();
    public Table Table { get; set; }
    public TableHeader TableHeader { get; set; }
    public List<string> Headers { get; set; } = new List<string>();
    public List<List<string>> Values { get; set; } = new List<List<string>>();

    protected override Task OnParametersSetAsync()
    {
        if (Data == null || !Data.GetType().GenericTypeArguments.Any())
            return base.OnParametersSetAsync();

        Headers = new List<string>();
        var properties = Data.GetType().GenericTypeArguments[0].GetProperties();
        foreach (var property in properties)
            Headers.Add(property.Name);

        Values = new List<List<string>> { new List<string>() };

        foreach (var data in Data)
        {
            var values = new List<string>();
            foreach (var property in properties)
                values.Add(property.GetValue(data).ToJson());

            Values.Add(values);
        }
        return base.OnParametersSetAsync();
    }
}

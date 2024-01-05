using static CruderSimple.Blazor.Components.BreadcrumbList;

namespace CruderSimple.Blazor.Services
{
    public class PageParameter
    {
        public string PageTitle { get; set; }
        public Breadcrum[] Breadcrums { get; set; }
    }
}

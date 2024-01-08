using CruderSimple.Blazor.Components;
using static CruderSimple.Blazor.Components.BreadcrumbList;

namespace CruderSimple.Blazor.Services
{
    public static class PageParameter
    {
        public static Guid Guid { get; set; } = Guid.NewGuid();
        public static string PageTitle { get; set; }
        public static Breadcrum[] Breadcrums { get; set; }
    }
}

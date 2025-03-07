namespace CruderSimple.Core.Services
{
    public class PermissionService
    {
        public bool CanRead { get; set; } = false;
        public bool CanWrite { get; set; } = false;

        public string CanReadTooltip(string text, bool ignore) 
            => ignore || CanRead ? text : $"{text}: Sem permissão";

        public string CanWriteTooltip(string text, bool ignore) 
            => ignore || CanWrite ? text : $"{text}: Sem permissão";

    }
}

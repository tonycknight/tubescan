using TubeScan.Models;

namespace TubeScan.Stations
{
    internal static class DtoExtensions
    {
        public static StationTag FromDto(this StationTagDto dto)
            => new StationTag(dto.NaptanId, dto.Tag);
    }
}

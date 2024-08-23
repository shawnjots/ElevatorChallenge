namespace EventChallenge.Services.Interfaces
{
    public interface IMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);
        List<TDestination> MapList<TSource, TDestination>(List<TSource> sourceList);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventChallenge.Services.Mappers
{
	public interface IMapper
	{
		TDestination Map<TSource, TDestination>(TSource source);
		List<TDestination> MapList<TSource, TDestination>(List<TSource> sourceList);
	}
}

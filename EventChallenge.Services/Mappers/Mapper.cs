using AutoMapper;
using ElevatorChallenge.Models;
using ElevatorChallenge.DTOs.Elevator;
using ElevatorChallenge.DTOs.Passenger;
using ElevatorChallenge.DTOs.Floor;
using EventChallenge.Services.Interfaces;
using IMapper = EventChallenge.Services.Interfaces.IMapper;

namespace EventChallenge.Services.Mappers
{
    public class Mapper : IMapper
	{
		private readonly AutoMapper.IMapper _mapper;

		public Mapper()
		{
			var configuration = new MapperConfiguration(config =>
			{
				config.CreateMap<Elevator, ElevatorDTO>()
					.ForMember(dest => dest.Status, opt => opt.MapFrom(src => new ElevatorStatusDTO { Status = src.CurrentStatus.ToString() }))
					.ForMember(dest => dest.Direction, opt => opt.MapFrom(src => new ElevatorDirectionDTO { Direction = src.CurrentDirection.ToString() }));

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
				config.CreateMap<ElevatorDTO, Elevator>()
					.ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => Enum.Parse<Elevator.Status>(src.Status.Status)))
					.ForMember(dest => dest.CurrentDirection, opt => opt.MapFrom(src => Enum.Parse<Elevator.Direction>(src.Direction.Direction)));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.

				config.CreateMap<FloorDTO, Floor>()
					.ForMember(dest => dest.ScheduledPassengers, opt => opt.MapFrom(src => src.PassengerQueue));

				config.CreateMap<Floor, FloorDTO>()
					.ForMember(dest => dest.PassengerQueue, opt => opt.MapFrom(src => src.ScheduledPassengers));

				config.CreateMap<Passenger, PassengerDTO>();

				config.CreateMap<PassengerDTO, Passenger>();
			});

			_mapper = configuration.CreateMapper();
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			return _mapper.Map<TSource, TDestination>(source);
		}

		public List<TDestination> MapList<TSource, TDestination>(List<TSource> sourceList)
		{
			return _mapper.Map<List<TSource>, List<TDestination>>(sourceList);
		}
	}
}

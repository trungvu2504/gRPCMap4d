using AutoMapper;
using Grpc.Core;
using gRPCMap4d.Protos;
using Map4dApiShared.Directions;
using Map4dApiShared.Directions.Models;
using Route = gRPCMap4d.Protos.Route;

namespace gRPCMap4d.Services
{
    public class RouteService : Route.RouteBase
    {
        /// <summary>
        /// Direction service
        /// </summary>
        private readonly IDirectionService directionService;

        /// <summary>
        /// Mapper service
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directionService"></param>
        /// <param name="mapper"></param>
        public RouteService(IDirectionService directionService, IMapper mapper)
        {
            this.directionService = directionService;
            this.mapper = mapper;
        }

        /// <summary>
        /// Get routes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<RoutesReply> GetRoutes(RouteRequest request, ServerCallContext context)
        {
            Direction? direction = await GetRoutesAsync(request);
            return mapper.Map<RoutesReply>(direction);
        }

        /// <summary>
        /// get routes eta 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<RoutesETAReply> GetRoutesETA(RouteETARequest request, ServerCallContext context)
        {
            List<DirectionETA>? directions = await GetRoutesETAAsync(request);
            List<RouteETAReply> routes = mapper.Map<List<RouteETAReply>>(directions);
            RoutesETAReply rs = new RoutesETAReply();
            rs.Routes.Add(routes);
            return rs;
        }

        /// <summary>
        /// Get routes from direction service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<Direction?> GetRoutesAsync(RouteRequest request)
        {
            return await directionService.GetRouteAsync(
                origin: request.Origin,
                destination: request.Destination,
                waypoints: request.Waypoints,
                mode: request.Mode,
                weighting: request.Weighting.ToString(),
                avoid: null,
                avoidRoads: null,
                language: request.Language
                );
        }

        /// <summary>
        /// Get routes eta from direction service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<List<DirectionETA>?> GetRoutesETAAsync(RouteETARequest request)
        {
            var origins = request.Origins!.Select(e => e.Location).ToList();
            var aliases = request.Origins!.Select(e => e.Alias).ToList();
            return await directionService.GetRouteETAAsync
                (
                    origins: origins,
                    aliases: aliases,
                    destination: request.Destination,
                    mode: request.Mode,
                    weighting: request.Weighting.ToString(),
                    avoid: null,
                    avoidRoads: null,
                    language: "vi"
                );
        }
    }
}

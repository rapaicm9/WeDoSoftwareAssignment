using Microsoft.EntityFrameworkCore;
using TrainingTracker.Database;
using TrainingTracker.Common.CQRS;
using TrainingTracker.Common.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace TrainingTracker.Features.Users.GetUserById
{
    public sealed class GetUserByIdQueryHandler 
        : IQueryHandler<GetUserByIdQuery, UserResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(
            AppDbContext dbContext, 
            ILogger<GetUserByIdQueryHandler> logger,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<UserResponse>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .Where(user => user.Id == request.UserId)
                    .ProjectTo<UserResponse>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user is null)
                {
                    return Result<UserResponse>.Failure(new Error(
                        Code: "Users.NotFound",
                        Message: "User was not found",
                        Type: ErrorType.NotFound));
                }

                return Result<UserResponse>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user with ID {UserId}", request.UserId);

                return Result<UserResponse>.Failure(new Error(
                    Code: "Users.GetByIdFailed",
                    Message: "An error occurred while getting the user",
                    Type: ErrorType.Failure));
            }
        }
    }
}

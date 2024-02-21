using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementations;

public class AuthProvider(
    IValidator<UserRegisterDTO> userRegisterValidator,
    UserManager<User> userManager,
    ITokenProvider tokenProvider,
    IValidator<UserLoginDTO> userLoginValidator,
    MikesRecipesDbContext dbContext,
    IClock clock,
    IOptions<JwtAuthOptions> jwtAuthOptions) : IAuthProvider
{
    private readonly IValidator<UserRegisterDTO> _userRegisterValidator = userRegisterValidator;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly MikesRecipesDbContext _dbContext = dbContext;
    private readonly IValidator<UserLoginDTO> _userLoginValidator = userLoginValidator;
    private readonly IClock _clock = clock;
    private readonly JwtAuthOptions _jwtAuthOptions = jwtAuthOptions.Value;

    public async Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = _userLoginValidator.Validate(userLoginDTO);
        if (!validationResult.IsValid)
        {
            return Response.Failure<TokensDTO>(new Error(validationResult.ToString()));
        }

        var user = await _userManager.FindByEmailAsync(userLoginDTO.Email);

        if (user is null)
        {
            return Response.Failure<TokensDTO>(new Error("Invalid email or password."));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Response.Failure<TokensDTO>(new Error("Try later."));
        }

        if (!await _userManager.CheckPasswordAsync(user, userLoginDTO.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Response.Failure<TokensDTO>(new Error("Invalid email or password."));
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var dto = new GenerateAccessTokenDTO(user.Id, user.UserName!, user.Email!, userRoles);
        string accessToken = _tokenProvider.GenerateAccessToken(dto);
        string refreshToken = _tokenProvider.GenerateRefreshToken();

        var existingRefreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id 
            && e.LoginProvider == UserToken.RefreshTokenLoginProviderName 
            && e.Name == UserToken.RefreshTokenName, cancellationToken);

        if (existingRefreshToken is not null)
        {
            existingRefreshToken.ExpiryDate = _clock.GetUtcNow().AddDays(_jwtAuthOptions.RefreshTokenLifetimeDaysCount);
            existingRefreshToken.Value = refreshToken;
        }
        else
        {
            var token = new UserToken
            {
                ExpiryDate = _clock.GetUtcNow().AddDays(_jwtAuthOptions.RefreshTokenLifetimeDaysCount),
                Value = refreshToken,
                UserId = user.Id,
                LoginProvider = UserToken.RefreshTokenLoginProviderName,
                Name = UserToken.RefreshTokenName
            };

            _dbContext.UserTokens.Add(token);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(accessToken, refreshToken);
    }

    public Task<Response<TokensDTO>> RefreshToken(TokensDTO tokensDTO, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = _userRegisterValidator.Validate(userRegisterDTO);
        if (!validationResult.IsValid)
        {
            return Response.Failure(new Error(validationResult.ToString()));
        }

        var userWithPassedEmail = await _userManager.FindByEmailAsync(userRegisterDTO.Email);
        if (userWithPassedEmail is not null)
        {
            return Response.Failure(new Error("Email is already taken."));
        }

        var user = new User
        {
            Email = userRegisterDTO.Email.Trim(),
            UserName = userRegisterDTO.Username.Trim(),
        };

        var creationResult = await _userManager.CreateAsync(user, userRegisterDTO.Password);
        if (!creationResult.Succeeded)
        {
            return Response.Failure(new Error(string.Join(Environment.NewLine, creationResult.Errors.Select(e => e.Description))));
        }

        var addedUser = await _userManager.FindByEmailAsync(user.Email);
        await _userManager.AddToRoleAsync(user, DefaultRoles.User);
        return Response.Success();
    }
}

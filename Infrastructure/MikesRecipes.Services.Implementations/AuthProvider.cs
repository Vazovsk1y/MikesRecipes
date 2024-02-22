using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MikesRecipes.DAL;
using MikesRecipes.Domain.Constants;
using MikesRecipes.Domain.Models;
using MikesRecipes.Domain.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static MikesRecipes.Services.Contracts.Auth;

namespace MikesRecipes.Services.Implementations;

public class AuthProvider : BaseService, IAuthProvider
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly JwtAuthOptions _jwtAuthOptions;
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;

    public AuthProvider(
        IClock clock, 
        ILogger<BaseService> logger, 
        MikesRecipesDbContext dbContext, 
        IServiceScopeFactory serviceScopeFactory,
        UserManager<User> userManager, 
        ITokenProvider tokenProvider, 
        IOptions<JwtAuthOptions> jwtAuthOptions, 
        IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory) : base(clock, logger, dbContext, serviceScopeFactory)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _jwtAuthOptions = jwtAuthOptions.Value;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    }

    public async Task<Response<TokensDTO>> LoginAsync(UserLoginDTO userLoginDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(userLoginDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure<TokensDTO>(validationResult.Error);
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

        var claimsPrincipal = await _userClaimsPrincipalFactory.CreateAsync(user);
        string accessToken = _tokenProvider.GenerateAccessToken(claimsPrincipal);
        string refreshToken = _tokenProvider.GenerateRefreshToken();

        var existingRefreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == user.Id
            && e.LoginProvider == UserToken.RefreshTokenLoginProvider
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
                LoginProvider = UserToken.RefreshTokenLoginProvider,
                Name = UserToken.RefreshTokenName
            };

            _dbContext.UserTokens.Add(token);
        }

        if (user.AccessFailedCount > 0)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TokensDTO(accessToken, refreshToken);
    }

    public async Task<Response> RegisterAsync(UserRegisterDTO userRegisterDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(userRegisterDTO);
        if (validationResult.IsFailure)
        {
            return Response.Failure(validationResult.Error);
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

    public async Task<Response<string>> RefreshTokenAsync(TokensDTO tokensDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var principal = GetClaimsPrincipalFromExpiredJwtToken(tokensDTO.AccessToken);
        if (principal is null || principal.Identity is null || principal.Identity.IsAuthenticated is false)
        {
            return Response.Failure<string>(new Error("Invalid access token."));
        }

        var userId = Guid.Parse(principal.Claims.Single(e => e.Type == ClaimTypes.NameIdentifier).Value);
        var refreshToken = await _dbContext
            .UserTokens
            .SingleOrDefaultAsync(e => e.UserId == userId
            && e.LoginProvider == UserToken.RefreshTokenLoginProvider
            && e.Name == UserToken.RefreshTokenName, cancellationToken);

        var currentDate = _clock.GetUtcNow();
        if (refreshToken is null || refreshToken.Value != tokensDTO.RefreshToken || refreshToken.ExpiryDate < currentDate)
        {
            return Response.Failure<string>(new Error("Refresh token was invalid or was not found."));
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        var newClaimsPrincipal = await _userClaimsPrincipalFactory.CreateAsync(user!);

        string newJwtToken = _tokenProvider.GenerateAccessToken(newClaimsPrincipal);
        return newJwtToken;
    }

    private ClaimsPrincipal? GetClaimsPrincipalFromExpiredJwtToken(string token)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.SecretKey));

        var tokenValidationParametrs = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtAuthOptions.Issuer,
            ValidAudience = _jwtAuthOptions.Audience,
            IssuerSigningKey = signingKey,
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, tokenValidationParametrs, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when jwt token validating.");
            return null;
        }
    }
}

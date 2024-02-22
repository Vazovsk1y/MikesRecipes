using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

public class AuthProvider(
    IValidator<UserRegisterDTO> userRegisterValidator,
    UserManager<User> userManager,
    ITokenProvider tokenProvider,
    IValidator<UserLoginDTO> userLoginValidator,
    MikesRecipesDbContext dbContext,
    IClock clock,
    IOptions<JwtAuthOptions> jwtAuthOptions,
    ILogger<AuthProvider> logger,
    IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory) : IAuthProvider
{
    private readonly IValidator<UserRegisterDTO> _userRegisterValidator = userRegisterValidator;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly MikesRecipesDbContext _dbContext = dbContext;
    private readonly IValidator<UserLoginDTO> _userLoginValidator = userLoginValidator;
    private readonly IClock _clock = clock;
    private readonly JwtAuthOptions _jwtAuthOptions = jwtAuthOptions.Value;
    private readonly ILogger _logger = logger;
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;

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

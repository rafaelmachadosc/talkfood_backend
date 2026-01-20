using Backend.Application.DTOs.Auth;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> CreateUserAsync(string name, string email, string password, Role role, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email já cadastrado");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Name = name,
            Email = email.ToLower().Trim(),
            Password = hashedPassword,
            Role = role
        };

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        return new UserDto
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email,
            Role = createdUser.Role
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}
